using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.WebPortal.Binding;
using System.Reflection;
using System.Linq.Expressions;

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

namespace OpenSlx.Lib.Web.Utility
{
    /// <summary>
    /// Helper for creating data binding on a form.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class TypedWebEntityBindingGenerator<TEntity>
    {
        /// <summary>
        /// Create a data binding.
        /// The resulting binding needs to be added to BindingSource.Bindings.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TProperty2"></typeparam>
        /// <param name="entityProperty"></param>
        /// <param name="component"></param>
        /// <param name="componentProperty"></param>
        /// <returns></returns>
        public WebEntityBinding CreateBinding<TComponent, TProperty, TProperty2>(Expression<Func<TEntity, TProperty>> entityProperty, 
            TComponent component,
            Expression<Func<TComponent, TProperty2>> componentProperty)
        {
            var propFrom = BuildPropertyAccessString((MemberExpression)entityProperty.Body);
            var propTo = (PropertyInfo)((MemberExpression)componentProperty.Body).Member;
            return new WebEntityBinding(propFrom, component, propTo.Name);
        }


        /// <summary>
        /// Create a data binding.
        /// The resulting binding needs to be added to BindingSource.Bindings.
        /// </summary>
        /// <typeparam name="TComponent"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TProperty2"></typeparam>
        /// <param name="entityProperty"></param>
        /// <param name="component"></param>
        /// <param name="componentProperty"></param>
        /// <param name="formatString">Format string (e.g., {0:c} for a currency format)</param>
        /// <returns></returns>
        public WebEntityBinding CreateBinding<TComponent, TProperty, TProperty2>(Expression<Func<TEntity, TProperty>> entityProperty,
            TComponent component,
            Expression<Func<TComponent, TProperty2>> componentProperty,
            String formatString)
        {
            var bdg = this.CreateBinding(entityProperty, component, componentProperty);
            bdg.FormatString = formatString;
            return bdg;
        }


        private String BuildPropertyAccessString(MemberExpression memberExpression)
        {
            String b = "";
            
            if (memberExpression.Expression is MemberExpression)
                b = BuildPropertyAccessString((MemberExpression)memberExpression.Expression) + ".";
            return b + ((PropertyInfo)memberExpression.Member).Name;
        }
    }
}
