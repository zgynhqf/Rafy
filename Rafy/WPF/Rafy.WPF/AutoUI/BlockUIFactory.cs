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
using System.Runtime;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Automation;
using Rafy.WPF.Command.UI;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;
using Rafy.WPF.Command;
using Rafy.ManagedProperty;

namespace Rafy.WPF
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
        /// <param name="evm"></param>
        /// <param name="listShowInWhere">
        /// 这个表格需要显示在哪个位置。
        /// <remarks>生成引擎根据元数据中各属性定义的显示逻辑来生成列。</remarks>
        /// </param>
        /// <param name="properties">如果提供了这个参数，则表示创建的列表控件，只显示给定的这些属性</param>
        /// <returns></returns>
        public RafyTreeGrid CreateTreeGrid(EntityViewMeta evm, ListShowInWhere listShowInWhere, IList<IManagedProperty> properties)
        {
            if (evm == null) throw new ArgumentNullException("vm");

            //使用TreeGrid作为TreeListControl
            var treeGrid = new RafyTreeGrid
            {
                OnlyGridMode = !evm.EntityMeta.IsTreeEntity,
                GroupingStyle = RafyResources.GroupContainerStyle,
                CheckingColumnTemplate = RafyResources.Rafy_MTTG_SelectionColumnTemplate,
                NoDataText = "没有数据".Translate(),
                CheckingColumnHeader = "选择".Translate(),
                SummaryRowTitle = "合计：".Translate(),
                ShowSummaryRow = GetNeedSummary(evm)
            };
            if (!string.IsNullOrEmpty(evm.Label))
            {
                AutomationProperties.SetName(treeGrid, evm.Label);
            }
            TreeGridRow.SetAutomationProperty(treeGrid, evm.TryGetPrimayDisplayProperty());

            var showInWhere = (ShowInWhere)listShowInWhere;

            //使用list里面的属性生成每一列
            foreach (WPFEntityPropertyViewMeta property in evm.OrderedEntityProperties())
            {
                if (property.CanShowIn(showInWhere) && (properties == null || properties.Contains(property.PropertyMeta.ManagedProperty)))
                {
                    var column = this.TreeColumnFactory.Create(property);

                    treeGrid.Columns.Add(column);
                }
            }

            treeGrid.ApplyTemplate();

            /*********************** 代码块解释 *********************************
             * 表格的设计，与 FocusTrackerAdorner 不兼容，暂时把它排除。
             * 目前，有以下问题：
             * * 表格中横向拖动滚动条时，焦点跟踪框的位置没有更新。
             * * 表格中从一行的编辑框进入另一行某个单元格的编辑时，跟踪框位置没有更新。
            **********************************************************************/
            FocusTrackerAdorner.SetTrackFocusScope(treeGrid, TrackFocusScope.Exclude);

            return treeGrid;
        }

        /// <summary>
        /// 生成一个AutoGrid，用于承载所有的字段显示控件。
        /// </summary>
        /// <param name="detailView">逻辑视图</param>
        /// <returns></returns>
        public Form CreateDetailPanel(DetailLogicalView detailView)
        {
            if (detailView == null) throw new ArgumentNullException("detailView");

            Form form = null;

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

                form = new Form();

                var groups = meta.DetailGroups;
                if (groups.Count > 0)
                {
                    form.Content = this.GenerateGroups(detailView, groups);
                }
                else
                {
                    var properties = OrderedDetailProperties(meta.EntityProperties);
                    form.Content = this.GenerateEditors(detailView, properties, meta.DetailLayoutMode);
                }

                //var groups = meta.OrderedEntityProperties().Where(pv => pv.CanShowIn(ShowInWhere.Detail))
                //    .GroupBy(pv => pv.DetailGroupName).ToArray();
                //if (groups.Length == 0) throw new InvalidOperationException(meta.EntityType.FullName + " 没有属性在表单中显示时，不能生成表单控件。");

                //if (groups.Length == 1)
                //{
                //    form.Content = this.GenerateEditors(detailView, groups[0]);
                //}
                //else
                //{
                //    form.Content = this.GenerateGroups(detailView, groups);
                //}
            }
            //自定义表单布局
            else
            {
                form = Activator.CreateInstance(meta.DetailPanelType) as Form;
                if (form == null) throw new InvalidProgramException("自定义面板必须是 Form 元素。");
            }

            form.DetailView = detailView;

            //即时调用 EditorHost 的 ApplyTemplate 方法，
            //可以使 EditorHost 即时生成 PropertyEditor，而不是等到界面 Loaded 时再生成。
            ApplyEditorHostTemplate(form);

            return form;
        }

        /// <summary>
        /// 迭归查找某个元素中所有的 EditorHost 子元素，
        /// 并调用它们的 ApplyTemplate 方法。
        /// </summary>
        /// <param name="element"></param>
        private static void ApplyEditorHostTemplate(DependencyObject element)
        {
            if (element is EditorHost)
            {
                (element as EditorHost).ApplyTemplate();
                return;
            }

            var children = LogicalTreeHelper.GetChildren(element);
            foreach (var child in children)
            {
                if (child is DependencyObject)
                {
                    ApplyEditorHostTemplate(child as DependencyObject);
                }
            }
        }

        /// <summary>
        /// 则使用 TabControl 或者 GroupBox 把每个组分开生成，并添加到最终的容器控件中返回。
        /// </summary>
        /// <param name="detailView"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        protected FrameworkElement GenerateGroups(DetailLogicalView detailView, IList<WPFDetailPropertyGroup> groups)
        {
            if (detailView.Meta.DetailGroupingMode == DetailGroupingMode.GroupBox)
            {
                var groupsPanel = new StackPanel();

                foreach (var group in groups)
                {
                    var panel = this.GenerateEditors(detailView, OrderedDetailProperties(group.Properties), group.LayoutMode ?? detailView.Meta.DetailLayoutMode);

                    groupsPanel.Children.Add(new GroupBox
                    {
                        Header = new Label { Content = group.GroupLabel.Translate() },
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
                    var panel = this.GenerateEditors(detailView, OrderedDetailProperties(group.Properties), group.LayoutMode ?? detailView.Meta.DetailLayoutMode);

                    tabControl.Items.Add(new TabItem
                    {
                        Header = new Label { Content = group.GroupLabel.Translate() },
                        //这里需要使用 AdornerDecorator，否则在页签切换后会造成验证状态丢失。
                        Content = new AdornerDecorator
                        {
                            Child = panel
                        }
                    });
                }

                return tabControl;
            }
        }

        /// <summary>
        /// 获取按照显示排序后的属性列表。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<WPFEntityPropertyViewMeta> OrderedDetailProperties(IEnumerable<EntityPropertyViewMeta> properties)
        {
            return EntityPropertyViewMeta.Order(properties)
                .Where(pv => pv.CanShowIn(ShowInWhere.Detail))
                .Cast<WPFEntityPropertyViewMeta>();
        }

        /// <summary>
        /// 根据 DetailLayoutMode 的值生成动态布局的表单。
        /// </summary>
        /// <param name="detailView"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected FrameworkElement GenerateEditors(
            DetailLogicalView detailView,
            IEnumerable<WPFEntityPropertyViewMeta> properties,
            DetailLayoutMode layoutMode
            )
        {
            if (detailView is QueryLogicalView)
            {
                return this.GenerateGridEditors(detailView, properties);
            }

            switch (layoutMode)
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
        protected FrameworkElement GenerateWrappingEditors(DetailLogicalView detailView, IEnumerable<WPFEntityPropertyViewMeta> properties)
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

            return EnableScoller(panel, panel.Orientation);
        }

        /// <summary>
        /// 使用 AutoGrid 来生成表单。
        /// </summary>
        /// <param name="detailView"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected FrameworkElement GenerateGridEditors(DetailLogicalView detailView, IEnumerable<WPFEntityPropertyViewMeta> properties)
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

            return EnableScoller(panel, panel.Orientation);
        }

        private FrameworkElement EnableScoller(FrameworkElement element, Orientation orientation)
        {
            //暂时不用
            return element;

            //var scroller = new ScrollViewer
            //{
            //    Content = element
            //};

            //if (orientation == Orientation.Horizontal)
            //{
            //    scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            //    scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            //}

            //return scroller;
        }

        #region 生成命令

        private CommandAutoUIManager _cmdAutoUI = new CommandAutoUIManager();

        /// <summary>
        /// 为指定的容器生成命令。
        /// </summary>
        /// <param name="commandsContainer"></param>
        /// <param name="commandArg"></param>
        /// <param name="availableCommands"></param>
        public void AppendCommands(
            ItemsControl commandsContainer,
            object commandArg,
            params WPFCommand[] availableCommands
            )
        {
            DoAppendCommands(commandsContainer, commandArg, availableCommands as IEnumerable<WPFCommand>);
        }

        /// <summary>
        /// 为指定的容器生成命令。
        /// </summary>
        /// <param name="commandsContainer"></param>
        /// <param name="commandArg"></param>
        /// <param name="availableCommands"></param>
        public void AppendCommands(
            ItemsControl commandsContainer,
            object commandArg,
            IEnumerable<WPFCommand> availableCommands
            )
        {
            DoAppendCommands(commandsContainer, commandArg, availableCommands);
        }

        /// <summary>
        /// 为指定的容器生成命令。
        /// 
        /// 子类重写此方法来实现更新的命令生成逻辑。
        /// </summary>
        /// <param name="commandsContainer"></param>
        /// <param name="commandArg"></param>
        /// <param name="availableCommands"></param>
        protected virtual void DoAppendCommands(ItemsControl commandsContainer, object commandArg, IEnumerable<WPFCommand> availableCommands)
        {
            _cmdAutoUI.Generate(commandsContainer, commandArg, availableCommands);
        }

        #endregion

        #region EntityViewMeta.NeedSummary

        private const string BlockUIFactory_NeedSummary = "BlockUIFactory_NeedSummary";

        /// <summary>
        /// 获取一个列表视图是否需要显示合计行。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static bool GetNeedSummary(EntityViewMeta meta)
        {
            return meta.GetPropertyOrDefault<bool>(BlockUIFactory_NeedSummary);
        }

        internal static void SetNeedSummary(EntityViewMeta meta, bool value)
        {
            meta.SetExtendedProperty(BlockUIFactory_NeedSummary, value);
        }

        #endregion
    }
}