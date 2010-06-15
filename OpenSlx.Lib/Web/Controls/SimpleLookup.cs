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
using log4net;

/*
    OpenSlx - Open Source SalesLogix Library and Tools

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// Attempts to build a lookup based on the definition in the SLX metadata.
    /// This only works for simple lookups.
    /// </summary>
    [ValidationProperty("LookupResultValue")]
    public class SimpleLookup : LookupControl
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimpleLookup));

        #region Lookup metadata extraction

        /// <summary>
        /// Attempt to extract properties from the lookup metadata.
        /// The result of this operation is cached.
        /// </summary>
        /// <param name="lookupName"></param>
        /// <param name="entityTypeName"></param>
        /// <returns></returns>
        internal static LookupPropertyCollection GetLookupProperties(String lookupName, String entityTypeName)
        {
            LookupPropertyCollection result = null;
            if (HttpContext.Current != null &&
                ((result = HttpContext.Current.Cache["LookupProperties$" + entityTypeName + "$" + lookupName] as LookupPropertyCollection) != null))
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
                foreach (LookupLayoutField lookupField in GetLookupFields(sess, persister.TableName, lookupName))
                {
                    String[] tableField = lookupField.Path.Split(new char[] { ':' });
                    if (persister == null || persister.TableName != tableField[0])
                    {
                        throw new ArgumentException("Invalid lookup data - table name does not match persister table (" + persister.TableName + " vs " + tableField[0] + ") - check EntityName setting");
                    }
                    String propName = DecomposePath((SessionFactoryImpl)sess.SessionFactory, persister, tableField[1], lookupField.Format);
                    if (propName != null)
                    {
                        result.Add(new LookupProperty(propName, lookupField.Caption));
                        // TODO: we should set the property format here
                    }
                }
            }
            if (LOG.IsDebugEnabled)
            {
                foreach (LookupProperty prop in result)
                {
                    LOG.Debug("Using property '" + prop.PropertyName + "' with header '" + prop.PropertyHeader + "'");
                }
            }
            HttpContext.Current.Cache["LookupProperties$" + entityTypeName + "$" + lookupName] = result;
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
        /// <param name="sf"></param>
        /// <param name="root"></param>
        /// <param name="path"></param>
        /// <param name="format"></param>
        internal static String DecomposePath(SessionFactoryImpl sf, AbstractEntityPersister root, String path, String format)
        {
            String[] parts;

            parts = path.Split(new char[] { '!' }, 2);
            if (parts.Length == 1)
            {
                // field name
                // remove initial "@" (this is used to indicate calculated fields)                
                if (parts[0][0] == '@')
                    parts[0] = parts[0].Substring(1);
                for (int i = 0; i < root.PropertyTypes.Length; i++)
                {
                    IType propType = root.PropertyTypes[i];
                    if (propType.IsCollectionType)
                    {
                        continue;
                    }
                    String propName = root.PropertyNames[i];
                    String[] columns = root.ToColumns(propName);
                    if (columns.Length == 1 && columns[0] == parts[0])
                    {
                        return FormatProperty(propName, propType, format);
                    }
                }

                LOG.Warn("Unable to locate property by column - " + parts[0]);
                return null;
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
                return propertyName + "." + DecomposePath(sf, root, newpath, format);
            }
        }

        /// <summary>
        /// Attempt to fix up the property according to the format.
        /// If it fails, return null.
        /// </summary>
        /// <param name="propName"></param>
        /// <param name="propType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private static String FormatProperty(String propName, IType propType, String format)
        {
            // adjust field for specific SLX formats
            switch (format)
            {
                case "6":  // user - change the table, but only if this is a simple field
                    if (propType.IsEntityType)
                    {
                        if (propType.Name == "Sage.SalesLogix.Security.UserInfo")
                        {
                            return propName + ".UserName";
                        }
                        else if (propType.Name == "Sage.SalesLogix.Security.User")
                        {
                            return propName + ".UserInfo.UserName";
                        }
                    }
                    break;
                case "8":  // owner - change the table, but only if this is a simple field
                    if (propType.IsEntityType && propType.Name == "Sage.SalesLogix.Security.Owner")
                    {
                        return propName + ".OwnerDescription";
                    }
                    break;
            }
            if (propType.IsEntityType)
                // if we failed to find a correct format for the property
                // don't use an entity type inside of a lookup as it messes
                // up the query
                return null;
            return propName;
        }

        /// <summary>
        /// Find a join.  Return the name of the corresponding property.
        /// </summary>
        private static AbstractEntityPersister FindJoinedEntity(SessionFactoryImpl sf, AbstractEntityPersister root, string toTable, string fromField, out string propertyName)
        {
            //   root.ClassMetadata.PropertyTypes.First().Na
            for (int i = 0; i < root.PropertyTypes.Length; i++)
            {
                if (root.PropertyTypes[i].IsAssociationType &&
                    !root.PropertyTypes[i].IsCollectionType)
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
        private static IEnumerable<LookupLayoutField> GetLookupFields(ISession sess, String tableName, String lookupName)
        {
            var lst = sess.CreateSQLQuery("select layout from lookup where maintable=? and (slxstr(lookupname)=? or searchfield=?)")
                    .SetString(0, tableName)
                    .SetString(1, lookupName)
                    .SetString(2, lookupName)
                    .SetMaxResults(1).List();
            if (lst.Count == 0)
                throw new ArgumentException("Invalid lookup " + tableName + ":" + lookupName);
            String layout = (String)((object[])lst[0])[1];
            String[] layoutParts = Regex.Split(layout, "\\|\r\n\\|");
            foreach (String layoutPart in layoutParts)
            {
                String tmp = layoutPart;
                if (tmp.StartsWith("|"))
                    tmp = layoutPart.Substring(1);
                String[] fields = tmp.Split(new char[] { '|' });
                yield return new LookupLayoutField
                {
                    Path = fields[0],
                    Caption = fields[2],
                    Format = fields[5]
                };
            }
        }

        private class LookupLayoutField
        {
            public String Path { get; set; }
            public String Format { get; set; }
            public String Caption { get; set; }
        }

        #endregion

        /// <summary>
        /// Load the lookup properties from the lookup metadata
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LookupProperties = GetLookupProperties(this.LookupName, this.LookupEntityTypeName);
            if (this.ViewState["LookupImageURL"] == null)
            {
                // Fix the image - by default the SLX controls tries to load it from the current type's assembly.
                // since this is a subclass of LookupControl it is not in the "correct" assembly anymore.
                // this fix ensures that the image is loaded from the original assembly
                LookupImageURL = this.Page.ClientScript.GetWebResourceUrl(typeof(LookupControl), "Sage.SalesLogix.Web.Controls.Resources.Find_16x16.gif");
            }
        }


        /// <summary>
        /// Lookup name, as defined in the SLX Lookup Manager.  It can also specify the search field,
        /// for example "ACCOUNT:ACCOUNT".
        /// If a blank is passed then the first lookup for the specified table will be used.
        /// </summary>
        public String LookupName { get; set; }
    }
}
