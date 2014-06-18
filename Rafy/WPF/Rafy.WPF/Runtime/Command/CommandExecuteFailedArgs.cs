/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100311
 * 
*******************************************************/

using System;
using System.ComponentModel;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 命令执行失败时的命令参数
    /// </summary>
    public class CommandExecuteFailedArgs : CancelEventArgs
    {
        public CommandExecuteFailedArgs(Exception exception, object param)
        {
            this.Exception = exception;

            this.Parameter = param;
        }

        /// <summary>
        /// 执行命令时，发生了这个异常
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// 发生错误时的参数
        /// </summary>
        public object Parameter { get; private set; }
    }

    /// <summary>
    /// 命令执行失败时的命令参数
    /// </summary>
    public class CommandExecutedArgs : CancelEventArgs
    {
        public CommandExecutedArgs(object param)
        {
            this.Parameter = param;
        }

        /// <summary>
        /// 命令参数
        /// </summary>
        public object Parameter { get; private set; }
    }
}
