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
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        /// 构造一个默认的文件记录器。
        /// </summary>
        public FileLogger()
        {
            this.InfoLogFileName = ConfigurationHelper.GetAppSettingOrDefault($"Rafy.FileLogger.InfoLogFileName", "ApplicationInfo.log");
            this.ExceptionLogFileName = ConfigurationHelper.GetAppSettingOrDefault($"Rafy.FileLogger.ExceptionLogFileName", "Exception.log");
            this.SqlTraceFileName = ConfigurationHelper.GetAppSettingOrDefault($"Rafy.FileLogger.SqlTraceFileName", string.Empty);
        }

        /// <summary>
        /// 常用信息的日志的文件名。
        /// 默认使用配置：Rafy.FileLogger.InfoLogFileName(default:ApplicationInfo.log)。
        /// </summary>
        public string InfoLogFileName { get; set; }

        /// <summary>
        /// 错误日志的文件名。
        /// 默认使用配置：Rafy.FileLogger.ExceptionLogFileName(default:Exception.log)。
        /// </summary>
        public string ExceptionLogFileName { get; set; }

        /// <summary>
        /// 默认使用配置文件中的 Rafy:FileLogger:SqlTraceFileName 配置项
        ///   NetFramework 分隔符为“.”，NetCore 分隔符为“:”
        /// </summary>
        public string SqlTraceFileName { get; set; }

        /// <summary>
        /// 是否需要将 Info 的内容同时输出到 Console。默认为 false。
        /// </summary>
        public bool WriteConcole { get; set; }

        /// <summary>
        /// 把所有的参数嵌入到 Sql 语句中。（方便开发者粘贴并调试）
        /// 默认为 true。
        /// </summary>
        public bool EmbadParameters { get; set; } = true;

        ///// <summary>
        ///// 是否只输出 Sql，忽略其它的信息。（方便开发者生成一个 .SQL 文件）
        ///// 默认为 false。
        ///// </summary>
        //public bool WriteSqlOnly { get; set; }

        /// <summary>
        /// 记录某个消息到 Log 日志中。
        /// </summary>
        /// <param name="message"></param>
        public override void LogInfo(string message)
        {
            this.WriteInfo(this.InfoLogFileName, $@"{DateTime.Now.ToString("MMdd HH:mm:ss.fff")}: {message}");
        }

        /// <summary>
        /// 记录某个已经生成的异常到文件中。
        /// </summary>
        /// <param name="title">异常对应的标题，用于描述当前异常的信息。</param>
        /// <param name="e"></param>
        public override void LogException(string title, Exception e)
        {
            LogInfo(title);

            if (string.IsNullOrEmpty(this.ExceptionLogFileName)) return;

            var stackTrace = e.StackTrace;//使用最外层的 Exception，可以获取到最完整的堆栈信息。
            e = e.GetBaseException();

            this.WriteInfo(this.ExceptionLogFileName, 
$@"=== {title} ===
记录时间：{DateTime.Now}
Thread Id:[ {Thread.CurrentThread.ManagedThreadId} ]
Exception Message：{e.Message}
StackTrace：
{stackTrace}

");
        }

        private void WriteInfo(string fileName, string msg)
        {
            if (this.WriteConcole)
            {
                Console.WriteLine(msg);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                lock (this)
                {
                    File.AppendAllText(fileName, msg + Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// 把 SQL 语句及参数，写到 'Rafy:FileLogger:SqlTraceFileName' 配置所对应的文件中。
        ///   NetFramework 分隔符为“.”，NetCore 分隔符为“:”
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="result"></param>
        /// <param name="connectionSchema"></param>
        /// <param name="connection"></param>
        public override void LogDbAccessed(string sql, IDbDataParameter[] parameters, object result, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            if (string.IsNullOrEmpty(this.SqlTraceFileName)) return;

            var content = new StringBuilder();

            content.AppendLine().AppendLine().AppendLine()
                .Append("--").Append(DateTime.Now).AppendLine();

            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                content.Append("--ClientConnectionId: ").Append(sqlConnection.ClientConnectionId).AppendLine();
            }

            //"--Database:  " + connectionSchema.Database +
            content.Append("--ConnectionString: ").Append(connectionSchema.ConnectionString).AppendLine();

            if (parameters?.Length > 0)
            {
                this.WriteSqlAndParameters(content, sql, parameters, connectionSchema);
            }
            else
            {
                content.Append(sql);
            }

            content.Append(';');

            if (result is int)
            {
                content.AppendLine()
                    .Append("Rows affected: ").Append(result).Append(";");
            }
            else if (result is Exception)
            {
                content.AppendLine()
                    .Append("Exception occurred: ").Append((result as Exception).Message).Append(";");
            }

            lock (this)
            {
                File.AppendAllText(this.SqlTraceFileName, content.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// 把 Sql 和 参数写入到 content 中。
        /// </summary>
        /// <param name="content"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionSchema"></param>
        protected virtual void WriteSqlAndParameters(StringBuilder content, string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema)
        {
            var isOracle = DbConnectionSchema.IsOracleProvider(connectionSchema);

            //code is copied from SqlGenerator.

            var pValues = parameters.Select(p =>
            {
                var value = p.Value;
                if (value == null)
                {
                    value = "null";
                }
                else if (p.DbType == DbType.DateTime || p.DbType == DbType.Date)
                {
                    if (isOracle)
                    {
                        value = string.Format("to_date('{0}', 'yyyy-MM-dd hh24:mi:ss')", value);
                    }
                    else
                    {
                        value = '\'' + value.ToString() + '\'';
                    }
                }
                else if (value is string)
                {
                    value = '\'' + value.ToString() + '\'';
                    //value = '"' + value.ToString() + '"';
                }
                else if (p.DbType == DbType.Boolean)
                {
                    value = Convert.ToByte(value);
                }

                return value;
            }).ToArray();

            if (this.EmbadParameters)
            {
                var toReplace = isOracle ? @"\:" : "@";
                var formattedSql = Regex.Replace(sql, toReplace + @"p(?<number>\d+)", @"{${number}}");
                content.AppendFormat(formattedSql, pValues);
            }
            else
            {
                content.Append(sql);
            }

            if (pValues.Length > 0)
            {
                content.AppendLine();
                content.Append("Parameters:").Append(string.Join(",", pValues));
            }
        }
    }
}