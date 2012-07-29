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
using OEA.WPF.Command;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using Wpf.Controls;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 命令生成子系统的生成上下文
    /// 
    /// 在整个生成过程中，每一个 GroupGenerator 可以直接把生成的控件以以下种方式添加到 Context 中：
    /// 1. 直接添加到 ContainerItems 中。
    /// 2. 直接添加到 ContextMenuItems 中。
    /// 3. 使用 RegisterGroupedContainerItem 添加经过分组的控件，
    /// 这些控件会被最终使用分隔符添加到 ContainerItems 中（见：AddGroupedItems 方法）。
    /// </summary>
    public class CommandAutoUIContext
    {
        private Dictionary<CommandGroupType, IList<FrameworkElement>> _groupedCommands = new Dictionary<CommandGroupType, IList<FrameworkElement>>();

        /// <summary>
        /// 为这个Toolbar进行生成子项
        /// </summary>
        private ItemsControl _commandsContainer;

        public CommandAutoUIContext(ItemsControl commandsContainer, object commandArg)
        {
            this._commandsContainer = commandsContainer;
            this.CommandArg = commandArg;
        }

        /// <summary>
        /// 所有生成的命令的参数
        /// </summary>
        public object CommandArg { get; private set; }

        /// <summary>
        /// 命令控件的主要存储列表
        /// </summary>
        public IList ContainerItems
        {
            get { return this._commandsContainer.Items; }
        }

        /// <summary>
        /// 找到或者创建右键菜单
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
        /// 在整个生成过程中，每一个 GroupGenerator 可以直接把生成的控件以以下种方式添加到 Context 中：
        /// 1. 直接使用 CommandsContainerItems 添加到 CommandsContainerItems 中
        /// 2. 直接
        /// </summary>
        /// <param name="groupType"></param>
        /// <param name="control"></param>
        public void RegisterGroupedContainerItem(CommandGroupType groupType, FrameworkElement control)
        {
            IList<FrameworkElement> cmds;
            if (!this._groupedCommands.TryGetValue(groupType, out cmds))
            {
                cmds = new List<FrameworkElement>();
                this._groupedCommands.Add(groupType, cmds);
            }

            cmds.Add(control);
        }

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
    }
}