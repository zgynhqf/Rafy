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

namespace OEA.Module.WPF
{
    /// <summary>
    /// 一个简单的错误日志记录类
    /// </summary>
    public static class Logger
    {
        private static readonly string FileName = "Log.txt";

        public static void LogError(string title, Exception e)
        {
            string message = string.Format(@"
===================================================================
========{0}=========
===================================================================
记录时间：{4}
线程ID:[ {3} ]
错误描述：{1}

{2}

", title, e.Message, e.StackTrace, Thread.CurrentThread.ManagedThreadId, DateTime.Now);
            try
            {
                File.AppendAllText(FileName, message);
            }
            catch { }
        }
    }
}