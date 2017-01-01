/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.5.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2008
 * 从 IDbAccesser 更名，并把 Normal 方法都分离到 IRawDbAccesser 接口中。 胡庆访 20130509
 * 
*******************************************************/

using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Data.Common;
using System.Text.RegularExpressions;
using Rafy.Data.Providers;
using System.Runtime;

namespace Rafy.Data
{
    /// <summary>
    /// Encapsulate the common operations to communicate with database.
    /// <remarks>
    /// It supports the query use DBParameter.(But it doesn't catch any exception, so the client program should deal with it by itself)
    /// 
    /// There are two categories of query method:
    /// 1.The public methods:
    ///     These methods use a special sql sentences which looks like the parameter of String.Format,
    ///     and input the needed parameters follow.
    /// 2.The public methods on IRawDbAccesser property:
    ///     These methods use the normal sql sentences, the input parameters array should be created outside.
    ///     You can use <see cref="IRawDbAccesser.ParameterFactory"/> to create parameters.
    /// </remarks>
    /// </summary>
    /// <author>whiteLight</author>
    /// <createDate>2008-7-6, 20:19:08</createDate>
    /// <modify>2008-7-22，add ConvertParamaters method，and change the interface，use a common formatted sql to search database</modify>
    /// <modify>2008-8-7, If any one of the parameters is null, it is converted to DBNull.Value.</modify>
    public class DbAccesser : IDisposable, IDbAccesser, IRawDbAccesser, IDbParameterFactory, IDbCommandFactory
    {
        #region fields

        /// <summary>
        /// Was the connection opened by my self.
        /// </summary>
        private bool _openConnectionBySelf;

        /// <summary>
        /// Is this connection created by my self;
        /// </summary>
        private bool _connectionCreatedBySelf;

        private DbConnectionSchema _connectionSchema;

        /// <summary>
        /// inner db connection
        /// </summary>
        private IDbConnection _connection;

        /// <summary>
        /// abstract db provider factory
        /// </summary>
        private DbProviderFactory _factory;

        /// <summary>
        /// used to format sql and its corresponding parameters.
        /// </summary>
        private ISqlProvider _converter;

        //private IDbTransaction _transaction; 

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// 
        /// this accessor uses <see cref="DbSetting"/> class to find its connection string, and creates connection by itself.
        /// </summary>
        /// <param name="connectionStringSettingName">the setting name in configuration file.</param>
        public DbAccesser(string connectionStringSettingName)
        {
            var setting = DbSetting.FindOrCreate(connectionStringSettingName);
            this.Init(setting);
        }

        /// <summary>
        /// Constructor
        /// 
        /// this accessor creates the db connection by itself.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="connectionProvider">
        /// The provider.
        /// eg.
        /// "System.Data.SqlClient"
        /// </param>
        public DbAccesser(string connectionString, string connectionProvider)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(connectionProvider))
            {
                throw new ArgumentNullException("connectionProvider");
            }

