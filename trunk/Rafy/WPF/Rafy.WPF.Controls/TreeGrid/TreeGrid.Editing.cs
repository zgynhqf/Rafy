using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rafy.WPF.Controls
{
    partial class TreeGrid
    {
        /// <summary>
        /// 当前正在被编辑的数据项。
        /// </summary>
        private object _editingItem;

        /// <summary>
        /// 如果当前编辑模式是单元格编辑，则这个字段表示正在被编辑的列。
        /// </summary>
        private TreeGridColumn _editingColumn;

        /// <summary>
        /// 当前正在被编辑的数据项。
        /// </summary>
        internal object EditingItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._editingItem; }
        }

        /// <summary>
        /// 如果当前编辑模式是单元格编辑，则这个字段表示正在被编辑的列。
        /// </summary>
        internal TreeGridColumn EditingColumn
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._editingColumn; }
        }

        /// <summary>
        /// 返回是否正在处于编辑状态中。
        /// </summary>
        public bool IsEditing
        {
            get { return this._editingItem != null; }
        }

        #region EditingMode DependencyProperty

        public static readonly DependencyProperty EditingModeProperty = DependencyProperty.Register(
            "EditingMode", typeof(TreeGridEditingMode), typeof(TreeGrid),
            new PropertyMetadata(TreeGridEditingMode.Row, (d, e) => (d as TreeGrid).OnEditingModeChanged(e))
            );

        public TreeGridEditingMode EditingMode
        {
            get { return (TreeGridEditingMode)this.GetValue(EditingModeProperty); }
            set { this.SetValue(EditingModeProperty, value); }
        }

        private void OnEditingModeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this._editingItem != null) throw new NotSupportedException("当前正在处于编辑状态下，不支持编辑模式变更。");
            //var value = (TreeGridEditingMode)e.NewValue;
        }

        #endregion

        #region RowEditingMode DependencyProperty

        public static readonly DependencyProperty RowEditingChangeModeProperty = DependencyProperty.Register(
            "RowEditingChangeMode", typeof(RowEditingChangeMode), typeof(TreeGrid),
            new PropertyMetadata(RowEditingChangeMode.SingleClick)
            );

        /// <summary>
        /// 在行编辑模式下，此属性表示切换编辑行的方式，是使用单击还是双击。
        /// </summary>
        public RowEditingChangeMode RowEditingChangeMode
        {
            get { return (RowEditingChangeMode)this.GetValue(RowEditingChangeModeProperty); }
            set { this.SetValue(RowEditingChangeModeProperty, value); }
        }

        #endregion

        #region IsReadOnly DependencyProperty

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata((d, e) => (d as TreeGrid).OnIsReadOnlyChanged(e))
            );

        /// <summary>
        /// 整个表格是否处于只读状态。
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        private void OnIsReadOnlyChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;

            if (value) { this.ExitEditing(); }
        }

        #endregion

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (this.IsEditing)
            {
                //如果焦点移动到本窗口中本 TreeGrid 以外的地方。
                //注意，如果 TreeGrid 中有下拉等生成 Popup 控件的编辑控件时，单纯的 IsAncestorOf 判断是不足够的。
                var newFocus = e.NewFocus as DependencyObject;
                if (newFocus != null && !this.IsAncestorOf(newFocus) &&
                    (newFocus.GetLogicalRoot() == Window.GetWindow(this))
                    )
                {
                    this.ExitEditing();
                }
            }

            base.OnLostKeyboardFocus(e);
        }

        /// <summary>
        /// 退出当前的编辑状态。
        /// </summary>
        public void ExitEditing()
        {
            if (this.EditingMode == TreeGridEditingMode.Cell)
            {
                this.ExitCellEditing();
            }
            else
            {
                this.ExitRowEditing();
            }
        }

        #region Cell Editing

        /// <summary>
        /// 尝试通知本树型控件编辑某个单元格。
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>返回是否成功进入编辑状态</returns>
        public bool TryEditCell(TreeGridCell cell)
        {
            return this.TryEditCell(cell, null);
        }

        /// <summary>
        /// 尝试开始编辑某一个单元格。
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="editingEventArgs"></param>
        /// <returns></returns>
        internal bool TryEditCell(TreeGridCell cell, RoutedEventArgs editingEventArgs)
        {
            if (this.CanEditCell(cell))
            {
                this.EditCell(cell, editingEventArgs);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 返回是否可以成功编辑某个单元格。
        /// 
        /// 以下情况下，不能编辑：
        /// * 整个表格不能进行编辑。
        /// * 该单元格所在的列不能进行编辑。
        /// * 表格没有处于 CellEditing 模式下。
        /// * 当前未选择唯一行（单选状态下可编辑）。
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private bool CanEditCell(TreeGridCell cell)
        {
            var success = false;

            if (!this.IsReadOnly && this.EditingMode == TreeGridEditingMode.Cell)
            {
                var column = cell.Column;
                //如果是列是只读状态，就不能进入编辑状态。
                if (cell.Column != null && cell.Column.CanEdit(cell.DataContext))
                {
                    var row = cell.Row;
                    var selectedItems = this.SelectionModel.InnerItems;
                    //由于点击任何一行的该列都会发生此事件，所以需要检测如果当前只选择了一列
                    if (row != null && selectedItems.Count == 1 && row.DataContext == selectedItems[0])
                    {
                        success = true;
                    }
                }
            }

            return success;
        }

        private void EditCell(TreeGridCell cell, RoutedEventArgs editingEventArgs)
        {
            //如果当前已经开始编辑某个单元格
            if (this._editingItem != null && this._editingColumn != null)
            {
                if (this._editingColumn != cell.Column || this._editingItem != cell.Row.DataContext)
                {
                    this.ExitCellEditing();

                    this.EditCellCore(cell, editingEventArgs);
                }
            }
            else
            {
                this.EditCellCore(cell, editingEventArgs);
            }
        }

        /// <summary>
        /// 修改表格的编辑状态，并让某个单元格进入编辑状态。
        /// </summary>
        /// <param name="editingCell"></param>
        /// <param name="editingEventArgs">引发此操作的事件参数。可为 null。</param>
        private void EditCellCore(TreeGridCell editingCell, RoutedEventArgs editingEventArgs)
        {
            this._editingItem = editingCell.Row.DataContext;
            this._editingColumn = editingCell.Column;

            editingCell.SetIsEditingField(true);

            editingCell.FocusOnEditing(editingEventArgs);

            //需要把这个事件处理掉，模仿：DataGridCell.OnAnyMouseButtonDown。
            if (editingEventArgs != null)
            {
                editingEventArgs.Handled = true;
            }
        }

        /// <summary>
        /// 退出当前单元格编辑状态
        /// </summary>
        private void ExitCellEditing()
        {
            if (this._editingItem != null && this._editingColumn != null)
            {
                var editingCell = this.FindEditingCell();
                if (editingCell != null) editingCell.SetIsEditingField(false);

                this._editingItem = null;
                this._editingColumn = null;
            }
        }

        /// <summary>
        /// 正在编辑的单元格。
        /// 如果该行该列正处于虚拟化中，则返回 null。
        /// </summary>
        private TreeGridCell FindEditingCell()
        {
            return this.FindCell(this._editingItem, this._editingColumn);
        }

        #endregion

        #region Row Editing

        /// <summary>
        /// 开始编辑某行。
        /// </summary>
        /// <param name="row"></param>
        /// <param name="focusCell">编辑后，应该首先进入编辑状态的单元格。</param>
        public bool TryEditRow(TreeGridRow row, TreeGridCell focusCell)
        {
            return this.TryEditRow(row, null, focusCell);
        }

        /// <summary>
        /// 开始编辑某行。
        /// </summary>
        /// <param name="row"></param>
        /// <param name="editingEventArgs">引发编辑的事件参数。</param>
        /// <param name="focusCell">编辑后，应该首先进入编辑状态的单元格。</param>
        internal bool TryEditRow(TreeGridRow row, RoutedEventArgs editingEventArgs, TreeGridCell focusCell)
        {
            var success = false;

            if (!this.IsReadOnly && this.EditingMode == TreeGridEditingMode.Row)
            {
                var item = row.DataContext;
                var selectedItems = this.SelectionModel.InnerItems;
                //由于点击任何一行的该列都会发生此事件，所以需要检测如果当前只选择了一列
                if (selectedItems.Count == 1 && item == selectedItems[0])
                {
                    //先退出之前行的编辑状态。
                    if (this._editingItem != item)
                    {
                        this.ExitRowEditing();

                        this._editingItem = item;
                    }

                    //focused 表示是否已经有单元格设置了焦点。
                    bool focused = focusCell != null;
                    var mouseArgs = editingEventArgs as MouseButtonEventArgs;

                    //设置所有单元格的编辑状态，及焦点。
                    var cells = row.TraverseCells();
                    foreach (var cell in cells)
                    {
                        //设置单元格的编辑状态
                        if (!cell.IsEditing && cell.Column.CanEdit(this._editingItem))
                        {
                            cell.SetIsEditingField(true);
                        }

                        //如果成功设置单元格的编辑状态，而且还没有设置单元格焦点，
                        //那么就通过鼠标的坐标来判断是否需要设置当前单元格的焦点。
                        if (!focused && mouseArgs != null && cell.IsEditing && cell.VisibleOnVirtualizing)
                        {
                            var position = mouseArgs.GetPosition(cell);
                            var cellPosition = new Rect(cell.RenderSize);
                            if (cellPosition.Contains(position))
                            {
                                cell.FocusOnEditing(editingEventArgs);
                                focused = true;
                            }
                        }
                    }

                    if (focusCell != null)
                    {
                        focusCell.FocusOnEditing(editingEventArgs);
                    }

                    //标记事件已经处理，防止事件继续冒泡导致引发 TreeGridRow.OnGotFocus 事件。
                    if (editingEventArgs != null) editingEventArgs.Handled = true;

                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// 退出行编辑状态。
        /// </summary>
        private void ExitRowEditing()
        {
            if (this._editingItem != null)
            {
                var row = this.FindRow(this._editingItem);
                if (row != null)
                {
                    var cells = row.TraverseCells();
                    foreach (var cell in cells)
                    {
                        cell.SetIsEditingField(false);
                    }
                }

                this._editingItem = null;
            }
        }

        #endregion

        private void OnCommitEditCommandExecuted(ExecutedRoutedEventArgs e)
        {
            //由于目前文本框都是即时绑定（UpdateSourceTrigger.PropertyChanged），所以暂时不需要再显式提交数据。
            e.Handled = true;
        }
    }

    /// <summary>
    /// 当前表格正在编辑的模式。
    /// </summary>
    public enum TreeGridEditingMode
    {
        /// <summary>
        /// 单个格子编辑
        /// </summary>
        Cell,
        /// <summary>
        /// 单行编辑
        /// </summary>
        Row,
    }

    /// <summary>
    /// 在行编辑模式下，切换编辑行的模式。
    /// </summary>
    public enum RowEditingChangeMode
    {
        /// <summary>
        /// 双击切换
        /// </summary>
        DoubleClick,
        /// <summary>
        /// 单击切换
        /// </summary>
        SingleClick
    }
}