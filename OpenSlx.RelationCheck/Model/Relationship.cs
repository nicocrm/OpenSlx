using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenSlx.Lib.Db;
using Sage.Platform.Orm.Entities;


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
    /// Wrapper for a relationship item.
    /// </summary>
    public class Relationship : INotifyPropertyChanged
    {
        private String _parentTable, _childTable, _parentField, _childField;

        public Relationship(OrmRelationship sageRel)
        {
            RelationshipName = sageRel.ToString();
            _parentTable = sageRel.ParentEntity.TableName;
            _parentField = sageRel.Columns[0].ParentProperty.ColumnName;
            _childTable = sageRel.ChildEntity.TableName;
            _childField = sageRel.Columns[0].ChildProperty.ColumnName;
        }

        #region Properties

        ///<summary>
        /// Description of the relationship
        ///</summary>
        public String RelationshipName
        {
            get { return _RelationshipName; }
            set
            {
                if (_RelationshipName != value)
                {
                    _RelationshipName = value;
                    OnPropertyChanged("RelationshipName");
                    OnPropertyChanged("Description");   
                }
            }
        }
        private String _RelationshipName;

        ///<summary>
        /// If there is an error in the test or update, this will show the message
        ///</summary>
        public String Error
        {
            get { return _Error; }
            set
            {
                if (_Error != value)
                {
                    _Error = value;
                    OnPropertyChanged("Error");
                }
            }
        }
        private String _Error;
        

        ///<summary>
        /// Number of invalid FK
        ///</summary>
        public int? ErrorCount
        {
            get { return _ErrorCount; }
            set
            {
                if (_ErrorCount != value)
                {
                    _ErrorCount = value;
                    OnPropertyChanged("ErrorCount");
                    OnPropertyChanged("Description");
                }
            }
        }
        private int? _ErrorCount;

        public string Description 
        {
            get
            {
                return RelationshipName + (ErrorCount == null ? "" : String.Format(" ({0})", ErrorCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(String propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion

        /// <summary>
        /// Recalculate the # of invalid FKs
        /// </summary>
        /// <param name="db"></param>
        public void UpdateCount(DbHelper db)
        {
            // note, since we are looking at the relationship from the M:1 side, the "Child Table" is actually the "1" side (which would correspond to the parent 
            // side in a diagram)
            try
            {
                this.ErrorCount = (int)db.GetField("count(*)", _parentTable,
                    String.Format("{0} is not null and {0} not in (select {1} from {2} where {1} is not null)", _parentField, _childField, _childTable));
            }
            catch (Exception x)
            {
                Error = x.Message;
            }
        }

        /// <summary>
        /// Update the database
        /// </summary>
        /// <param name="db"></param>
        public void FixData(DbHelper db)
        {
            if (ErrorCount.GetValueOrDefault() > 0)
            {
                try
                {
                    db.ExecuteSQL(String.Format("update {0} set {1}=null where {1} is not null and {1} not in (select {2} from {3} where {2} is not null)",
                        _parentTable, _parentField, _childField, _childTable));
                }
                catch (Exception x)
                {
                    Error = x.Message;
                }
            }
            UpdateCount(db);
        }

    }
}
