/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：单个命令的生成器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF.Command;
using Itenso.Windows.Input;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 单个命令的生成器
    /// </summary>
    [DebuggerDisplay("SingleItem : {CommandItem.Label}")]
    public abstract class ItemGenerator : GroupGenerator
    {
        /// <summary>
        /// 单个命令
        /// </summary>
        protected WPFCommand CommandItem
        {
            get { return this.CommandGroup.SingleItem; }
        }

        protected override void AttachToContextCore(FrameworkElement control)
        {
            //如果元数据中指定了确切的附加方式，则直接附加成功。否则抛出异常。
            var loc = this.CommandItem.Location;
            if (loc == CommandLocation.Menu)
            {
                this.AttachToContextMenu(control);
            }
            else if (loc == CommandLocation.Toolbar)
            {
                this.AttachToContainer(control);
            }
            else
            {
                throw new NotSupportedException(
@"一个 ItemGenerator 只能使用一种附加方式（Toolbar/Menu）。
请重写 AttachToContextCore 方法来实现自己的 AttachToContext 逻辑");
            }
        }

        #region 按照顺序加入到 Toolbar/ContextMenu 中。

        protected override void AttachToContainer(FrameworkElement control)
        {
            //外部已经排过序了，此处不需要再根据顺序插入
            this.Context.RegisterGroupedContainerItem(this.CommandItem.GroupType, control);
        }

        protected override void AttachToContextMenu(FrameworkElement control)
        {
            var groups = this.CommandItem.Groups;

            var items = this.Context.ContextMenuItems;

            //自动生成多组菜单
            if (groups.Count > 0)
            {
                HeaderedItemsControl curItem = null;

                for (int i = 0; i < groups.Count; i++)
                {
                    var groupName = groups[i];
                    curItem = items.OfType<HeaderedItemsControl>()
                        .FirstOrDefault(m => m.Header.ToString() == groupName);

                    if (curItem == null)
                    {
                        curItem = new MenuItem
                        {
                            Header = groupName
                        };
                        items.Add(curItem);
                    }

                    items = curItem.Items;
                }
            }

            items.Add(control);
        }

        #endregion

        #region 控件生成

        /// <summary>
        /// 生成控件，并设置控件的Tag为Command.Index。
        /// </summary>
        /// <returns></returns>
        protected sealed override FrameworkElement CreateControlCore()
        {
            var result = this.CreateItemControl();

            //绑定运行时 Command 的 IsVisible 属性到控件上。
            result.Element.DataContext = result.Command;
            result.Element.SetBinding(UIElement.VisibilityProperty, new Binding("IsVisible")
            {
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            });

            return result.Element;
        }

        /// <summary>
        /// 这个类的子类实现此方法，来生成具体的控件。
        /// </summary>
        /// <returns></returns>
        protected abstract ItemControlResult CreateItemControl();

        #endregion

        #region 方便的 API

        public CommandAdapter CreateItemCommand()
        {
            var runtimeCommand = CommandRepository.NewCommand(this.CommandItem);

            var args = this.Context.CommandArg;
            var view = args as ObjectView;
            if (view != null) { view.Commands.Add(runtimeCommand.CoreCommand); }

            return runtimeCommand;
        }

        protected TextBox CreateTextBox(CommandAdapter command)
        {
            return CreateTextBox(command.CoreCommand);
        }

        protected void TryExecuteCommand(CommandAdapter command)
        {
            command.CoreCommand.TryExecute(this.Context.CommandArg);
        }

        #endregion
    }

    /// <summary>
    /// ItemGenerator 生成的控件结果
    /// </summary>
    public class ItemControlResult
    {
        public ItemControlResult(FrameworkElement element, CommandAdapter runtimCmd)
        {
            if (element == null) throw new ArgumentNullException("element");
            if (runtimCmd == null) throw new ArgumentNullException("runtimCmd");

            this.Element = element;
            this.Command = runtimCmd.CoreCommand;
        }

        /// <summary>
        /// 生成的控件
        /// </summary>
        public FrameworkElement Element { get; private set; }

        /// <summary>
        /// 控件所使用的运行时命令
        /// </summary>
        public ClientCommand Command { get; private set; }
    }
}