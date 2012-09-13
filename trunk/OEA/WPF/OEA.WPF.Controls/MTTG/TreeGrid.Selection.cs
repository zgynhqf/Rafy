/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110621
 * 说明：此文件只包含一个类，具体内容见类型注释。
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// MTTG 控件有两种选择模式，一种是直接选择行，一种是使用勾选框进行选择。
    /// </summary>
    public partial class TreeGrid
    {
        private void OnSelectionConstruct()
        {
            this.CheckingModel = new CheckSelectionModel(this);
            this.SelectionModel = new NormalSelectionModel(this);
        }

        private void OnRebinding_Selection()
        {
            this.CheckingModel.ClearSelection();

            //不能清空 SelectionModel，否则会造成在刷新控件时，丢失当前布局而无法保留当前选中行。
            //this.SelectionModel.ClearSelection();
        }

        /// <summary>
        /// 直接选择行的选择模型。
        /// </summary>
        public AbstractSelectionModel SelectionModel { get; private set; }

        /// <summary>
        /// 在 CheckingMode.Row 模式（勾选行模式）下，勾选的选择模型。
        /// </summary>
        public AbstractSelectionModel CheckingModel { get; private set; }

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
            if (this.IsCheckingRow)
            {
                this.ResetCheckingTemplate();
            }
        }

        #endregion

        #region IsCheckingRow DependencyProperty

        public static readonly DependencyProperty CheckingModeProperty = DependencyProperty.Register(
            "IsCheckingRow", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata(false, (d, e) => (d as TreeGrid).OnCheckingModeChanged(e))
            );

        /// <summary>
        /// 当前表格控件是否已经打开选择功能
        /// </summary>
        public bool IsCheckingRow
        {
            get { return (bool)this.GetValue(CheckingModeProperty); }
            set { this.SetValue(CheckingModeProperty, value); }
        }

        private void OnCheckingModeChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            if (value)
            {
                this.EnableChecking();
            }
            else
            {
                this.DisableSelect();
            }
        }

        #endregion

        #region CheckingRowCascade DependencyProperty

        public static readonly DependencyProperty CheckingRowCascadeProperty = DependencyProperty.Register(
            "CheckingRowCascade", typeof(CheckingRowCascade), typeof(TreeGrid),
            new PropertyMetadata(CheckingRowCascade.None, (d, e) => (d as TreeGrid).OnCheckingRowCascadeChanged(e))
            );

        /// <summary>
        /// 此属性的设置不会影响当前已有的选择项，只对未来的选择行为起作为。
        /// </summary>
        public CheckingRowCascade CheckingRowCascade
        {
            get { return (CheckingRowCascade)this.GetValue(CheckingRowCascadeProperty); }
            set { this.SetValue(CheckingRowCascadeProperty, value); }
        }

        private void OnCheckingRowCascadeChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (CheckingRowCascade)e.NewValue;
        }

        private bool NeedCascade(CheckingRowCascade value)
        {
            return (this.CheckingRowCascade & value) == value;
        }

        #endregion

        #region 实现 IsCheckingRow

        private const int CHECKING_COLUMN_INDEX = 0;

        private void DisableSelect()
        {
            this.Columns.RemoveAt(CHECKING_COLUMN_INDEX);
        }

        private void EnableChecking()
        {
            //创建选择列
            var column = this.CreateCheckingColumn();

            this.Columns.Insert(CHECKING_COLUMN_INDEX, column);

            this.ResetCheckingTemplate();
        }

        protected abstract TreeGridColumn CreateCheckingColumn();

        private void ResetCheckingTemplate()
        {
            var column = this.Columns[CHECKING_COLUMN_INDEX];

            column.CellTemplate = this.CheckingColumnTemplate;
        }

        #endregion

        #region 勾选行 - 核心方法

        /// <summary>
        /// 设置某行的选择状态，并级联选择
        /// </summary>
        /// <param name="row"></param>
        /// <param name="value"></param>
        internal void CheckRowWithCascade(GridTreeViewRow row, bool value)
        {
            var argsCache = new CheckChangedEventArgs
            {
                RoutedEvent = CheckChangedEvent,
                Source = this,
            };

            var entity = GetEntity(row);

            this.CheckingModel.SelectRow(row, value);
            RaiseEventAsChecked(argsCache, entity, value);

            if (!this.IsCheckingRow) { return; }

            //CascadeParent
            if (value && this.NeedCascade(CheckingRowCascade.CascadeParent))
            {
                object parent = this.GetParentItem(entity);

                while (parent != null)
                {
                    var parentRow = this.FindRow(parent);
                    if (parentRow == null || parentRow.IsChecked) { break; }

                    this.CheckingModel.SelectRow(parentRow, true);
                    RaiseEventAsChecked(argsCache, parent, true);

                    parent = this.GetParentItem(parent);
                }
            }

            //CascadeChildren
            if (this.NeedCascade(CheckingRowCascade.CascadeChildren))
            {
                this.CheckChildrenRecur(row, entity, value, argsCache);
            }
        }

        /// <summary>
        /// 迭归设置指定行的子行列表的选择状态
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowEntity"></param>
        /// <param name="value"></param>
        /// <param name="argsCache"></param>
        private void CheckChildrenRecur(GridTreeViewRow row, object rowEntity, bool value, CheckChangedEventArgs argsCache)
        {
            var children = this.GetChildItems(rowEntity);

            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                {
                    var childRow = this.FindRow(child);

                    //如果该孩子对象对应的行已经生成，则直接选择。
                    if (childRow != null)
                    {
                        this.CheckingModel.SelectRow(childRow, value);
                    }
                    else
                    {
                        //还没有生成，把它加到选择列表中，下次生成时会继续选中。见 SelectAsCreated 方法。
                        if (value)
                        {
                            this.CheckingModel.AddToItems(child);
                        }
                        else
                        {
                            this.CheckingModel.Items.Remove(child);
                        }
                    }

                    this.RaiseEventAsChecked(argsCache, child, value);
                    this.CheckChildrenRecur(childRow, child, value, argsCache);
                }
            }
        }

        private void RaiseEventAsChecked(CheckChangedEventArgs e, object rowEntity, bool isChecked)
        {
            e.Item = rowEntity;
            e.IsChecked = isChecked;

            this.RaiseEvent(e);
        }

        public static readonly RoutedEvent CheckChangedEvent = EventManager.RegisterRoutedEvent("CheckChanged", RoutingStrategy.Bubble, typeof(CheckChangedEventHandler), typeof(TreeGrid));

        public event CheckChangedEventHandler CheckChanged
        {
            add
            {
                this.AddHandler(CheckChangedEvent, value);
            }
            remove
            {
                this.RemoveHandler(CheckChangedEvent, value);
            }
        }

        #endregion

        #region 选择行 - Shift / Ctrl 多选

        /// <summary>
        /// 使用 SelectedItems 集合添加项时，需要强制进入多选模式。
        /// </summary>
        private bool _forceMultiSelect = false;

        private GridTreeViewRow _shiftStartItem;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    OnShiftKeyDown();
                    break;
            }

            base.OnKeyDown(e);
        }

        private void OnShiftKeyDown()
        {
            var selectedItems = this.SelectionModel.Items;
            if (selectedItems.Count > 0)
            {
                if (this._shiftStartItem != null)
                {
                    if (selectedItems.Contains(GetEntity(this._shiftStartItem))) { return; }
                }

                var lastOne = selectedItems[selectedItems.Count - 1];
                this._shiftStartItem = this.FindRow(lastOne);
            }
            else
            {
                this._shiftStartItem = null;
            }
        }

        private void MultiSelectionCore(SelectedItemChangedEventArgs e)
        {
            //如果不是 SetSelectedValue 导致的变更事件，则表示是用户点击等界面操作引起，
            //这时，需要考虑多选、勾选，并同步到 IsMultiSelected 值上。
            if (!IsCtrlPressed() && !IsShiftPressed())
            {
                var oldRow = this.FindRow(e.OldItem);
                if (oldRow != null)
                {
                    this.SelectionModel.SelectRow(oldRow, false);
                }
            }

            if (e.NewItem != null)
            {
                var newRow = this.FindRow(e.NewItem);
                if (newRow != null)
                {
                    //如果没有按住 Ctrl，并且在 CheckRow 的模式下，则清空的选择项。
                    //（在 CheckRow 的模式下，单击某一行并不需要清空历史记录。）
                    if (!IsCtrlPressed() && !this._forceMultiSelect) this.SelectionModel.ClearSelectedItems();

                    //反选该行
                    this.SelectionModel.SelectRow(newRow, !newRow.IsMultiSelected);

                    if (this._shiftStartItem != null && IsShiftPressed())
                    {
                        this.SelectRowsOnShift(newRow);
                    }

                    this.ClearEditingCellOnSelectionChanged();
                }
            }
            else
            {
                //在 CheckRow 的模式下，未选中某一行并不需要清空历史记录，其它情况均需要清空。
                if (!this._forceMultiSelect) this.SelectionModel.ClearSelectedItems();
            }
        }

        /// <summary>
        /// Shift键多选
        /// </summary>
        /// <param name="row"></param>
        private void SelectRowsOnShift(GridTreeViewRow row)
        {
            var tiParent = ItemsControl.ItemsControlFromItemContainer(this._shiftStartItem);
            var itemsALayer = tiParent.Items;

            //查找区间值
            int startIndex = itemsALayer.IndexOf(this._shiftStartItem);
            int endIndex = itemsALayer.IndexOf(row);
            int start = Math.Min(startIndex, endIndex);
            int end = Math.Max(startIndex, endIndex);

            //没有跨级别才能进行选择
            if (start >= 0 && this._shiftStartItem.Level == row.Level)
            {
                //由于之前已经有过一次Select操作，所以这个集合并不是空的，
                //直接添加会导致集合中的数据的顺序不一致。
                this.SelectionModel.Items.Clear();

                for (int i = start; i <= end; i++)
                {
                    this.SelectionModel.SelectRow(itemsALayer.GetItemAt(i) as GridTreeViewRow, true);
                }
            }
        }

        private static bool IsCtrlPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        private static bool IsShiftPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }

        #endregion

        #region 选择行 - 核心方法

        public void SelectAll()
        {
            this.SelectionModel.SelectAll();
        }

        public void ClearSelection()
        {
            this.SelectionModel.ClearSelection();
        }

        /// <summary>
        /// 整个选择的源头：TreeView 的 SelectedItemChanged 事件。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTreeViewSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            //如果 IsSettingInternal值为真，说明是来自于最下层的 SelectRow 方法，则不需要进行任何处理！
            var newRow = (e.NewValue ?? e.OldValue) as GridTreeViewRow;
            if (newRow.IsSettingInternal) { return; }

            base.OnTreeViewSelectedItemChanged(e);
        }

        protected override void RaiseSelectedItemChanged(SelectedItemChangedEventArgs e)
        {
            this.MultiSelectionCore(e);

            //抛出路由事件
            base.RaiseSelectedItemChanged(e);
        }

        internal override void SelectedRowOnCreated(TreeViewItem row)
        {
            /*********************** 代码块解释 *********************************
             * 由于在新创建某一行时，设置它的 IsSelected 属性并不会造成 OnTreeViewSelectedItemChanged 事件的发生，
             * 所以也就不会同步到它的 IsMultiSelected 属性上，这样的情况，需要特别处理。
            **********************************************************************/

            base.SelectedRowOnCreated(row);

            (row as GridTreeViewRow).IsMultiSelected = true;
        }

        #endregion

        #region 选择行 - 其它

        /// <summary>
        /// 当重新生成某一行时，如果是已经选中的实体，需要初始化它们的选中状态
        /// </summary>
        /// <param name="row"></param>
        /// <param name="item"></param>
        private void SelectAsCreated(GridTreeViewRow row, object item)
        {
            if (this.SelectionModel.Items.Contains(item)) { this.SelectionModel.SelectRow(row, true); }
        }

        #endregion

        #region public abstract class AbstractSelectionModel

        /// <summary>
        /// 选择模型
        /// 
        /// 此设计方案来自于 ExtJs
        /// </summary>
        public abstract class AbstractSelectionModel
        {
            /// <summary>
            /// 当前选中的所有项都存储在这个列表中。
            /// </summary>
            internal List<object> Items = new List<object>();

            /// <summary>
            /// 对应的 MTTG 控件
            /// </summary>
            internal TreeGrid Owner;

            #region 三个主要的控制接口

            /// <summary>
            /// 一个选择项的集合
            /// 
            /// 可直接使用本集合来控制选中项。(这个集合其实只是一个代理，用于提供方便的 API。）
            /// </summary>
            public ISelectionItems SelectedItems { get; protected set; }

            /// <summary>
            /// 选择全部行。
            /// </summary>
            public virtual void SelectAll()
            {
                this.Items.Clear();

                foreach (object entity in this.Owner._itemsSource)
                {
                    this.Items.Add(entity);

                    var row = this.Owner.FindRow(entity);
                    if (row != null) this.SelectRow(row, true);
                }
            }

            /// <summary>
            /// 清空所有选择行。
            /// </summary>
            public abstract void ClearSelection();

            internal void ClearSelectedItems()
            {
                for (int i = this.Items.Count - 1; i >= 0; i--)
                {
                    var item = this.Items[i];

                    var viewItem = this.Owner.FindRow(item);
                    if (viewItem != null)
                    {
                        this.SelectRow(viewItem, false);
                    }
                    else
                    {
                        this.Items.RemoveAt(i);
                    }
                }
            }

            #endregion

            /// <summary>
            /// 设置某行的“选中”状态
            /// </summary>
            /// <param name="row"></param>
            /// <param name="isSelected">是“选中”状态还是“未选中”状态。</param>
            internal virtual void SelectRow(GridTreeViewRow row, bool isSelected)
            {
                /*********************** 代码块解释 *********************************
                 * 同时处理以下几个属性：
                 * this._selectedItems、row.IsMultiSelected、row.IsSelected
                **********************************************************************/

                if (isSelected)
                {
                    this.AddToItems(GetEntity(row));
                }
                else
                {
                    this.Items.Remove(GetEntity(row));
                }

                try
                {
                    row.IsSettingInternal = true;

                    this.SetRowSelectedProperty(row, isSelected);
                }
                finally
                {
                    row.IsSettingInternal = false;
                }
            }

            protected abstract void SetRowSelectedProperty(GridTreeViewRow row, bool isSelected);

            internal bool AddToItems(object entity)
            {
                if (!this.Items.Contains(entity))
                {
                    this.Items.Add(entity);
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region private class NormalSelectionModel

        private class NormalSelectionModel : AbstractSelectionModel
        {
            public NormalSelectionModel(TreeGrid owner)
            {
                this.Owner = owner;

                this.SelectedItems = new MTTGSelectedItemsCollection { _model = this };
            }

            public override void ClearSelection()
            {
                this.ClearSelectedItems();

                this.Owner.SelectedItem = null;
            }

            protected override void SetRowSelectedProperty(GridTreeViewRow row, bool isSelected)
            {
                //设置这个属性，皮肤中会绑定此属性来实现界面高亮显示
                row.IsMultiSelected = isSelected;

                //然后，需要设置 TreeListItem.IsSelected 属性。
                //否则，会造成内部使用的 TreeView 的 SelectedItem 属性不变，但是高亮的状态已经被清除，
                //这时用户再次点击该行时，TreeView 的 SelectedItemChanged 事件不会发生，
                //也就不会发生整个 MTTG 的 SelectedItem 处理流程，造成无法选中该行。
                //具体情况见 bug：http://ipm.grandsoft.com.cn/issues/247260
                row.IsSelected = isSelected;
            }
        }

        #endregion

        #region private class CheckSelectionModel

        private class CheckSelectionModel : AbstractSelectionModel
        {
            public CheckSelectionModel(TreeGrid owner)
            {
                this.Owner = owner;

                this.SelectedItems = new MTTGCheckedItemsCollection { _model = this };
            }

            public override void ClearSelection()
            {
                this.ClearSelectedItems();
            }

            protected override void SetRowSelectedProperty(GridTreeViewRow row, bool isSelected)
            {
                row.IsChecked = isSelected;
            }
        }

        #endregion

        #region private class MTTGSelectedItemsCollection

        /// <summary>
        /// 把 AbstractSelectionModel 和 MTTG 封装出一些可以给外界直接操作集合而达到操作控件的方法。
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        private class MTTGSelectedItemsCollection : IList, ISelectionItems
        {
            internal AbstractSelectionModel _model;

            private bool TryAddSelection(object item)
            {
                var row = this._model.Owner.FindOrGenerateNode(item);

                if (row != null)
                {
                    try
                    {
                        this._model.Owner._forceMultiSelect = true;

                        this.TryAddSelectionCore(item, row);
                    }
                    finally
                    {
                        this._model.Owner._forceMultiSelect = false;
                    }

                    return true;
                }

                return false;
            }

            protected virtual void TryAddSelectionCore(object item, GridTreeViewRow row)
            {
                //如果还没有当前选择项，则直接设置当前项，否则直接添加该项到集合中。
                if (this._model.Owner.SelectedItem == null)
                {
                    this._model.Owner.SelectedItem = item;
                }
                else
                {
                    this._model.SelectRow(row, true);
                }
            }

            private bool TryDeselectEntity(object item)
            {
                var row = this._model.Owner.FindRow(item);

                if (row != null) { this._model.SelectRow(row, false); }

                return true;
            }

            private void TryDeselectAll()
            {
                this._model.ClearSelection();
            }

            #region IList Members

            public int IndexOf(object item)
            {
                return this._model.Items.IndexOf(item);
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
                get { return this._model.Items[index]; }
                set { this._model.Items[index] = value; }
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
                return this._model.Items.Contains(item);
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                this._model.Items.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return this._model.Items.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool IsFixedSize
            {
                get { return (this._model.Items as IList).IsFixedSize; }
            }

            object IList.this[int index]
            {
                get { return this[index]; }
                set { this[index] = value as object; }
            }

            public void CopyTo(Array array, int index)
            {
                (this._model.Items as IList).CopyTo(array, index);
            }

            public bool IsSynchronized
            {
                get { return (this._model.Items as IList).IsSynchronized; }
            }

            public object SyncRoot
            {
                get { return (this._model.Items as IList).SyncRoot; }
            }

            public IEnumerator GetEnumerator()
            {
                return this._model.Items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._model.Items.GetEnumerator();
            }

            #endregion
        }

        #endregion

        #region private class MTTGCheckedItemsCollection

        private class MTTGCheckedItemsCollection : MTTGSelectedItemsCollection
        {
            protected override void TryAddSelectionCore(object item, GridTreeViewRow row)
            {
                this._model.SelectRow(row, true);
            }
        }

        #endregion
    }

    /// <summary>
    /// 一个选择项的集合
    /// </summary>
    /// <typeparam name="object"></typeparam>
    public interface ISelectionItems : IList { }

    /// <summary>
    /// 勾选项变更事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void CheckChangedEventHandler(object sender, CheckChangedEventArgs e);

    /// <summary>
    /// 勾选项变更事件参数
    /// </summary>
    public class CheckChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 当前被勾选上的对象
        /// </summary>
        public object Item { get; internal set; }

        /// <summary>
        /// 当前被反勾选上的对象
        /// </summary>
        public bool IsChecked { get; internal set; }
    }
}