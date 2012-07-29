/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100408
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100408
 * 不再使用类名DefaultUIFactory，同时删除Commands相关方法。 胡庆访 20100216
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AvalonDock;
using OEA.WPF.Command;
using OEA.Editors;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Automation;
using OEA.Module.WPF.CommandAutoUI;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;
using System.Runtime;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 界面中最某一具体的UI块的控件生成工厂
    /// </summary>
    public class BlockUIFactory
    {
        public BlockUIFactory(PropertyEditorFactory propertyEditorFactory)
        {
            if (propertyEditorFactory == null) throw new ArgumentNullException("propertyEditorFactory");
            this.PropertyEditorFactory = propertyEditorFactory;

            this.TreeColumnFactory = new TreeColumnFactory(propertyEditorFactory);
        }

        /// <summary>
        /// 属性编辑器工厂
        /// </summary>
        public PropertyEditorFactory PropertyEditorFactory { get; private set; }

        /// <summary>
        /// 树型控件的列工厂
        /// </summary>
        public TreeColumnFactory TreeColumnFactory { get; private set; }

        /// <summary>
        /// 自动生成树形列表UI
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="showInWhere"></param>
        /// <returns></returns>
        public virtual MultiTypesTreeGrid CreateTreeListControl(EntityViewMeta vm, ShowInWhere showInWhere)
        {
            if (vm == null) throw new ArgumentNullException("vm");

            //使用MultiTypesTreeGrid作为TreeListControl
            var treeListControl = new MultiTypesTreeGrid(vm);

            //使用list里面的属性生成每一列
            foreach (var property in vm.OrderedEntityProperties())
            {
                if (property.CanShowIn(showInWhere))
                {
                    var column = this.TreeColumnFactory.Create(property);

                    treeListControl.Columns.Add(column);
                }
            }

            return treeListControl;
        }

        /// <summary>
        /// 生成一个AutoGrid，用于承载所有的字段显示控件。
        /// </summary>
        /// <param name="detailView">逻辑视图</param>
        /// <returns></returns>
        public virtual FrameworkElement CreateDetailPanel(DetailObjectView detailView)
        {
            if (detailView == null) throw new ArgumentNullException("detailView");

            var form = new Form { DetailView = detailView };

            var meta = detailView.Meta;
            if (meta.DetailPanelType == null)
            {
                /*********************** 代码块解释 *********************************
                 * 表单自动生成解释：
                 * 
                 * 首先，所有属性分组，如果：
                 * 1. 没有分组时，根据 DetailLayoutMode 的值，分以下两种情况来生成表单：
                 * * 使用 AutoGrid 作为 Editor 的容器，以保证每个 Editor 有动态的宽度。
                 * * 使用 WrapPanel 作为 Editor 的容器，使得实现 Editor 的自动换行。（此时 Editor 应该有最小的宽度。）
                 * 2. 如果分为多个组，则使用 TabControl 或者使用 GroupBox 把每个组分开生成，每个组则都使用上一条算法进行生成。
                 * 
                 * 最外围的容器是 Form 控件，它的 VerticalScrollBarVisibility 在样式中将被设置为 Auto，
                 * 而 HorizontalScrollBarVisibility 则为 Disable。
                 * 
                 * 这样，整体设计上，保证竖直方向上有滚动条，水平方向上是动态宽度或动态换行。
                **********************************************************************/

                var groups = meta.OrderedEntityProperties().Where(pv => pv.CanShowIn(ShowInWhere.Detail))
                    .GroupBy(pv => pv.DetailGroupName).ToArray();
                if (groups.Length == 0) throw new InvalidOperationException(meta.EntityType.FullName + " 没有属性时，不能生成表单控件。");

                if (groups.Length == 1)
                {
                    form.Content = this.GenerateEditors(detailView, groups[0]);
                }
                else
                {
                    form.Content = this.GenerateGroups(detailView, groups);
                }
            }
            //自定义表单布局
            else
            {
                form.Content = Activator.CreateInstance(meta.DetailPanelType) as FrameworkElement;
            }

            return form;
        }

        /// <summary>
        /// 则使用 TabControl 或者 GroupBox 把每个组分开生成，并添加到最终的容器控件中返回。
        /// </summary>
        /// <param name="detailView"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        protected FrameworkElement GenerateGroups(DetailObjectView detailView, IGrouping<string, EntityPropertyViewMeta>[] groups)
        {
            if (detailView.Meta.DetailGroupingMode == DetailGroupingMode.GroupBox)
            {
                var groupsPanel = new StackPanel();

                foreach (var group in groups)
                {
                    var panel = this.GenerateEditors(detailView, group);

                    groupsPanel.Children.Add(new GroupBox
                    {
                        Header = new Label { Content = group.Key },
                        Content = panel
                    });
                }

                return groupsPanel;
            }
            else
            {
                var tabControl = new TabControl { TabStripPlacement = Dock.Left };

                foreach (var group in groups)
                {
                    var panel = this.GenerateEditors(detailView, group);

                    tabControl.Items.Add(new TabItem
                    {
                        Header = new Label { Content = group.Key },
                        Content = panel
                    });
                }

                return tabControl;
            }
        }

        /// <summary>
        /// 根据 DetailLayoutMode 的值生成动态布局的表单。
        /// </summary>
        /// <param name="detailView"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected Panel GenerateEditors(DetailObjectView detailView, IGrouping<string, EntityPropertyViewMeta> properties)
        {
            if (detailView is QueryObjectView)
            {
                return this.GenerateGridEditors(detailView, properties);
            }

            switch (detailView.Meta.DetailLayoutMode)
            {
                case DetailLayoutMode.Dynamic:
                    if (detailView.CalculateColumnsCount(properties) == 1)
                    {
                        return this.GenerateGridEditors(detailView, properties);
                    }
                    else
                    {
                        return this.GenerateWrappingEditors(detailView, properties);
                    }
                case DetailLayoutMode.Wrapping:
                    return this.GenerateWrappingEditors(detailView, properties);
                case DetailLayoutMode.AutoGrid:
                    return this.GenerateGridEditors(detailView, properties);
                default:
                    return this.GenerateWrappingEditors(detailView, properties);
            }
        }

        /// <summary>
        /// 为所有属性生成自动折行的编辑器列表。
        /// </summary>
        /// <param name="panel"></param>
        /// <param name="properties"></param>
        protected Panel GenerateWrappingEditors(DetailObjectView detailView, IGrouping<string, EntityPropertyViewMeta> properties)
        {
            var panel = new WrapPanel
            {
                Orientation = detailView.Meta.DetailAsHorizontal ? Orientation.Horizontal : Orientation.Vertical
            };

            var wp = new WrapPanel();
            panel.Children.Add(wp);
            foreach (var propertyView in properties)
            {
                if (propertyView.DetailNewLine)
                {
                    wp = new WrapPanel();
                    panel.Children.Add(wp);
                }

                wp.Children.Add(new EditorHost { EntityProperty = propertyView });
            }

            return panel;
        }

        /// <summary>
        /// 使用 AutoGrid 来生成表单。
        /// </summary>
        /// <param name="detailView"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected Panel GenerateGridEditors(DetailObjectView detailView, IGrouping<string, EntityPropertyViewMeta> properties)
        {
            var panel = new AutoGrid
            {
                ColumnsCount = detailView.CalculateColumnsCount(properties),
                Orientation = detailView.Meta.DetailAsHorizontal ? Orientation.Horizontal : Orientation.Vertical
            };

            foreach (var propertyView in properties)
            {
                panel.Children.Add(new EditorHost { EntityProperty = propertyView });
            }

            return panel;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void AppendCommands(
            ItemsControl commandsContainer,
            object commandArg,
            params WPFCommand[] availableCommands
            )
        {
            this.AppendCommands(commandsContainer, commandArg, availableCommands as IEnumerable<WPFCommand>);
        }

        public virtual void AppendCommands(
            ItemsControl commandsContainer,
            object commandArg,
            IEnumerable<WPFCommand> availableCommands
            )
        {
            new CommandAutoUIManager().Generate(commandsContainer, commandArg, availableCommands);
        }
    }
}