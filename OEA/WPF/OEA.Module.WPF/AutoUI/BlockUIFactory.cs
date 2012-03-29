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
using System.Windows.Data;
using System.Windows.Media;
using AvalonDock;
using SimpleCsla;
using Itenso.Windows.Input;
using OEA.Editors;
using OEA.Library;

using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.CommandAutoUI;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;


using OEA.WPF.Command;

using System.Windows.Controls.Primitives;
using OEA.Module.WPF.Automation;
using System.Windows.Input;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 界面中最某一具体的UI块的控件生成工厂
    /// </summary>
    public class BlockUIFactory
    {
        private PropertyEditorFactory _propertyEditorFactory;

        private TreeColumnFactory _treeColumnFactory;

        public BlockUIFactory(PropertyEditorFactory propertyEditorFactory)
        {
            if (propertyEditorFactory == null) throw new ArgumentNullException("propertyEditorFactory");
            this._propertyEditorFactory = propertyEditorFactory;

            this._treeColumnFactory = new TreeColumnFactory(propertyEditorFactory);
        }

        /// <summary>
        /// 属性编辑器工厂
        /// </summary>
        public PropertyEditorFactory PropertyEditorFactory
        {
            get { return this._propertyEditorFactory; }
        }

        /// <summary>
        /// 树型控件的列工厂
        /// </summary>
        public TreeColumnFactory TreeColumnFactory
        {
            get { return this._treeColumnFactory; }
        }

        /// <summary>
        /// 自动生成树形列表UI
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="showInWhere"></param>
        /// <returns></returns>
        public virtual MultiTypesTreeGrid CreateTreeListControl(EntityViewMeta vm, ShowInWhere showInWhere)
        {
            if (vm == null) throw new ArgumentNullException("vm");

            //装载多个对象的属性
            var propInfos = vm.OrderedEntityProperties().ToList();

            //使用MultiTypesTreeGrid作为TreeListControl
            var treeListControl = new MultiTypesTreeGrid(vm);

            //使用list里面的属性生成每一列
            var columns = treeListControl.Columns;
            foreach (var propertyViewInfo in propInfos)
            {
                if (propertyViewInfo.CanShowIn(showInWhere))
                {
                    var column = this._treeColumnFactory.Create(propertyViewInfo);

                    columns.Add(column);
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
            Type boType = detailView.EntityType;

            var vm = detailView.Meta;

            //生成一个AutoGrid，用于承载所有的字段显示控件。
            var detailGrid = new AutoGrid();

            //一般加四列，ConditionQuery/NavigateQuery加两列
            var colCount = detailView.ColumnsCount;
            for (int i = 0; i < colCount; i++)
            {
                detailGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                detailGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //加入所有标记了ShowInDetail的属性
            foreach (var propertyView in vm.EntityProperties)
            {
                if (propertyView.CanShowIn(ShowInWhere.Detail))
                {
                    var editor = this._propertyEditorFactory.Create(propertyView);

                    //在创建详细面板时，如果是动态的属性编辑器
                    var dynamicPE = editor as EntityDynamicPropertyEditor;
                    if (dynamicPE != null)
                    {
                        detailView.CurrentObjectChanged += (o, e) =>
                        {
                            editor.PrepareElementForEdit(dynamicPE.Control, new RoutedEventArgs());
                        };
                    }

                    detailGrid.Children.Add(new ContentControl()
                    {
                        Style = OEAStyles.DetailPanel_ItemContentControl,
                        Content = editor.LabelControl
                    });
                    detailGrid.Children.Add(new ContentControl()
                    {
                        Style = OEAStyles.DetailPanel_ItemContentControl,
                        Content = editor.Control
                    });

                    var view = detailView as QueryObjectView;
                    if (view != null) { view.AddPropertyEditor(editor); }

                    //支持UI Test
                    AutomationProperties.SetName(editor.Control, propertyView.Label);
                }
            }

            return detailGrid;
        }

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