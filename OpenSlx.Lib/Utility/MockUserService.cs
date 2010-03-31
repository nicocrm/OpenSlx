using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Security;
using Sage.Platform;
using Sage.SalesLogix.Security;
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

namespace OpenSlx.Lib.Utility
{
    /// <summary>
    /// Used to mock the user service.
    /// Contains method that lets caller specify the user id to be returned by subsequent calls (this will default to
    /// ADMIN if not specified).
    /// </summary>
    public class MockUserService : SLXUserService
    {
        private String _userId = "ADMIN";

        /// <summary>
        /// Setup user service for "ADMIN" username
        /// </summary>
        public MockUserService()
        {
        }

        /// <summary>
        /// Setup user service with the specified user name.
        /// </summary>
        /// <param name="username">Saleslogix user name (logon name)</param>
        /// <exception cref="InvalidOperationException">User name not valid in the DB</exception>
        public MockUserService(String username)
        {
            var matches = EntityFactory.GetRepository<IUser>().FindByProperty("UserName", username);
            if (matches.Count == 0)
                throw new InvalidOperationException("Invalid user name " + username);
            _userId = matches[0].Id.ToString();
        }


        #region IUserService Members

        /// <summary>
        /// UserId
        /// </summary>
        public override string UserId
        {
            get { return _userId; }
        }

        /// <summary>
        /// UserName
        /// </summary>
        public override string UserName
        {
            get
            {
                IUser user = EntityFactory.GetById<IUser>(_userId.PadRight(12, ' '));
                if (user == null)
                    throw new InvalidOperationException("Invalid user id " + _userId);

                return user.UserName;
            }
        }

        #endregion


        /// <summary>
        /// Set the fake user id to be used.
        /// </summary>
        /// <param name="userId"></param>
        public void SetUserId(String userId)
        {
            _userId = userId;
        }

    }
}