using System;
using System.Data;
namespace hxy.Common.Data
{
    public interface IDBAccesser : IDisposable
    {
        //#region Control connection/transaction

        ///// <summary>
        ///// Begin a transaction.
        ///// </summary>
        ///// <returns></returns>
        //IDbTransaction BeginTransaction();
        ///// <summary>
        ///// If business transaction is failed, this method should be invoked to rollback all changes in database.
        ///// </summary>
        //void RollBackTransaction();
        ///// <summary>
        ///// If business transaction is successed, this method should be invoked to commit changes to database.
        ///// </summary>
        //void CommitTransaction();

        ///// <summary>
        ///// Invoke this method, if there will be many query.
        ///// </summary>
        //void OpenConnection();
        ///// <summary>
        ///// If "Open" method is invoked before, this method should be invoked too.
        ///// If the object used is a IDataReader, it need to close the reader only.
        ///// </summary>
        //void CloseConnection();

        //#endregion

        /// <summary>
        /// The underlying db connection
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 数据连接结构
        /// </summary>
        DbConnectionSchema ConnectionSchema { get; }

        #region NonQuery

        /// <summary>
        /// Execute a procudure, and return the value returned by this procedure
        /// </summary>
        /// <param name="procedureName">The name of this procedure</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The value returned by procedure</returns>
        int ExecuteProcedureNormal(string procedureName, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute a procudure, and return the value returned by this procedure
        /// </summary>
        /// <param name="procedureName">The name of this procedure</param>
        /// <param name="rowsAffect">The number of rows effected</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The value returned by procedure</returns>
        int ExecuteProcedureNormal(string procedureName, out int rowsAffect, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute a sql which is not a database procudure, return rows effected.
        /// </summary>
        /// <param name="formatSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The number of rows effected</returns>
        int ExecuteText(string formatSql, params object[] parameters);

        /// <summary>
        /// Execute a sql which is not a database procudure, return rows effected.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The number of rows effected</returns>
        int ExecuteTextNormal(string strSql, params IDbDataParameter[] parameters);

        #endregion

        #region Query

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="formatSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string formatSql, params object[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="formatSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string formatSql, bool closeConnection, params object[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReaderNormal(string strSql, bool closeConnection, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReaderNormal(string strSql, CommandType type, bool closeConnection, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReaderNormal(string strSql, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReaderNormal(string strSql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow QueryDataRow(string strSql, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow QueryDataRowNormal(string strSql, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow QueryDataRowNormal(string strSql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="formatSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable QueryDataTable(string formatSql, params object[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable QueryDataTableNormal(string strSql, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable QueryDataTableNormal(string strSql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="formatSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        object QueryValue(string formatSql, params object[] parameters);

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        object QueryValueNormal(string strSql, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="strSql">specific sql</param>
        /// <param name="type">
        /// Indicates or specifies how the System.Data.IDbCommand.CommandText property
        /// is interpreted.
        /// </param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        object QueryValueNormal(string strSql, CommandType type, params IDbDataParameter[] parameters);

        #endregion

        /// <summary>
        /// A factory to create parameters.
        /// </summary>
        IParameterFactory ParameterFactory { get; }
    }
}
