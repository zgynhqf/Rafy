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
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Command;

namespace Rafy.WPF.Command.UI
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
        protected WPFCommand SingleMeta
        {
            get { return this.CommandMetaGroup.Single; }
        }

        protected override void AttachToContextCore(FrameworkElement control)
        {
            //如果元数据中指定了确切的附加方式，则直接附加成功。否则抛出异常。
            var loc = this.SingleMeta.Location;
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

        #region 加入到 Toolbar/ContextMenu 中。

        protected override void AttachToContainer(FrameworkElement control)
        {
            //每个单项生成的控件，默认先回到对应的组中，最后再把这个组添加一起添加到容器中。
            this.Context.RegisterGroupedContainerItem(this.SingleMeta.GroupType, control);
        }

        protected override void AttachToContextMenu(FrameworkElement control)
        {
            var hierarchy = this.SingleMeta.Hierarchy;

            var items = this.Context.ContextMenuItems;

            //自动生成多级菜单
            if (hierarchy.Count > 0)
            {
                HeaderedItemsControl curItem = null;

                for (int i = 0; i < hierarchy.Count; i++)
                {
                    var current = hierarchy[i];
                    curItem = items.OfType<HeaderedItemsControl>()
                        .FirstOrDefault(m => m.Header.ToString() == current);

                    if (curItem == null)
                    {
                        curItem = new MenuItem
                        {
                            Header = current
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
            var command = this.Context.GetClientCommand(this.SingleMeta);
            var element = this.CreateCommandUI(command);
            if (element == null)
            {
                throw new InvalidProgramException("CreateCommandUI 方法没有为 ClientCommand 生成界面元素。");
            }

            //绑定运行时 Command 的 IsVisible 属性到控件上。
            element.DataContext = command;
            element.SetBinding(UIElement.VisibilityProperty, new Binding("IsVisible")
            {
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            });

            command.AddUIElement(element);

            return element;
        }

        /// <summary>
        /// 这个类的子类实现此方法，来生成具体的命令与控件。
        /// </summary>
        /// <returns></returns>
        protected abstract FrameworkElement CreateCommandUI(ClientCommand command);

        #endregion
    }
}