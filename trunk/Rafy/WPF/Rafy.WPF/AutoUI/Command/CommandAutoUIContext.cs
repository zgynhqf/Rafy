/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：命令生成子系统的生成上下文
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rafy.WPF.Command;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 命令生成子系统的生成上下文
    /// 
    /// <remarks>
    /// 在整个生成过程中，每一个 GroupGenerator 可以直接把生成的控件以以下几种方式添加到 Context 中：
    /// 1. 直接添加到 ContainerItems 中。
    /// 2. 直接添加到 ContextMenuItems 中。
    /// 3. 使用 RegisterGroupedContainerItem 添加经过分组的控件，这些控件会被最终使用分隔符添加到 ContainerItems 中（见：AddGroupedItems 方法）。
    /// </remarks>
    /// </summary>
    public class CommandAutoUIContext
    {
        private ItemsControl _commandsContainer;
        private object _commandArg;
        private IEnumerable<WPFCommand> _commands;
        private Dictionary<WPFCommand, ClientCommand> _clientCommands;
        private Dictionary<int, IList<FrameworkElement>> _groupedCommands;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="commandsContainer">为这个Toolbar进行生成子项</param>
        /// <param name="commandArg"></param>
        /// <param name="commands"></param>
        public CommandAutoUIContext(ItemsControl commandsContainer, object commandArg, IEnumerable<WPFCommand> commands)
        {
            _commandsContainer = commandsContainer;
            _commandArg = commandArg;
            _commands = commands;

            _groupedCommands = new Dictionary<int, IList<FrameworkElement>>();
            _clientCommands = new Dictionary<WPFCommand, ClientCommand>();

            foreach (var meta in commands)
            {
                _clientCommands[meta] = CreateItemCommand(meta, commandArg);
            }
        }

        /// <summary>
        /// 所有生成的命令的参数
        /// </summary>
        public object CommandArg
        {
            get { return _commandArg; }
        }

        /// <summary>
        /// 所有的命令元数据。
        /// </summary>
        public IEnumerable<WPFCommand> Commands
        {
            get { return _commands; }
        }

        /// <summary>
        /// 命令控件的主要存储列表
        /// 
        /// 向这个列表中添加的项，会显示在命令容器中。
        /// </summary>
        public IList ContainerItems
        {
            get { return this._commandsContainer.Items; }
        }

        /// <summary>
        /// 右键菜单中的项列表。
        /// 
        /// 向这个列表中添加的项，会显示在菜单中。
        /// </summary>
        /// <returns></returns>
        public IList ContextMenuItems
        {
            get
            {
                var mainContentControl = this._commandsContainer.GetServicedControl();
                var contextMenu = mainContentControl.ContextMenu;
                if (contextMenu == null)
                {
                    contextMenu = new ContextMenu();
                    mainContentControl.ContextMenu = contextMenu;
                }

                return contextMenu.Items;
            }
        }

        /// <summary>
        /// 注册某个分组分成的控件。
        /// </summary>
        /// <param name="groupType"></param>
        /// <param name="control"></param>
        public void RegisterGroupedContainerItem(int groupType, FrameworkElement control)
        {
            IList<FrameworkElement> cmds;
            if (!this._groupedCommands.TryGetValue(groupType, out cmds))
            {
                cmds = new List<FrameworkElement>();
                this._groupedCommands.Add(groupType, cmds);
            }

            cmds.Add(control);
        }

        /// <summary>
        /// 把分组生成的控件，都加入到容器中。
        /// </summary>
        internal void AttachGroupedItems()
        {
            var items = this.ContainerItems;

            foreach (var kv in this._groupedCommands)
            {
                //添加分隔符
                if (items.Count > 0) items.Add(new Separator());

                foreach (var cmd in kv.Value) { items.Add(cmd); }
            }
        }

        /// <summary>
        /// 获取元数据对应的客户端运行时命令。
        /// 
        /// 一个元数据只会生成一个运行时命令。而一个运行时命令，则可以同时生成工具栏、菜单等多个位置的控件。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public ClientCommand GetClientCommand(WPFCommand meta)
        {
            return _clientCommands[meta];
        }

        /// <summary>
        /// 为当前的命令元数据生成一个客户端运行时命令对象。
        /// </summary>
        /// <param name="groupGenerator"></param>
        /// <returns></returns>
        private static ClientCommand CreateItemCommand(WPFCommand commandMeta, object commandArg)
        {
            var command = CommandRepository.CreateCommand(commandMeta, commandArg);

            var view = commandArg as LogicalView;
            if (view != null)
            {
                view.Commands.Add(command);
            }

            return command;
        }
    }
}