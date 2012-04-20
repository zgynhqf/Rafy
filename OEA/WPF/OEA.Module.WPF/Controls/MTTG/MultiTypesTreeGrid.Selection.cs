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
using Hardcodet.Wpf.GenericTreeView;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;
using OEA.Module.WPF.Automation;

namespace OEA.Module.WPF.Controls
{
    public partial class MultiTypesTreeGrid
    {
        #region SelectedItems Property

        /// <summary>
        /// 当前选中的所有项都存储在这个列表中。
        /// </summary>
        private List<Entity> _selectedItems = new List<Entity>();

        private MTTGSelectedItemsCollection _selectedItemsAgent;

        /// <summary>
        /// 一个选择项的集合
        /// </summary>
        public ISelectionItems SelectedItems
        {
            get
            {
                if (this._selectedItemsAgent == null)
                {
                    this._selectedItemsAgent = new MTTGSelectedItemsCollection
                    {
                        _owner = this
                    };
                }

                return this._selectedItemsAgent;
            }
        }

        #endregion

        #region CheckingMode DependencyProperty

        public static readonly DependencyProperty CheckingModeProperty = DependencyProperty.Register(
            "CheckingMode", typeof(CheckingMode), typeof(MultiTypesTreeGrid),
            new PropertyMetadata(OEA.CheckingMode.None, (d, e) => (d as MultiTypesTreeGrid).OnCheckingModeChanged(e))
            );

        /// <summary>
        /// 当前表格控件是否已经打开选择功能
        /// </summary>
        public CheckingMode CheckingMode
        {
            get { return (CheckingMode)this.GetValue(CheckingModeProperty); }
            set { this.SetValue(CheckingModeProperty, value); }
        }

