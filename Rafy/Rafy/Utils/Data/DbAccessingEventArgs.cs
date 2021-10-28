using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// 数据访问事件参数。
    /// </summary>
    public struct DbAccessingEventArgs
    {
        public DbAccessingEventArgs(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema)
        {
            this.Sql = sql;
            this.Parameters = parameters;
            this.ConnectionSchema = connectionSchema;
        }

        /// <summary>
        /// 执行的 Sql
        /// </summary>
        public string Sql { get; private set; }

        /// <summary>
        /// 所有的参数值。
        /// </summary>
        public IDbDataParameter[] Parameters { get; private set; }

        /// <summary>
        /// 对应的数据库连接
        /// </summary>
        public DbConnectionSchema ConnectionSchema { get; private set; }
    }
}