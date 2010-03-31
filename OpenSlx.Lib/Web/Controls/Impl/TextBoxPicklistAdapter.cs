using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Renders the picklist as a textbox (used for the read-only display)
    /// </summary>
    public class TextBoxPicklistAdapter : IPicklistAdapter
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TextBoxPicklistAdapter()
        {
        }

        private TextBox _textbox;

        #region IPicklistAdapter Members

        /// <summary>
        /// GetValue
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            return _textbox.Text;
        }

        /// <summary>
        /// SetValue
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            _textbox.Text = value;
        }

        /// <summary>
        /// CreateChildControls
        /// </summary>
        /// <param name="parentControl"></param>
        public void CreateChildControls(SimplePicklist parentControl)
        {
            _textbox = new TextBox();
            _textbox.ID = "txt";
            _textbox.ReadOnly = parentControl.ReadOnly;
            _textbox.AutoPostBack = parentControl.AutoPostBack;
            _textbox.TextChanged += delegate
            {
                if (TextChanged != null)
                    TextChanged(this, EventArgs.Empty);
            };
            parentControl.Controls.Add(_textbox);
        }

        /// <summary>
        /// Mark textbox readonly
        /// </summary>
        public bool ReadOnly
        {
            set
            {
                if (_textbox != null)
                    _textbox.Enabled = !value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler TextChanged;

        #endregion
    }
}
