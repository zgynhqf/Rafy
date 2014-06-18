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
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Rafy;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Command;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Command
{
    public static class CommandRepository
    {
        //public static ClientCommand NewCommand(Type cmdType)
        //{
        //    var cmdInfo = UIModel.WPFCommands.Find(cmdType);
        //    return NewCommand(cmdInfo);
        //}

        //public static ClientCommand NewCommand(string name)
        //{
        //    var cmdInfo = UIModel.WPFCommands.Find(name);
        //    return NewCommand(cmdInfo);
        //}

        internal static ClientCommand CreateCommand(WPFCommand meta, object cmdArg)
        {
            var command = Activator.CreateInstance(meta.RuntimeType) as ClientCommand;

            command.Meta = meta;
            command.Parameter = cmdArg;
            command.Label = meta.Label.Translate();

            //元数据中所有的扩展属性都会拷贝到 ClientCommand 上。
            command.CopyExtendedProperties(meta);

            var gestures = InputGestureParser.Parse(meta.Gestures);
            command.UICommand = new UICommand(command, typeof(CommandRepository), gestures);

            command.NotifyCreated();

            OnCommandCreated(command);

            return command;
        }

        /// <summary>
        /// 系统中所有命令在生成完毕后，都会触发此事件。
        /// </summary>
        public static event EventHandler<InstanceEventArgs<ClientCommand>> CommandCreated;

        private static void OnCommandCreated(ClientCommand cmd)
        {
            var handler = CommandCreated;
            if (handler != null) { handler(null, new InstanceEventArgs<ClientCommand>(cmd)); }
        }

        #region 直接执行命令

        /// <summary>
        /// 直接生成某个命令，并即刻执行。
        /// </summary>
        /// <param name="cmdId"></param>
        /// <param name="cmdArg"></param>
        /// <returns></returns>
        public static bool TryExecuteCommand(Type cmdId, object cmdArg)
        {
            var cmdInfo = UIModel.WPFCommands.Find(cmdId);
            return TryExecuteCommand(cmdInfo, cmdArg);
        }

        /// <summary>
        /// 直接生成某个命令，并即刻执行。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="cmdArg"></param>
        /// <returns></returns>
        public static bool TryExecuteCommand(WPFCommand meta, object cmdArg)
        {
            if (meta == null) throw new ArgumentNullException("cmdInfo");

            var cmd = CreateCommand(meta, cmdArg);

            return cmd.TryExecute();
        }

        #endregion

        #region CommandBindingCanExecute & CommandBindingExecuted

        internal static CommandBinding CreateCommandBinding(ClientCommand cmd)
        {
            return new CommandBinding(cmd.UICommand, CommandBindingExecuted, CommandBindingCanExecute);
        }

        private static void CommandBindingCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var command = e.Command as UICommand;
            if (command == null) { return; }

            e.CanExecute = command.ClientCommand.CanExecute();

            e.Handled = true;
        }

        private static void CommandBindingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var command = e.Command as UICommand;
            if (command == null) { return; }

            //在命令执行前，先尝试焦点设置到生成的控件上。
            //这样做的主要原因是：当焦点从输入控件移动到按钮上时，输入控件可以强制进行验证。
            //具体场景：WMS 弹出选择编辑器，在手动输入值后，按快捷键 F5 进行查询，这时应该先把输入的值转换为引用实体再进行查询。
            command.ClientCommand.UIElements.Any(element => element.Focus());
            command.ClientCommand.Execute();

            e.Handled = true;
        }

        #endregion
    }
}