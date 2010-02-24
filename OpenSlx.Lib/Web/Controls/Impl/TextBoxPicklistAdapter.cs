using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Renders the picklist as a textbox (used for the read-only display)
    /// </summary>
    public class TextBoxPicklistAdapter : IPicklistAdapter
    {
        public TextBoxPicklistAdapter()
        {
        }

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

        public event EventHandler TextChanged;

        #endregion
    }
}
