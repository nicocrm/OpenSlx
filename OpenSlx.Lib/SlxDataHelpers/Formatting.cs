using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform;
using Sage.Entity.Interfaces;

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

namespace OpenSlx.Lib.SlxDataHelpers
{
    /// <summary>
    /// Formatting of SalesLogix data
    /// </summary>
    public class Formatting
    {
        /// <summary>
        /// Retrieve user name based on user id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static String FormatUserName(String userId)
        {
            if (String.IsNullOrEmpty(userId))
                return "";
            var user = EntityFactory.GetById<IUser>(userId);
            return user == null ? "" : user.UserInfo.UserName;
        }


    }
}
