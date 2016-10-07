using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sage.Platform;
using Sage.Platform.Orm.Attributes;
using Sage.Platform.Orm.Interfaces;
using Sage.Platform.Orm.Services;

namespace OpenSlx.Lib.Utility
{
    /// <summary>
    /// Miscellaneous utilities dealing with SLX entities.
    /// </summary>
    public static class SlxEntityUtility
    {
        /// <summary>
        /// Create a copy of an existing entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CloneEntity<T>(T source)
            where T : IDynamicEntity
        {
            T target = EntityFactory.Create<T>();
            CopyEntityProperties(target, source);
            return target;
        }

        public static void CopyEntityProperties<T>(T target, T source, ISet<string> propsToExclude = null)
            where T : IDynamicEntity
        {
            var iface = GetInterface(source.GetType());
            foreach (PropertyInfo prop in iface.GetProperties())
            {
                if (propsToExclude != null && propsToExclude.Contains(prop.Name))
                    continue;

                // only copy the ones associated with DB fields
                // (note that this includes M-1 relationships)
                if (Attribute.GetCustomAttribute(prop, typeof(FieldAttribute)) != null)
                {
                    var extendedType =
                        (ExtendedTypeAttribute)
                            prop.GetCustomAttributes(
                                typeof(ExtendedTypeAttribute),
                                false).FirstOrDefault();
                    // don't copy ID fields - we'll pick up the reference properties instead 
                    if (extendedType != null && extendedType.ExtendedTypeName ==
                        "Sage.Platform.Orm.DataTypes.StandardIdDataType,  Sage.Platform")
                    {
                        continue;
                    }
                    target[prop.Name] = source[prop.Name];
                }
            }
        }

        public static Type GetInterface(Type entityType)
        {
            if (entityType.IsInterface)
                return entityType;
            var ifaceType = entityType.GetInterface("I" + entityType.Name);
            if (ifaceType == null)
                throw new ApplicationException("Unable to locate interface for " + entityType.Name);
            return ifaceType;
        }
    }
}
