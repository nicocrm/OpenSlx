using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Sage.Platform.WebPortal;
using Sage.Common.Syndication.Json;
using OpenSlx.Lib.Properties;

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
        public String FormatArgument { get; set; }

        /// <summary>
        /// Register javascript include.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScriptManager.RegisterClientScriptResource(this, GetType(), "OpenSlx.Lib.Web.Controls.JS.formatted.js");
            String script = JavaScriptConvert.SerializeObject(new
            {
                EnterOnlyNumbers = Resources.EnterOnlyNumbers,
                EnterOnlyWholeNumbers = Resources.EnterOnlyWholeNumbers
            });
            ScriptManager.RegisterClientScriptBlock(this, GetType(), "OpenSlx.FormattedField.Strings",
                "OpenSlx.FormattedField.Strings = " + script + ";", true);
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
            if (!String.IsNullOrEmpty(FormatArgument))
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
