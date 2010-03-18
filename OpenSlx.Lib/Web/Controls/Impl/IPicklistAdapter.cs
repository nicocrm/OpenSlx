using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

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
    /// Utility interfaces used to access the picklist's value.
    /// </summary>
    public interface IPicklistAdapter
    {
        /// <summary>
        /// Returns the underlying value (or values, for a multi-select list)
        /// </summary>
        /// <returns></returns>
        String GetValue();

        /// <summary>
        /// Sets the underlying value on the control (or values, for a multi-select list)
        /// </summary>
        /// <param name="value"></param>
        void SetValue(String value);

        /// <summary>
        /// Creates controls and attaches them to the designated parent.
        /// </summary>
        /// <param name="parentControl"></param>
        void CreateChildControls(SimplePicklist parentControl);

        /// <summary>
        /// Set readonly flag 
        /// (normally this is not hit, because we'll render the picklist as a read-only textbox.
        /// But if the read-only flag is set late in the page lifecycle it will happen)
        /// </summary>
        bool ReadOnly { set; }

        /// <summary>
        /// Events should get triggered when the value may have changed.
        /// </summary>
        event EventHandler TextChanged;
    }
}
