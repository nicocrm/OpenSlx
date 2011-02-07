using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenSlx.Lib.QuickForms.Interfaces;
using Sage.Platform.QuickForms.QFControls;
using System.Drawing.Design;
using Sage.Platform.QuickForms.Controls;
using System.Xml.Serialization;
using Sage.Platform.QuickForms;
using OpenSlx.Lib.QuickForms.Editors;


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


namespace OpenSlx.Lib.QuickForms
{
    [DisplayName("OpenSlx Entity Data Source")]
    [Description("Binds to a property of the form's entity.  Used as a standard ASP.NET datasource for standard and 3rd party controls - not compatible with the Sage data binding")]
    [VisibleControl(false)]
    public class QFEntityDataSource : QuickFormsControlBase, IQFEntityDataSource
    {
        [Editor(typeof(BoundEntityPropertyNameEditor), typeof(UITypeEditor))]
        [DisplayName("Get By Property")]
        [Description("Property to use to retrieve the collection data.")]
        public String GetByProperty { get; set; }

        #region Overrides to remove some properties from the editor

        [XmlIgnore, Browsable(false)]
        public override short TabIndex
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        [XmlIgnore, Browsable(false)]
        public override string Caption
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        [Browsable(false), XmlIgnore]
        public override HAlign CaptionAlignment
        {
            get
            {
                return base.CaptionAlignment;
            }
            set
            {
                base.CaptionAlignment = value;
            }
        }

        [Browsable(false), XmlIgnore, Bindable(false)]
        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                base.Enabled = value;
            }
        }
 
        [XmlIgnore, Browsable(false), Bindable(false)]
        public override string StyleScheme
        {
            get
            {
                return base.StyleScheme;
            }
            set
            {
                base.StyleScheme = value;
            }
        }

        [Browsable(false), XmlIgnore, Bindable(false)]
        public override LabelPlacement ControlLabelPlacement
        {
            get
            {
                return LabelPlacement.none;
            }
        }

        [Browsable(false)]
        public override char HotKey
        {
            get
            {
                return base.HotKey;
            }
        }

        [Bindable(false), Browsable(false)]
        public override bool IsReadOnly
        {
            get
            {
                return base.IsReadOnly;
            }
        }

        [XmlIgnore, Browsable(false)]
        public override string ToolTip
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        [Browsable(false), XmlIgnore, Bindable(false)]
        public override bool Visible
        {
            get
            {
                return false;
            }
        }

        #endregion

    }
}