        private void OnCheckingModeChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (CheckingMode)e.NewValue;
            switch (value)
            {
                case CheckingMode.None:
                    this.DisableSelect();
                    break;
                case CheckingMode.CheckingViewModel:
                case CheckingMode.CheckingRow:
                    this.EnableChecking(value);
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region CheckingRowCascade DependencyProperty

        public static readonly DependencyProperty CheckingRowCascadeProperty = DependencyProperty.Register(
            "CheckingRowCascade", typeof(CheckingRowCascade), typeof(MultiTypesTreeGrid),
            new PropertyMetadata(CheckingRowCascade.None, (d, e) => (d as MultiTypesTreeGrid).OnCheckingRowCascadeChanged(e))
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

        #region 实现 CheckingMode

        private const int CHECKING_COLUMN_INDEX = 0;

        private void DisableSelect()
        {
            this.Columns.RemoveAt(CHECKING_COLUMN_INDEX);

            this.CheckingMode = CheckingMode.None;
        }

        private void EnableChecking(CheckingMode mode)
        {
            //如果是绑定 ViewModel，则使用 IsSelected 属性；
            //如果是绑定当前行，则绑定到行对象的 IsMultiSelectedProperty 属性上。
            var binding = mode == OEA.CheckingMode.CheckingViewModel ?
                new Binding(PropertyConvention.IsSelected) :
                new Binding
                {
                    Path = new PropertyPath(GridTreeViewRow.IsMultiSelectedProperty),
                    RelativeSource = new RelativeSource
                    {
                        Mode = RelativeSourceMode.FindAncestor,
                        AncestorType = typeof(GridTreeViewRow)
                    }
                };

            var column = CreateCheckingColumnTemplate(binding);

            this.Columns.Insert(CHECKING_COLUMN_INDEX, column);
        }

        /// <summary>
        /// 创建选择列的模板
        /// </summary>
        /// <param name="checkBinding"></param>
        /// <returns></returns>
        private static TreeColumn CreateCheckingColumnTemplate(Binding checkBinding)
        {
            var column = new ReadonlyTreeColumn();

            var element = new FrameworkElementFactory(typeof(CheckBox));
            element.SetBinding(CheckBox.IsCheckedProperty, checkBinding);
            AutomationHelper.SetEditingElement(element);

            var cell = MTTGCell.Wrap(element, column);

            TreeColumnFactory.CreateCellTemplate(column, "选择", cell);
            column.Width = 50;

            return column;
        }

        #endregion

        #region Shift / Ctrl 多选

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
            if (this._selectedItems.Count > 0)
            {
                if (this._shiftStartItem != null)
                {
                    if (this._selectedItems.Contains(GetEntity(this._shiftStartItem))) { return; }
                }

                var lastOne = this._selectedItems[this._selectedItems.Count - 1];
                this._shiftStartItem = this.FindRow(lastOne);
            }
            else
            {
                this._shiftStartItem = null;
            }
        }

        private void MultiSelectionCore(RoutedTreeItemEventArgs<Entity> e)
        {
            //如果不是 SetSelectedValue 导致的变更事件，则表示是用户点击等界面操作引起，
            //这时，需要考虑多选、勾选，并同步到 IsMultiSelected 值上。
            if (!IsCtrlPressed() && !IsShiftPressed())
            {
                var oldRow = this.FindRow(e.OldItem);
                if (oldRow != null)
                {
                    this.SelectRow(oldRow, false);
                }
            }

            if (e.NewItem != null)
            {
                var newRow = this.FindRow(e.NewItem);
                if (newRow != null)
                {
                    //如果没有按住 Ctrl，并且在 CheckRow 的模式下，则清空的选择项。
                    //（在 CheckRow 的模式下，单击某一行并不需要清空历史记录。）
                    if (!IsCtrlPressed() && this.CheckingMode != CheckingMode.CheckingRow) this.ClearSelectedItems();

                    //反选该行
                    this.SelectRow(newRow, !newRow.IsMultiSelected);

                    if (this._shiftStartItem != null && IsShiftPressed())
                    {
                        SelectRowsOnShift(newRow);
                    }

                    this.ClearEditingCellOnSelectionChanged();
                }
            }
            else
            {
                //在 CheckRow 的模式下，未选中某一行并不需要清空历史记录，其它情况均需要清空。
                if (this.CheckingMode != CheckingMode.CheckingRow) this.ClearSelectedItems();
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
                this._selectedItems.Clear();

                for (int i = start; i <= end; i++)
                {
                    this.SelectRow(itemsALayer.GetItemAt(i) as GridTreeViewRow, true);
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

        #region 勾选时发生的选择方法

        /// <summary>
        /// 设置某行的选择状态，并级联选择
        /// </summary>
        /// <param name="row"></param>
        /// <param name="value"></param>
        internal void CheckRowWithCascade(GridTreeViewRow row, bool value)
        {
            var argsCache = new RoutedTreeItemEventArgs<Entity>(this);

            var entity = GetEntity(row);

            this.SelectRow(row, value);
            RaiseEventAsChecked(argsCache, entity, value);

            if (this.CheckingMode == CheckingMode.None) { return; }

            //CascadeParent
            if (value && this.NeedCascade(CheckingRowCascade.CascadeParent))
            {
                Entity parent = this.GetParentItem(entity);

                while (parent != null)
                {
                    var parentRow = this.FindRow(parent);
                    if (parentRow == null || parentRow.IsMultiSelected) { break; }

                    this.SelectRow(parentRow, true);
                    RaiseEventAsChecked(argsCache, parent, true);

                    parent = this.GetParentItem(parent);
                }
            }

            //CascadeChildren
            if (this.NeedCascade(CheckingRowCascade.CascadeChildren))
            {
                this.SelectChildrenRecur(row, entity, value, argsCache);
            }
        }

        /// <summary>
        /// 迭归设置指定行的子行列表的选择状态
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowEntity"></param>
        /// <param name="value"></param>
        /// <param name="argsCache"></param>
        private void SelectChildrenRecur(GridTreeViewRow row, Entity rowEntity, bool value, RoutedTreeItemEventArgs<Entity> argsCache)
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
                        this.SelectRow(childRow, value);
                    }
                    else
                    {
                        //还没有生成，把它加到选择列表中，下次生成时会继续选中。见 SelectAsCreated 方法。
                        if (value)
                        {
                            this.AddToSelectedItems(child);
                        }
                        else
                        {
                            this._selectedItems.Remove(child);
                        }
                    }

                    this.RaiseEventAsChecked(argsCache, child, value);
                    this.SelectChildrenRecur(childRow, child, value, argsCache);
                }
            }
        }

