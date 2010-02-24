using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using System.Web.UI.WebControls;

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Adapter for displaying a picklist as a simple dropdown control.
    /// Used for picklists where the value must exist in the list.
    /// </summary>
    public class DropDownPicklistAdapter : IPicklistAdapter
    {
        public DropDownPicklistAdapter(PickListAttributes attr, List<PicklistItem> items)
        {
            _items = items;
            _attr = attr;
        }

        private List<PicklistItem> _items;
        private PickListAttributes _attr;
        private DropDownList _dropdown;

        #region IPicklistAdapter Members

        public string GetValue()
        {
            return _dropdown.SelectedValue;
        }

        public void SetValue(string value)
        {
            try
            {
                _dropdown.SelectedValue = value;
            }
            catch { }  // ignore missing values
        }

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
            };
            _dropdown.AutoPostBack = parentControl.AutoPostBack;
            parentControl.Controls.Add(_dropdown);
            // TODO: validation for required fields?
        }

        public event EventHandler TextChanged;

        #endregion
    }
}
