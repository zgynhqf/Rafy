/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130523
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130523 10:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 使用 ConnectionManager 管理链接的数据库访问器。
    /// </summary>
    internal class ManagedConnectionDbAccesser : IDbAccesser
    {
        private IConnectionManager _connectionManager;

        private DbAccesser _dba;

        internal ManagedConnectionDbAccesser(DbSetting dbSetting)
        {
            if (dbSetting == null) throw new ArgumentNullException("dbSetting");

            _connectionManager = TransactionDependentConnectionManager.GetManager(dbSetting);

            _dba = new DbAccesser(dbSetting, _connectionManager.Connection);
        }

        public IDbConnection Connection
        {
            get { return _dba.Connection; }
        }

        public DbConnectionSchema ConnectionSchema
        {
            get { return _dba.ConnectionSchema; }
        }

        public IRawDbAccesser RawAccesser
        {
            get { return _dba.RawAccesser; }
        }

        public int ExecuteText(string formatSql, params object[] parameters)
        {
            return _dba.ExecuteText(formatSql, parameters);
        }

        public object QueryValue(string formatSql, params object[] parameters)
        {
            return _dba.QueryValue(formatSql, parameters);
        }

        public IDataReader QueryDataReader(string formatSql, params object[] parameters)
        {
            return _dba.QueryDataReader(formatSql, parameters);
        }

        public IDataReader QueryDataReader(string formatSql, bool closeConnection, params object[] parameters)
        {
            return _dba.QueryDataReader(formatSql, closeConnection, parameters);
        }

        public DataRow QueryDataRow(string formatSql, params object[] parameters)
        {
            return _dba.QueryDataRow(formatSql, parameters);
        }

        public DataTable QueryDataTable(string formatSql, params object[] parameters)
        {
            return _dba.QueryDataTable(formatSql, parameters);
        }

        public LiteDataRow QueryLiteDataRow(string formatSql, params object[] parameters)
        {
            return _dba.QueryLiteDataRow(formatSql, parameters);
        }

        public LiteDataTable QueryLiteDataTable(string formatSql, params object[] parameters)
        {
            return _dba.QueryLiteDataTable(formatSql, parameters);
        }

        #region Dispose Pattern

        ~ManagedConnectionDbAccesser()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connectionManager.Dispose();
                _dba.Dispose();
            }
        }

        #endregion
    }
}
