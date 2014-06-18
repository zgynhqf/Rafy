/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 14:00
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 14:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Rafy.DomainModeling.Controls;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 可执行的设计器命令抽象类
    /// </summary>
    public abstract class DesignerCommand : IDesignerCommand
    {
        /// <summary>
        /// 检查设计器状态，返回当前是否可执行本命令。
        /// </summary>
        /// <param name="designer"></param>
        /// <returns></returns>
        public bool CanExecute(ModelingDesigner designer)
        {
            return this.CanExecuteCore(designer);
        }

        /// <summary>
        /// 执行本命令。
        /// </summary>
        /// <param name="designer"></param>
        public void Execute(ModelingDesigner designer)
        {
            this.ExecuteCore(designer);
        }

        /// <summary>
        /// 检查设计器状态，返回当前是否可执行本命令。
        /// </summary>
        /// <param name="designer"></param>
        /// <returns></returns>
        protected virtual bool CanExecuteCore(ModelingDesigner designer)
        {
            return true;
        }

        /// <summary>
        /// 执行本命令。
        /// </summary>
        /// <param name="designer"></param>
        protected abstract void ExecuteCore(ModelingDesigner designer);
    }
}