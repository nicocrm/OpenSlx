using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OpenSlx.Lib.Web.Controls.Impl;
using Sage.Platform;
using Sage.SalesLogix.PickLists;
using log4net;
using Sage.Platform.WebPortal;
using Sage.Platform.WebPortal.Services;

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
    /// Simple picklist control emulates a SalesLogix picklist, attempting to use the settings 
    /// established in the (legacy) picklist manager.
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:SimplePicklist runat=server></{0}:SimplePicklist>")]
    [ValidationProperty("Text")]
    public class SimplePicklist : CompositeControl, ITextControl
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SimplePicklist));

        /// <summary>
        /// Represents the selected value.
        /// The picklist's StorageMode will dictate how exactly this is rendered.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                EnsureChildControls();
                return _adapter.GetValue();
            }

            set
            {
                EnsureChildControls();
                // remember selected value so that we can properly detect change.
                ViewState["Text"] = value;
                _adapter.SetValue(value);
            }
        }

        /// <summary>
        /// For compatibility with the SLX controls.
        /// </summary>
        public String PickListValue
        {
            get { return Text; }
            set { Text = value; }
        }

        /// <summary>
        /// For compatibility with the SLX controls.
        /// </summary>
        public event EventHandler PickListValueChanged
        {
            add { TextChanged += value; }
            remove { TextChanged -= value; }
        }

        /// <summary>
        /// Name of the picklist in Saleslogix.
        /// If this is not set the picklist will be displayed as a readonly textbox.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public string PickListName
        {
            get { return (String)ViewState["PickListName"]; }
            set { ViewState["PickListName"] = value; }
        }

        /// <summary>
        /// For compatibility with Saleslogix.
        /// This is not actually used.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public String PickListId { get; set; }

        private bool _readonly = false;
        /// <summary>
        /// When set to true the underlying adapter will be made read-only.
        /// If this is set before the child controls have been created (e.g. in the aspx source markup), 
        /// then the control will be rendered as a read-only textbox.  Because of this, the value is not persisted
        /// to the ViewState (as it would make it hard to switch back and forth between read-only and read-write).
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public bool ReadOnly
        {
            get { return _readonly; }
            set
            {
                _readonly = value;
                if (_adapter != null)
                    _adapter.ReadOnly = value;
            }
        }

        /// <summary>
        /// When set the underlying control will be set to auto postback (if supported).
        /// This must be set before the child controls have been created.
        /// </summary>
        [Bindable(true)]
        public bool AutoPostBack { get; set; }

        /// <summary>
        /// How should the value be displayed in the client.
        /// This must be set before the child controls have been created.
        /// </summary>
        [Bindable(true)]
        public PicklistStorageMode DisplayMode { get; set; }

        /// <summary>
        /// How the value should be reflected in the Text property.
        /// This must be set before the child controls have been created.
        /// </summary>
        [Bindable(true)]
        public PicklistStorageMode StorageMode { get; set; }

        /// <summary>
        /// Attempt to be as compatible as possible with the Saleslogix picklist
        /// (prevents the use of custom adapters)
        /// This must be set before the child controls have been created.
        /// </summary>
        [Bindable(true)]
        public bool Compatible { get; set; }

        /// <summary>
        /// Triggered when the picklist's text may have changed.
        /// </summary>
        public event EventHandler TextChanged;

        private IPicklistAdapter _adapter = null;

        /// <summary>
        /// Constructor with default values.
        /// </summary>
        public SimplePicklist()
        {
            StorageMode = PicklistStorageMode.Text;
            DisplayMode = PicklistStorageMode.Text;
        }

        /// <summary>
        /// Picks adapter and calls its createchildcontrols method.
        /// </summary>
        protected override void CreateChildControls()
        {
            _adapter = SelectPicklistAdapter();
            _adapter.CreateChildControls(this);
            if (this.Page is EntityPage && ((EntityPage)this.Page).IsNewEntity)
            {
                ViewState.Remove("Text");
            }
            else
            {
                _adapter.SetValue((String)ViewState["Text"]);
            }

            _adapter.TextChanged += delegate
            {
                if (TextChanged != null && (String)ViewState["Text"] != _adapter.GetValue())
                {
                    ViewState["Text"] = _adapter.GetValue();
                    TextChanged(this, EventArgs.Empty);
                }
            };
        }

        /// <summary>
        /// Register scripts.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RegisterClientScript();
        }


        /// <summary>
        /// Load the picklist data from the database and constructs corresponding adapter.
        /// </summary>
        /// <returns></returns>
        private IPicklistAdapter SelectPicklistAdapter()
        {
            PickListAttributes attr;
            List<PicklistItemDisplay> items;

            if (ReadOnly || String.IsNullOrEmpty(PickListName))
                return new TextBoxPicklistAdapter();

            if (Compatible)
            {
                attr = GetPicklistAttributes();
                if (attr == null)
                {
                    ReadOnly = true;
                    return new TextBoxPicklistAdapter();
                }
                return new PicklistPicklistAdapter(PickListName, attr, StorageMode);
            }
            else
            {
                GetPicklistItems(out attr, out items);
                if (attr == null || items == null)
                {
                    LOG.Debug("Could not retrieve picklist attributes for '" + PickListName + "'");
                    ReadOnly = true;
                    return new TextBoxPicklistAdapter();
                }
                if (attr.AllowMultiples)
                    // custom multi-select picklist
                    return new MultiSelectPicklistAdapter(attr, items);
                if (!attr.ValueMustExist)
                    // standard Saleslogix picklist
                    return new PicklistPicklistAdapter(PickListName, attr, StorageMode);
                return new DropDownPicklistAdapter(attr, items);
            }
        }

        /// <summary>
        /// Retrieve the picklist items.
        /// This will be cached the first time.
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="items"></param>
        private void GetPicklistItems(out PickListAttributes attr, out List<PicklistItemDisplay> items)
        {
            attr = (PickListAttributes)Page.Cache["pkl" + (PickListName) + "$Attr"];
            items = (List<PicklistItemDisplay>)Page.Cache["pkl" + (PickListName) + "$Items"];
            if (attr != null && items != null)
                return;
            String pklId = null;
            if (pklId == null)
                pklId = PickList.PickListIdFromName(PickListName);
            if (String.IsNullOrEmpty(pklId))
            {
                //throw new InvalidOperationException("Picklist name " + ListName + " is not valid.");
                ReadOnly = true;
                return;
            }
            attr = PickList.GetPickListAttributes(pklId);
            IEnumerable<PickList> pklItems = PickList.GetPickListItems(pklId);            
            items = (from pkl in pklItems
                     where pkl.UserId.Trim() == "ADMIN"
                     select new PicklistItemDisplay(pkl, DisplayMode, StorageMode))
                     .ToList();
            if (attr.AlphaSorted)
                items.Sort(ComparePicklistItems);
                //items = items.OrderBy(v => v.Text).ToList();
        }

        /// <summary>
        /// Helper method for the alpha sort
        /// </summary>
        /// <returns></returns>
        private static int ComparePicklistItems(PicklistItemDisplay x, PicklistItemDisplay y)
        {
            return x.Text.CompareTo(y.Text);
        }

        /// <summary>
        /// Retrieve the picklist attributes only (used for the PicklistPicklistAdapter)
        /// </summary>
        /// <returns></returns>
        private PickListAttributes GetPicklistAttributes()
        {
            PickListAttributes attr = (PickListAttributes)Page.Cache["pkl" + (PickListName) + "$Attr"];
            if (attr != null)
                return attr;
            String pklId = null;
            if (pklId == null)
                pklId = PickList.PickListIdFromName(PickListName);
            if (String.IsNullOrEmpty(pklId))
            {
                return null;
            }
            attr = PickList.GetPickListAttributes(pklId);
            return attr;
        }

        private void RegisterClientScript()
        {
            ScriptManager.RegisterClientScriptResource(this, GetType(), "OpenSlx.Lib.Web.Controls.Impl.SimplePicklist.js");
        }
        
    }
}
