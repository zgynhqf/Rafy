/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：运行时命令
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110215
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace OEA
{
    /// <summary>
    /// 运行时命令
    /// </summary>
    public interface IClientCommand
    {
        /// <summary>
        /// 是否这个命令所对应的按钮可以被执行
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        bool CanExecute(object param);

        /// <summary>
        /// 执行这个命令
        /// </summary>
        /// <param name="param"></param>
        void Execute(object param);

        /// <summary>
        /// 执行成功后的事件。
        /// </summary>
        event EventHandler<CommandExecutedArgs> Executed;

        /// <summary>
        /// 执行发生异常后的事件。
        /// </summary>
        event EventHandler<CommandExecuteFailedArgs> ExecuteFailed;
    }
}