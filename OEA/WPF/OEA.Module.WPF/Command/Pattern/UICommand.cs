/*******************************************************
 * 
 * 作者：http://www.codeproject.com/Articles/25445/WPF-Command-Pattern-Applied
 * 创建时间：周金根 2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 重新整理 胡庆访 20120518
 * 
*******************************************************/

using System;
using System.Runtime;
using System.Windows.Input;
using OEA.Module;

namespace OEA.WPF.Command
{
    public class UICommand : RoutedUICommand
    {
        private readonly ClientCommand _command;

        internal UICommand(ClientCommand command, Type ownerType, InputGestureCollection gestures) :
            base(command.CommandInfo.Label ?? string.Empty, command.Id, ownerType, gestures)
        {
            if (command == null) { throw new ArgumentNullException("command"); }

            this._command = command;
        }

        public ClientCommand CoreCommand
        {
            get { return this._command; }
        }

        public CommandContext CreateCanExecuteContext(object bindingSource, object commandSource, object commandParameter)
        {
            return this.OnCreateContext(bindingSource, commandSource, commandParameter);
        }

        public CommandContext CreateExecuteContext(object bindingSource, object commandSource, object commandParameter)
        {
            return this.OnCreateContext(bindingSource, commandSource, commandParameter);
        }

        public bool CanExecute(CommandContext commandContext)
        {
            return this._command.CanExecute(commandContext.CommandParameter);
        }

        public void Execute(CommandContext commandContext)
        {
            this._command.Execute(commandContext.CommandParameter);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private CommandContext OnCreateContext(object bindingSource, object commandSource, object commandParameter)
        {
            return new CommandContext
            {
                BindingSource = bindingSource,
                CommandSource = commandSource,
                CommandParameter = commandParameter,
            };
        }
    }
}