using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Metadata;
using NHibernate.Type;
using NHibernate;
using NHibernate.Impl;
using NHibernate.Persister.Entity;
using Sage.SalesLogix.Web.Controls.Lookup;
using System.Web.UI;
using Sage.SalesLogix.HighLevelTypes;
using System.Web;
using Sage.Platform.Orm;
using System.Text.RegularExpressions;
using Sage.SalesLogix.Security;

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// Attempts to build a lookup based on the definition in the SLX metadata.
    /// This only works for simple lookups.
    /// </summary>
    [ValidationProperty("LookupResultValue")]
    public class SimpleLookup : LookupControl
    {
        #region Lookup metadata extraction

        internal static LookupPropertyCollection GetLookupProperties(String tableName, String lookupName, String entityTypeName)
        {
            LookupPropertyCollection result = null;
            if (HttpContext.Current != null &&
                ((result = HttpContext.Current.Cache["LookupProperties$" + tableName + "$" + lookupName] as LookupPropertyCollection) != null))
            {
                return result;
            }
            result = new LookupPropertyCollection();

            using (var sess = new SessionScopeWrapper())
            {
                Type entityType = Type.GetType(entityTypeName);
                if (entityType == null)
                    throw new ArgumentException("Unable to locate type " + entityTypeName);
                if (entityType.IsInterface)
                    throw new ArgumentException("Must use the concrete class as EntityTypeName (e.g., Sage.SalesLogix.Entities.Contact)");
                String entityName = ((SessionFactoryImpl)sess.SessionFactory).TryGetGuessEntityName(entityType);
                if (entityName == null)
                    throw new ArgumentException("Unable to locate persister for entity type " + entityType.FullName);
                AbstractEntityPersister persister = (AbstractEntityPersister)((SessionFactoryImpl)sess.SessionFactory).GetEntityPersister(entityName);
                foreach (String[] lookupField in GetLookupFields(sess, tableName, lookupName))
                {
                    String[] tableField = lookupField[0].Split(new char[] { ':' });
                    if (persister == null || persister.TableName != tableField[0])
                    {
                        throw new ArgumentException("Invalid lookup data - table name does not match persister table (" + persister.TableName + ") - check EntityName settin");
                    }
                    String propName = DecomposePath((SessionFactoryImpl)sess.SessionFactory, persister, tableField[1]);

                    result.Add(new LookupProperty(propName, lookupField[1]));
                }
            }
            return result;
        }

        private static readonly Regex _fieldPathRegexp =
            new Regex(@"(\w+)(=|>|<)(\w+)\.(\w+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        /// <summary>
        /// Path represents path to a field, based at origin.
        /// Return the field object.
        /// A field path is normally of the form:
        /// Source Table:Path
        /// where Path is recursively defined as either:
        /// FieldName
        /// or
        /// From Field=To Field.To Table!Path
        /// </summary>
        /// <param name="root"></param>
        /// <param name="path"></param>
        internal static String DecomposePath(SessionFactoryImpl sf, AbstractEntityPersister root, String path)
        {
            String[] parts;

            parts = path.Split(new char[] { '!' }, 2);
            if (parts.Length == 1)
            {
                // field name
                // remove initial "@" (this is used to indicate calculated fields)
                // TODO: fetch calculation from calculatedfielddata, to bypass the
                // SLX provider interpreter
                if (parts[0][0] == '@')
                    parts[0] = parts[0].Substring(1);
                foreach (String propName in root.PropertyNames)
                {
                    String[] columns = root.ToColumns(propName);
                    if (columns.Length == 1 && columns[0] == parts[0])
                    {
                        return propName;
                    }
                }
                throw new ArgumentException("Unable to locate property by column - " + parts[0]);
            }
            else
            {
                String newpath = parts[1];  // part after the exclamation mark
                Match matches = _fieldPathRegexp.Match(parts[0]);
                if (!matches.Success)
                    throw new ArgumentException("Path did not match field expression pattern: " + parts[0]);
                System.Diagnostics.Debug.Assert(matches.Groups.Count == 5, "Number of Groups should have been 5, was " + matches.Groups.Count + " (path = " + parts[0] + ")");
                String toTable = matches.Groups[4].Value;
                String fromField = matches.Groups[1].Value;
                String propertyName;
                root = FindJoinedEntity(sf, root, toTable, fromField, out propertyName);
                if (root == null)
                    throw new ArgumentException("Unable to locate linked property " + toTable + " via " + fromField + "!");
                return propertyName + "." + DecomposePath(sf, root, newpath);
            }
        }

        /// <summary>
        /// Find a join.  Return the name of the corresponding property.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="toTable"></param>
        /// <param name="toField"></param>
        /// <param name="fromField"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static AbstractEntityPersister FindJoinedEntity(SessionFactoryImpl sf, AbstractEntityPersister root, string toTable, string fromField, out string propertyName)
        {
            //   root.ClassMetadata.PropertyTypes.First().Na
            for (int i = 0; i < root.PropertyTypes.Length; i++)
            {
                if (root.PropertyTypes[i].IsAssociationType)
                {
                    String[] cols = root.ToColumns(root.PropertyNames[i]);
                    if (cols.Length == 1 && cols[0] == fromField)
                    {
                        propertyName = root.PropertyNames[i];
                        Type t = root.PropertyTypes[i].ReturnedClass;
                        String entityName = sf.TryGetGuessEntityName(t);
                        AbstractEntityPersister persister = (AbstractEntityPersister)sf.GetEntityPersister(entityName);
                        if (persister.TableName == toTable)
                            return persister;
                        // special case for acct mgr
                        if (toTable == "USERINFO" && persister.TableName == "USERSECURITY")
                        {
                            propertyName = propertyName + ".UserInfo";
                            entityName = "Sage.SalesLogix.Security.UserInfo";
                            return (AbstractEntityPersister)sf.GetEntityPersister(entityName);
                        }
                    }
                }
            }
            propertyName = null;
            return null;
        }


        /// <summary>
        /// Return array of fields for the lookup (a pair field name, caption)
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="lookupName"></param>
        /// <returns></returns>
        private static IEnumerable<String[]> GetLookupFields(ISession sess, String tableName, String lookupName)
        {
            var lst = sess.CreateSQLQuery("select layout from lookup where maintable=? and lookupname=?")
                    .SetString(0, tableName)
                    .SetString(1, lookupName)
                    .SetMaxResults(1).List();
            if (lst.Count == 0)
                throw new ArgumentException("Invalid lookup " + tableName + ":" + lookupName);
            String layout = (String)((object[])lst[0])[1];
            String[] layoutParts = Regex.Split(layout, "\\|\r\n\\|");
            foreach (String layoutPart in layoutParts)
            {
                String[] fields = layoutPart.Split(new char[] { '|' });
                yield return new String[] { fields[1], fields[3] };
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LookupProperties = GetLookupProperties(this.LookupTableName, this.LookupName, this.LookupEntityTypeName);
        }

        /// <summary>
        /// Table for the lookup (as defined in the SLX Lookup Manager)
        /// </summary>
        public String LookupTableName { get; set; }

        /// <summary>
        /// Lookup name, as defined in the SLX Lookup Manager.
        /// If a blank is passed then the first lookup for the specified table will be used.
        /// </summary>
        public String LookupName { get; set; }
    }
}
