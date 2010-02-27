using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Web.UI;

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// The default ASP.NET dropdownlist can't be bound by SLX because it lacks a "SelectedValueChanged" event.
    /// This control adds the event, and also adds error handling for the case of the value not matching an existing
    /// list item.
    /// </summary>
    [DefaultProperty("SelectedValue")]
    [ToolboxData("<{0}:BindableRadioButtonList runat=server></{0}:BindableRadioButtonList>")]
    [ValidationProperty("Text")]
    public class BindableRadioButtonList : RadioButtonList
    {
        /// <summary>
        /// Raised when the value of the control may have changed.
        /// Used by Saleslogix databinding.
        /// </summary>
        public event EventHandler SelectedValueChanged;

        // add our event hook
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.SelectedIndexChanged += delegate
            {
                if (SelectedValueChanged != null)
                    SelectedValueChanged(this, EventArgs.Empty);
            };
        }


        /// <summary>
        /// Currently selected value.
        /// </summary>
        [Bindable(true)]
        [Description("Currently selected value")]
        [Category("Data")]
        public override string SelectedValue
        {
            get
            {
                return base.SelectedValue == "" ? null : base.SelectedValue;
            }
            set
            {
                try
                {
                    base.SelectedValue = (value == null ? null : value.Trim());
                }
                catch (ArgumentOutOfRangeException)
                {
                    base.SelectedValue = null;
                }
            }
        }

        protected override void PerformDataBinding(System.Collections.IEnumerable dataSource)
        {
            //log4net.LogManager.GetLogger(ClientID).Debug("Before Databinding, selected index = " + SelectedIndex);
            //log4net.LogManager.GetLogger(ClientID).Debug("PerformDataBinding");
            try
            {
                base.PerformDataBinding(dataSource);
            }
            catch (ArgumentOutOfRangeException)
            {
                base.SelectedValue = null;
            }
            //log4net.LogManager.GetLogger(ClientID).Debug("After Databinding, selected index = " + SelectedIndex);
        }
    }
}
