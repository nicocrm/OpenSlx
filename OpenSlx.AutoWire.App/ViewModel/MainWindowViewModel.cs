using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenSlx.Lib.Utility.LegacySlx;
using System.Windows.Input;
using System.IO;
using System.Threading;
using OpenSlx.Lib.Db;
using Sage.Platform.Application;
using Sage.Platform.Data;
using Sage.Platform.Projects;
using System.Windows.Navigation;
using System.Windows;
using Sage.Platform.Projects.Interfaces;
using OpenSlx.AutoWire.App.Commands;


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


namespace OpenSlx.AutoWire.App.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Properties

        ///<summary>
        /// Available SLX Connections
        ///</summary>
        public IList<SlxConnectionInfo> Connections
        {
            get { return _Connections; }
            set
            {
                if (_Connections != value)
                {
                    _Connections = value;
                    OnPropertyChanged("Connections");
                }
            }
        }
        private IList<SlxConnectionInfo> _Connections;

        ///<summary>
        /// Selection
        ///</summary>
        public SlxConnectionInfo SelectedConnection
        {
            get { return _SelectedConnection; }
            set
            {
                if (_SelectedConnection != value)
                {
                    _SelectedConnection = value;
                    CheckConnection();
                    OnPropertyChanged("SelectedConnection");
                }
            }
        }

        private SlxConnectionInfo _SelectedConnection;
        

        ///<summary>
        /// Admin password
        ///</summary>
        public String Password
        {
            get { return _Password; }
            set
            {
                if (_Password != value)
                {
                    _Password = value;
                    CheckConnection();
                    OnPropertyChanged("Password");
                }
            }
        }
        private String _Password;

        ///<summary>
        /// Path to SLX project model (we can't use the DB VFS)
        ///</summary>
        public String ModelPath
        {
            get { return _ModelPath; }
            set
            {
                if (_ModelPath != value)
                {
                    _ModelPath = value;
                    if (String.IsNullOrEmpty(value))
                        _validationErrors["ModelPath"] = "Please select model path";
                    else if (!File.Exists(Path.Combine(value, "project.info.xml")))
                        _validationErrors["ModelPath"] = "Selected path is not a valid SLX project";
                    else if (_validationErrors.ContainsKey("ModelPath"))
                        _validationErrors.Remove("ModelPath");
                    OnPropertyChanged("ModelPath");
                }
            }
        }
        private String _ModelPath;
        

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Commands

        private DelegateCommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                    _loginCommand = new DelegateCommand(Login, CanLogin);
                return _loginCommand;
            }
        }

        #endregion

        #region Events

        public event EventHandler LoginSuccessful;

        #endregion

        #region Actions

        private void Login()
        {
            ApplicationContext.Initialize(Guid.NewGuid().ToString());
            ApplicationContext.Current.Services.Add<IDataService>(new ConnectionStringDataService(SelectedConnection.BuildConnectionString(Password)));
            IProject project = ProjectUtility.InitProject(ModelPath);
            ApplicationContext.Current.Services.Add<IProjectContextService>(new SimpleProjectContextService(project));
            if(LoginSuccessful != null)
                LoginSuccessful(this, EventArgs.Empty);
        }

        private bool CanLogin()
        {
            return !String.IsNullOrEmpty(_ModelPath) && _validationErrors.Count == 0;
        }

        /// <summary>
        /// Check validity of the connection asynchronously.
        /// </summary>
        private void CheckConnection()
        {
            SlxConnectionInfo con = SelectedConnection;
            lock (_validationErrors)
            {
                if (con == null)
                {
                    _validationErrors["SelectedConnection"] = "Please select connection";
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        lock (_validationErrors)
                        {
                            DbHelper db = new DbHelper(SelectedConnection.BuildConnectionString(Password));
                            try
                            {
                                db.GetField("1", "SYSTEMINFO", "");
                                if (_validationErrors.ContainsKey("SelectedConnection"))
                                    _validationErrors.Remove("SelectedConnection");
                            }
                            catch (Exception x)
                            {
                                _validationErrors["SelectedConnection"] = "Failed to connect: " + x.Message;
                            }
                            OnPropertyChanged("SelectedConnection");
                        }
                    });
                }
            }
        }

        #endregion

        #region Validation

        private Dictionary<String, String> _validationErrors = new Dictionary<string, string>();

        public string Error
        {
            get
            {
                if (_validationErrors.Count > 0)
                    return "Invalid Entry";
                return "";
            }
        }

        public string this[string columnName]
        {
            get
            {
                String error;
                if (_validationErrors.TryGetValue(columnName, out error))
                    return error;
                return "";
            }
        }

        #endregion


        public void Initialize()
        {
            Connections = SlxConnectionInfo.ListAllConnections();
            if (Connections.Count > 0)
                SelectedConnection = Connections[0];
            ModelPath = @"E:\Projects\SSS\SlxBaseline\Model";
        }

    }
}
