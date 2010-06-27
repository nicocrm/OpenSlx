using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;

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

namespace OpenSlx.Lib.Web.Controls.Impl
{
    /// <summary>
    /// Representation for a list item, for the SimplePicklist control.
    /// The value and text do not necessarily correspond to the values from the picklist table,
    /// instead they depend on the StorageMode / DisplayMode
    /// </summary>
    public class PicklistItemDisplay
    {
        /// <summary>
        /// Create a picklist item from a SalesLogix picklist object.
        /// Value and Text will be extracted from the picklist item based on display mode and storage mode.
        /// </summary>
        /// <param name="pkl"></param>
        /// <param name="displayMode"></param>
        /// <param name="storageMode"></param>
        public PicklistItemDisplay(PickList pkl, PicklistStorageMode displayMode, PicklistStorageMode storageMode)
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
