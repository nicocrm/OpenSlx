using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

/*
    OpenSlx - Open Source SalesLogix Library and Tools
    Copyright (C) 2010 Nicolas Galler

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
    /// Formatted textbox (the formatting is done with JavaScript)
    /// </summary>
    [ToolboxData("<{0}:FormattedTextBox runat=server></{0}:FormattedTextBox>")]
    public class FormattedTextBox : TextBox
    {
        /// <summary>
        /// Format type - defaults to currency format
        /// </summary>
        public FormatType Format { get; set; }

        /// <summary>
        /// Format argument (the meaning depends on the format type - e.g. for decimal format it 
        /// indicates the number of decimals
        /// </summary>
        public String FormatArgument {get; set; }

        /// <summary>
        /// Register javascript include.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScriptManager.RegisterClientScriptResource(this, GetType(), "OpenSlx.Lib.Web.Controls.JS.formatted.js");
        }

        /// <summary>
        /// Ensure the required javascript is output.
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            String className = "OpenSlx.FormattedField";
            if (Format > 0)
                className += Format.ToString();
            
            String script = "new " + className + "('" + this.ClientID + "'";
            if(!String.IsNullOrEmpty(FormatArgument))
                script += "," + FormatArgument;
            script += ");";
                
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(), script, true);
        }

        /// <summary>
        /// Type of format to apply (see formatted.js for details)
        /// </summary>
        public enum FormatType
        {
            /// <summary>
            /// Currency format (hard-coded to US currency)
            /// </summary>
            Currency = 0,
            /// <summary>
            /// Phone format (hard-coded to US phone format)
            /// </summary>
            Phone,
            /// <summary>
            /// A number format (use FormatArgument to specify number of decimals)
            /// </summary>
            Decimal,
            /// <summary>
            /// Decimal with percent sign and converts 0.1 to 10% (and vice versa)
            /// </summary>
            Percent
        }
    }
}