            this.Init(new DbConnectionSchema(connectionString, connectionProvider));
        }

        /// <summary>
        /// Constructor
        /// 
        /// this accessor uses schema to find its connection string, and creates connection by itself.
        /// </summary>
        /// <param name="schema">the connection schema.</param>
        public DbAccesser(DbConnectionSchema schema)
        {
            this.Init(schema);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="schema">the connection schema.</param>
        /// <param name="dbConnection">use a exsiting connection, rather than to create a new one.</param>
        public DbAccesser(DbConnectionSchema schema, IDbConnection dbConnection)
        {
            this.Init(schema, dbConnection);
        }

        private void Init(DbConnectionSchema schema, IDbConnection connection = null)
        {
            this._connectionSchema = schema;

            this._factory = ConverterFactory.GetFactory(schema.ProviderName);
            this._converter = ConverterFactory.Create(schema.ProviderName);
            if (connection == null)
            {
                this._connection = this._factory.CreateConnection();
                this._connection.ConnectionString = schema.ConnectionString;
                this._connectionCreatedBySelf = true;
            }
            else
            {
                this._connection = connection;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the connection schema of current database.
        /// </summary>
        public DbConnectionSchema ConnectionSchema
        {
            get { return this._connectionSchema; }
        }

        /// <summary>
        /// The underlying db connection
        /// </summary>
        public IDbConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        /// <summary>
        /// current avaiable transaction.
        /// this transaction is retrieved from <see cref="LocalTransactionBlock"/> class, and it is created by current connection.
        /// </summary>
        public IDbTransaction Transaction
        {
            get
            {
                var tran = LocalTransactionBlock.GetCurrentTransaction(_connectionSchema.Database);
                if (tran != null && tran.Connection == _connection)
                {
                    return tran;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets a raw accesser which is oriented to raw sql and <c>IDbDataParameter</c>。
        /// </summary>
        public IRawDbAccesser RawAccesser
        {
            get { return this; }
        }

        /// <summary>
        /// Open the connection
        /// </summary>
        private void MakeConnectionOpen()
        {
            if (this._connection.State == ConnectionState.Closed)
            {
                this._openConnectionBySelf = true;
                this._connection.Open();
            }
        }

        /// <summary>
        /// This method only close the connection which is opened by this object itself.
        /// </summary>
        private void MakeConnectionClose()
        {
            if (this._openConnectionBySelf)
            {
                this._connection.Close();
                this._openConnectionBySelf = false;
            }
        }

        ///// <summary>
        ///// Invoke this method, if there will be many query.
        ///// </summary>
        //public void OpenConnection()
        //{
        //    if (this._connection.State == ConnectionState.Closed)
        //    {
        //        this._connection.Open();
        //    }
        //}
        ///// <summary>
        ///// If "Open" method is invoked before, this method should be invoked too.
        ///// If the object used is a IDataReader, it need to close the reader only.
        ///// </summary>
        //public void CloseConnection()
        //{
        //    //if (this.connection.State != ConnectionState.Closed)//不会异常
        //    this._connection.Close();
        //}

        ///// <summary>
        ///// Begin a transaction.
        ///// </summary>
        ///// <returns></returns>
        //public IDbTransaction BeginTransaction()
        //{
        //    //if (this._connection == null)
        //    //{
        //    //    throw new InvalidOperationException("connection is null.");
        //    //}
        //    //if (this._connection.State == ConnectionState.Closed)
        //    //{
        //    //    throw new InvalidOperationException("connection is not opened.");
        //    //}
        //    this.MakeConnectionOpen();
        //    this._transaction = this._connection.BeginTransaction(IsolationLevel.ReadCommitted);
        //    return this._transaction;
        //}
        ///// <summary>
        ///// If business transaction is successed, this method should be invoked to commit changes to database.
        ///// </summary>
        //public void CommitTransaction()
        //{
        //    if (this._transaction != null)
        //    {
        //        try
        //        {
        //            this._transaction.Commit();
        //            this._transaction.Dispose();
        //            this._transaction = null;
        //        }
        //        catch { }
        //    }
        //}
        ///// <summary>
        ///// If business transaction is failed, this method should be invoked to rollback all changes in database.
        ///// </summary>
        //public void RollBackTransaction()
        //{
        //    if (this._transaction != null)
        //    {
        //        try
        //        {
        //            this._transaction.Rollback();
        //            this._transaction.Dispose();
        //            this._transaction = null;
        //        }
        //        catch { }
        //    }
        //}

        #endregion

        #region IDbAccesser Members

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        public DataTable QueryDataTable(string formattedSql, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                formattedSql = _converter.ConvertToSpecialDbSql(formattedSql);
                IDbDataParameter[] dbParameters = ConvertFormatParamaters(parameters);
                return this.DoQueryDataTable(formattedSql, CommandType.Text, dbParameters);
            }
            else
            {
                return this.DoQueryDataTable(formattedSql, CommandType.Text);
            }
        }

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="formattedSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        public DataRow QueryDataRow(string formattedSql, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                formattedSql = _converter.ConvertToSpecialDbSql(formattedSql);
                IDbDataParameter[] dbParameters = ConvertFormatParamaters(parameters);
                return this.DoQueryDataRow(formattedSql, CommandType.Text, dbParameters);
            }
            else
            {
                return this.DoQueryDataRow(formattedSql, CommandType.Text);
            }
        }

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        public LiteDataTable QueryLiteDataTable(string formattedSql, params object[] parameters)
        {
            using (var reader = this.QueryDataReader(formattedSql, parameters))
            {
                return ConvertToLiteDataTable(reader);
            }
        }

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="formattedSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        public LiteDataRow QueryLiteDataRow(string formattedSql, params object[] parameters)
        {
            using (var reader = this.QueryDataReader(formattedSql, parameters))
            {
                return ConvertToLiteDataRow(reader);
            }
        }

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        public IDataReader QueryDataReader(string formattedSql, params object[] parameters)
        {
            return this.QueryDataReader(formattedSql, this._connection.State == ConnectionState.Closed, parameters);
        }

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        public IDataReader QueryDataReader(string formattedSql, bool closeConnection, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                formattedSql = _converter.ConvertToSpecialDbSql(formattedSql);
                IDbDataParameter[] dbParameters = ConvertFormatParamaters(parameters);
                return this.DoQueryDataReader(formattedSql, CommandType.Text, closeConnection, dbParameters);
            }
            else
            {
                return this.DoQueryDataReader(formattedSql, CommandType.Text, closeConnection);
            }
        }

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>DBNull or value object.</returns>
        public object QueryValue(string formattedSql, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                formattedSql = _converter.ConvertToSpecialDbSql(formattedSql);
                IDbDataParameter[] dbParameters = ConvertFormatParamaters(parameters);
                return this.DoQueryValue(formattedSql, CommandType.Text, dbParameters);
            }
            else
            {
                return this.DoQueryValue(formattedSql, CommandType.Text);
            }
        }

        /// <summary>
        /// Execute a sql which is not a database procudure, return rows effected.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The number of rows effected</returns>
        public int ExecuteText(string formattedSql, params object[] parameters)
        {
            if (parameters.Length > 0)
            {
                formattedSql = _converter.ConvertToSpecialDbSql(formattedSql);
                IDbDataParameter[] dbParameters = ConvertFormatParamaters(parameters);
                return this.DoExecuteText(formattedSql, dbParameters);
            }
            else
            {
                return this.DoExecuteText(formattedSql);
            }
        }

        /// <summary>
        /// 此方法提供特定数据库的参数列表。
        /// </summary>
        /// <param name="parametersValues">formattedSql参数列表</param>
        /// <returns>数据库参数列表</returns>
        private IDbDataParameter[] ConvertFormatParamaters(object[] parametersValues)
        {
            IDbDataParameter[] dbParameters = new DbParameter[parametersValues.Length];
            string parameterName = null;
            for (int i = 0, l = parametersValues.Length; i < l; i++)
            {
                parameterName = _converter.GetParameterName(i);
                object value = parametersValues[i];

                //convert null value.
                if (value == null) { value = DBNull.Value; }
                IDbDataParameter param = (this as IDbParameterFactory).CreateParameter(parameterName, value, ParameterDirection.Input);
                dbParameters[i] = param;
            }
            return dbParameters;
        }

        #endregion

        #region Do Query/Execute

        public IDbCommand CreateCommand(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            return this.PrepareCommand(sql, type, parameters);
        }

        /// <summary>
        /// Prepare a command for communicate with database.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual IDbCommand PrepareCommand(string sql, CommandType type, IDbDataParameter[] parameters)
        {
            IDbCommand command = _factory.CreateCommand();
            command.Connection = _connection;
            command.CommandText = sql;
            command.CommandType = type;
            //如果连接中的过期时间不是默认值，那么命令也使用这个作为过期时间。
            if (_connection.ConnectionTimeout != 15)
            {
                command.CommandTimeout = _connection.ConnectionTimeout;
            }

            var tran = this.Transaction;
            if (tran != null) command.Transaction = tran;

            var pas = command.Parameters;
            for (int i = 0, c = parameters.Length; i < c; i++)
            {
                var p = parameters[i];
                pas.Add(p);
            }

            Logger.LogDbAccessed(sql, parameters, _connectionSchema, _connection);

            return command;
        }

        private DataTable DoQueryDataTable(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            IDbDataAdapter da = _factory.CreateDataAdapter();
            da.SelectCommand = this.PrepareCommand(sql, type, parameters);

            DataSet ds = new DataSet();
            try
            {
                this.MakeConnectionOpen();

                da.Fill(ds);

                return ds.Tables[0];
            }
            finally
            {
                this.MakeConnectionClose();
            }
        }

        private DataRow DoQueryDataRow(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            using (DataTable dt = this.DoQueryDataTable(sql, type, parameters))
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    return dt.Rows[0];
                }
                return null;
            }
        }

        private IDataReader DoQueryDataReader(string sql, CommandType type, bool closeConnection, params IDbDataParameter[] parameters)
        {
            IDbCommand cmd = this.PrepareCommand(sql, type, parameters);

            if (this._connection.State == ConnectionState.Closed) { this._connection.Open(); }

            IDataReader reader = null;

            if (closeConnection)
            {
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            else
            {
                reader = cmd.ExecuteReader();
            }

            return reader;
        }

        private object DoQueryValue(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            IDbCommand cmd = this.PrepareCommand(sql, type, parameters);

            try
            {
                this.MakeConnectionOpen();

                return cmd.ExecuteScalar();
            }
            finally
            {
                this.MakeConnectionClose();
            }
        }

        private int DoExecuteProcedure(string procedureName, out int rowsAffect, params IDbDataParameter[] parameters)
        {
            IDbCommand command = this.PrepareCommand(procedureName, CommandType.StoredProcedure, parameters);

            //返回值
            DbParameter paraReturn = this._factory.CreateParameter();
            paraReturn.DbType = DbType.Int32;
            paraReturn.Direction = ParameterDirection.ReturnValue;
            paraReturn.ParameterName = this._converter.ProcudureReturnParameterName;
            paraReturn.Size = 4;
            command.Parameters.Add(paraReturn);

            try
            {
                this.MakeConnectionOpen();

                rowsAffect = command.ExecuteNonQuery();
                return Convert.ToInt32(paraReturn.Value);
            }
            finally
            {
                this.MakeConnectionClose();
            }
        }

        private int DoExecuteText(string sql, params IDbDataParameter[] parameters)
        {
            IDbCommand command = this.PrepareCommand(sql, CommandType.Text, parameters);
            try
            {
                this.MakeConnectionOpen();

                return command.ExecuteNonQuery();
            }
            finally
            {
                this.MakeConnectionClose();
            }
        }

        private static LiteDataTable ConvertToLiteDataTable(IDataReader reader)
        {
            var table = new LiteDataTable();
            LiteDataTableAdapter.Fill(table, reader);
            return table;
        }

        private static LiteDataRow ConvertToLiteDataRow(IDataReader reader)
        {
            var dt = ConvertToLiteDataTable(reader);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        #endregion

        #region IDbParameterFactory Members

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter()
        {
            return _factory.CreateParameter();
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            return para;
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name, object value)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            para.Value = value;
            return para;
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name, object value, DbType type)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            para.DbType = type;
            para.Value = value;
            return para;
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name, object value, ParameterDirection direction)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            para.Value = value;
            para.Direction = direction;
            return para;
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name, object value, DbType type, int size)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            para.DbType = type;
            para.Size = size;
            para.Value = value;
            return para;
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name, object value, DbType type, ParameterDirection direction)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            para.DbType = type;
            para.Value = value;
            para.Direction = direction;
            return para;
        }

        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter IDbParameterFactory.CreateParameter(string name, object value, DbType type, int size, ParameterDirection direction)
        {
            IDbDataParameter para = _factory.CreateParameter();
            para.ParameterName = name;
            para.DbType = type;
            para.Value = value;
            para.Size = size;
            para.Direction = direction;
            return para;
        }

        #endregion

        #region IRawDbAccesser Members

        /// <summary>
        /// A factory to create parameters.
        /// </summary>
        IDbParameterFactory IRawDbAccesser.ParameterFactory
        {
            get { return this; }
        }

        /// <summary>
        /// A factory to create IDbCommand.
        /// </summary>
        IDbCommandFactory IRawDbAccesser.CommandFactory
        {
            get { return this; }
        }

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataTable IRawDbAccesser.QueryLiteDataTable(string sql, params IDbDataParameter[] parameters)
        {
            using (var reader = (this as IRawDbAccesser).QueryDataReader(sql, parameters))
            {
                return ConvertToLiteDataTable(reader);
            }
        }

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataTable IRawDbAccesser.QueryLiteDataTable(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            using (var reader = (this as IRawDbAccesser).QueryDataReader(sql, type, parameters))
            {
                return ConvertToLiteDataTable(reader);
            }
        }

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataRow IRawDbAccesser.QueryLiteDataRow(string sql, params IDbDataParameter[] parameters)
        {
            using (var reader = (this as IRawDbAccesser).QueryDataReader(sql, parameters))
            {
                return ConvertToLiteDataRow(reader);
            }
        }

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataRow IRawDbAccesser.QueryLiteDataRow(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            using (var reader = (this as IRawDbAccesser).QueryDataReader(sql, type, parameters))
            {
                return ConvertToLiteDataRow(reader);
            }
        }

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable IRawDbAccesser.QueryDataTable(string sql, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataTable(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable IRawDbAccesser.QueryDataTable(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataTable(sql, type, parameters);
        }

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow IRawDbAccesser.QueryDataRow(string sql, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataRow(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow IRawDbAccesser.QueryDataRow(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataRow(sql, type, parameters);
        }

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader IRawDbAccesser.QueryDataReader(string sql, bool closeConnection, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataReader(sql, CommandType.Text, closeConnection, parameters);
        }

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader IRawDbAccesser.QueryDataReader(string sql, CommandType type, bool closeConnection, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataReader(sql, type, closeConnection, parameters);
        }

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader IRawDbAccesser.QueryDataReader(string sql, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataReader(sql, CommandType.Text, this._connection.State == ConnectionState.Closed, parameters);
        }

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader IRawDbAccesser.QueryDataReader(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            return this.DoQueryDataReader(sql, type, this._connection.State == ConnectionState.Closed, parameters);
        }

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>DBNull or object.</returns>
        object IRawDbAccesser.QueryValue(string sql, params IDbDataParameter[] parameters)
        {
            return this.DoQueryValue(sql, CommandType.Text, parameters);
        }

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>DBNull or object.</returns>
        object IRawDbAccesser.QueryValue(string sql, CommandType type, params IDbDataParameter[] parameters)
        {
            return this.DoQueryValue(sql, type, parameters);
        }

        /// <summary>
        /// Execute a procudure, and return the value returned by this procedure
        /// </summary>
        /// <param name="procedureName">The name of this procedure</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The value returned by procedure</returns>
        int IRawDbAccesser.ExecuteProcedure(string procedureName, params IDbDataParameter[] parameters)
        {
            int rowsAffect;
            return this.DoExecuteProcedure(procedureName, out rowsAffect, parameters);
        }

        /// <summary>
        /// Execute a procudure, and return the value returned by this procedure
        /// </summary>
        /// <param name="procedureName">The name of this procedure</param>
        /// <param name="rowsAffect">The number of rows effected</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The value returned by procedure</returns>
        int IRawDbAccesser.ExecuteProcedure(string procedureName, out int rowsAffect, params IDbDataParameter[] parameters)
        {
            return this.DoExecuteProcedure(procedureName, out rowsAffect, parameters);
        }

        /// <summary>
        /// Execute a sql which is not a database procudure, return rows effected.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The number of rows effected</returns>
        int IRawDbAccesser.ExecuteText(string sql, params IDbDataParameter[] parameters)
        {
            return this.DoExecuteText(sql, parameters);
        }

        ///// <summary>
        ///// 根据查询语句给一个DataSet对象加一个表
        ///// </summary>
        ///// <param name="sql">给定的SQL语句</param>
        ///// <param name="type">如何解释命令字符串</param>
        ///// <param name="ds">要加表的DataSet</param>
        ///// <param name="tableName">赋于表名,可空</param>
        ///// <param name="parameters">有参数的SQL语句的参数集合</param>
        ///// <returns></returns>
        //public void FillDataSetNormal(string sql, CommandType type, DataSet ds, string tableName, params IDbDataParameter[] parameters)
        //{
        //    DbDataAdapter da = _factory.CreateDataAdapter();
        //    da.SelectCommand = this.PrepareCommand(sql, type, parameters);

        //    try
        //    {
        //        this.MakeConnectionOpen();

        //        if (string.IsNullOrEmpty(tableName) == false)
        //        {
        //            da.Fill(ds, tableName);
        //        }
        //        else
        //        {
        //            da.Fill(ds);
        //        }

        //        this.SingleOperationComplete();
        //    }
        //    catch
        //    {
        //        this.SingleOperationComplete();
        //        throw;
        //    }
        //}
        ///// <summary>
        ///// 根据查询语句给一个DataSet对象加一个表
        ///// </summary>
        ///// <param name="formattedSql">类似string.Format方法格式的sql语句</param>
        ///// <param name="type">如何解释命令字符串</param>
        ///// <param name="ds">要加表的DataSet</param>
        ///// <param name="tableName">赋于表名,可以为NULL</param>
        ///// <param name="parameters">有参数的SQL语句的参数集合,可以为NULL</param>
        ///// <returns></returns>
        //public void FillDataSet(string formattedSql, DataSet ds, string tableName, params object[] parameters)
        //{
        //    if (parameters.Length > 0)
        //    {
        //        formattedSql = _converter.ConvertToSpecialDbSql(formattedSql);
        //        DbParameter[] dbParameters = ConvertFormatParamaters(parameters);
        //        this.FillDataSetNormal(formattedSql, CommandType.Text, ds, tableName, dbParameters);
        //    }
        //    else
        //    {
        //        this.FillDataSetNormal(formattedSql, CommandType.Text, ds, tableName);
        //    }
        //}

        #endregion

        #region IDisposable Members

        ~DbAccesser()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// dispose this accesser.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (this._transaction != null)
                //{
                //    this._transaction.Dispose();
                //}
                if (this._connectionCreatedBySelf && this._connection != null)
                {
                    this._connection.Dispose();
                }
                this._connection = null;
                this._converter = null;
                this._factory = null;

                //this._transaction = null;
            }
        }

        #endregion

        ///// <summary>
        ///// A common paging query method(Notice: this method could be used only in sql server)
        ///// </summary>
        ///// <param name="queryStr">查询语句</param>
        ///// <param name="pageIndex">页面索引</param>
        ///// <param name="pageSize">页面大小</param>
        ///// <param name="fdShow">显示字段(可为String.Empty)</param>
        ///// <param name="fdOrder">排序表达式(可为String.Empty)</param>
        ///// <returns></returns>
        //public IDataReader PageQuery(string queryStr, PagerInfo pi, string fdShow, string fdOrder)
        //{
        //    //count
        //    if (pi != null && pi.IsNeedCount)
        //    {
        //        this.Open();
        //        //List<DbParameter> parameters = new List<DbParameter>();
        //        pi.TotalCount = Convert.ToInt32(
        //            this.GetUniqueValue("select count(*) from (" + queryStr + ") as t")
        //            );
        //    }
        //    return this.GetDataReaderNormal(
        //        "sp_Common_PagingQuery",
        //        CommandType.StoredProcedure,
        //        true,
        //        this.CreateParameter("@QueryStr", queryStr),
        //        this.CreateParameter("@PageSize", pi != null ? pi.PageSize : int.MaxValue),
        //        this.CreateParameter("@PageIndex", pi != null ? pi.PageIndex : 1),
        //        this.CreateParameter("@FdShow", fdShow),
        //        this.CreateParameter("@FdOrder", fdOrder)
        //        );
        //}
    }
}

