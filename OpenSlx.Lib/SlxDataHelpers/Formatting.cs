using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform;
using Sage.Entity.Interfaces;

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
