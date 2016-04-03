/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2007
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2007
 * 
*******************************************************/

using System;
using System.Data;
namespace Rafy.Data
{
    /// <summary>
    /// A db accesser which can use formatted sql to communicate with data base.
    /// </summary>
    public interface IDbAccesser : IDisposable
    {
        /// <summary>
        /// The underlying db connection
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// 数据连接结构
        /// </summary>
        DbConnectionSchema ConnectionSchema { get; }

        /// <summary>
        /// Gets a raw accesser which is oriented to raw sql and <c>IDbDataParameter</c>。
        /// </summary>
        IRawDbAccesser RawAccesser { get; }

        /// <summary>
        /// Execute a sql which is not a database procudure, return rows effected.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>The number of rows effected.</returns>
        int ExecuteText(string formattedSql, params object[] parameters);

        /// <summary>
        /// Execute the sql, and return the element of first row and first column, ignore the other values.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns>DBNull or value object.</returns>
        object QueryValue(string formattedSql, params object[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string formattedSql, params object[] parameters);

        /// <summary>
        /// Query out some data from database.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="closeConnection">Indicates whether to close the corresponding connection when the reader is closed?</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        IDataReader QueryDataReader(string formattedSql, bool closeConnection, params object[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataRow QueryDataRow(string formattedSql, params object[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        DataTable QueryDataTable(string formattedSql, params object[] parameters);

        /// <summary>
        /// Query out a row from database.
        /// If there is not any records, return null.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataRow QueryLiteDataRow(string formattedSql, params object[] parameters);

        /// <summary>
        /// Query out a DataTable object from database by the specific sql.
        /// </summary>
        /// <param name="formattedSql">a formatted sql which format looks like the parameter of String.Format</param>
        /// <param name="parameters">If this sql has some parameters, these are its parameters.</param>
        /// <returns></returns>
        LiteDataTable QueryLiteDataTable(string formattedSql, params object[] parameters);
    }
}