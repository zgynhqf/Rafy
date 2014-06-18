/*******************************************************
 * 
 * 作者：http://www.codeproject.com/Articles/25445/WPF-Command-Pattern-Applied
 * 创建时间：周金根 2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 2.0 胡庆访 20120518
 * 
*******************************************************/

using System;
using System.Runtime;
using System.Windows;
using System.Windows.Input;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 使用 ClientCommand 来实现的 WPF 路由命令对象。
    /// </summary>
    public class UICommand : RoutedUICommand
    {
        private readonly ClientCommand _command;

        internal UICommand(ClientCommand command, Type ownerType, InputGestureCollection gestures) :
            base(command.Meta.Label ?? string.Empty, command.Meta.Name, ownerType, gestures)
        {
            if (command == null) { throw new ArgumentNullException("command"); }

            this._command = command;
        }

        /// <summary>
        /// 基于这个 ClientCommand 来实现路由命令。
        /// </summary>
        public ClientCommand ClientCommand
        {
            get { return this._command; }
        }
    }
}