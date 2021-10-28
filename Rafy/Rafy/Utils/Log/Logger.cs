/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Data;
using Rafy.Data;

namespace Rafy
{
    /// <summary>
    /// 一个简单的日志记录类。
    /// 
    /// 目前只有处理异常的方法。
    /// </summary>
    public static class Logger
    {
        private static LoggerBase _impl = new FileLogger();

        /// <summary>
        /// 使用具体的日志记录器来接管本 API。
        /// </summary>
        /// <param name="loggerImpl"></param>
        public static void SetImplementation(LoggerBase loggerImpl)
        {
            _impl = loggerImpl;
        }

        /// <summary>
        /// 记录某个已经生成的异常到文件中。
        /// </summary>
        /// <param name="title">异常对应的标题，用于描述当前异常的信息。</param>
        /// <param name="e"></param>
        public static void LogError(string title, Exception e)
        {
            try
            {
                _impl.LogError(title, e);
            }
            catch { }
        }

        /// <summary>
        /// 记录某个消息到 Log 日志中。
        /// </summary>
        /// <param name="message">要记录的消息。</param>
        public static void LogInfo(string message)
        {
            try
            {
                _impl.LogInfo(message);
            }
            catch { }
        }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="result">The result.</param>
        /// <param name="connectionSchema">The connection schema.</param>
        /// <param name="connection">The connection.</param>
        public static void LogDbAccessed(string sql, IDbDataParameter[] parameters, object result, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            try
            {
                _impl.LogDbAccessed(sql, parameters, result, connectionSchema, connection);
            }
            catch { }
        }
    }
}