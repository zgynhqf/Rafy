using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OEA.Module.WPF.Controls
{
    public partial class TreeGrid
    {
        private bool _isReadOnly;

        /// <summary>
        /// 当前正在编辑的单元格。
        /// </summary>
        private MTTGCell _editingCell;

        /// <summary>
        /// 提交当前的修改。
        /// </summary>
        public void CommitEdit(IInputElement target)
        {
            if (target == null) target = this;

            if (CommitEditCommand.CanExecute(null, target))
            {
                CommitEditCommand.Execute(null, target);
            }
        }

        public bool IsReadOnly
        {
            get { return this._isReadOnly; }
            set
            {
                this._isReadOnly = value;
                if (value && this._editingCell != null)
                {
                    UpdateEditingStatus(this._editingCell, false);
                }
            }
        }

        /// <summary>
        /// 尝试通知本树型控件编辑某个单元格。
        /// </summary>
        /// <param name="cell"></param>
        /// <returns>返回是否成功进入编辑状态</returns>
        public bool TryEditCell(MTTGCell cell)
        {
            var emptyArgs = new RoutedEventArgs(UIElement.MouseDownEvent, this);
            return this.TryEditCell(cell, emptyArgs);
        }

        /// <summary>
        /// 某一列被左键点击时，会通知本树型控件需要进入编辑状态。
        /// </summary>
        /// <param name="column"></param>
        internal bool TryEditCell(MTTGCell cell, RoutedEventArgs editingEventArgs)
        {
            if (!this._isReadOnly)
            {
                var row = GridTreeViewRow.GetRowContainingElement(cell);

                //由于点击任何一行的该列都会发生此事件，所以需要检测
                if (row == this.GetSingleSelectedRow())
                {
                    var column = cell.Column;

                    if (this._editingCell != null)
                    {
                        if (this._editingCell.Column != column || GridTreeViewRow.GetRowContainingElement(this._editingCell) != row)
                        {
                            UpdateEditingStatus(this._editingCell, false);
                            this.EditCell(cell, editingEventArgs);
                            return true;
                        }
                    }
                    else
                    {
                        this.EditCell(cell, editingEventArgs);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 选择行变更时，需要把当前编辑行的状态清除。
        /// </summary>
        private void ClearEditingCellOnSelectionChanged()
        {
            var rowSelected = this.GetSingleSelectedRow();
            if (this._editingCell != null && rowSelected != GridTreeViewRow.GetRowContainingElement(this._editingCell))
            {
                UpdateEditingStatus(this._editingCell, false);
                this._editingCell = null;
            }
        }

        private void EditCell(MTTGCell editingCell, RoutedEventArgs editingEventArgs)
        {
            var editingElement = UpdateEditingStatus(editingCell, true);
            if (editingElement == null) return;
            this._editingCell = editingCell;

            //模仿 DataGrid.BeginEdit(FrameworkElement editingElement, RoutedEventArgs e) 方法
            editingElement.UpdateLayout();
            editingElement.Focus();
            editingCell.Column.PrepareElementForEdit(editingElement, editingEventArgs);

            //需要把这个事件处理掉，模仿：DataGridCell.OnAnyMouseButtonDown。
            editingEventArgs.Handled = true;
        }

        /// <summary>
        /// 获取单独选中的一行。
        /// 
        /// 如果多选，则返回 null
        /// </summary>
        /// <returns></returns>
        private GridTreeViewRow GetSingleSelectedRow()
        {
            var selectedItems = this.SelectionModel.Items;
            if (selectedItems.Count == 1) { return this.FindRow(selectedItems[0]); }

            return null;
        }

        /// <summary>
        /// 更新指定的 GridTreeViewRow 内容为编辑状态/非编辑状态。
        /// </summary>
        /// <param name="lvi"></param>
        /// <param name="isEditing"></param>
        private FrameworkElement UpdateEditingStatus(MTTGCell cell, bool isEditing)
        {
            var treeColumn = cell.Column;

            FrameworkElement resultControl = null;

            if (isEditing)
            {
                resultControl = treeColumn.GenerateEditingElement();
            }
            else
            {
                //此处需要调用更新绑定的源，给 cp.Content 赋值时会将原有的值清空，而不触发绑定属性的更改
                this.CommitEdit(cell);
                resultControl = treeColumn.GenerateDisplayElement();
            }

            cell.ContentTemplate = null;
            cell.Content = resultControl;

            return resultControl;
        }
    }
}