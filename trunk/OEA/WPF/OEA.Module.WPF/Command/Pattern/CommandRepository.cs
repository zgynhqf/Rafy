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
using System.Windows.Input;
using System.Collections.Generic;
using System.Diagnostics;
using OEA.WPF.Command;
using OEA;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;
using OEA.Module;

namespace OEA.WPF.Command
{
    public static class CommandRepository
    {
        public static UICommand NewUICommand(Type cmdType)
        {
            var cmdInfo = UIModel.WPFCommands.Find(cmdType);
            return NewUICommand(cmdInfo);
        }

        public static UICommand NewUICommand(string name)
        {
            var cmdInfo = UIModel.WPFCommands.Find(name);
            return NewUICommand(cmdInfo);
        }

        /// <summary>
        /// CreateRuntimeCommand
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static UICommand NewUICommand(WPFCommand meta)
        {
            var coreCommand = Activator.CreateInstance(meta.RuntimeType) as ClientCommand;

            coreCommand.CommandInfo = meta;

            var gestures = InputGestureParser.Parse(meta.Gestures);
            var command = new UICommand(coreCommand, typeof(CommandRepository), gestures);

            OnCommandCreated(coreCommand);

            return command;
        }

        public static event EventHandler<InstanceCreatedEventArgs<ClientCommand>> CommandCreated;

        private static void OnCommandCreated(ClientCommand cmd)
        {
            var handler = CommandCreated;
            if (handler != null) { handler(null, new InstanceCreatedEventArgs<ClientCommand>(cmd)); }
        }

        #region 通过命令名称来执行命令

        public static bool TryExecuteCommand(Type cmdId, object cmdArg)
        {
            var cmdInfo = UIModel.WPFCommands.Find(cmdId);
            return TryExecuteCommand(cmdInfo, cmdArg);
        }

        public static bool TryExecuteCommand(WPFCommand cmdInfo, object cmdArg)
        {
            if (cmdInfo == null) throw new ArgumentNullException("cmdInfo");

            var runtimeCmd = NewUICommand(cmdInfo).CoreCommand;

            return runtimeCmd.TryExecute(cmdArg);
        }

        #endregion

        #region CommandBindingCanExecute & CommandBindingExecuted

        internal static CommandBinding CreateCommandBinding(UICommand cmd)
        {
            return new CommandBinding(cmd, CommandBindingExecuted, CommandBindingCanExecute);
        }

        private static void CommandBindingCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            UICommand command = e.Command as UICommand;
            if (command == null) { return; }

            using (var commandContext = command.CreateCanExecuteContext(sender, e.OriginalSource, e.Parameter))
            {
                e.CanExecute = command.CanExecute(commandContext);
            }

            e.Handled = true;
        }

        private static void CommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            UICommand command = e.Command as UICommand;
            if (command == null) { return; }

            using (CommandContext commandContext =
                command.CreateCanExecuteContext(sender, e.OriginalSource, e.Parameter))
            {
                command.Execute(commandContext);
            }

            e.Handled = true;
        }

        #endregion
    }
}