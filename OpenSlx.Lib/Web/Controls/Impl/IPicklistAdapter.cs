using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

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

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Utility interfaces used to access the picklist's value.
    /// </summary>
    public interface IPicklistAdapter
    {
        /// <summary>
        /// Returns the underlying value (or values, for a multi-select list)
        /// </summary>
        /// <returns></returns>
        String GetValue();

        /// <summary>
        /// Sets the underlying value on the control (or values, for a multi-select list)
        /// </summary>
        /// <param name="value"></param>
        void SetValue(String value);

        /// <summary>
        /// Creates controls and attaches them to the designated parent.
        /// </summary>
        /// <param name="parentControl"></param>
        void CreateChildControls(SimplePicklist parentControl);

        /// <summary>
        /// Set readonly flag 
        /// (normally this is not hit, because we'll render the picklist as a read-only textbox.
        /// But if the read-only flag is set late in the page lifecycle it will happen)
        /// </summary>
        bool ReadOnly { set; }

        /// <summary>
        /// Events should get triggered when the value may have changed.
        /// </summary>
        event EventHandler TextChanged;
    }
}
