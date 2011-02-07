using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using OpenSlx.Lib.QuickForms.Interfaces;
using Sage.Platform.WebPortal.Adapters;
using OpenSlx.Lib.QuickForms.Adapters;


/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

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
    /// Wrapper for Ext JS combobox
    /// 
    /// TODO: support for AutoPostBack, DataTextField / DataValueField
    /// </summary>
    [Adapter(typeof(ComboBoxAdapter))]
    public class ComboBox : ListControl, IPostBackDataHandler
    {
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context != null && Context.Items["OpenSlxComboBoxStyle"] == null)
            {
                Context.Items["OpenSlxComboBoxStyle"] = "";
                writer.Write("<style> .openslx-combobox .x-form-trigger { padding: 0; height: auto }\n" +
                    ".openslx-combobox .x-form-text { height: auto; background-image: none }</style>");
            }

            writer.Write("<div class='openslx-combobox'>");
            base.Render(writer);
            writer.Write("</div>");
            ScriptManager.RegisterStartupScript(this, GetType(), Guid.NewGuid().ToString(),
                "Ext.onReady(function() { " +
                String.Format("new Ext.form.ComboBox({{ transform: '{0}', triggerAction: 'all', hiddenName: '{1}'}})", ClientID, UniqueID) +
                // following will ensure the hidden field value is set to the text value.
                // it will not work right if the user is expecting to be able to use different value than text (DataValueField / DataTextField)
                ".on('blur', function() { this.hiddenField.value = this.el.dom.value }) " +
                "});", true);

        }


        public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
            String value = postCollection[postDataKey];
            EnsureDataBound();
            if (!String.IsNullOrEmpty(value))
            {
                int newIndex = -1;
                for (int i = 0; i < Items.Count && newIndex == -1; i++)
                {
                    if (Items[i].Value == value)
                        newIndex = i;
                }
                if (newIndex == -1)
                {
                    Items.Add(value);
                    newIndex = Items.Count - 1;
                }
                if (newIndex >= 0 && newIndex != this.SelectedIndex)
                {
                    base.SetPostDataSelection(newIndex);
                    return true;
                }
            }
            return false;
        }

        public void RaisePostDataChangedEvent()
        {
            this.OnSelectedIndexChanged(EventArgs.Empty);
        }

        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);
            if (Items.Count > 0 && Items[0].Value != "")
                Items.Insert(0, new ListItem(" ", ""));
            if (Text != "" && Items.FindByValue(Text) == null)
            {
                Items.Add(Text);
            }
        }
    }
}
