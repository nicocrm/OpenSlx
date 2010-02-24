using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Representation for a list item, for the SimplePicklist control.
    /// The value and text do not necessarily correspond to the values from the picklist table,
    /// instead they depend on the StorageMode / DisplayMode
    /// </summary>
    public class PicklistItem
    {
        public PicklistItem(PickList pkl, PicklistStorageMode displayMode, PicklistStorageMode storageMode)
        {
            Value = FormatText(pkl, storageMode);
            Text = FormatText(pkl, displayMode);
        }

        /// <summary>
        /// Value to be used for storage
        /// </summary>
        public String Value { get; set; }

        /// <summary>
        /// Text to be used for display
        /// </summary>
        public String Text { get; set; }


        /// <summary>
        /// Format the given picklist item according to the specified mode.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="picklistMode"></param>
        /// <returns></returns>
        private string FormatText(PickList item, PicklistStorageMode picklistMode)
        {
            String txt = "";
            if ((picklistMode & PicklistStorageMode.Code) != 0)
            {
                if (txt != "")
                    txt += " - ";
                txt += item.Shorttext;
            }
            if ((picklistMode & PicklistStorageMode.Id) != 0)
            {
                if (txt != "")
                    txt += " - ";
                txt += item.ItemId;
            }
            if ((picklistMode & PicklistStorageMode.Text) != 0)
            {
                if (txt != "")
                    txt += " - ";
                txt += item.Text;
            }
            return txt;
        }
    }
}
