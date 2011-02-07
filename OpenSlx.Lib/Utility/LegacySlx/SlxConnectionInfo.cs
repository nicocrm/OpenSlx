using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using log4net;


/*
   OpenSlx - Open Source SalesLogix Library and Tools
   Copyright 2010 nicocrm (http://github.com/ngaller/OpenSlx)

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

namespace OpenSlx.Lib.Utility.LegacySlx
{
    /// <summary>
    /// Used to enumerate connections on a SalesLogix installation and build the connection strings.
    /// Really more for legacy installations since it will access the connections defined by the LAN client.
    /// </summary>
    public class SlxConnectionInfo
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(SlxConnectionInfo));

        #region Private Fields
        private String _server = null;
        private String _database = null;
        private float _version = 0;
        private String _username = "ADMIN";
        private String _password = null;
        private String _conString = null;
        private String _alias = null;
        #endregion

        #region Public Properties

        /// <summary>
        /// DB Alias (this will default to the database name)
        /// </summary>
        public String Alias
        {
            get
            {
                if (_alias == null)
                    return _database;
                return _alias;
            }
            set
            {
                _alias = value;
            }
        }

        public String Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public String Database
        {
            get { return _database; }
            set { _database = value; }
        }

        public float Version
        {
            get { return _version; }
            set { _version = value; }
        }

        #endregion

        public SlxConnectionInfo()
        {
        }

        /// <summary>
        /// Define the SlxConnectionInfo with pre-built connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        public SlxConnectionInfo(String connectionString)
        {
            _conString = connectionString;
        }

        /// <summary>
        /// Build connection string with the default password.
        /// If the password has not been set yet, it will be passed as a blank password.
        /// </summary>
        /// <returns></returns>
        public String BuildConnectionString()
        {
            return BuildConnectionString(_password);
        }

        /// <summary>
        /// Create a connection string using the provided password.
        /// Note this doesn't modify the Password property.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public String BuildConnectionString(String password)
        {
            return BuildConnectionString(_username, password);
        }

        /// <summary>
        /// Build a user connection string.
        /// Note that this doesn't modify the Username or Password properties.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public String BuildConnectionString(String username, String password)
        {
            OleDbConnectionStringBuilder builder = new OleDbConnectionStringBuilder();
            if (_conString != null)
            {
                builder.ConnectionString = _conString;
            }
            else
            {
                builder["Provider"] = "SLXOLEDB";
                builder["Data Source"] = _server;
                builder["Initial Catalog"] = _database;
            }
            if(username != null)
                builder["User Id"] = username;
            if(password != null)
                builder["Password"] = password;
            return builder.ToString();
        }

        public override string ToString()
        {
            return Alias;
        }

        #region Static methods to help load connection strings

        /// <summary>
        /// Read list of connections defined in registry and return them in order.
        /// The connections don't include the password as this is not stored in the registry.
        /// If no connection is defined, an empty list is returned.
        /// </summary>
        /// <returns></returns>
        public static IList<SlxConnectionInfo> ListAllConnections()
        {
            List<SlxConnectionInfo> list = new List<SlxConnectionInfo>();
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(@"Software\SalesLogix\ADOLogin");
            if (rk != null)
            {
                try
                {
                    String[] connectionKeyNames = rk.GetSubKeyNames();
                    foreach (String keyName in connectionKeyNames)
                    {
                        RegistryKey conKey = rk.OpenSubKey(keyName);
                        try
                        {
                            if (conKey != null && !String.IsNullOrEmpty((String)conKey.GetValue("Data Source")))
                            {
                                list.Add(new SlxConnectionInfo()
                                {
                                    Server = (String)conKey.GetValue("Data Source"),
                                    Database = (String)conKey.GetValue("Initial Catalog"),
                                    Alias = (String)conKey.GetValue("Alias")
                                });                                
                            }
                        }
                        finally
                        {
                            conKey.Close();
                        }
                    }
                }
                finally
                {
                    rk.Close();
                }
            }
            return list;
        }

        #endregion
    }
}