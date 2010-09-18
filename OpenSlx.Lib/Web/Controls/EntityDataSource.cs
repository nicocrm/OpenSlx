using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
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
    /// Allow update of a collection of entities.
    /// </summary>
    public class EntityDataSource : DataSourceControl
    {

        /// <summary>
        /// If this event is defined then it will be raised every time the entity list
        /// needs to be retrieved.
        /// Do not use on non-trivial amount of data.
        /// </summary>
        public event EventHandler<EntityDataSource.FilterEventArgs>
            EntityFilter;

        private IEnumerable<object> _dataSource;
        /// <summary>
        /// Designate the collection of entities this source will be bound to.
        /// </summary>
        public IEnumerable<object> DataSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        [Description("The name of the method used to delete entities, which must be defined on the entities manipulated by the datasource."),
        DefaultValue("Delete")]
        public String DeleteMethodName
        {
            get { return (String)ViewState["DeleteMethodName"]; }
            set { ViewState["DeleteMethodName"] = value; }
        }

        private EntityDataSourceView _view = null;


        protected override DataSourceView GetView(string viewName)
        {
            if (_view == null)
            {
                _view = new EntityDataSourceView(this, new WebCacheService());
            }
            return _view;
        }

        /// <summary>
        /// Signals objects connected to this data source that it may have been updated.
        /// </summary>
        public virtual void Refresh()
        {
            RaiseDataSourceChangedEvent(EventArgs.Empty);
        }

        /// <summary>
        /// Used to filter down the entity list.
        /// Default implementation uses the FilterEntity event.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<object> FilterEntityList(IEnumerable<object> entities)
        {
            if (EntityFilter == null || entities == null || entities.Count() == 0)
                return entities;
            LinkedList<object> selection = new LinkedList<object>();
            FilterEventArgs args = new FilterEventArgs(null);
            foreach (object o in entities)
            {
                args.Reset(o);
                EntityFilter(this, args);
                if (!args.IsEntityExcluded())
                    selection.AddLast(o);
            }
            return selection;
        }

        /// <summary>
        /// Return the sorted
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="sortExpression"></param>
        /// <returns></returns>
        public virtual IEnumerable<object> SortEntityList(IEnumerable<object> entities, String sortExpression)
        {
            if (entities == null || entities.Count() == 0)
                return entities;
            IEnumerable<object> queryable = entities.Cast<object>();
            String[] expressionParts = sortExpression.Split(new char[] { ' ' });
            PropertyInfo prop = ReflectionHelper.FindPropertyOnEntity(queryable.First().GetType(), expressionParts[0], new WebCacheService());
            if (prop == null)
                throw new InvalidOperationException(String.Format(Resources.InvalidPropertyExpressionForSortX0, expressionParts[0]));
            if (expressionParts.Length == 2 && expressionParts[1].Equals("DESC", StringComparison.InvariantCultureIgnoreCase))
                return (IEnumerable<object>)queryable.OrderByDescending(e => prop.GetValue(e, null));
            else
                return (IEnumerable<object>)queryable.OrderBy(e => prop.GetValue(e, null));
        }

        /// <summary>
        /// Remove the specified entities, using the configured DeleteMethodName.
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public virtual int DeleteEntities(IEnumerable<object> entities)
        {
            MethodInfo deleteMethod = null;
            if (String.IsNullOrEmpty(DeleteMethodName))
                throw new WebControlConfigurationException(Resources.DeleteMethodNameMustBeConfiguredInOrderToU);
            foreach (object entity in entities)
            {
                if (deleteMethod == null)
                {
                    deleteMethod = entity.GetType().GetMethod(DeleteMethodName, BindingFlags.Public | BindingFlags.Instance);
                    if (deleteMethod == null)
                        throw new WebControlConfigurationException(String.Format(Resources.TheMethodX0CanTBeFoundOnTypeX1, DeleteMethodName, entity.GetType().FullName));
                }
                deleteMethod.Invoke(entity, null);
            }
            RaiseDataSourceChangedEvent(EventArgs.Empty);
            return entities.Count();
        }

        /// <summary>
        /// Update the specified entities using the given values (all entities in the list are updated).
        /// Return number of entities modified (which will be entities.count, unless there is an error,
        /// or a concurrency check fails)
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="values"></param>
        /// <param name="oldValues"></param>
        /// <returns></returns>
        public virtual int UpdateEntities(IEnumerable<object> entities, IDictionary values, IDictionary oldValues)
        {
            PropertyInfo[] props = new PropertyInfo[values.Count];
            foreach (object entity in entities)
            {
                int i = 0;
                foreach (DictionaryEntry entry in values)
                {
                    if (props[i] == null)
                        props[i] = ReflectionHelper.FindPropertyOnEntity(entity.GetType(), (String)entry.Key, new WebCacheService());
                    props[i].SetValue(entity,
                        BuildObjectValue(entry.Value, props[i].PropertyType, (String)entry.Key), null);
                    i++;
                }
            }
            RaiseDataSourceChangedEvent(EventArgs.Empty);
            return entities.Count();
        }


        /// <summary>
        /// Add additional properties to the list (by default it just returns the list)
        /// </summary>
        /// <param name="pagedList"></param>
        /// <returns></returns>
        public virtual IEnumerable DecorateList(IEnumerable pagedList)
        {
            return pagedList;                       
        }


        /// <summary>
        /// Convert the specified value to destinationType.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <param name="paramName">Name used for reporting</param>
        /// <returns></returns>
        private static object BuildObjectValue(object value, Type destinationType, string paramName)
        {
            if ((value != null) && !destinationType.IsInstanceOfType(value))
            {
                Type elementType = destinationType;
                bool isNullableType = false;
                if (destinationType.IsGenericType && (destinationType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    elementType = destinationType.GetGenericArguments()[0];
                    isNullableType = true;
                }
                else if (destinationType.IsByRef)
                {
                    elementType = destinationType.GetElementType();
                }
                value = ConvertType(value, elementType, paramName);
                if (isNullableType)
                {
                    Type type = value.GetType();
                    if (elementType != type)
                    {
                        throw new InvalidOperationException(String.Format(Resources.CanTConvertTypeFromX0ToNullableX1,
                            destinationType.GetGenericArguments()[0].FullName));
                    }
                }
            }
            return value;
        }


        /// <summary>
        /// Helper for type converter.  Contains some help for generic types.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        private static object ConvertType(object value, Type type, string paramName)
        {
            string text = value as string;
            if (text != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter == null)
                {
                    return value;
                }
                try
                {
                    value = converter.ConvertFromInvariantString(text);
                }
                catch (NotSupportedException)
                {
                    // this is a real type conversion error
                    throw new InvalidOperationException(String.Format(Resources.CanTConvertX0FromStringToX1, paramName, type.FullName));
                }
                catch (FormatException)
                {
                    // number format errors need to be passed as is
                    throw;
                }
            }
            return value;
        }


        /// <summary>
        /// Parameter class for the filter event.
        /// </summary>
        public class FilterEventArgs : EventArgs
        {
            private object _entity;

            /// <summary>
            /// Entity being filtered.
            /// </summary>
            public object Entity
            {
                get { return _entity; }
            }

            private bool _excludeEntity;

            /// <summary>
            /// Return true if the entity should not be included in the list.
            /// </summary>
            /// <returns></returns>
            public bool IsEntityExcluded()
            {
                return _excludeEntity;
            }

            /// <summary>
            /// Specify that the entity is to be excluded from the list.
            /// Call this within the event handler if needed.
            /// </summary>
            public void ExcludeEntity()
            {
                _excludeEntity = true;
            }

            /// <summary>
            /// Used to reset the class without having to create a new object.
            /// </summary>
            /// <param name="entity"></param>
            internal void Reset(object entity)
            {
                _entity = entity;
                _excludeEntity = false;
            }

            public FilterEventArgs(object entity)
            {
                Reset(entity);
            }
        }
    }
}
