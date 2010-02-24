using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;
using Sage.SalesLogix.Web.Controls.PickList;

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

        public PicklistPicklistAdapter(String picklistName, PickListAttributes attr, PicklistStorageMode storageMode)
        {
            _picklistName = picklistName;
            _attr = attr;
            _storageMode = storageMode;
        }

        #region IPicklistAdapter Members

        public string GetValue()
        {
            return _picklist.PickListValue;
        }

        public void SetValue(string value)
        {
            _picklist.PickListValue = value;
        }

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
            _picklist.Required = _attr.Required;
            _picklist.MustExistInList = _attr.ValueMustExist;
            _picklist.PickListValueChanged += delegate
            {
                if (TextChanged != null)
                    TextChanged(this, EventArgs.Empty);
            };
            parentControl.Controls.Add(_picklist);
        }

        public event EventHandler TextChanged;

        #endregion
    }
}
