using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Design;
using Sage.Platform.QuickForms.Controls;
using Sage.Platform.QuickForms;
using System.Reflection;
using System.Collections;


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

namespace OpenSlx.Lib.QuickForms.Editors
{
    /// <summary>
    /// A simplified version of the Sage EntityPropertyNameEditor, it only lists properties for the 
    /// entity bound to the current form
    /// (Sage's version only works with the QFDataSource control)
    /// </summary>
    public class BoundEntityPropertyNameEditor : ListBoxTypeEditor
    {
        protected override IEnumerable<string> GetValues(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            QuickFormsControlBase control = context.Instance as QuickFormsControlBase;
            if (control == null)
            {
                throw new InvalidOperationException("Invalid context - null instance (or not a QuickFormsControl)");
            }
            if (!(control.QuickFormDefinition is IEntityQuickFormDefinition))
            {
                throw new InvalidOperationException("Can only be used on a bound form");
            }
            Type entityType = ((IEntityQuickFormDefinition)control.QuickFormDefinition).EntityType;
            List<String> list = new List<String>();
            //List<Type> interfaces = new List<Type>(entityType.GetInterfaces());
            //interfaces.Add(entityType);
            //foreach (Type type2 in list2)
            //{

            foreach (PropertyInfo info in entityType.GetProperties())
            {
                if (((info.MemberType == MemberTypes.Property) && !info.IsSpecialName) && 
                    (!info.PropertyType.IsValueType && (info.PropertyType != typeof(string))) &&
                    typeof(IEnumerable).IsAssignableFrom(info.PropertyType))
                {
                    list.Add(info.Name);
                }
            }
            //}
            list.Sort();
            return list;
        }
    }
}
