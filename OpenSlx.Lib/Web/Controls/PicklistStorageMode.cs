using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

namespace OpenSlx.Lib.Web.Controls
{
    /// <summary>
    /// Indicates how a picklist is stored or displayed
    /// (note - this is used, rather than the default Sage StoreModeEnum, to allow for more than 1 items to be selected,
    /// if the user wants to see both code and text)
    /// </summary>
    [Flags]
    public enum PicklistStorageMode
    {
        /// <summary>
        /// Store as item id
        /// </summary>
        Id = 1,
        /// <summary>
        /// Store as code (shorttext)
        /// </summary>
        Code = 2,
        /// <summary>
        /// Store as actual text
        /// </summary>
        Text = 4
    }
}
