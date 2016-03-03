using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using Sage.Platform.Application;
using Sage.Platform.Data;


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


namespace OpenSlx.Lib.Db
{
    /// <summary>   
    /// Utility methods for database access.
    /// This object wraps an actual database connection which will be 
    /// closed when it is disposed, unless the DontCloseConnection property
    /// is set to true.
    /// </summary>
    public class DbHelper : IDisposable
    {
        private OleDbConnection _connection = null;
        private List<string> _idCache = null;
        private string _connectionString = null;

        /// <summary>
        /// Uses the current dataservice to retrieve the database connection string.
        /// Use when running within a SalesLogix application context.
        /// The connection will be closed when the DbHelper is disposed.
        /// </summary>
        public DbHelper()
            : this (ApplicationContext.Current.Services.Get<IDataService>().GetConnectionString())
        {
        }

        /// <summary>
        /// Wraps an existing connection.
        /// By default the connection will NOT be released when the DbHelper is disposed, 
        /// as it is assumed that the controller will take care of that.
        /// </summary>
        /// <param name="con"></param>
        public DbHelper(OleDbConnection con)
        {
            _connection = con;
            DontCloseConnection = true;
        }

        public DbHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// If set to true, the wrapped connection will not automatically be closed
        /// when the object is disposed.
        /// This is useful when we need to wrap a "shared" connection.
        /// </summary>
        public bool DontCloseConnection { get; set; }

        #region Simple DB access

        /// <summary>
        /// Shortcut to execute sql and return the first result (null if no match).
        /// Note that a null value in the database will be returned as a DBNull object, not a null value.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public object DoSQL(string sql)
        {
            return DoSQL(sql, null);
        }

        /// <summary>
        /// Shortcut to execute sql and return the first result (null if no match).
        /// Note that if a NULL is stored in the DB it will be returned as a DBNULL value.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object DoSQL(string sql, params object[] parameters)
        {
            ICollection pvals;
            if (parameters != null && parameters.Length == 1 && parameters[0] is ICollection)
                pvals = (ICollection)parameters[0];
            else
                pvals = (ICollection)parameters;
            return DoSQL(sql, pvals);
        }

        /// <summary>
        /// Shortcut to execute sql and return the first result (null if no match).
        /// Note that if a NULL is stored in the DB it will be returned as a DBNULL value.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
        public object DoSQL(string sql, ICollection parameters)
        {
            object result;
            IEnumerator iter;

