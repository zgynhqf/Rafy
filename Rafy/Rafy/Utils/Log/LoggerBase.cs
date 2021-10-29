/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130314 14:42
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130314 14:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy
{
    /// <summary>
    /// 日志记录器。
    /// </summary>
    public abstract class LoggerBase
    {
        /// <summary>
        /// 记录某个消息到 Log 日志中。
        /// </summary>
        /// <param name="message"></param>
        public virtual void LogInfo(string message) { }

        /// <summary>
        /// 记录某个已经生成的异常到文件中。
        /// </summary>
        /// <param name="title"></param>
        /// <param name="e"></param>
        public virtual void LogError(string title, Exception e) { }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="result">
        /// Sql 执行后的结果。
        /// 如果是查询，可能是：DataSet、DataTable、DataRow、DataReader；
        /// 如果是非查询，返回的是受影响的行数。
        /// 如果发生了异常，返回的是异常对象。
        /// </param>
        /// <param name="connectionSchema">The connection schema.</param>
        /// <param name="connection">The connection.</param>
        public virtual void LogDbAccessed(string sql, IDbDataParameter[] parameters, object result, DbConnectionSchema connectionSchema, IDbConnection connection) { }
    }
}