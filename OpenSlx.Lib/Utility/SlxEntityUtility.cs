using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sage.Platform;
using Sage.Platform.Orm.Attributes;
using Sage.Platform.Orm.Interfaces;

namespace OpenSlx.Lib.Utility
{
    /// <summary>
    /// Miscellaneous utilities dealing with SLX entities.
    /// </summary>
    public static class SlxEntityUtility
    {
        public static T CloneEntity<T>(T source)
            where T : IDynamicEntity
        {
            T target = EntityFactory.Create<T>();
            foreach (PropertyInfo prop in source.GetType().GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(FieldAttribute)) != null)
                {
                    target[prop.Name] = source[prop.Name];
                }
            }
            return target;
        }
    }
}
