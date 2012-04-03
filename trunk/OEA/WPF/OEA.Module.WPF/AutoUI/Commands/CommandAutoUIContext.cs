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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Itenso.Windows.Input;

using OEA.Library;


using OEA.WPF.Command;
using Wpf.Controls;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Collections;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 命令生成子系统的生成上下文
    /// </summary>
    public class CommandAutoUIContext
    {
        private Dictionary<CommandGroupType, IList<FrameworkElement>> _cmdGroups = new Dictionary<CommandGroupType, IList<FrameworkElement>>();

        public CommandAutoUIContext(ItemsControl commandsContainer, object commandArg)
        {
            this.CommandsContainer = commandsContainer;
            this.CommandArg = commandArg;
        }

        /// <summary>
        /// 为这个Toolbar进行生成子项
        /// </summary>
        public ItemsControl CommandsContainer { get; private set; }

        internal IList Items
        {
            get { return this.CommandsContainer.Items; }
        }

        /// <summary>
        /// 所有生成的命令的参数
        /// </summary>
        public object CommandArg { get; private set; }

        public void AddItem(CommandGroupType groupType, FrameworkElement control)
        {
            IList<FrameworkElement> cmds;
            if (this._cmdGroups.TryGetValue(groupType, out cmds))
            {
                cmds.Add(control);
            }
            else
            {
                this._cmdGroups[groupType] = new List<FrameworkElement>() { control };
            }
        }

        public void AddCommandsByGroupType()
        {
            foreach (var group in this._cmdGroups.Keys)
            {
                //添加分隔符
                if (this.Items.Count > 0) this.Items.Add(new Separator());

                foreach (var cmd in this._cmdGroups[group])
                {
                    this.Items.Add(cmd);
                }
            }
        }
    }
}