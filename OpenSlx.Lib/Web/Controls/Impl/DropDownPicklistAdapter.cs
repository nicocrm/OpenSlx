using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using System.Web.UI.WebControls;

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
        public DropDownPicklistAdapter(PickListAttributes attr, List<PicklistItem> items)
        {
            _items = items;
            _attr = attr;
        }

        private List<PicklistItem> _items;
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
