/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130314 14:41
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130314 14:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Rafy;
using Rafy.Data;

namespace Rafy
{
    /// <summary>
    /// 一个默认使用文件进行记录的日志器。
    /// </summary>
    public class FileLogger : LoggerBase
    {
        /// <summary>
        /// 错误日志的文件名。
        /// </summary>
        public static readonly string FileName = "ExceptionLog.txt";

        public override void LogError(string title, Exception e)
        {
            var stackTrace = e.StackTrace;//需要记录完整的堆栈信息。
            e = e.GetBaseException();

            string message = string.Format(@"
===================================================================
======== {0} =========
===================================================================
记录时间：{4}
线程ID:[ {3} ]
错误描述：{1}

{2}

", title, e.Message, stackTrace, Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            File.AppendAllText(FileName, message);
        }

        private string _sqlTraceFile;

        /// <summary>
        /// 记录 Sql 执行过程。
        /// 
        /// 把 SQL 语句及参数，写到 'Rafy.FileLogger.SqlTraceFileName' 配置所对应的文件中。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionSchema"></param>
        public override void LogDbAccessed(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema)
        {
            if (_sqlTraceFile == null)
            {
                _sqlTraceFile = ConfigurationHelper.GetAppSettingOrDefault("Rafy.FileLogger.SqlTraceFileName", string.Empty);
                if (_sqlTraceFile.Length == 0)
                {
                    _sqlTraceFile = ConfigurationHelper.GetAppSettingOrDefault("SQL_TRACE_FILE", string.Empty);
                }
            }

            if (_sqlTraceFile.Length > 0)
            {
                var content = sql;

                if (parameters.Length > 0)
                {
                    var pValues = parameters.Select(p =>
                    {
                        var value = p.Value;
                        if (value is string)
                        {
                            value = '"' + value.ToString() + '"';
                        }
                        return value;
                    });
                    content += Environment.NewLine + "Parameters:" + string.Join(",", pValues);
                }

                content = DateTime.Now +
                    //"\r\nDatabase:  " + connectionSchema.Database +
                    "\r\nConnectionString:  " + connectionSchema.ConnectionString +
                    "\r\n" + content + "\r\n\r\n\r\n";

                File.AppendAllText(_sqlTraceFile, content, Encoding.UTF8);
            }
        }
    }
}