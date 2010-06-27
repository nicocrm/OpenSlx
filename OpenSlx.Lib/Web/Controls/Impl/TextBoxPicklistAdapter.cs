using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
