﻿/*******************************************************
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
using System.Linq;
using System.Text;

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
                    try
                    {
                        _sqlTraceEnabled = System.IO.File.Exists(@"C:\OEA_SQL_TRACE_ENABLED");
                    }
                    catch
                    {
                        _sqlTraceEnabled = false;
                    }
                }

                return _sqlTraceEnabled.Value;
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Trace(string sql, IDbDataParameter[] parameters)
        {
            if (SqlTraceEnabled)
            {
                if (parameters.Length > 0)
                {
                    sql += Environment.NewLine + "Parameters:" + string.Join(",", parameters.Select(p => p.Value));
                }

                try
                {
                    System.IO.File.AppendAllText(
                        @"C:\SQLTraceLog.txt",
                        DateTime.Now + "  " + sql + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine,
                        Encoding.UTF8
                        );
                }
                catch { }
            }
        }
    }
}