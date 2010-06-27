using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using System.Web.UI.WebControls;

/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/nicocrm/OpenSlx)

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
    /// Adapter for displaying a picklist as a simple dropdown control.
    /// Used for picklists where the value must exist in the list.
    /// </summary>
    public class DropDownPicklistAdapter : IPicklistAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="items"></param>
        public DropDownPicklistAdapter(PickListAttributes attr, List<PicklistItemDisplay> items)
        {
            _items = items;
            _attr = attr;
        }

        private List<PicklistItemDisplay> _items;
        private PickListAttributes _attr;
        private DropDownList _dropdown;

        #region IPicklistAdapter Members

        /// <summary>
        /// GetValue
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            return _dropdown.SelectedValue;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            _dropdown.SelectedValue = value;
        }

        /// <summary>
        /// CreateChildControls
        /// </summary>
        /// <param name="parentControl"></param>
        public void CreateChildControls(SimplePicklist parentControl)
        {
            _dropdown = new BindableDropDownList();
            _dropdown.AppendDataBoundItems = true;
            _dropdown.Items.Add(new ListItem { Text = "", Value = "" });
            foreach (var i in _items)
            {
                _dropdown.Items.Add(new ListItem(i.Text, i.Value));
            }
            _dropdown.EnableViewState = false;

            _dropdown.SelectedIndexChanged += delegate
            {
                if (TextChanged != null)
                    TextChanged(this, EventArgs.Empty);
                // we have to reset the value here, otherwise the dropdown will keep using the 
                // cached value from its viewstate 
                // (this comes from the fact that we are maintaining the state for the adapter itself 
                // but the dropdown's viewstate is disabled)
                _dropdown.SelectedValue = _dropdown.SelectedValue;
            };
            _dropdown.AutoPostBack = parentControl.AutoPostBack;
            parentControl.Controls.Add(_dropdown);
        }

        /// <summary>
        /// If true dropdown will be disabled
        /// </summary>
        public bool ReadOnly
        {
            set
            {
                if (_dropdown != null)
                    _dropdown.Enabled = !value;
            }
        }

        /// <summary>
        /// Raised when text may have changed.
        /// </summary>
        public event EventHandler TextChanged;

        #endregion
    }
}
