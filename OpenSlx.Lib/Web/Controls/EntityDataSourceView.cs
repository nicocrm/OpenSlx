using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Collections;
using System.Web;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using log4net;
using OpenSlx.Lib.Services;
using OpenSlx.Lib.Utility;
using OpenSlx.Lib.Web.Utility;
using OpenSlx.Lib.Properties;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/nicocrm/OpenSlx)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// Allow update of a collection of business entities.    
    /// </summary>
    public class EntityDataSourceView : DataSourceView
    {
        private ICacheService _cache;
        private EntityDataSource _owner;
        //private static readonly ILog LOG = LogManager.GetLogger(typeof(EntityDataSourceView));

        public EntityDataSourceView(EntityDataSource owner, ICacheService cache)
            : base(owner, "")
        {            
            _cache = cache;
            _owner = owner;
            ((IDataSource)_owner).DataSourceChanged += delegate
            {
                OnDataSourceViewChanged(EventArgs.Empty);
            };
            MatchByAnyKey = false;
        }

        protected virtual IEnumerable<object> Entities
        {
            get
            {
                return _owner.DataSource;
            }
        }

        public override bool CanRetrieveTotalRowCount
        {
            get
            {
                return true;
            }
        }

        public override bool CanUpdate
        {
            get
            {
                return true;
            }
        }

        public override bool CanDelete
        {
            get
            {
                return true;
            }
        }

        public override bool CanPage
        {
            get
            {
                return true;
            }
        }

        public override bool CanSort
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// If this is set, then when trying to find a match using the DataKeys provided by the bound control 
        /// we will consider ANY of the property listed.  I.E., instead of doing an "AND" match, we'll do an "OR"
        /// match.
        /// This is useful for SalesLogix because EITHER InstanceId or Id will be valid, but never both at the same time.
        /// </summary>
        public bool MatchByAnyKey { get; set; }

        /// <summary>
        /// Return data selection.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
            IEnumerable<object> selection = _owner.FilterEntityList(Entities);
            if (selection == null)
            {
                if (arguments.RetrieveTotalRowCount)
                    arguments.TotalRowCount = 0;
                return new object[] { };
            }
            if (!String.IsNullOrEmpty(arguments.SortExpression))
            {
                selection = _owner.SortEntityList(selection, arguments.SortExpression);
            }
            else
            {
                selection = selection.OrderBy(v => v == null ? "" : v.ToString());
            }
            
            if (arguments.RetrieveTotalRowCount)
                arguments.TotalRowCount = selection.Count();
            
            if (arguments.MaximumRows > 0 && 
                (arguments.StartRowIndex > 0 || arguments.MaximumRows < selection.Count()))
            {
                // paging
                ArrayList pagedList = new ArrayList(arguments.MaximumRows);
                IEnumerator en = selection.GetEnumerator();

                for (int i = 0; en.MoveNext(); i++)
                {
                    if(i > arguments.StartRowIndex + arguments.MaximumRows)
                        break;
                    if(i >= arguments.StartRowIndex)
                        pagedList.Add(en.Current);
                }
                return _owner.DecorateList(pagedList);
            }
            else
            {
                return _owner.DecorateList(selection);
            }
        }

        /// <summary>
        /// Update specified rows of data (normally, only one row).
        /// </summary>
        /// <param name="keys">Dictionary of keyname/keyvalue</param>
        /// <param name="values">Dictionary of property name/property value</param>
        /// <param name="oldValues">Dictionary of property name/property value (for old values) - not used at this time</param>
        /// <returns></returns>
        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            if (keys.Count == 0)
                throw new WebControlConfigurationException(Resources.ExecuteUpdatePassedEmptyKeysArrayIsDataKey);
            IEnumerable<object> entities = FindEntityByProperties(keys);
            int updateCount = _owner.UpdateEntities(entities, values, oldValues);
            if (updateCount > 0)
                OnDataSourceViewChanged(EventArgs.Empty);
            return updateCount;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
        {
            if (keys.Count == 0)
                throw new WebControlConfigurationException(Resources.ExecuteUpdatePassedEmptyKeysArrayIsDataKey);
            var entities = FindEntityByProperties(keys);
            int deleteCount = _owner.DeleteEntities(entities);
            if(deleteCount > 0)
                OnDataSourceViewChanged(EventArgs.Empty);
            return entities.Count();
        }

        

        /// <summary>
        /// Find the objects that have the specified property values.
        /// Return empty list if not found.
        /// Used to locate entities to update.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected virtual IEnumerable<object> FindEntityByProperties(IDictionary keys)
        {
            PropertyInfo[] props = new PropertyInfo[keys.Count];
            IList<object> list = new List<object>();

            if (Entities == null || Entities.Count() == 0)
                return list;      

            foreach (object o in Entities)
            {
                bool found = true;
                int i = 0;
                foreach (DictionaryEntry entry in keys)
                {
                    if(props[i] == null)
                        props[i] = ReflectionHelper.FindPropertyOnEntity(o.GetType(), (String)entry.Key, new WebCacheService());

                    if (!object.Equals(props[i].GetValue(o, null), entry.Value))
                    {
                        found = false;
                        if (!MatchByAnyKey)
                            break;
                    }
                    else if (MatchByAnyKey && entry.Value != null)
                    {
                        found = true;
                        break;
                    }
                    i++;
                }
                if (found)
                    list.Add(o);
            }

            return list; // ignore
        }


        /// <summary>
        /// If t is a dynamic type, this will return the first static ancestor.
        /// If t is already a static type, this will return t.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Type GetStaticType(Type t)
        {
            bool found = false;
            while (!found)
            {
                try
                {
                    if (t.Assembly.Location != null)
                        found = true;
                    else
                        t = t.BaseType;
                }
                catch (NotSupportedException)
                // under .NET2 this will be thrown when accessing location of a dynamic ass
                {
                    t = t.BaseType;
                }
            }
            return t;
        }


    }
}