        private void RaiseEventAsChecked(RoutedTreeItemEventArgs<Entity> e, Entity rowEntity, bool isChecked)
        {
            if (isChecked)
            {
                e.OldItem = null;
                e.NewItem = rowEntity;
            }
            else
            {
                e.OldItem = rowEntity;
                e.NewItem = null;
            }

            base.OnSelectedItemChanged(e);
        }

        /// <summary>
        /// 当重新生成某一行时，如果是已经选中的实体，需要初始化它们的选中状态
        /// </summary>
        /// <param name="row"></param>
        /// <param name="item"></param>
        private void SelectAsCreated(GridTreeViewRow row, Entity item)
        {
            if (this._selectedItems.Contains(item)) { this.SelectRow(row, true); }
        }

        #endregion

        #region 选择核心方法

        public void SelectAll()
        {
            this._selectedItems.Clear();

            foreach (Entity entity in this._itemsSource)
            {
                this._selectedItems.Add(entity);

                var row = this.FindRow(entity);
                if (row != null) this.SelectRow(row, true);
            }
        }

        public void ClearSelection()
        {
            this.ClearSelectedItems();

            this.SelectedItem = null;
        }

        private void ClearSelectedItems()
        {
            for (int i = this._selectedItems.Count - 1; i >= 0; i--)
            {
                var item = this._selectedItems[i];

                var viewItem = this.FindRow(item);
                if (viewItem != null)
                {
                    this.SelectRow(viewItem, false);
                }
                else
                {
                    this._selectedItems.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 整个选择的源头：TreeView 的 SelectedItemChanged 事件。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTreeViewSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            //如果处于勾选行的状态中，则忽略所有 TreeView 和事件。这样，用户只能通过勾选来选择行。
            if (this.CheckingMode == CheckingMode.CheckingRow) return;

            //如果 IsSettingInternal值为真，说明是来自于最下层的 SelectRow 方法，则不需要进行任何处理！
            var newRow = e.NewValue as GridTreeViewRow;
            if (newRow != null && newRow.IsSettingInternal) { return; }

            base.OnTreeViewSelectedItemChanged(e);
        }

        protected override void OnSelectedItemPropertyChanged(Entity newValue, Entity oldValue)
        {
            /*********************** 代码块解释 *********************************
             * 这里重写此方法只是为了添加以下注释：
             * 
             * 基类的此方法以一种防止 TreeView 重入的方式来同步 this.SelectedItem 到 TreeView.SelecteItem 上。
             * 并在最后调用 this.OnSelectedItemChanged 方法，以抛出路由事件。
             * 
             * 注意这三个方法的顺序：
             * OnTreeViewSelectedItemChanged
             * OnSelectedItemPropertyChanged
             * OnSelectedItemChanged
             * 
            **********************************************************************/

            base.OnSelectedItemPropertyChanged(newValue, oldValue);
        }

        protected override void OnSelectedItemChanged(RoutedTreeItemEventArgs<Entity> e)
        {
            this.MultiSelectionCore(e);

            //抛出路由事件
            base.OnSelectedItemChanged(e);
        }

        internal override void SelectedRowOnCreation(TreeViewItem row)
        {
            /*********************** 代码块解释 *********************************
             * 由于在新创建某一行时，设置它的 IsSelected 属性并不会造成 OnTreeViewSelectedItemChanged 事件的发生，
             * 所以也就不会同步到它的 IsMultiSelected 属性上，这样的情况，需要特别处理。
            **********************************************************************/

            base.SelectedRowOnCreation(row);

            (row as GridTreeViewRow).IsMultiSelected = true;
        }

        /// <summary>
        /// 设置某行的“选中”状态
        /// </summary>
        /// <param name="row"></param>
        /// <param name="isSelected">是“选中”状态还是“未选中”状态。</param>
        private void SelectRow(GridTreeViewRow row, bool isSelected)
        {
            /*********************** 代码块解释 *********************************
             * 同时处理以下几个属性：
             * this._selectedItems、row.IsMultiSelected、row.IsSelected
            **********************************************************************/

            if (isSelected)
            {
                this.AddToSelectedItems(GetEntity(row));
            }
            else
            {
                this._selectedItems.Remove(GetEntity(row));
            }

            try
            {
                row.IsSettingInternal = true;

                //设置这个属性，皮肤中会绑定此属性来实现界面高亮显示
                row.IsMultiSelected = isSelected;

                //然后，需要设置 TreeListItem.IsSelected 属性。
                //否则，会造成内部使用的 TreeView 的 SelectedItem 属性不变，但是高亮的状态已经被清除，
                //这时用户再次点击该行时，TreeView 的 SelectedItemChanged 事件不会发生，
                //也就不会发生整个 MTTG 的 SelectedItem 处理流程，造成无法选中该行。
                //具体情况见 bug：http://ipm.grandsoft.com.cn/issues/247260
                row.IsSelected = isSelected;
            }
            finally
            {
                row.IsSettingInternal = false;
            }
        }

        private bool AddToSelectedItems(Entity entity)
        {
            if (!this._selectedItems.Contains(entity))
            {
                this._selectedItems.Add(entity);
                return true;
            }

            return false;
        }

        #endregion

        #region private class MTTGSelectedItemsCollection

        /// <summary>
        /// 把 realSelectedItems 和 MTTG 封装出一些可以给外界直接操作集合而达到操作控件的方法。
        /// </summary>
        [DebuggerDisplay("Count = {Count}")]
        private class MTTGSelectedItemsCollection : IList<Entity>, IList, ISelectionItems
        {
            internal MultiTypesTreeGrid _owner;

            private bool TryAddSelection(Entity item)
            {
                var row = this._owner.FindOrGenerateNode(item);

                if (row != null)
                {
                    //如果还没有当前选择项，则直接设置当前项，否则直接添加该项到集合中。
                    if (this._owner.SelectedItem == null)
                    {
                        this._owner.SelectedItem = item;
                    }
                    else
                    {
                        this._owner.SelectRow(row, true);
                    }

                    return true;
                }

                return false;
            }

            private bool TryDeselectEntity(Entity item)
            {
                var row = this._owner.FindRow(item);

                if (row != null) { this._owner.SelectRow(row, false); }

                return true;
            }

            private void TryDeselectAll()
            {
                this._owner.ClearSelection();
            }

            #region IList<Entity> Members

            public int IndexOf(Entity item)
            {
                return this._owner._selectedItems.IndexOf(item);
            }

            public void Insert(int index, Entity item)
            {
                throw new NotSupportedException("暂时不支持对选择项进行此操作。");
            }

            public void RemoveAt(int index)
            {
                throw new NotSupportedException("暂时不支持对选择项进行此操作。");
            }

            public Entity this[int index]
            {
                get { return this._owner._selectedItems[index]; }
                set { this._owner._selectedItems[index] = value; }
            }

            public void Add(Entity item)
            {
                this.TryAddSelection(item);
            }

            public void Clear()
            {
                this.TryDeselectAll();
            }

            public bool Contains(Entity item)
            {
                return this._owner._selectedItems.Contains(item);
            }

            public void CopyTo(Entity[] array, int arrayIndex)
            {
                this._owner._selectedItems.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return this._owner._selectedItems.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(Entity item)
            {
                return this.TryDeselectEntity(item);
            }

            public IEnumerator<Entity> GetEnumerator()
            {
                return this._owner._selectedItems.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this._owner._selectedItems.GetEnumerator();
            }

            #endregion

            #region IList Members

            public int Add(object value)
            {
                this.Add(value as Entity);
                return this.Count - 1;
            }

            public bool Contains(object value)
            {
                return this.Contains(value as Entity);
            }

            public int IndexOf(object value)
            {
                return this.IndexOf(value as Entity);
            }

            public void Insert(int index, object value)
            {
                this.Insert(index, value as Entity);
            }

            public bool IsFixedSize
            {
                get { return (this._owner._selectedItems as IList).IsFixedSize; }
            }

            public void Remove(object value)
            {
                this.Remove(value as Entity);
            }

            object IList.this[int index]
            {
                get { return this[index]; }
                set { this[index] = value as Entity; }
            }

            public void CopyTo(Array array, int index)
            {
                (this._owner._selectedItems as IList).CopyTo(array, index);
            }

            public bool IsSynchronized
            {
                get { return (this._owner._selectedItems as IList).IsSynchronized; }
            }

            public object SyncRoot
            {
                get { return (this._owner._selectedItems as IList).SyncRoot; }
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// 一个选择项的集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISelectionItems : IList<Entity>, IList { }
}