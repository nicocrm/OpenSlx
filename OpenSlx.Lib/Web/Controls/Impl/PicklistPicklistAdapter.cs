using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using Sage.SalesLogix.Web.Controls.PickList;

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

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Renders the picklist as a regular Saleslogix picklist control
    /// </summary>
    public class PicklistPicklistAdapter : IPicklistAdapter
    {
        private PickListControl _picklist;
        private String _picklistName;
        private PickListAttributes _attr;
        private PicklistStorageMode _storageMode;

        /// <summary>
        /// Create adapter
        /// </summary>
        /// <param name="picklistName"></param>
        /// <param name="attr"></param>
        /// <param name="storageMode"></param>
        public PicklistPicklistAdapter(String picklistName, PickListAttributes attr, PicklistStorageMode storageMode)
        {
            _picklistName = picklistName;
            _attr = attr;
            _storageMode = storageMode;
        }

        #region IPicklistAdapter Members

        /// <summary>
        /// Get underlying value.
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            return _picklist.PickListValue;
        }

        /// <summary>
        /// Set value on the underlying control.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            _picklist.PickListValue = value;
        }

        /// <summary>
        /// Create the control representing the picklist.
        /// </summary>
        /// <param name="parentControl"></param>
        public void CreateChildControls(SimplePicklist parentControl)
        {
            _picklist = new PickListControl();
            if ((_storageMode & PicklistStorageMode.Id) != 0)
                _picklist.StorageMode = Sage.Platform.Controls.StorageModeEnum.ID;
            else if ((_storageMode & PicklistStorageMode.Code) != 0)
                _picklist.StorageMode = Sage.Platform.Controls.StorageModeEnum.Code;
            _picklist.PickListName = _picklistName;
            _picklist.AllowMultiples = _attr.AllowMultiples;
            _picklist.AlphaSort = _attr.AlphaSorted;
            _picklist.NoneEditable = _attr.NoneEditable;
            // do not set this attribute - the default validator created is too limited
            //_picklist.Required = _attr.Required;
            _picklist.MustExistInList = _attr.ValueMustExist;
            _picklist.PickListValueChanged += delegate
            {
                if (TextChanged != null)
                    TextChanged(this, EventArgs.Empty);
            };
            _picklist.AutoPostBack = parentControl.AutoPostBack;
            parentControl.Controls.Add(_picklist);
        }

        /// <summary>
        /// Disable / enable the picklist.
        /// </summary>
        public bool ReadOnly
        {
            set
            {
                if (_picklist != null)
                {
                    _picklist.Enabled = !value;
                    _picklist.ReadOnly = value;
                }
            }
        }

        /// <summary>
        /// Text may have changed.
        /// </summary>
        public event EventHandler TextChanged;

        #endregion
    }
}
