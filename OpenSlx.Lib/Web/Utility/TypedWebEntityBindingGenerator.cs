using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.WebPortal.Binding;
using System.Reflection;
using System.Linq.Expressions;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

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
