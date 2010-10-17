using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sage.Platform.Projects.Interfaces;
using OpenSlx.Lib.Db;
using System.Threading;
using Sage.Platform.Orm.Entities;
using Sage.Platform.Application;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Input;


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


namespace OpenSlx.RelationCheck.Model
{
    /// <summary>
    /// ViewModel for RelationCheck form
    /// </summary>
    public class RelationCheckFormModel : INotifyPropertyChanged
    {
        private Dispatcher _dispatcher;

        #region Properties

        ///<summary>
        /// 
        ///</summary>
        public ObservableCollection<Relationship> Relationships
        {
            get { return _Relationships; }
            set
            {
                if (_Relationships != value)
                {
                    _Relationships = value;
                    OnPropertyChanged("Relationships");
                }
            }
        }
        private ObservableCollection<Relationship> _Relationships;

        ///<summary>
        /// Progress percent
        ///</summary>
        public int Progress
        {
            get { return _Progress; }
            set
            {
                if (_Progress != value)
                {
                    _Progress = value;
                    OnPropertyChanged("Progress");
                }
            }
        }
        private int _Progress;

        ///<summary>
        /// What we are currently doing
        ///</summary>
        public String Status
        {
            get { return _Status; }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnPropertyChanged("Status");
                }
            }
        }
        private String _Status;

        ///<summary>
        /// Can we do something else right now
        ///</summary>
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                if (_IsBusy != value)
                {
                    _IsBusy = value;
                    OnPropertyChanged("IsBusy");
                    _dispatcher.Invoke((Action)delegate
                    {
                        CommandManager.InvalidateRequerySuggested();
                    });
                }
            }
        }
        private bool _IsBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(String propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        #region Commands

        private DelegateCommand _testRelationshipsCommand = null;
        public DelegateCommand TestRelationshipsCommand
        {
            get
            {
                if (_testRelationshipsCommand == null)
                    _testRelationshipsCommand = new DelegateCommand(TestRelationships, CheckBusy);
                return _testRelationshipsCommand;
            }
        }

        private DelegateCommand _repairRelationshipsCommand = null;
        public DelegateCommand RepairRelationshipsCommand
        {
            get
            {
                if (_repairRelationshipsCommand == null)
                    _repairRelationshipsCommand = new DelegateCommand(RepairRelationships, CheckBusy);
                return _repairRelationshipsCommand;
            }
        }

        #endregion

        #region Actions

        /// <summary>
        /// Used as "CanExecute", checking hte busy flag
        /// </summary>
        /// <returns></returns>
        private bool CheckBusy()
        {
            return !IsBusy;
        }

        /// <summary>
        /// Load the relationship info from the project (uses the ProjectContextService to determine the current project)
        /// </summary>
        /// <param name="project"></param>
        public void LoadRelationships()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                IProject project = ApplicationContext.Current.Services.Get<IProjectContextService>().ActiveProject;
                IsBusy = true;
                try
                {
                    Relationships = new ObservableCollection<Relationship>();
                    OrmModel model = project.Models.Get<OrmModel>();
                    foreach (OrmRelationship relationship in model.Relationships)
                    {
                        if (relationship.Columns.Count > 1)
                            continue;
                        
                        _dispatcher.Invoke(DispatcherPriority.Normal, new Action<OrmRelationship>(AddRelationship), relationship);
                    }
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        private void AddRelationship(OrmRelationship relationship)
        {
            lock (Relationships)
            {
                this.Relationships.Add(new Relationship(relationship));
            }
        }

        /// <summary>
        /// Update the counts for relationships
        /// </summary>        
        private void TestRelationships()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                IsBusy = true;
                Progress = 0;
                int count = 0;
                DbHelper db = new DbHelper();
                try
                {
                    foreach (Relationship r in Relationships)
                    {
                        r.UpdateCount(db);
                        count++;
                        Progress = (count * 100) / Relationships.Count;
                    }
                }
                finally
                {
                    IsBusy = false;
                    db.Dispose();
                }
            });
        }


        /// <summary>
        /// Repair all relationships
        /// </summary>        
        private void RepairRelationships()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                IsBusy = true;
                Progress = 0;
                int count = 0;
                DbHelper db = new DbHelper();
                try
                {
                    foreach (Relationship r in Relationships)
                    {
                        r.FixData(db);
                        count++;
                        Progress = (count * 100) / Relationships.Count;
                    }
                }
                finally
                {
                    IsBusy = false;
                    db.Dispose();
                }
            });
        }


        #endregion

        public RelationCheckFormModel()
        {
            // get a reference to the UI thread dispatcher
            _dispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
