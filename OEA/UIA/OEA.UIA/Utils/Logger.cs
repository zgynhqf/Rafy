/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111201
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.UIA.Utils
{
    /// <summary>
    /// 日志记录器
    /// </summary>
    public static class Logger
    {
        private static ILogger _logger;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void LogLine(string message)
        {
            if (_logger != null) _logger.LogLine(message);
            else Console.WriteLine(message);
        }
    }

    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger
    {
        void LogLine(string message);
    }
}