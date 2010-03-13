using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.Platform.Security;
using Sage.Platform;
using Sage.SalesLogix.Security;
using Sage.Entity.Interfaces;

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

        public override string UserId
        {
            get { return _userId; }
        }

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