//catch (SqlException e)
//{
//    switch (e.Number)
//    {
//        case 2601://主键冲突
//        case 2627://UNIQUE 约束
//        case 547://外键冲突
//            return -1;
//    }
//    throw e;
//}

/*************************sp_Common_PagingQuery存储过程**************************/
// CREATE Proc sp_Common_PagingQuery 
// @QueryStr nvarchar(4000), --表名、视图名、查询语句 
// @PageSize int, --每页的大小(行数) 
// @PageIndex int, --要显示的页 
// @FdShow nvarchar (4000)= ' ', --要显示的字段列表,如果查询结果不需要标识字段,需要指定此值,且不包含标识字段 
// @FdOrder nvarchar (1000)= ' ' --排序字段列表
// as 
// set nocount on 
// declare @FdName nvarchar(250) --表中的主键或表、临时表中的标识列名 
//        ,@Id1 varchar(20),@Id2 varchar(20) --开始和结束的记录号 
//        ,@Obj_ID int --对象ID 
// --表中有复合主键的处理
// declare @strfd nvarchar(2000) --复合主键列表 
//        ,@strjoin nvarchar(4000) --连接字段 
//        ,@strwhere nvarchar(2000) --查询条件 

// select @Obj_ID=object_id(@QueryStr) --返回数据库对象标识号。

