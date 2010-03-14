using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using System.Web.UI;
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
    /// Adapter for a combination of textbox/listbox.  Used for a multi-select picklist, or a picklist where
    /// the selected items don't need to match the picklist's values.
    /// </summary>
    public class MultiSelectPicklistAdapter : IPicklistAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="attr">Picklist attributes from SLX</param>
        /// <param name="items">Picklist items from SLX</param>
        public MultiSelectPicklistAdapter(PickListAttributes attr, List<PicklistItem> items)
        {
            _items = items;
            _attr = attr;
        }

        private List<PicklistItem> _items;
        private PickListAttributes _attr;
        private TextBox _textbox;


        #region IPicklistAdapter Members

        public string GetValue()
        {
            return _textbox.Text;
        }

        public void SetValue(string value)
        {
            _textbox.Text = value;
        }

        /// <summary>
        /// Create the textbox, associated select control, and send the javascript needed to control them.
        /// </summary>
        /// <param name="parentControl"></param>
        public void CreateChildControls(SimplePicklist parentControl)
        {
            parentControl.Style["position"] = "relative";
            parentControl.Style["top"] = "0px";
            parentControl.Style["left"] = "0px";

            _textbox = new TextBox();
            _textbox.ID = "txt";
            _textbox.TextChanged += delegate
            {
                if (TextChanged != null)
                    TextChanged(this, EventArgs.Empty);
            };
            _textbox.AutoPostBack = parentControl.AutoPostBack;
            // CANNOT set this here, because it will prevent the TextChanged event from firing
            //_textbox.ReadOnly = _attr.ValueMustExist;
            parentControl.Controls.Add(_textbox);
            CreateChildListBox(parentControl);            
        }

        /// <summary>
        /// Create the associated listbox and send the required javascript.
        /// </summary>
        /// <param name="parentControl"></param>
        private void CreateChildListBox(SimplePicklist parentControl){
            Panel lstContainer = new Panel();
            ListBox lst = new ListBox();
            lst.ID = "lst";            
            if (!_attr.Required)
                lst.Items.Add(new ListItem("", ""));
            foreach (var i in _items)
            {
                lst.Items.Add(new ListItem(i.Text, i.Value));
            }
            if (_attr.AllowMultiples)
                lst.SelectionMode = ListSelectionMode.Multiple;            
            lst.Style["width"] = "100%";
            lst.Rows = 8;
            lst.EnableViewState = false;
            if (_attr.ValueMustExist)
                lstContainer.Style.Add(HtmlTextWriterStyle.Top, "0px");
            else
                lstContainer.Style.Add(HtmlTextWriterStyle.Top, "24px");            
            lstContainer.Style["width"] = "100%";
            lstContainer.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            lstContainer.Style.Add(HtmlTextWriterStyle.Left, "0px");
            lstContainer.Style.Add(HtmlTextWriterStyle.Display, "none");
            lstContainer.Style.Add(HtmlTextWriterStyle.ZIndex, "999");
            lstContainer.Controls.Add(lst);
            parentControl.Controls.Add(lstContainer);
            if (_attr.Required)
            {
                RequiredFieldValidator vld = new RequiredFieldValidator();
                vld.ControlToValidate = this._textbox.ID;
                vld.Text = "*";
                // todo - add error message
                parentControl.Controls.Add(vld);
            }

            ScriptManager.RegisterClientScriptBlock(parentControl, parentControl.GetType(), "Pkl$" + parentControl.ClientID,
                "OpenSlx_MultiSelectPicklist('" + _textbox.ClientID + "','" + 
                lstContainer.ClientID + "','" + lst.ClientID + "');", true);
        }

        public event EventHandler TextChanged;

        #endregion
    }
}
