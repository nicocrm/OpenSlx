using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using Sage.SalesLogix.Web.Controls.PickList;

/*
    OpenSlx - Open Source SalesLogix Library and Tools

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
