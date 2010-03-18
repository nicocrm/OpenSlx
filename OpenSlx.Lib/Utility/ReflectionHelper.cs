using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using log4net;
using OpenSlx.Lib.Services;

/*
    OpenSlx - Open Source SalesLogix Library and Tools
    Copyright (C) 2010 Strategic Sales Systems

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

namespace OpenSlx.Lib.Utility
{
    /// <summary>
    /// Collection of small useful utility methods for dealing with common reflection problems.
    /// </summary>
    public class ReflectionHelper
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ReflectionHelper));

        /// <summary>
        /// Retrieve a property value.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="propertyPathString">Path to property to set, separated by periods if setting a subproperty (Account.Address.City)</param>
        /// <param name="cacheProvider">Used to cache the propertyinfo information.  May be null to skip cache.</param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException">Property can't be found</exception>
        public static object GetPropertyValue(object instance, String propertyPathString, ICacheService cacheProvider)
        {
            IPropertyPath propertyPath = GetPropertyPath(instance.GetType(), propertyPathString, cacheProvider);
            PropertyInfo propInfo;
            propertyPath.FollowPropertyPath(instance, out instance, out propInfo);
            if (instance == null)
                return null;
            return propInfo.GetValue(instance, null);            
        }

        /// <summary>
        /// Convenience function to retrieve the value of a field.
        /// An exception will be thrown if it cannot be located.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldName">Name of the field on the instance (the field must be on the instance itself - not a subproperty)</param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static object GetFieldValue(object instance, String fieldName, ICacheService cache)
        {
            Type entityType = instance.GetType();
            String cacheKey = entityType.ToString() + "#" + fieldName;
            FieldInfo f = cache[cacheKey] as FieldInfo;
            if (f == null)
            {
                f = FindField(entityType, fieldName);
                if(f == null)
                    throw new KeyNotFoundException("Can't locate field " + fieldName + " on type " + entityType.FullName);
                cache[cacheKey] = f;
            }
            return f.GetValue(instance);
        }

        /// <summary>
        /// Set a value on the specified object.
        /// </summary>
        /// <param name="instance">Instance the value will be set on</param>
        /// <param name="propertyPathString">Path to property.  Subproperties may be separated by periods (Account.Address.City).</param>
        /// <param name="value">Value to be set</param>
        /// <param name="cacheProvider">Used to cache the propertyinfo information.  May be null to skip cache.</param>
        /// <exception cref="KeyNotFoundException">Property can't be found</exception>
        public static void SetPropertyValue(object instance, String propertyPathString, object value, ICacheService cacheProvider)
        {
            IPropertyPath propertyPath = GetPropertyPath(instance.GetType(), propertyPathString, cacheProvider);
            PropertyInfo propInfo;
            propertyPath.FollowPropertyPath(instance, out instance, out propInfo);
            //if (propInfo == null)
            //    throw new KeyNotFoundException("Property " + propertyPathString + " can't be located on " + instance.GetType().FullName);
            if (value != null && !propInfo.ReflectedType.IsAssignableFrom(value.GetType()))
                value = BuildObjectValue(value, propInfo.PropertyType, propertyPathString);
            propInfo.SetValue(instance, value, null);
        }

        /// <summary>
        /// Locate a property (will throw error if prop not available!)
        /// Cache the property info.
        /// Does not handle recursive path (Account.Address.City...)
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <exception cref="KeyNotFoundException">Property can't be found</exception>
        public static PropertyInfo FindPropertyOnEntity(Type entityType, String propertyName, ICacheService cacheProvider)
        {
            return FindPropertyOnEntity(entityType, propertyName, cacheProvider, false);
        }

        /// <summary>
        /// Locate a property (will throw error if prop not available!)
        /// Cache the property info.
        /// Does not handle recursive path (Account.Address.City...)
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyName"></param>
        /// <exception cref="KeyNotFoundException">Property can't be found</exception>    
        public static PropertyInfo FindPropertyOnEntity(Type entityType, String propertyName, ICacheService cacheProvider, bool ignoreCase)
        {
            PropertyInfo propInfo = null;
            String cacheKey = null;

            if (cacheProvider != null)
            {
                cacheKey = entityType.ToString() + "#" + propertyName;
                propInfo = cacheProvider[cacheKey] as PropertyInfo;
            }
            if (propInfo == null)
            {
                entityType = GetStaticType(entityType);
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                if(ignoreCase)
                    flags |= BindingFlags.IgnoreCase;
                propInfo = entityType.GetProperty(propertyName, flags);
                if (propInfo == null)
                {
                    throw new KeyNotFoundException("Can't locate property " + propertyName + " on type " + entityType.FullName);
                }
                if (cacheProvider != null)
                {
                    cacheProvider[cacheKey] = propInfo;
                }
            }

            return propInfo;
        }

        /// <summary>
        /// Attempt to locate a field on the specified type.
        /// Unlike Type.GetField, this will crawl up the hierarchy to try and
        /// locate a private field on a base class, if needed.
        /// For this reason this method can be resource intensive and the result
        /// should be cached.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private static FieldInfo FindField(Type t, String fieldName)
        {
            FieldInfo f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (f == null && t.BaseType != null)
                return FindField(t.BaseType, fieldName);
            return f;
        }


        /// <summary>
        /// Retrieve property path info.
        /// Throw error if unavailable.
        /// This only examines public instance properties.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="propertyPathString"></param>
        /// <param name="cacheProvider"></param>
        /// <returns></returns>
        private static IPropertyPath GetPropertyPath(Type t, String propertyPathString, ICacheService cacheProvider)
        {
            String cacheKey = null;
            IPropertyPath propertyPath = null;
            if (cacheProvider != null)
            {
                cacheKey = t.FullName + "#" + propertyPathString;
                propertyPath = cacheProvider[cacheKey] as IPropertyPath;
            }
            if (propertyPath == null)
            {
                if (propertyPathString.IndexOf('.') > -1)
                {
                    propertyPath = new ComplexPropertyPath(t, propertyPathString);
                }
                else
                {
                    propertyPath = new SimplePropertyPath(t, propertyPathString);
                }
                if (cacheProvider != null)
                {
                    cacheProvider[cacheKey] = propertyPath;
                }
            }
            return propertyPath;
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

        /// <summary>
        /// Convert the specified value to destinationType.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <param name="paramName">Name used for reporting</param>
        /// <returns></returns>
        public static object BuildObjectValue(object value, Type destinationType, string paramName)
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
                value = ConvertType(value, elementType);
                if (isNullableType)
                {
                    Type type = value.GetType();
                    if (elementType != type)
                    {
                        throw new InvalidOperationException(String.Format("Can't convert type from {0} to Nullable<{1}>",
                            destinationType.GetGenericArguments()[0].FullName));
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Attempt to automatically convert a string to a specified type.
        /// </summary>
        /// <param name="value">String Value.  If this is not castable to a string the function will fail and return value itself</param>
        /// <param name="type">Type to be converted to.</param>
        /// <returns></returns>
        private static object ConvertType(object value, Type type)
        {
            string text = value as string;
            if (text != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter != null)
                {
                    try
                    {
                        value = converter.ConvertFromInvariantString(text);
                    }
                    catch (NotSupportedException)
                    {
                        // this is a real type conversion error
                        throw;
                    }
                }
            }
            return value;
        }


        private interface IPropertyPath
        {
            /// <summary>
            /// Execute the property path.
            /// </summary>
            /// <param name="source">source entity</param>
            /// <param name="instance">value of the actual instance the property is defined on</param>
            /// <param name="propInfo">the final propertyinfo</param>
            void FollowPropertyPath(object source, out object instance, out PropertyInfo propInfo);
        }

        /// <summary>
        /// A non-recursive property path.
        /// </summary>
        [Serializable]
        private class SimplePropertyPath : IPropertyPath
        {
            private PropertyInfo _prop;

            public SimplePropertyPath(Type type, String propName)
            {
                _prop = type.GetProperty(propName);
                if (_prop == null)
                    throw new KeyNotFoundException("Can't retrieve property " + propName + " on " + type.FullName);
            }

            #region IPropertyPath Members

            public void FollowPropertyPath(object source, out object instance, out PropertyInfo propInfo)
            {
                instance = source;
                propInfo = _prop;
            }

            #endregion
        }

        /// <summary>
        /// A recursive property path.
        /// </summary>
        [Serializable]
        private class ComplexPropertyPath : IPropertyPath
        {
            private LinkedList<PropertyInfo> _propertyPath;

            public ComplexPropertyPath(Type t, String propertyPathString)
            {
                _propertyPath = FindProperty(t, propertyPathString.Split('.'));
                if (_propertyPath == null)
                    throw new KeyNotFoundException("Can't find property " + propertyPathString + " on " + t.FullName);
            }

            #region IPropertyPath Members

            public void FollowPropertyPath(object source, out object instance, out PropertyInfo propInfo)
            {
                LinkedListNode<PropertyInfo> node = _propertyPath.First;
                instance = source;
                while (node != _propertyPath.Last && instance != null)
                {
                    instance = node.Value.GetValue(instance, null);
                    node = node.Next;
                }
                propInfo = node.Value;
            }

            #endregion

            /// <summary>
            /// Recursively search for a property and return its path.
            /// </summary>
            /// <param name="t"></param>
            /// <param name="propertyPath"></param>
            /// <returns></returns>
            private static LinkedList<PropertyInfo> FindProperty(Type t, String[] propertyPath)
            {
                LinkedList<PropertyInfo> pathAccu = new LinkedList<PropertyInfo>();
                return FindProperty_wkh(t, propertyPath, 0, pathAccu);
            }

            /// <summary>
            /// Workhorse for the function above - browse object properties recursively, return path.
            /// If no match return null.
            /// </summary>
            /// <param name="t"></param>
            /// <param name="propertyPath"></param>
            /// <param name="pathIndex">Indicates the current fragment of the path being processed</param>
            private static LinkedList<PropertyInfo> FindProperty_wkh(Type t, String[] propertyPath, int pathIndex, LinkedList<PropertyInfo> pathAccu)
            {
                t = GetStaticType(t);
                PropertyInfo p = t.GetProperty(propertyPath[pathIndex]);
                if (p != null)
                {
                    pathAccu.AddLast(p);
                    if (pathIndex == propertyPath.Length - 1)
                        return pathAccu;
                    else
                        return FindProperty_wkh(p.PropertyType, propertyPath, pathIndex + 1, pathAccu);
                }
                return null;
            }
        }
    }
}
