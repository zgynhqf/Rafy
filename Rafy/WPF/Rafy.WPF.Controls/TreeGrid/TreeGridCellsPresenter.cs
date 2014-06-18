/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111123
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111123
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows.Automation;
using System.Collections.ObjectModel;
using System.Runtime;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGridRow 的行显示器。
    /// </summary>
    public class TreeGridCellsPresenter : ItemsControl
    {
        static TreeGridCellsPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridCellsPresenter), new FrameworkPropertyMetadata(typeof(TreeGridCellsPresenter)));
            DataContextProperty.OverrideMetadata(typeof(TreeGridCellsPresenter), new FrameworkPropertyMetadata(typeof(TreeGridCellsPresenter), (d, e) => (d as TreeGridCellsPresenter).OnDataContextChanged(e)));
        }

        /// <summary>
        /// 尝试找到上层的 TreeGridRow。
        /// </summary>
        internal TreeGridRow Row
        {
            get { return base.TemplatedParent as TreeGridRow; }
        }

        private TreeGridColumnCollection Columns
        {
            get
            {
                var row = this.Row;
                if (row != null)
                {
                    var grid = row.TreeGrid;
                    if (grid != null) { return grid.Columns; }
                }

                return null;
            }
        }

        /// <summary>
        /// 内部使用的 TreeGridCellsPanel
        /// </summary>
        internal TreeGridCellsPanel InternalItemsHost;

        public override void OnApplyTemplate()
        {
            if (this.InternalItemsHost != null && !base.IsAncestorOf(this.InternalItemsHost))
            {
                this.InternalItemsHost = null;
            }

            base.OnApplyTemplate();

            this.UpdateItemsSource();

            var row = this.Row;
            if (row != null) { row.CellsPresenter = this; }

            //this.SyncProperties(false);
        }

        #region 实现 ItemsControl 基类的方法

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeGridCell();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeGridCell;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var cell = element as TreeGridCell;

            var index = this.ItemContainerGenerator.IndexFromContainer(element);
            var column = this.Columns[index];

            this.PrepareCell(cell, column);

            base.PrepareContainerForItemOverride(element, item);
        }

        private void PrepareCell(TreeGridCell cell, TreeGridColumn column)
        {
            cell.Column = column;
            if (column.CellStyle != null)
            {
                cell.Style = column.CellStyle;
            }

            cell.UpdateContent(false);

            //IsEditing
            var row = this.Row;
            if (row != null)
            {
                var grid = row.TreeGrid;

                //当前行是表格中正在编辑的行
                if (grid != null && this.DataContext == grid.EditingItem)
                {
                    //整行编辑、或者正在编辑该单元格
                    if (grid.EditingMode == TreeGridEditingMode.Row || column == grid.EditingColumn)
                    {
                        if (cell.Column.CanEdit(cell.DataContext))
                        {
                            cell.SetIsEditingField(true);

                            //this.RestoreEditingContent(cell, column, grid);
                        }
                    }
                }
            }

            //Automation
            var headerLabel = column.HeaderLabel;
            if (!string.IsNullOrEmpty(headerLabel)) { AutomationProperties.SetName(cell, headerLabel); }
        }

        #region 保存焦点信息

        ///// <summary>
        ///// 编辑模式下的编辑元素，保存起来；
        ///// 当单元格再次被生成时，直接使用这些元素而不再次生成。这样可以防止用户信息的丢失。
        ///// 
        ///// 详见：PrepareCell & ClearCell
        ///// </summary>
        //private Dictionary<TreeGridColumn, UIElement> _editingElements;

        /// <summary>
        /// 当虚拟化 Panel 重新使用一个单元格时，需要尝试恢复它的焦点信息。
        /// </summary>
        /// <param name="cell"></param>
        internal void RestoreEditingCellFocus(TreeGridCell cell)
        {
            var row = this.Row;
            if (row != null)
            {
                var grid = row.TreeGrid;

                if (grid != null && grid.EditingItem != null && grid.EditingItem == this.DataContext)
                {
                    //目前只支持单元格编辑模式下的焦点恢复。
                    if (grid.EditingMode == TreeGridEditingMode.Cell && cell.Column == grid.EditingColumn)
                    {
                        var content = cell.Content as UIElement;
                        if (content != null) content.Focus();
                    }
                }
            }
        }

        //protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        //{
        //    var cell = element as TreeGridCell;

        //    base.ClearContainerForItemOverride(element, item);

        //    this.ReserveEditingContent(cell);
        //}

        //private void ReserveEditingContent(TreeGridCell cell)
        //{
        //    //IsEditing：把编辑控件存储下来。
        //    var row = this.Row;
        //    if (row != null)
        //    {
        //        var grid = row.TreeGrid;
        //        if (grid != null && this.DataContext == grid.EditingItem)
        //        {
        //            //非回收模式下，需要把
        //            if (!grid.IsRecycleMode)
        //            {
        //                if (grid.EditingMode == TreeGridEditingMode.Row || cell.Column == grid.EditingColumn)
        //                {
        //                    if (this._editingElements == null)
        //                    {
        //                        this._editingElements = new Dictionary<TreeGridColumn, UIElement>();
        //                    }
        //                    this._editingElements[cell.Column] = cell.Content as UIElement;
        //                }
        //            }
        //        }
        //    }
        //}

        private void RestoreEditingContent(TreeGridCell cell, TreeGridColumn column, TreeGrid grid)
        {
            ////非回收模式下，为了返回原来的焦点，需要使用之前的编辑控件。
            //if (!grid.IsRecycleMode)
            //{
            //    //如果之前已经生成了编辑控件，则使用老的控件，这样可以继续之前的编辑信息。
            //    if (this._editingElements != null)
            //    {
            //        UIElement content = null;
            //        if (this._editingElements.TryGetValue(column, out content))
            //        {
            //            cell.UpdateContent(content);
            //            //column.PrepareElementForEdit(content as FrameworkElement, null);
            //            //var res = content.Focus();
            //            //Debug.WriteLine(" content.IsVisible:" + content.IsVisible
            //            //    + " content.Focus():" + res
            //            //    + " KeyboardFocus:" + Keyboard.FocusedElement.GetType().FullName
            //            //    );
            //            this._editingElements.Remove(column);
            //        }
            //    }
            //}
        }

        #endregion

        #endregion

        #region ItemsSource

        /*********************** 代码块解释 *********************************
         * TreeGridRow 下的所有元素都使用 TreeGridRow 中定义的数据作为 DataContext，
         * 包括：TreeGridCellsPresenter、TreeGridCellsPanel、TreeGridCell。
        **********************************************************************/

        private void OnDataContextChanged(DependencyPropertyChangedEventArgs e)
        {
            this.UpdateItemsSource();
        }

        private void UpdateItemsSource()
        {
            var columns = this.Columns;
            if (columns != null)
            {
                var newItem = this.DataContext;

                var multipleCopiesCollection = base.ItemsSource as MultipleCopiesCollection;
                if (multipleCopiesCollection == null)
                {
                    multipleCopiesCollection = new MultipleCopiesCollection(newItem, columns.Count);
                    base.ItemsSource = multipleCopiesCollection;
                }
                else
                {
                    multipleCopiesCollection.CopiedItem = newItem;
                }
            }
        }

        internal void UpdateItemsSourceOnColumnsChanged(TreeGridColumnCollectionChangedEventArgs e)
        {
            var multipleCopiesCollection = base.ItemsSource as MultipleCopiesCollection;
            if (multipleCopiesCollection != null)
            {
                multipleCopiesCollection.MirrorCollectionChange(e);
            }
        }

        #endregion

        #region IsOnCurrentPage

        private const string PART_ScrollContentPresenter = "PART_ScrollContentPresenter";

        private FrameworkElement _viewPort;

        private bool _isOnCurrentPage;

        internal bool IsOnCurrentPageValid;

        /// <summary>
        /// 返回当前行是否正处于虚拟化的当前页中。
        /// </summary>
        internal bool IsOnCurrentPage
        {
            get
            {
                if (!this.IsOnCurrentPageValid)
                {
                    this._isOnCurrentPage = base.IsVisible && this.CheckVisibleOnCurrentPage();
                    this.IsOnCurrentPageValid = true;
                }
                return this._isOnCurrentPage;
            }
        }

        private bool CheckVisibleOnCurrentPage()
        {
            bool result = true;

            var row = base.TemplatedParent as TreeGridRow;
            if (row != null)
            {
                this.FindViewPort(row);

                if (this._viewPort != null)
                {
                    Rect container = new Rect(default(Point), this._viewPort.RenderSize);
                    Rect rect = new Rect(default(Point), row.RenderSize);
                    rect = row.TransformToAncestor(this._viewPort).TransformBounds(rect);
                    result = this.CheckContains(container, rect);
                }
            }

            return result;
        }

        private void FindViewPort(TreeGridRow row)
        {
            var itemsControl = row.ParentItemsControl;
            if (itemsControl != null)
            {
                var scrollViewer = MSInternal.GetScrollHost(itemsControl);
                if (scrollViewer != null && scrollViewer.CanContentScroll)
                {
                    var itemsHost = MSInternal.GetItemsHost(itemsControl);
                    if (itemsHost is VirtualizingPanel)
                    {
                        this._viewPort = scrollViewer.Template.FindName(PART_ScrollContentPresenter, scrollViewer) as FrameworkElement;
                        if (this._viewPort == null)
                        {
                            this._viewPort = scrollViewer;
                        }
                    }
                }
            }
        }

        private bool CheckContains(Rect container, Rect element)
        {
            return (this.CheckIsPointBetween(container, element.Top) && this.CheckIsPointBetween(container, element.Bottom)) || this.CheckIsPointBetween(element, container.Top + 2.0) || this.CheckIsPointBetween(element, container.Bottom - 2.0);
        }

        private bool CheckIsPointBetween(Rect rect, double pointY)
        {
            return DoubleUtil.LessThanOrClose(rect.Top, pointY) && DoubleUtil.LessThanOrClose(pointY, rect.Bottom);
        }

        #endregion
    }
}