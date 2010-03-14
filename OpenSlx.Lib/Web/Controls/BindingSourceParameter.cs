using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Sage.Platform.WebPortal.SmartParts;
using System.Web.UI;
using OpenSlx.Lib.Utility;
using System.ComponentModel;
using OpenSlx.Lib.Web.Utility;

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
