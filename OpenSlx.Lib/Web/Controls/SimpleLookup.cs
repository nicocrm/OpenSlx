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

        private static LookupPropertyCollection GetLookupProperties(String tableName, String lookupName, String entityTypeName)
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
                if(entityType.IsInterface)
                    throw new ArgumentException("Must use the concrete class as EntityTypeName (e.g., Sage.SalesLogix.Entities.Contact)");
                String entityName = ((SessionFactoryImpl)sess.SessionFactory).TryGetGuessEntityName(entityType);
                if (entityName == null)
                    throw new ArgumentException("Unable to locate persister for entity type " + entityType.FullName);
                AbstractEntityPersister persister = (AbstractEntityPersister)((SessionFactoryImpl)sess.SessionFactory).GetEntityPersister(entityName);
                foreach (String[] lookupField in GetLookupFields(sess, tableName, lookupName))
                {
                    if (!Regex.IsMatch(lookupField[0], @"^[a-z0-9_]+:@?[a-z0-9_]+$"))
                        throw new ArgumentException("Invalid lookup data - only single table fields supported at this time (was: " + lookupField[0] + ")");
                    String[] tableField = lookupField[0].Split(new char[] { ':' });
                    if (tableField[1].StartsWith("@"))
                        tableField[1] = tableField[1].Substring(1);
                    if (persister == null || persister.TableName != tableField[0])
                    {
                        throw new ArgumentException("Invalid lookup data - table name does not match persister table (" + persister.TableName + ") - check EntityName settin");
                    }
                    bool found = false;
                    foreach (String propName in persister.ClassMetadata.PropertyNames)
                    {
                        String[] columns = persister.ToColumns(propName);
                        if (columns.Length == 1 && columns[0] == tableField[1])
                        {
                            result.Add(new LookupProperty(propName, lookupField[1]));
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        throw new ArgumentException("Unable to locate property for column " + tableField[1]);
                }                
            }
            return result;
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
