using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// 数据访问事件参数。
    /// </summary>
    public class DbAccessEventArgs
    {
        /// <summary>
        /// 执行的 Sql
        /// </summary>
        public string Sql { get; internal set; }

        /// <summary>
        /// 所有的参数值。
        /// </summary>
        public IDbDataParameter[] Parameters { get; internal set; }

        /// <summary>
        /// 对应的数据库连接
        /// </summary>
        public DbConnectionSchema ConnectionSchema { get; internal set; }
    }

    /// <summary>
    /// 数据访问事件参数。
    /// </summary>
    public class DbAccessedEventArgs : DbAccessEventArgs
    {
        /// <summary>
        /// Sql 执行后的结果。
        /// 如果是查询，可能是：DataSet、DataTable、DataRow、DataReader；
        /// 如果是非查询，返回的是受影响的行数。
        /// </summary>
        public object Result { get; internal set; }
    }
}