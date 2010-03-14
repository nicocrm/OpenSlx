using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Application;
using Sage.Platform.Security;
using Sage.Platform.Orm;
using Sage.Entity.Interfaces;
using Sage.Platform;

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

namespace OpenSlx.Lib.Security
{
    /// <summary>
    /// Common utility functions related to security.
    /// </summary>
    public class SecUtils
    {
        /// <summary>
        /// Shortcut to retrieve the current user id.
        /// </summary>
        /// <returns></returns>
        public static String CurrentUserId 
        {
            get
            {
                return ApplicationContext.Current.Services.Get<IUserService>().UserId;
            }
        }

        /// <summary>
        /// Shortcut to retrieve the current user object.
        /// </summary>
        public static IUser CurrentUser
        {
            get
            {
                return EntityFactory.GetById<IUser>(CurrentUserId);
            }
        }

        /// <summary>
        /// True if current user is in specified team (or department).
        /// Note that this will return true whether the user is a direct member of the team,
        /// or manager of someone who is.
        /// </summary>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public static bool IsCurrentUserInTeam(String teamName)
        {
            return IsUserInTeam(CurrentUserId, teamName);
        }

        /// <summary>
        /// True if specified user is in specified team (or department).
        /// Note that this will return true whether the user is a direct member of the team,
        /// or manager of someone who is.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="teamName"></param>
        /// <returns></returns>
        public static bool IsUserInTeam(String userId, String teamName)
        {
            using (var sess = new SessionScopeWrapper())
            {
                return 0 < sess.CreateQuery("select count(*) from OwnerRights sr where sr.User.Id=? and sr.Owner.OwnerDescription=?")
                    .SetString(0, userId)
                    .SetString(1, teamName)
                    .UniqueResult<long>();
            }
        }
    }
}
