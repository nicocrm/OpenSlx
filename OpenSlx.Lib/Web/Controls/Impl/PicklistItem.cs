using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SalesLogix.PickLists;

/*
    OpenSlx - Open Source SalesLogix Library and Tools
    Copyright (C) 2010 Strategic Sales Systems

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