//     ,@FdShow=case isnull(@FdShow, ' ') when ' ' then ' * ' else ' '+@FdShow end --没有指定,则全部选取  select * from 
//     ,@FdOrder=case isnull(@FdOrder, ' ') when ' ' then ' ' else ' order by '+@FdOrder end 
//     ,@QueryStr=case when @Obj_ID is not null then ' '+@QueryStr else ' ( '+@QueryStr+ ') a ' end 

// --如果显示第一页，可以直接用top来完成 
// if @PageIndex=1 
// begin 
//     select @Id1=cast(@PageSize as varchar(20)) 
//     exec( 'select top '+@Id1+@FdShow+ ' from '+@QueryStr+@FdOrder) 
//     return 
// end 

// --如果是表,则检查表中是否有标识更或主键 
// if @Obj_ID is not null and objectproperty(@Obj_ID, 'IsTable ')=1
// begin --是表,且有标识
//     select @Id1=cast(@PageSize as varchar(20))--页面大小
//        ,@Id2=cast((@PageIndex-1)*@PageSize as varchar(20)) --upbound

//     select @FdName=name from syscolumns where  id=@Obj_ID and status=0x80 -- status=0x80表示列允许空值

//     if @@rowcount=0 --如果表中无标识列,则检查表中是否有主键
//     begin 
//         if not exists(select 1 from sysobjects where parent_obj=@Obj_ID and xtype= 'PK ') 
//         GOTO lbUseTemp ---------------无主键,则用临时表处理 

