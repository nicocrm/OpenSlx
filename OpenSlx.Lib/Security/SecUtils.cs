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
