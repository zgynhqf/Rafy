/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 14:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 14:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Input;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 可用于 WPF 绑定的命令类型。
    /// </summary>
    public abstract class WPFDesignerCommand : DesignerCommand
    {
        /// <summary>
        /// 用于绑定的 WPF 命令。
        /// </summary>
        /// <returns></returns>
        public RoutedUICommand GetWPFCommand()
        {
            return this.GetWPFCommandCore();
        }

        /// <summary>
        /// 每个子类，需要指定自己的 WPF 命令名称。这样才可以用于绑定。
        /// </summary>
        protected virtual RoutedUICommand GetWPFCommandCore()
        {
            var field = this.GetType().GetField("WPFCommand", BindingFlags.Static | BindingFlags.Public);
            if (field == null) throw new NotImplementedException("请在子类中定义名为 WPFCommand、类型为 RoutedUICommand 的静态公有字段，否则请重写此方法。");

            return field.GetValue(null) as RoutedUICommand;
        }
    }
}
