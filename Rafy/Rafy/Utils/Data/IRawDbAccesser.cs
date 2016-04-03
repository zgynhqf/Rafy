/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130509
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130509 13:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// A db accesser which can use raw sql to communicate with data base.
    /// </summary>
    public interface IRawDbAccesser : IDisposable
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
        /// current avaiable transaction.
        /// this transaction is retrieved from <see cref="LocalTransactionBlock"/> class, and it is created by current connection.
        /// </summary>
        IDbTransaction Transaction { get; }

        /// <summary>
        /// 数据连接结构
        /// </summary>
        DbConnectionSchema ConnectionSchema { get; }

        /// <summary>
        /// A factory to create parameters.
        /// </summary>
        IDbParameterFactory ParameterFactory { get; }

        /// <summary>
        /// A factory to create database command.
        /// </summary>
        IDbCommandFactory CommandFactory { get; }

        #region NonQuery

        /// <summary>
        /// Execute a procudure, and return the value returned by this procedure
        /// </summary>
        /// <param name="procedureName">The name of this procedure</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The value returned by procedure</returns>
        int ExecuteProcedure(string procedureName, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute a procudure, and return the value returned by this procedure
        /// </summary>
        /// <param name="procedureName">The name of this procedure</param>
        /// <param name="rowsAffect">The number of rows effected</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The value returned by procedure</returns>
        int ExecuteProcedure(string procedureName, out int rowsAffect, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute a sql which is not a database procudure, return rows effected.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The number of rows effected</returns>
        int ExecuteText(string sql, params IDbDataParameter[] parameters);

        #endregion

        #region Query

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string sql, bool closeConnection, params IDbDataParameter[] parameters);

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
        IDataReader QueryDataReader(string sql, CommandType type, bool closeConnection, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string sql, params IDbDataParameter[] parameters);

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
        IDataReader QueryDataReader(string sql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataRow QueryLiteDataRow(string sql, params IDbDataParameter[] parameters);

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
        LiteDataRow QueryLiteDataRow(string sql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataTable QueryLiteDataTable(string sql, params IDbDataParameter[] parameters);

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
        LiteDataTable QueryLiteDataTable(string sql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow QueryDataRow(string sql, params IDbDataParameter[] parameters);

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
        DataRow QueryDataRow(string sql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable QueryDataTable(string sql, params IDbDataParameter[] parameters);

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
        DataTable QueryDataTable(string sql, CommandType type, params IDbDataParameter[] parameters);

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="sql">specific sql</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>DBNull or object.</returns>
        object QueryValue(string sql, params IDbDataParameter[] parameters);

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
        object QueryValue(string sql, CommandType type, params IDbDataParameter[] parameters);

        #endregion
    }
}