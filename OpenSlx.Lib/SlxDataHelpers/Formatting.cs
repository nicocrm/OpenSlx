using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform;
using Sage.Entity.Interfaces;

namespace OpenSlx.Lib.SlxDataHelpers
{
    /// <summary>
    /// Formatting of SalesLogix data
    /// </summary>
    public class Formatting
    {
        public static String FormatUserName(String userId)
        {
            if (String.IsNullOrEmpty(userId))
                return "";
            var user = EntityFactory.GetById<IUser>(userId);
            return user == null ? "" : user.UserInfo.UserName;
        }
    }
}
