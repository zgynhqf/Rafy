/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110421
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110421
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Common;

namespace hxy.Common.Data
{
    /// <summary>
    /// 用于测试SQL
    /// </summary>
    public static class SQLTrace
    {
        private static bool? _sqlTraceEnabled;

        private static bool SqlTraceEnabled
        {
            get
            {
                if (_sqlTraceEnabled == null)
                {
                    _sqlTraceEnabled = ConfigurationHelper.GetAppSettingOrDefault("SQL_TRACE_ENABLED", false);
                }

                return _sqlTraceEnabled.Value;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Trace(string sql, IDbDataParameter[] parameters, IDbConnection connection)
        {
            if (SqlTraceEnabled)
            {
                var content = sql;

                if (parameters.Length > 0)
                {
                    content += Environment.NewLine + "Parameters:" + string.Join(",", parameters.Select(p => p.Value));
                }

                content = DateTime.Now + "\r\nDatabase:  " + connection.Database + "\r\n" + content + "\r\n\r\n\r\n";

                try
                {
                    File.AppendAllText(@"C:\SQLTraceLog.txt", content, Encoding.UTF8);
                }
                catch { }
            }
        }
    }
}
