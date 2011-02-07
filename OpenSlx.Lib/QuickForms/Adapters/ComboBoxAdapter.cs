using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSlx.Lib.Web.Controls;
using Sage.Platform.WebPortal.Adapters;
using OpenSlx.Lib.QuickForms.Interfaces;
using Sage.Platform.Controls;

namespace OpenSlx.Lib.QuickForms.Adapters
{
    public class ComboBoxAdapter : WebControlAdapter<ComboBox>, IQFComboBox, IControl
    {
        private ComboBox _control;

        public ComboBoxAdapter(ComboBox control) : base(control)
        {
            _control = control;
        }

        public string Text
        {
            get
            {
                return _control.Text;
            }
            set
            {
                _control.Text = value;
            }
        }
    }
}
