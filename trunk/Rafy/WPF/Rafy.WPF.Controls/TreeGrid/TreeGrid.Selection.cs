/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110621
 * 说明：TreeGrid 中与勾选、选择行相关的所有代码。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110621
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// MTTG 控件有两种选择模式，一种是直接选择行，一种是使用勾选框进行选择。
    /// </summary>
    partial class TreeGrid
    {
        /// <summary>
        /// 当前 TreeGrid 的选择事件流程
        /// </summary>
        internal ProcessSourceIndicator<TGSelectionProcess> SelectionProcess = new ProcessSourceIndicator<TGSelectionProcess>();

        private void OnSelectionConstruct()
        {
            this.CheckingModel = new TreeGridSelectionModel(this, true);
            this.SelectionModel = new TreeGridSelectionModel(this, false);
        }

        private void ClearSelectionOnRefreshing()
        {
            //不能使用 ClearSelection 清空 SelectionModel，这样会使得 this.SelectedItem 属性为 null，
            //而造成在刷新控件时，丢失当前布局而无法保留当前选中行。
            //this.SelectionModel.ClearSelectedItems();
            //this.CheckingModel.ClearSelectedItems();

            this.CheckingModel.InnerItems.Clear();

            //在清空选择项时，需要保证 SelectedItem 属性与 SelectionModel.InnerItems 的同步。
            var selection = this.SelectionModel.InnerItems;
            var selectedItem = this.SelectedItem;
            if (selectedItem != null)
            {
                //除了 selectedItem 以外的全部移除。
                for (int i = selection.Count - 1; i >= 0; i--)
                {
                    if (selection[i] != selectedItem) selection.RemoveAt(i);
                }
            }
            else
            {
                selection.Clear();
            }
        }

        /// <summary>
        /// 直接选择行的选择模型。
        /// </summary>
        public TreeGridSelectionModel SelectionModel { get; private set; }

        /// <summary>
        /// 在 CheckingMode.Row 模式（勾选行模式）下，勾选的选择模型。
        /// </summary>
        public TreeGridSelectionModel CheckingModel { get; private set; }

        #region SelectedItem DependencyProperty

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(object), typeof(TreeGrid),
            new FrameworkPropertyMetadata(null, (o, e) => (o as TreeGrid).OnSelectedItemChanged(e))
            );

        /// <summary>
        /// 获取或者设置当前最后一个选中项。
        /// null 表示没有任何选中项。
        /// </summary>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// 当前被选中的行。
        /// 
        /// 如果已经打开 UIV，并且该行并没有在实例化时，此属性会将会返回 null。
        /// </summary>
        public TreeGridRow SelectedRow
        {
            get { return this.FindRow(this.SelectedItem); }
        }

        #endregion

        #region SelectParentOnCollapsed DependencyProperty

        public static readonly DependencyProperty SelectParentOnCollapsedProperty = DependencyProperty.Register(
            "SelectParentOnCollapsed", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata(true)
            );

        /// <summary>
        /// 在父节点被选择后，是否把 Parent 设置为当前选择项。
        /// 
        /// 默认为 true。
        /// </summary>
        public bool SelectParentOnCollapsed
        {
            get { return (bool)this.GetValue(SelectParentOnCollapsedProperty); }
            set { this.SetValue(SelectParentOnCollapsedProperty, value); }
        }

        #endregion

        #region CheckingColumnTemplate DependencyProperty

        public static readonly DependencyProperty CheckingColumnTemplateProperty = DependencyProperty.Register(
            "CheckingColumnTemplate", typeof(DataTemplate), typeof(TreeGrid),
            new PropertyMetadata((d, e) => (d as TreeGrid).OnCheckingColumnTemplateChanged(e))
            );

        public DataTemplate CheckingColumnTemplate
        {
            get { return (DataTemplate)this.GetValue(CheckingColumnTemplateProperty); }
            set { this.SetValue(CheckingColumnTemplateProperty, value); }
        }

        private void OnCheckingColumnTemplateChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (DataTemplate)e.NewValue;
            if (this.IsCheckingEnabled)
            {
                this.ResetCheckingTemplate();
            }
        }

        #endregion

        #region IsCheckingOpend DependencyProperty

        public static readonly DependencyProperty IsCheckingEnabledProperty = DependencyProperty.Register(
            "IsCheckingEnabled", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata(false, (d, e) => (d as TreeGrid).OnIsCheckingEnabledChanged(e))
            );

        /// <summary>
        /// 当前表格控件是否已经打开选择功能
        /// </summary>
        public bool IsCheckingEnabled
        {
            get { return (bool)this.GetValue(IsCheckingEnabledProperty); }
            set { this.SetValue(IsCheckingEnabledProperty, value); }
        }

        private void OnIsCheckingEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            if (value)
            {
                this.EnableChecking();
            }
            else
            {
                this.DisableChecking();
            }
        }

        #endregion

        #region CheckingRowCascade DependencyProperty

        public static readonly DependencyProperty CheckingCascadeModeProperty = DependencyProperty.Register(
            "CheckingCascadeMode", typeof(CheckingCascadeMode), typeof(TreeGrid),
            new PropertyMetadata(CheckingCascadeMode.None)
            );

        /// <summary>
        /// 此属性的设置不会影响当前已有的选择项，只对未来的选择行为起作为。
        /// </summary>
        public CheckingCascadeMode CheckingCascadeMode
        {
            get { return (CheckingCascadeMode)this.GetValue(CheckingCascadeModeProperty); }
            set { this.SetValue(CheckingCascadeModeProperty, value); }
        }

        private bool NeedCascade(CheckingCascadeMode value)
        {
            return (this.CheckingCascadeMode & value) == value;
        }

        #endregion

        #region SelectedItemChanged RoutedEvent

        public static readonly RoutedEvent SelectedItemChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectedItemChanged", RoutingStrategy.Bubble, typeof(EventHandler<SelectedItemChangedEventArgs>), typeof(TreeGrid)
            );

        /// <summary>
        /// 选择项变更路由事件。
        /// 当 <see cref="SelectedItem"/> 属性被变更时，此事件会被触发。
        /// 
        /// 注意，TreeView 本身的 SelectedItem 及 SelectedItemChanged 事件已经不再使用。
        /// </summary>
        public event EventHandler<SelectedItemChangedEventArgs> SelectedItemChanged
        {
            add { AddHandler(SelectedItemChangedEvent, value); }
            remove { RemoveHandler(SelectedItemChangedEvent, value); }
        }

        #endregion

        #region 实现 IsCheckingOpend

        #region CheckingColumnHeader DependencyProperty

        public static readonly DependencyProperty CheckingColumnHeaderProperty = DependencyProperty.Register(
            "CheckingColumnHeader", typeof(string), typeof(TreeGrid)
            );

        public string CheckingColumnHeader
        {
            get { return (string)this.GetValue(CheckingColumnHeaderProperty); }
            set { this.SetValue(CheckingColumnHeaderProperty, value); }
        }

        #endregion

        private const int CHECKING_COLUMN_INDEX = 0;

        private void DisableChecking()
        {
            this.Columns.RemoveAt(CHECKING_COLUMN_INDEX);
        }

        private void EnableChecking()
        {
            var column = new ReadonlyTreeGridColumn
            {
                Width = 50,
                HeaderLabel = this.CheckingColumnHeader
            };

            this.Columns.Insert(CHECKING_COLUMN_INDEX, column);

            this.ResetCheckingTemplate();
        }

        private void ResetCheckingTemplate()
        {
            var column = this.Columns[CHECKING_COLUMN_INDEX];

            column.CellContentTemplate = this.CheckingColumnTemplate;
        }

        #endregion

        #region 勾选行 - 核心方法

        /// <summary>
        /// 设置某行的选择状态，并级联选择
        /// </summary>
        /// <param name="row"></param>
        /// <param name="value"></param>
        internal void CheckRowWithCascade(TreeGridRow row, bool value)
        {
            if (!this.IsCheckingEnabled) { return; }

            var entity = row.DataContext;

            this.CheckingModel.MarkSelected(row, value);
            this.RasieCheckChanged(entity, value);

            //CascadeParent 模式下，需要把所有父对象选中。
            if (value && this.NeedCascade(CheckingCascadeMode.CascadeParent))
            {
                object parent = this.GetParentItem(entity);

                while (parent != null)
                {
                    var parentRow = this.FindRow(parent);
                    if (parentRow == null || parentRow.IsChecked) { break; }

                    this.CheckingModel.MarkSelected(parentRow, true);
                    this.RasieCheckChanged(parent, true);

                    parent = this.GetParentItem(parent);
                }
            }

            //CascadeChildren 模式下，需要把所有子对象选中。
            if (this.NeedCascade(CheckingCascadeMode.CascadeChildren))
            {
                this.CheckChildrenRecur(entity, value);
            }
        }

        /// <summary>
        /// 迭归设置指定行的子行列表的选择状态
        /// </summary>
        /// <param name="rowEntity"></param>
        /// <param name="value"></param>
        private void CheckChildrenRecur(object rowEntity, bool value)
        {
            var children = this.GetChildItems(rowEntity);

            if (children != null && children.Any())
            {
                foreach (var child in children)
                {
                    this.CheckingModel.MarkItemSelected(child, value);

                    this.RasieCheckChanged(child, value);

                    this.CheckChildrenRecur(child, value);
                }
            }
        }

        /// <summary>
        /// 向外界抛出 CheckChangedEvent 事件。
        /// </summary>
        /// <param name="rowEntity"></param>
        /// <param name="isChecked"></param>
        private void RasieCheckChanged(object rowEntity, bool isChecked)
        {
            //注册到汇总勾选事件中，最后统一触发。
            using (this.BatchChecking())
            {
                this.RegisterBatchCheckingItem(rowEntity, isChecked);
            }
        }

        #region 多选时的汇总事件。

        /*********************** 代码块解释 *********************************
         * 
         * 以下可以在多行进行同时勾选时，例如：全选时，在最后才发生一个汇总事件。
         * 
         **********************************************************************/

        private int _batchCheckingVersion;

        private CheckItemsChangedEventArgs _batchArgs;

        private bool IsBatchChecking
        {
            get { return this._batchCheckingVersion > 0; }
        }

        internal IDisposable BatchChecking()
        {
            this.BeginBatchChecking();
            return new BatchCheckingDelegate { TreeGrid = this };
        }

        private void BeginBatchChecking()
        {
            if (this._batchCheckingVersion == 0)
            {
                this._batchArgs = new CheckItemsChangedEventArgs
                {
                    RoutedEvent = CheckItemsChangedEvent,
                    Source = this
                };
            }

            this._batchCheckingVersion++;
        }

        private void RegisterBatchCheckingItem(object rowEntity, bool isChecked)
        {
            if (this.IsBatchChecking)
            {
                var items = isChecked ? this._batchArgs.NewItems : this._batchArgs.OldItems;
                items.Add(rowEntity);
            }
        }

        private void EndBatchChecking()
        {
            this._batchCheckingVersion--;
            if (this._batchCheckingVersion == 0)
            {
                if (this._batchArgs.NewItems.Count > 0 || this._batchArgs.OldItems.Count > 0)
                {
                    this.RaiseEvent(this._batchArgs);
                }

                this._batchArgs = null;
            }
        }

        /// <summary>
        /// 当某些行被选中或者被取消选中时，触发此冒泡事件。
        /// </summary>
        public static readonly RoutedEvent CheckItemsChangedEvent = EventManager.RegisterRoutedEvent("CheckItemsChanged", RoutingStrategy.Bubble, typeof(CheckItemsChangedEventHandler), typeof(TreeGrid));

        /// <summary>
        /// 当某些行同时被选中或者被取消选中时，触发此冒泡事件。
        /// </summary>
        public event CheckItemsChangedEventHandler CheckItemsChanged
        {
            add { this.AddHandler(CheckItemsChangedEvent, value); }
            remove { this.RemoveHandler(CheckItemsChangedEvent, value); }
        }

        private class BatchCheckingDelegate : IDisposable
        {
            internal TreeGrid TreeGrid;
            void IDisposable.Dispose()
            {
                this.TreeGrid.EndBatchChecking();
            }
        }

        #endregion

        #endregion

        #region 选择行 - Shift / Ctrl 多选

        /// <summary>
        /// 使用 SelectedItems 集合添加项时，需要强制进入多选模式。
        /// </summary>
        private bool _forceMultiSelect = false;

        private object _shiftStartItem;

        private void ShiftSelectStart(bool shiftPressed, object oldItem, object newItem)
        {
            if (shiftPressed)
            {
                //如果还没有开始 Shift 选择。
                if (this._shiftStartItem == null)
                {
                    //如果是点击后第一次按住 Shift，则当前已经有选择项，设置该选择项为起始项。
                    if (oldItem != null)
                    {
                        this._shiftStartItem = oldItem;
                    }
                    //如果是按住 Shift 后第一次点击，则设置点击项为起始项。
                    else
                    {
                        this._shiftStartItem = newItem;
                    }
                }
                else
                {
                    //如果之前已经开始了 Shift 选择，并且该起始项和 newItem 并不在同一个父节点下，则重新进行 Shift 多选。
                    if (GetParentItem(this._shiftStartItem) != GetParentItem(newItem))
                    {
                        this._shiftStartItem = newItem;
                    }
                }
            }
            else
            {
                //如果没有按住 Shift，则清空起始项。
                this._shiftStartItem = null;
            }
        }

        /// <summary>
        /// Shift 键多选
        /// </summary>
        /// <param name="endItem"></param>
        private void ShiftSelectEnd(object endItem)
        {
            var endRow = this.FindRow(endItem);

            //如果后选的这条数据没有生成对应的行，则暂时无法继续 Shift 多选操作，直接返回。
            if (endRow == null) { return; }

            var parentItemsControl = endRow.ParentItemsControl;
            if (parentItemsControl == null) { return; }

            var items = parentItemsControl.Items;

            //查找区间值
            int startIndex = items.IndexOf(this._shiftStartItem);
            int endIndex = items.IndexOf(endItem);

            int start = Math.Min(startIndex, endIndex);
            //负数表示没有找到对应的元素，也就是说这两个节点并不在同一个 ItemCollection 中，不能多选。
            if (start < 0) return;

            int end = Math.Max(startIndex, endIndex);

            //由于之前已经有过一次 Select 操作，所以这个集合并不是空的，
            //直接添加会导致集合中的数据的顺序不一致。
            var sm = this.SelectionModel;

            for (int i = start; i <= end; i++)
            {
                var item = items.GetItemAt(i);
                sm.MarkItemSelected(item, true);
            }
        }

        #endregion

        #region 选择行 - 核心方法

        /*********************** 代码块解释 *********************************
         * 
         * 树结点的选择事件处理
         * 
         * 基类的此方法以一种防止 TreeView 重入的方式来同步 this.SelectedItem 到 TreeView.SelecteItem 上。
         * 并在最后调用 this.OnSelectedItemChanged 方法，以抛出路由事件。
         * 
         * 注意这三个方法的顺序：
         * OnTreeViewSelectedItemChanged
         * OnSelectedItemPropertyChanged
         * RaiseSelectedItemChanged
         * 
        **********************************************************************/

        /// <summary>
        /// Copy from TreeView class.
        /// </summary>
        /// <param name="collapsed"></param>
        internal void HandleSelectionOnCollapsed(TreeGridRow collapsed)
        {
            using (this.SelectionProcess.TryEnterProcess(TGSelectionProcess.Row_Collapsed))
            {
                if (this.SelectionProcess.Success)
                {
                    //目前只支持在单选的时候，提供此功能。
                    if (this.SelectParentOnCollapsed && this.SelectionModel.InnerItems.Count == 1)
                    {
                        var selectedRow = this.SelectedRow;
                        if (selectedRow != null && selectedRow != collapsed)
                        {
                            //检查 collapsed 是否为 selectedRow 的父行。
                            bool selectionInCollapsed = false;
                            for (var parent = selectedRow; parent != null; parent = parent.ParentRow)
                            {
                                //找到 collasped 对象
                                if (parent == collapsed)
                                {
                                    selectionInCollapsed = true;
                                    break;
                                }
                            }

                            if (selectionInCollapsed)
                            {
                                this.ChangeSelection(collapsed.DataContext, collapsed, true);

                                if (selectedRow.IsKeyboardFocusWithin)
                                {
                                    selectedRow.Focus();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Copy from TreeView class.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="newRow"></param>
        /// <param name="selected"></param>
        internal void ChangeSelection(object data, TreeGridRow newRow, bool selected)
        {
            //同步本对象的 SelectedItem 属性。
            if (selected)
            {
                this.SelectedItem = data;
            }
            else
            {
                if (this.SelectedItem == data)
                {
                    this.SelectedItem = null;
                }
            }
        }

        /// <summary>
        /// 在 SelectedItem 属性变更时，同步 TreeView 控件的选中项。
        /// </summary>
        /// <param name="e"></param>
        private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsInitialized) return;

            //展开根结点
            if (this.RootNode != null) this.RootNode.IsExpanded = true;

            object oldItem = e.OldValue;
            object newItem = e.NewValue;

            using (this.SelectionProcess.TryEnterProcess(TGSelectionProcess.SelectedItem))
            {
                this.SelectionCore(oldItem, newItem);
            }

            this.ExpandToView(newItem);

            //选择行变更时，需要把当前编辑行的状态清除。
            this.ExitEditing();

            this.ApplyAutoCollapse();

            this.RaiseSelectedItemChanged(oldItem, newItem);
        }

        /// <summary>
        /// 选择核心逻辑。
        /// 
        /// 处理多选及单选状态下的 SelectionModel 中数据状态。
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        private void SelectionCore(object oldItem, object newItem)
        {
            var ctrlPressed = TreeGridHelper.IsCtrlPressed();
            var shiftPressed = TreeGridHelper.IsShiftPressed();

            //当前是否正处于多选模式下。
            var inMultiSelectionMode = this._forceMultiSelect || ctrlPressed || shiftPressed;

            this.ShiftSelectStart(shiftPressed, oldItem, newItem);

            //如果不是多选状态，则先清空选中行状态。
            var sm = this.SelectionModel;
            if (!inMultiSelectionMode) { sm.ClearSelectedItems(); }

            if (newItem != null)
            {
                if (ctrlPressed || this._forceMultiSelect)
                {
                    //反选该行
                    var selected = sm.InnerItems.Contains(newItem);
                    sm.MarkItemSelected(newItem, !selected);
                }
                else if (shiftPressed)
                {
                    sm.ClearSelectedItems();

                    if (this._shiftStartItem != null)
                    {
                        this.ShiftSelectEnd(newItem);
                    }
                }
                else
                {
                    sm.MarkItemSelected(newItem, true);
                }
            }
            else
            {
                if (!this._forceMultiSelect) sm.ClearSelectedItems();
            }
        }

        /// <summary>
        /// 抛出路由事件
        /// </summary>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        private void RaiseSelectedItemChanged(object oldItem, object newItem)
        {
            var args = new SelectedItemChangedEventArgs
            {
                RoutedEvent = TreeGrid.SelectedItemChangedEvent,
                Source = this,
                OldItem = oldItem,
                NewItem = newItem
            };

            this.RaiseEvent(args);
        }

        #endregion

        #region class SelectedItemsAPI & TreeGridSelectionModel

        /// <summary>
        /// 把 AbstractSelectionModel 和 MTTG 封装出一些可以给外界直接操作集合而达到操作控件的方法。
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        private class SelectedItemsAPI : IList, ISelectionItemsAPI
        {
            internal TreeGridSelectionModel _model;

            private bool TryAddSelection(object item)
            {
                var grid = this._model.Owner;

                //根据模式设置选择行。
                if (this._model.IsChecking)
                {
                    this._model.MarkItemSelected(item, true);

                    //外部接口调用的方式，也需要触发 CheckChanged 事件。
                    grid.RasieCheckChanged(item, true);
                }
                else
                {
                    try
                    {
                        grid._forceMultiSelect = true;

                        //如果还没有当前选择项，则直接设置当前项，否则直接添加该项到集合中。
                        if (grid.SelectedItem == null)
                        {
                            using (grid.SelectionProcess.TryEnterProcess(TGSelectionProcess.SelectionModel))
                            {
                                grid.SelectedItem = item;
                            }
                        }
                        else
                        {
                            this._model.MarkItemSelected(item, true);
                        }
                    }
                    finally
                    {
                        this._model.Owner._forceMultiSelect = false;
                    }
                }

                return true;
            }

            private bool TryDeselectEntity(object item)
            {
                this._model.MarkItemSelected(item, false);

                //外部接口调用的方式，也需要触发 CheckChanged 事件。
                if (this._model.IsChecking) { this._model.Owner.RasieCheckChanged(item, false); }

                return true;
            }

            /// <summary>
            /// 清空所有选择行。
            /// </summary>
            private void TryDeselectAll()
            {
                this._model.ClearSelectedItems();

                //如果是选择模式下，还应该同步 SelectedItem 的值。
                if (!this._model.IsChecking)
                {
                    var grid = this._model.Owner;
                    using (grid.SelectionProcess.TryEnterProcess(TGSelectionProcess.SelectionModel))
                    {
                        grid.SelectedItem = null;
                    }
                }
            }

            #region IList Members

            public int IndexOf(object item)
            {
                return this._model.InnerItems.IndexOf(item);
            }

            public void Insert(int index, object item)
            {
                throw new NotSupportedException("暂时不支持对选择项进行此操作。");
            }

            public void Remove(object item)
            {
                this.TryDeselectEntity(item);
            }

            public void RemoveAt(int index)
            {
                this.Remove(this[index]);
            }

            public object this[int index]
            {
                get { return this._model.InnerItems[index]; }
                set { this._model.InnerItems[index] = value; }
            }

            public int Add(object item)
            {
                this.TryAddSelection(item);
                return this.Count - 1;
            }

            public void Clear()
            {
                this.TryDeselectAll();
            }

            public bool Contains(object item)
            {
                return this._model.InnerItems.Contains(item);
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                this._model.InnerItems.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return this._model.InnerItems.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool IsFixedSize
            {
                get { return (this._model.InnerItems as IList).IsFixedSize; }
            }

            object IList.this[int index]
            {
                get { return this[index]; }
                set { this[index] = value as object; }
            }

            public void CopyTo(Array array, int index)
            {
                (this._model.InnerItems as IList).CopyTo(array, index);
            }

            public bool IsSynchronized
            {
                get { return (this._model.InnerItems as IList).IsSynchronized; }
            }

            public object SyncRoot
            {
                get { return (this._model.InnerItems as IList).SyncRoot; }
            }

            public IEnumerator GetEnumerator()
            {
                return this._model.InnerItems.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._model.InnerItems.GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        /// 选择模型
        /// 
        /// 此设计方案来自于 ExtJs
        /// </summary>
        public class TreeGridSelectionModel
        {
            /// <summary>
            /// 当前选中的所有项都存储在这个列表中。
            /// </summary>
            internal List<object> InnerItems = new List<object>();

            /// <summary>
            /// 对应的 MTTG 控件
            /// </summary>
            internal TreeGrid Owner;

            /// <summary>
            /// 当前对象是勾选控制器，还是选择项控制器
            /// </summary>
            internal bool IsChecking { get; private set; }

            internal TreeGridSelectionModel(TreeGrid owner, bool isChecking)
            {
                this.Owner = owner;
                this.SelectedItems = new SelectedItemsAPI { _model = this };
                this.IsChecking = isChecking;
            }

            #region 几个主要的控制接口

            /// <summary>
            /// 一个选择项的集合
            /// 
            /// 可直接使用本集合来控制选中项。(这个集合其实只是一个代理，用于提供方便的 API。）
            /// </summary>
            public ISelectionItemsAPI SelectedItems { get; protected set; }

            /// <summary>
            /// 选择全部行。
            /// </summary>
            public void SelectAll()
            {
                using (this.BeginBatchSelection())
                {
                    this.InnerItems.Clear();

                    //直接把所有数据加入到列表中
                    var source = this.Owner.RootItems;
                    if (source != null)
                    {
                        foreach (object entity in this.Owner.RootItems)
                        {
                            this.MarkItemSelected(entity, true);

                            //外部接口调用的方式，也需要触发 CheckChanged 事件。
                            if (this.IsChecking) { this.Owner.RasieCheckChanged(entity, true); }
                        }
                    }
                }
            }

            /// <summary>
            /// 如果是批量操作，可以使用此接口来统一处理最后的事件。
            /// </summary>
            /// <returns></returns>
            public IDisposable BeginBatchSelection()
            {
                if (this.IsChecking)
                {
                    return this.Owner.BatchChecking();
                }

                //内部实现，暂时只支持 Check，不支持 Select。
                //这是因为现在使用 SelectAll 时，也不发生 SelectedItemChanged 事件，所以以后再一并支持。
                return new EmptyDisposer();
            }

            #endregion

            /// <summary>
            /// 清空所有选择项。
            /// 
            /// 清空 InnerItems，设置所有勾选行的选择状态属性。
            /// </summary>
            internal void ClearSelectedItems()
            {
                for (int i = this.InnerItems.Count - 1; i >= 0; i--)
                {
                    var item = this.InnerItems[i];

                    this.MarkItemSelected(item, false);

                    //外部接口调用的方式，也需要触发 CheckChanged 事件。
                    if (this.IsChecking) { this.Owner.RasieCheckChanged(item, false); }
                }
            }

            /// <summary>
            /// 设置某行的“选中”状态
            /// 
            /// 维护两个底层属性：一个是选择项 List，一个是 row 的选择状态属性。
            /// </summary>
            /// <param name="row"></param>
            /// <param name="isSelected">是“选中”状态还是“未选中”状态。</param>
            internal void MarkSelected(TreeGridRow row, bool isSelected)
            {
                this.ModifyList(row.DataContext, isSelected);

                this.ModifyRowProperty(row, isSelected);
            }

            /// <summary>
            /// 设置某行的“选中”状态
            /// 
            /// 维护两个底层属性：一个是选择项 List，一个是 row 的选择状态属性。
            /// </summary>
            /// <param name="item"></param>
            /// <param name="isSelected">是“选中”状态还是“未选中”状态。</param>
            internal void MarkItemSelected(object item, bool isSelected)
            {
                this.ModifyList(item, isSelected);

                var row = this.Owner.FindRow(item);
                if (row != null) this.ModifyRowProperty(row, isSelected);
            }

            /// <summary>
            /// 根据对应项的选中状态，维护选择项列表。
            /// </summary>
            /// <param name="item"></param>
            /// <param name="isSelected"></param>
            internal void ModifyList(object item, bool isSelected)
            {
                if (isSelected)
                {
                    if (!this.InnerItems.Contains(item)) { this.InnerItems.Add(item); }
                }
                else
                {
                    this.InnerItems.Remove(item);
                }
            }

            /// <summary>
            /// 设置行对应的选中状态属性
            /// </summary>
            /// <param name="row"></param>
            /// <param name="isSelected"></param>
            internal void ModifyRowProperty(TreeGridRow row, bool isSelected)
            {
                if (this.IsChecking)
                {
                    row.SetIsCheckedField(isSelected);
                }
                else
                {
                    //然后，需要设置 TreeListItem.IsSelected 属性。
                    //否则，会造成内部使用的 TreeView 的 SelectedItem 属性不变，但是高亮的状态已经被清除，
                    //这时用户再次点击该行时，TreeView 的 SelectedItemChanged 事件不会发生，
                    //也就不会发生整个 MTTG 的 SelectedItem 处理流程，造成无法选中该行。
                    //具体情况见 bug：http://ipm.grandsoft.com.cn/issues/247260
                    row.SetIsSelectedField(isSelected);
                }
            }

            private class EmptyDisposer : IDisposable
            {
                void IDisposable.Dispose() { }
            }
        }

        #endregion
    }

    /// <summary>
    /// TreeGrid 选择逻辑的起始流程
    /// </summary>
    internal enum TGSelectionProcess
    {
        /// <summary>
        /// 还没有进入任何选择逻辑。
        /// </summary>
        None,
        /// <summary>
        /// 直接设置 TreeGridRow.IsSelected 属性而引发的选择逻辑。
        /// </summary>
        Row_IsSelected,
        /// <summary>
        /// 行被折叠而导致发生的选择逻辑。
        /// </summary>
        Row_Collapsed,
        /// <summary>
        /// 通过设置 SelectedItem 属性来操作选择项。
        /// </summary>
        SelectedItem,
        /// <summary>
        /// 通过 SelectionModel API 直接操作选择项。
        /// </summary>
        SelectionModel
    }

    /// <summary>
    /// 一个选择项的集合
    /// </summary>
    /// <typeparam name="object"></typeparam>
    public interface ISelectionItemsAPI : IList { }

    /// <summary>
    /// 勾选项变更事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CheckItemsChangedEventHandler(object sender, CheckItemsChangedEventArgs e);

    /// <summary>
    /// 勾选项变更事件参数
    /// </summary>
    public class CheckItemsChangedEventArgs : RoutedEventArgs
    {
        public CheckItemsChangedEventArgs()
        {
            this.OldItems = new List<object>();
            this.NewItems = new List<object>();
        }

        /// <summary>
        /// 当前被反勾选上的对象
        /// </summary>
        public IList<object> OldItems { get; private set; }

        /// <summary>
        /// 当前被勾选上的对象
        /// </summary>
        public IList<object> NewItems { get; private set; }

        /// <summary>
        /// 遍历所有的项
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CheckItem> EnumerateItems()
        {
            foreach (var item in this.OldItems)
            {
                yield return new CheckItem { Item = item, IsChecked = false };
            }
            foreach (var item in this.NewItems)
            {
                yield return new CheckItem { Item = item, IsChecked = true };
            }
        }

        public struct CheckItem
        {
            public object Item;
            public bool IsChecked;
        }
    }
}