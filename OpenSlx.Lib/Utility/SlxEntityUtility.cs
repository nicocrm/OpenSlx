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

        public static void CopyEntityProperties<T>(T target, T source)
            where T : IDynamicEntity
        {
            foreach (PropertyInfo prop in source.GetType().GetProperties())
            {
                // only copy the ones associated with DB fields
                // (note that this includes M-1 relationships)
                if (Attribute.GetCustomAttribute(prop, typeof(FieldAttribute)) != null)
                {
                    var extendedType =
                        (DynamicEntityDescriptorConfigurationService.ExtendedTypeInformationAttribute)
                            prop.GetCustomAttributes(
                                typeof(DynamicEntityDescriptorConfigurationService.ExtendedTypeInformationAttribute),
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
    }
}