//         select @FdName=name from syscolumns where id=@Obj_ID and colid in-----------????????看不懂

//         (
//             select colid from sysindexkeys where @Obj_ID=id and indid in
//             (
//                 select indid from sysindexes where @Obj_ID=id and name in
//                 (
//                    select name from sysobjects where xtype= 'PK ' and parent_obj=@Obj_ID 
//                 )
//             )
//         )
//         if @@rowcount >1 --检查表中的主键是否为复合主键 
//         begin 
//             select @strfd= ' ',@strjoin= ' ',@strwhere= ' '
//             select @strfd=@strfd+ ',[ '+name+ '] ' 
//                 ,@strjoin=@strjoin+ ' and a.[ '+name+ ']=b.[ '+name+ '] ' 
//                 ,@strwhere=@strwhere+ ' and b.[ '+name+ '] is null ' 
//                 from syscolumns where id=@Obj_ID and colid in
//                 ( 
//                     select colid from sysindexkeys where @Obj_ID=id and indid in
//                     ( 
//                         select indid from sysindexes where @Obj_ID=id and name in
//                         ( 
//                            select name from sysobjects where xtype= 'PK ' and parent_obj=@Obj_ID 
//                         )
//                     )
//                 ) 
//             select @strfd=substring(@strfd,2,2000) 
//                 ,@strjoin=substring(@strjoin,5,4000) 
//                 ,@strwhere=substring(@strwhere,5,4000) 
//             GOTO lbUsePk ---------------------
//         end 
//     end 
// end 
// else --不是表或无标识.用临时表处理
//    GOTO lbUseTemp 

