using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Sage.Platform.WebPortal.SmartParts;
using System.Web.UI;
using OpenSlx.Lib.Utility;
using SSSWorld.WebControls;
using System.ComponentModel;

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// A custom parameter, for use as parameter to a standard ASP.NET datasource (SqlDataSource, ObjectDataSource etc).
    /// The value will be taken from the BindingSource of the current context.
    /// </summary>
    /// <seealso cref="SlxSqlDataSource"/>
    public class BindingSourceParameter : Parameter
    {
        /// <summary>
        /// The name of the property to retrieve on the current entity.
        /// EG, Address.City would be valid, if the form is bound to Contact.
        /// Defaults to the entity's id.
        /// </summary>
        [DefaultValue("Id")]
        public String PropertyPath { get; set; }

        /// <summary>
        /// Retrieve value for the parameter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="control"></param>
        /// <returns></returns>
        protected override object Evaluate(System.Web.HttpContext context, System.Web.UI.Control control)
        {
            object entity = FindParentSmartPart(control).BindingSource.Current;
            if (entity == null)
                throw new InvalidOperationException("BindingSource.Current is null");
            return ReflectionHelper.GetPropertyValue(entity, PropertyPath, new WebCacheService());
        }

        /// <summary>
        /// Attempt to find the smart part containing this control.
        /// </summary>
        /// <returns></returns>
        private EntityBoundSmartPart FindParentSmartPart(Control boundControl)
        {
            Control iteration = boundControl.Parent;
            while (iteration != null)
            {
                if (iteration is EntityBoundSmartPart)
                    return (EntityBoundSmartPart)iteration;
                iteration = iteration.Parent;
            }
            throw new InvalidOperationException("SlxEntityDataSource can only be used on an EntityBoundSmartPart");
        }

    }
}
