using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
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


namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// The default ASP.NET dropdownlist can't be bound by SLX because it lacks a "SelectedValueChanged" event.
    /// This control adds the event, and also adds error handling for the case of the value not matching an existing
    /// list item.
    /// </summary>
    [DefaultProperty("SelectedValue")]
    [ToolboxData("<{0}:BindableDropDownList runat=server></{0}:BindableDropDownList>")]
    [ValidationProperty("Text")]
    public class BindableDropDownList : DropDownList
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
        /// If this is set to a value not available in the control, it will be cleared.
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
                var item = this.Items.FindByValue(value);
                ClearSelection();
                if (item != null)
                {
                    item.Selected = true;
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