            using (OleDbCommand cmd = CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    iter = parameters.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        if (iter.Current != null)
                            cmd.Parameters.Add("p" + cmd.Parameters.Count, GetOleDbType(iter.Current.GetType())).Value = iter.Current;
                        else
                            cmd.Parameters.Add("p" + cmd.Parameters.Count, OleDbType.VarChar, 1).Value = "";
                    }
                }
                result = cmd.ExecuteScalar();
            }
            return result;
        }


        /// <summary>
        /// Execute non-query SQL.
        /// Return number of rows affected.
        /// </summary>
        /// <param name="sql"></param>
        public int ExecuteSQL(string sql)
        {
            return ExecuteSQL(sql, null);
        }

        /// <summary>
        /// Execute non-query sql.
        /// Return number of rows affected.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public int ExecuteSQL(string sql, params object[] parameters)
        {
            ICollection pvals;
            if (parameters != null && parameters.Length == 1 && parameters[0] is ICollection)
                pvals = (ICollection)parameters[0];
            else
                pvals = (ICollection)parameters;
            return ExecuteSQL(sql, pvals);
        }

        /// <summary>
        /// Execute non-query sql.
        /// Return number of rows affected.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
        public int ExecuteSQL(string sql, ICollection parameters)
        {
            using (OleDbCommand cmd = (OleDbCommand)CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    foreach (object p in parameters)
                    {
                        var param = cmd.Parameters.Add("p" + cmd.Parameters.Count,
                            (p == null) ? OleDbType.VarChar : GetOleDbType(p.GetType())).Value = p ?? "";
                    }
                }
                return cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Retrieve value of specified field (null if not found)
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="tableName"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public object GetField(string fieldName, string tableName, string condition)
        {
            return GetField(fieldName, tableName, condition, null);
        }

        /// <summary>
        /// Retrieve value of specified field (null if not found)
        /// Null value in the database are returned as a null value, NOT a DBNull object.
        /// </summary>
        public object GetField(string fieldName, string tableName, string condition, params object[] parameters)
        {
            string sql = "SELECT " + fieldName + " FROM " + tableName;
            if (condition != "")
            {
                sql = sql + " WHERE " + condition;
            }
            object result = DoSQL(sql, parameters);
            if (result == DBNull.Value)
                return null;
            else
                return result;
        }

        /// <summary>
        /// Retrieve first matching row.
        /// Return null if no match.
        /// </summary>
        /// <param name="fieldNames"></param>
        /// <param name="tableName"></param>
        /// <param name="condition"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public Dictionary<string, object> GetFields(string fieldNames, string tableName, string condition, params object[] parameters)
        {
            string sql = "SELECT " + fieldNames + " FROM " + tableName;
            if (condition != "")
            {
                sql = sql + " WHERE " + condition;
            }
            using (var reader = OpenDataReader(sql, parameters))
            {
                if (reader.Read())
                {
                    Dictionary<string, object> vals = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        vals[reader.GetName(i)] = reader[i];
                    }
                    return vals;
                }
            }
            return null;
        }


        /// <summary>
        /// Open and return datareader using provided SQL.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader(string sql)
        {
            return OpenDataReader(sql, (ICollection)null);
        }

        /// <summary>
        /// Open and return datareader using provided SQL.
        /// SQL may be parameterized, in which case the parameter values should be 
        /// provided (as scalars) in conditionParams.
        /// If there are no parameter, give null or an empty collection.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditionParams"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
        public IDataReader OpenDataReader(string sql, ICollection conditionParams)
        {
            OleDbCommand cmd = (OleDbCommand)CreateCommand();
            cmd.CommandText = sql;
            if (conditionParams != null)
            {
                IEnumerator iter = conditionParams.GetEnumerator();
                while (iter.MoveNext())
                {
                    OleDbType type = GetOleDbType(iter.Current.GetType());
                    OleDbParameter param = cmd.Parameters.Add("p" + cmd.Parameters.Count, type);
                    param.Value = iter.Current;
                }
            }

            return cmd.ExecuteReader();
        }

        /// <summary>
        /// Open and return datareader using provided SQL.
        /// SQL may be parameterized, in which case the parameter values should be 
        /// provided (as scalars) in conditionParams.
        /// If there are no parameter, give null or an empty collection.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditionParams"></param>
        /// <returns></returns>
        public IDataReader OpenDataReader(string sql, params object[] conditionParams)
        {
            return OpenDataReader(sql, (ICollection)conditionParams);
        }

        #endregion

        #region Transactions

        private IDbTransaction _transaction;
        private int _transactionCount = 0;

        /// <summary>
        /// Retrieve currently active transaction (null if no transaction active)
        /// </summary>
        public IDbTransaction Transaction
        {
            get
            {
                return _transaction;
            }
        }


        /// <summary>
        /// Start a transaction.  If a transaction is already active, this will not start a 
        /// nested transaction, instead it will make sure the next call to Rollback or
        /// Commit is ignored.
        /// </summary>
        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                _transactionCount++;
            }
            else
            {
                //throw new InvalidOperationException("Transaction is already opened");
                _connection = (OleDbConnection)GetConnection();
                _transaction = _connection.BeginTransaction();
                _transactionCount = 0;
            }
        }

        /// <summary>
        /// If a transaction was started, commit it then close the associated connection.
        /// </summary>
        public void CommitTransaction()
        {
            if (_transactionCount > 0)
            {
                _transactionCount--;
            }
            else if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        /// <summary>
        /// If a transaction was started, call rollback then close the associated connection.
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                if (_transactionCount > 0)
                {
                    _transactionCount--;
                }
                else
                {
                    try
                    {
                        _transaction.Rollback();
                    }
                    catch (OleDbException)
                    {
                        // add this catch to handle cases where SQL server already automatically
                        // rolled back the transaction
                    }
                    _transaction = null;
                }
            }
        }

        #endregion

        #region Dataset Helpers

        /// <summary>
        /// Fill and return dataset using provided SQL.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public DataSet OpenDataSet(string sql)
        {
            return OpenDataSet(sql, (ICollection)null);
        }

        /// <summary>
        /// Return a filled dataset with specified query
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditionParams"></param>
        /// <returns></returns>
        public DataSet OpenDataSet(string sql, params object[] conditionParams)
        {
            return OpenDataSet(sql, (ICollection)conditionParams);
        }

        /// <summary>
        /// Return a filled dataset with specified query
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conditionParams"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "We are returning the newly created object!")]
        public DataSet OpenDataSet(string sql, ICollection conditionParams)
        {
            DataSet ds = new DataSet();
            DataTable dt = ds.Tables.Add();
            FillDataTable(dt, sql, conditionParams);
            return ds;
        }

        /// <summary>
        /// Populate data table
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sql"></param>
        /// <param name="conditionParams"></param>
        /// <returns></returns>
        public DataTable FillDataTable(DataTable dt, string sql, params object[] conditionParams)
        {
            return FillDataTable(dt, sql, (ICollection)conditionParams);
        }

        /// <summary>
        /// Populate data table
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="sql"></param>
        /// <param name="conditionParams"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
        public DataTable FillDataTable(DataTable dt, string sql, ICollection conditionParams)
        {
            using (OleDbCommand cmd = (OleDbCommand)CreateCommand())
            {
                cmd.CommandText = sql;
                if (conditionParams != null)
                {
                    foreach (object p in conditionParams)
                    {
                        OleDbType type = ((p == null) ? OleDbType.VarChar : GetOleDbType(p.GetType()));
                        OleDbParameter param = cmd.Parameters.Add("p" + cmd.Parameters.Count, type);
                        param.Value = p;
                    }
                }
                using (OleDbDataAdapter da = new OleDbDataAdapter())
                {
                    da.SelectCommand = cmd;
                    dt.BeginLoadData();
                    da.Fill(dt);
                    dt.EndLoadData();
                    return dt;
                }
            }
        }

        /// <summary>
        /// Retrieve schema for the table.
        /// Uses the table's configured name for select.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
        public DataTable FillTableSchema(DataTable dt, string fields)
        {
            if (dt.Columns.Count > 0)
                // assume it was already filled
                return dt;
            using (OleDbDataAdapter da = new OleDbDataAdapter(string.Format("select {0} from {1} where 1=2", fields, dt.TableName),
                GetConnection()))
            {
                da.SelectCommand.Transaction = (OleDbTransaction)_transaction;
                return da.FillSchema(dt, SchemaType.Mapped);
            }
        }

        /// <summary>
        /// Attempt to build the insert, update and delete commands for the given data table.
        /// This assumes that dt.TableName is the physical table name, that all fields in the 
        /// datatable are to be mapped, and that the schema for the table is already populated.
        /// </summary>
        /// <param name="dt"></param>
        public void BuildDataSetCommands(DataTable dt, OleDbDataAdapter da)
        {
            SlxCommandBuilder.BuildDataSetCommands(dt, da, this);
        }

        /// <summary>
        /// Determine whether the data in the row has actually been changed,
        /// and update the row state accordingly.
        /// </summary>
        /// <param name="row"></param>
        public static void DetectRowChanges(DataRow row)
        {
            if (row.RowState == DataRowState.Modified)
            {
                foreach (DataColumn dc in row.Table.Columns)
                {
                    object oldValue = row[dc, DataRowVersion.Original];
                    object newValue = row[dc, DataRowVersion.Current];
                    if (oldValue is string)
                        oldValue = oldValue.ToString().Trim();
                    if (newValue is string)
                        newValue = newValue.ToString().Trim();
                    if (oldValue == DBNull.Value && newValue is string)
                        oldValue = oldValue.ToString();
                    if (oldValue != newValue &&
                        !oldValue.Equals(newValue))
                        // some different value - leave row state as modified
                        return;
                }
                // means the data was not really changed
                row.AcceptChanges();
            }
        }

        #endregion

        #region Command Helpers

        /// <summary>
        /// Return a command initialized with current connection.
        /// </summary>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "We are returning the newly created object!")]
        public OleDbCommand CreateCommand()
        {
            return SetupCommand(new OleDbCommand());
        }

        /// <summary>
        /// Set up transaction and connection properties on the command.
        /// Return the command itself.
        /// </summary>
        /// <param name="cmd"></param>
        public OleDbCommand SetupCommand(OleDbCommand cmd)
        {
            cmd.Connection = GetConnection();
            cmd.Transaction = (OleDbTransaction)_transaction;
            return cmd;
        }

        /// <summary>
        /// Use schema from data table to create the parameters on the command, one for each field.
        /// Value of the parameters is not set.
        /// </summary>
        public static void SetupCommandParameters(OleDbCommand cmd, DataTable dt)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                var p = cmd.Parameters.Add("p" + i, GetOleDbType(dt.Columns[i].DataType),
                    dt.Columns[i].MaxLength);
                p.SourceColumn = dt.Columns[i].ColumnName;
                p.SourceVersion = DataRowVersion.Current;
            }
        }

        /// <summary>
        /// Return the OleDbType that represents the specified system type.
        /// This is particularly useful when you have a DataTable and want to create a 
        /// OleDbParameter for onw of its columns, but don't know its type. 
        /// DataColumnName.DataType returns a Type, and you can convert it to OleDbType 
        /// by passing it to this function.
        /// </summary>
        /// <param name="sysType"></param>
        /// <returns></returns>
        public static OleDbType GetOleDbType(Type sysType)
        {
            if (sysType == typeof(string))
                return OleDbType.VarChar;
            else if (sysType == typeof(long))
                return OleDbType.BigInt;
            else if (sysType == typeof(int))
                return OleDbType.Integer;
            else if (sysType == typeof(short))
                return OleDbType.SmallInt;
            else if (sysType == typeof(bool))
                return OleDbType.Boolean;
            else if (sysType == typeof(DateTime))
                return OleDbType.DBTimeStamp;
            else if (sysType == typeof(char))
                return OleDbType.Char;
            else if (sysType == typeof(double))
                return OleDbType.Double;
            else if (sysType == typeof(double))
                return OleDbType.Double;
            else if (sysType == typeof(float))
                return OleDbType.Single;
            else if (sysType == typeof(byte))
                return OleDbType.Binary;
            else if (sysType == typeof(Guid))
                return OleDbType.Guid;
            else if (sysType == typeof(decimal))
                return OleDbType.Numeric;
            else
                throw new Exception("Type " + sysType.Name + " can't be converted to OleDbType");
        }

        #endregion

        #region Misc Helpers

        /// <summary>
        /// Open the connection if it was not opened yet.
        /// </summary>
        private void InitializeConnection()
        {
            if (_connection == null)
                _connection = new OleDbConnection(_connectionString);
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();
        }

        /// <summary>
        /// Retrieve reference to the underlying connection.
        /// Should NOT be closed by caller.
        /// </summary>
        /// <returns></returns>
        public OleDbConnection GetConnection()
        {
            InitializeConnection();
            return _connection;
        }

        /// <summary>
        /// Create new SLX Id.  Requires SLXOLEDB provider.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public string GetIDFor(string table)
        {
            string keyname;
            if (_idCache != null && _idCache.Count > 0)
            {
                string s = _idCache[0];
                _idCache.RemoveAt(0);
                return s;
            }

            switch (table.ToUpper())
            {
                // exceptions
                case "ATTACHMENT": keyname = "FILEATTACH"; break;
                case "USERNOTIFICATION": keyname = "USERNOTIFY"; break;
                case "AGENTS": keyname = "HOSTTASK"; break;
                case "RESOURCELIST": keyname = "RESOURCE"; break;
                case "USEROPTION": keyname = "USERVIEW"; break;
                case "JOINDATA": keyname = "JOIN"; break;
                case "PROCEDURES": keyname = "PROCEDURE"; break;
                case "SEC_FUNCTIONOWNER": keyname = "FUNCTIONHANDLER"; break;
                default: keyname = table; break;
            }

            string sql = "slx_DBIDs('" + keyname + "', 1)";
            return (string)DoSQL(sql);
        }

        /// <summary>
        /// Pre-generate some SLX ids (this will generate "Q" style ids only).
        /// This can be used to prepare some ids before entering a transaction (to avoid locking the 
        /// key table) or to 
        /// </summary>
        /// <param name="nIds">Number of ids to generate</param>
        public void LoadIDCache(int nIds)
        {
            _idCache = new List<string>();
            using (var reader = OpenDataReader("slx_DBIDs('Other', " + nIds + ")"))
            {
                while (reader.Read())
                {
                    _idCache.Add(reader.GetString(0));
                }
            }
        }

        #endregion

        #region General (static) helper methods

        /// <summary>
        /// Double single quotes inside s, and surround it with quotes. Return modified string.
        /// This is only valid on SQL Server or Oracle.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string SqlQuote(string s)
        {
            return "'" + s.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Format date for SLX
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static string FormatSlxDate(DateTime d)
        {
            return d.ToString("yyyyMMdd hh:mm:ss");
        }

        /// <summary>
        /// Simple sanity check on the provided SQL: avoid newlines, avoid comments and semicolons outside of strings.
        /// This is only valid on SQL Server or Oracle.
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static bool CheckSafeSQL(string sql)
        {
            string sqlNoQuotes = Regex.Replace(sql, "'[^']*'", "");
            if (Regex.Match(sqlNoQuotes, ";|--").Success)
                return false;
            return true;
        }

        /// <summary>
        /// Used to convert a datareader or datarow cell to a specific type.
        /// </summary>
        /// <typeparam name="T">Expected type for the datarow value</typeparam>
        /// <param name="value">value retrieved from datarow or datareader as object</param>
        /// <param name="ifnull">value to be provided if the source data is null</param>
        /// <returns></returns>
        public static T IsDBNull<T>(object value, T ifnull)
        {
            return value == DBNull.Value ? ifnull : (T)value;
        }

        #endregion



        #region IDisposable Members

        /// <summary>
        /// Close connection.
        /// If a transaction is still active this will roll it back.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_transaction != null)
                {
                    RollbackTransaction();
                }
                if (_connection != null && !DontCloseConnection)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }

        #endregion


        /// <summary>
        /// Helper class used to prepare and cache commands for insert / update of SalesLogix tables.
        /// </summary>
        private static class SlxCommandBuilder
        {
            /// <summary>
            /// Holds cache of insert commands
            /// </summary>
            private static Dictionary<string, CommandCacheItem> _commandCache = new Dictionary<string, CommandCacheItem>();

            /// <summary>
            /// Attempt to build the insert, update and delete commands for the given data table.
            /// This assumes that dt.TableName is the physical table name, that all fields in the 
            /// datatable are to be mapped, and that the schema for the table is already populated.
            /// </summary>
            /// <param name="dt"></param>
            public static void BuildDataSetCommands(DataTable dt, OleDbDataAdapter da, DbHelper db)
            {
                CommandCacheItem cmds;
                if (!_commandCache.TryGetValue(dt.TableName, out cmds))
                {
                    cmds = new CommandCacheItem();
                    DataTable dtSchema = dt;
                    if (dt.Columns[0].MaxLength == -1)
                    {
                        dtSchema = GetTableSchema(db, dt);
                    }
                    cmds.InsertCommand = BuildInsertCommand(dtSchema, db);
                    cmds.UpdateCommand = BuildUpdateCommand(dtSchema, db);
                    cmds.DeleteCommand = BuildDeleteCommand(dtSchema, db);
                    _commandCache[dt.TableName] = cmds;
                }
                else
                {
                    db.SetupCommand(cmds.UpdateCommand);
                    db.SetupCommand(cmds.DeleteCommand);
                    db.SetupCommand(cmds.InsertCommand);
                }

                da.UpdateCommand = cmds.UpdateCommand;
                da.DeleteCommand = cmds.DeleteCommand;
                da.InsertCommand = cmds.InsertCommand;
            }

            /// <summary>
            /// Helper method to retrieve the actual table schema from the database.
            /// This is needed with the SalesLogix provider as the originally returned datatable 
            /// does not have proper size information on its columns.
            /// </summary>
            /// <param name="db"></param>
            /// <param name="dt"></param>
            /// <returns></returns>
            private static DataTable GetTableSchema(DbHelper db, DataTable dt)
            {
                StringBuilder fields = new StringBuilder();
                foreach (DataColumn dc in dt.Columns)
                {
                    if (fields.Length > 0)
                        fields.Append(',');
                    fields.Append(dc.ColumnName);
                }
                DataTable dtSchema = new DataTable { TableName = dt.TableName };
                dtSchema = db.FillTableSchema(dtSchema, fields.ToString());
                return dtSchema;
            }

            /// <summary>
            /// Create a new command, populating the parameters connection from the database schema.
            /// Connection/Transaction are not set on the returned command.
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="db"></param>
            /// <returns></returns>
            [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
            private static OleDbCommand BuildInsertCommand(DataTable dt, DbHelper db)
            {
                OleDbCommand cmd = db.CreateCommand();
                StringBuilder str = new StringBuilder();

                str.Append("INSERT INTO ").
                    Append(dt.TableName).
                    Append(" (");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0)
                        str.Append(',');
                    str.Append(dt.Columns[i]);
                }
                str.Append(") VALUES (");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0)
                        str.Append(",");
                    str.Append("?");
                }
                str.Append(")");

                cmd.CommandText = str.ToString();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    cmd.Parameters.Add("p" + i.ToString(), DbHelper.GetOleDbType(dt.Columns[i].DataType),
                        dt.Columns[i].MaxLength, dt.Columns[i].ColumnName);
                }
                return cmd;
            }

            /// <summary>
            /// Build an update command for a DataTable
            /// The condition string is simply appended to the end of the command.
            /// If it contains parameters (? characters), the caller must add the appropriate 
            /// parameters to the cmd.Parameters collection.
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="db"></param>
            /// <returns></returns>
            [SuppressMessage("Microsoft.Security", "CA2100")]
            private static OleDbCommand BuildUpdateCommand(DataTable dt, DbHelper db)
            {
                OleDbCommand cmd = db.CreateCommand();
                StringBuilder str = new StringBuilder();

                str.Append("UPDATE ").
                    Append(dt.TableName).
                    Append(" SET ");
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0)
                        str.Append(",");
                    str.Append(dt.Columns[i].ColumnName).Append("=?");

                    cmd.Parameters.Add("p" + i.ToString(), DbHelper.GetOleDbType(dt.Columns[i].DataType),
                        dt.Columns[i].MaxLength, dt.Columns[i].ColumnName);
                }
                AddPrimaryKeyCondition(dt, cmd, str);

                cmd.CommandText = str.ToString();
                return cmd;
            }

            /// <summary>
            /// Prepare a delete command for the given table.
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="db"></param>
            /// <returns></returns>
            [SuppressMessage("Microsoft.Security", "CA2100", Justification = "Query is built from code")]
            private static OleDbCommand BuildDeleteCommand(DataTable dt, DbHelper db)
            {
                OleDbCommand cmd = db.CreateCommand();
                StringBuilder str = new StringBuilder();

                str.Append("DELETE FROM ").
                    Append(dt.TableName);
                AddPrimaryKeyCondition(dt, cmd, str);

                cmd.CommandText = str.ToString();
                return cmd;
            }

            /// <summary>
            /// Add a condition based on the table's configured PrimaryKey.
            /// If there is no primary key set, we'll guess one based on SLX conventions.
            /// </summary>
            /// <param name="dt"></param>
            /// <param name="cmd"></param>
            /// <param name="cmdText"></param>
            private static void AddPrimaryKeyCondition(DataTable dt, OleDbCommand cmd, StringBuilder cmdText)
            {
                cmdText.Append(" WHERE ");
                for (int i = 0; i < dt.PrimaryKey.Length; i++)
                {
                    if (i > 0)
                        cmdText.Append(" AND ");
                    cmdText.Append(dt.PrimaryKey[i].ColumnName).Append("=?");
                    cmd.Parameters.Add("pk" + i.ToString(), DbHelper.GetOleDbType(dt.PrimaryKey[i].DataType),
                        dt.PrimaryKey[i].MaxLength, dt.PrimaryKey[i].ColumnName);
                }
                if (dt.PrimaryKey.Length == 0)
                {
                    cmdText.Append(dt.TableName).Append("ID=?");
                    cmd.Parameters.Add("pk", OleDbType.Char, 12, dt.TableName + "ID");
                }
            }

            /*

            /// <summary>
            /// Do an update, but if there is no match, do an insert with specified values.
            /// Primary Key field name has to be specified for .net to be able to figure out
            /// the updates.
            /// </summary>
            /// <param name="tableName"></param>
            /// <param name="fieldNames">Field names, separated by commas</param>
            /// <param name="values">Collection of values</param>
            /// <param name="condition"></param>
            public void DoInsertUpdate(String tableName, String fieldNames, ICollection values, String condition)
            {
                if (DoUpdate(tableName, fieldNames, values, condition) == 0)
                    DoInsert(tableName, fieldNames, values);
            }

            /// <summary>
            /// Do an update, but if there is no match, do an insert with specified values.
            /// This uses parameterized condition for the update - pass the condition values in the
            /// conditionParams parameter.
            /// </summary>
            /// <param name="tableName"></param>
            /// <param name="fieldNames"></param>
            /// <param name="values"></param>
            /// <param name="condition"></param>
            /// <param name="conditionParams"></param>
            /// <param name="addSlxFields"></param>
            public void DoInsertUpdate(String tableName, String fieldNames, ICollection values, String condition, ICollection conditionParams, bool addSlxFields)
            {
                if (DoUpdate(tableName, fieldNames, values, condition, conditionParams, addSlxFields) == 0)
                    DoInsert(tableName, fieldNames, values, addSlxFields);
            }

             * */

            private class CommandCacheItem
            {
                public OleDbCommand UpdateCommand, InsertCommand, DeleteCommand;
            }
        }

    }
}
