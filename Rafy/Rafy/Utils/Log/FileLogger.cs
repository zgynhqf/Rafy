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
            this.ExceptionLogFileName = "ExceptionLog.txt";
            this.SqlTraceFileName = ConfigurationHelper.GetAppSettingOrDefault("Rafy.FileLogger.SqlTraceFileName", string.Empty);
        }

        /// <summary>
        /// 错误日志的文件名。
        /// 默认为：ExceptionLog.txt。
        /// </summary>
        public string ExceptionLogFileName { get; set; }

        /// <summary>
        /// 默认使用配置文件中的 Rafy.FileLogger.SqlTraceFileName 配置项。
        /// </summary>
        public string SqlTraceFileName { get; set; }

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
        /// 记录某个已经生成的异常到文件中。
        /// </summary>
        /// <param name="title">异常对应的标题，用于描述当前异常的信息。</param>
        /// <param name="e"></param>
        public override void LogError(string title, Exception e)
        {
            if (!string.IsNullOrEmpty(this.ExceptionLogFileName))
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
                File.AppendAllText(this.ExceptionLogFileName, message);
            }
        }

        /// <summary>
        /// 记录 Sql 执行过程。
        /// 把 SQL 语句及参数，写到 'Rafy.FileLogger.SqlTraceFileName' 配置所对应的文件中。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionSchema"></param>
        /// <param name="connection"></param>
        public override void LogDbAccessed(string sql, IDbDataParameter[] parameters, DbConnectionSchema connectionSchema, IDbConnection connection)
        {
            if (!string.IsNullOrEmpty(this.SqlTraceFileName))
            {
                var content = new StringBuilder();

                var sqlConnection = connection as SqlConnection;
                content.Append("--").Append(DateTime.Now);
                content.AppendLine();
                if (sqlConnection != null)
                {
                    content.Append("--ClientConnectionId: ").Append(sqlConnection.ClientConnectionId);
                    content.AppendLine();
                }
                //"--Database:  " + connectionSchema.Database +
                content.Append("--ConnectionString: ").Append(connectionSchema.ConnectionString);
                content.AppendLine();

                if (parameters.Length > 0)
                {
                    this.WriteSqlAndParameters(content, sql, parameters, connectionSchema);
                }
                else
                {
                    content.Append(sql);
                }

                content.AppendLine().AppendLine().AppendLine();

                File.AppendAllText(SqlTraceFileName, content.ToString(), Encoding.UTF8);
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

            var pValues = parameters.Select(p =>
            {
                var value = p.Value;
                if (p.DbType == DbType.DateTime)
                {
                    if (isOracle)
                    {
                        value = string.Format("to_date('{0}', 'dd-mm-yyyy hh24:mi:ss')", value);
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
                else if(p.DbType == DbType.Boolean)
                {
                    value = Convert.ToByte(value);
                }
                else if(value == null)
                {
                    value = "null";
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
                content.AppendLine();
                content.Append("Parameters:").Append(string.Join(",", pValues));
            }
        }
    }
}