// /*--使用标识列或主键为单一字段的处理方法--*/ 
// lbUseIdentity: 
// exec
// (
//    'select top '+@Id1+@FdShow+ ' from '+@QueryStr 
//     + ' where '+@FdName+ ' not in(select top ' 
//     +@Id2+ ' '+@FdName+ ' from '+@QueryStr+@FdOrder 
//     + ') '+@FdOrder 
// )
// return 

// /*--表中有复合主键的处理方法--*/ 
// lbUsePk: 
// exec
// (
//     'select '+@FdShow+ ' from(select top '+@Id1+ ' a.* from 
//     (select top 100 percent * from '+@QueryStr+@FdOrder+ ') a 
//     left join (select top '+@Id2+ ' '+@strfd+ ' 
//     from '+@QueryStr+@FdOrder+ ') b on '+@strjoin+ ' 
//     where '+@strwhere+ ') a ' 
// ) 
// return 

// /*--用临时表处理的方法--*/ 
// lbUseTemp: 
// select @FdName= '[ID_ '+cast(newid() as varchar(40))+ '] ' 
//     ,@Id1=cast(@PageSize*(@PageIndex-1) as varchar(20)) 
//     ,@Id2=cast(@PageSize*@PageIndex-1 as varchar(20)) 

// exec
// (
//      'select '+@FdName+ '=identity(int,0,1), '+@FdShow+ ' 
//     into #tb from(select top 100 percent * from '+@QueryStr+@FdOrder+ ')a 
//     select '+@FdShow+ ' from #tb where '+@FdName+ ' between ' 
//     +@Id1+ ' and '+@Id2 
// ) 
//GO