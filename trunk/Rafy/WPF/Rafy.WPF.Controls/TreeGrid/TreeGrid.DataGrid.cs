/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120926 18:05
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120926 18:05
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Controls
{
    partial class TreeGrid
    {
        #region ColumnsProperty

        private static readonly DependencyPropertyKey ColumnsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Columns", typeof(TreeGridColumnCollection), typeof(TreeGrid), new PropertyMetadata((d, e) => (d as TreeGrid).OnColumnsChanged(e))
            );

        public static readonly DependencyProperty ColumnsProperty = ColumnsPropertyKey.DependencyProperty;

        /// <summary> 
        /// 所有显示的列的列表
        /// </summary>
        public TreeGridColumnCollection Columns
        {
            get { return (TreeGridColumnCollection)this.GetValue(ColumnsProperty); }
            private set { this.SetValue(ColumnsPropertyKey, value); }
        }

        private void OnColumnsChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldColumns = e.OldValue as TreeGridColumnCollection;
            if (oldColumns != null)
            {
                oldColumns.Owner = null;
            }

            var newColumns = e.NewValue as TreeGridColumnCollection;
            if (newColumns != null)
            {
                newColumns.Owner = this;
            }
        }

        #endregion

        #region OnlyGridModeProperty

        public static readonly DependencyProperty OnlyGridModeProperty = DependencyProperty.Register(
            "OnlyGridMode", typeof(bool), typeof(TreeGrid)
            );

        /// <summary>
        /// 表示是否处于 “简单表格” 模式下。
        /// 此模式下，没有层级节点。
        /// </summary>
        public bool OnlyGridMode
        {
            get { return (bool)this.GetValue(OnlyGridModeProperty); }
            set { this.SetValue(OnlyGridModeProperty, value); }
        }

        #endregion

        #region UI Virtualizing

        /// <summary>
        /// 列虚拟化阀门
        /// </summary>
        private const int MinColumnsCountAsVirtualizing = 10;

        private bool _columnsVirtualizingEnabled;

        /// <summary>
        /// 设置或获取是否已经打开了内部 <see cref="TreeGridRowsPanel"/> 的行虚拟化功能。
        /// 
        /// 1. 表格模式下，虚拟化使用 VirtualizingPanel 来实现行与列的虚拟化，
        /// 2. 树型表格模式下，使用懒加载子节点的方式来提升性能。这里不使用 VirtualizingPanel 
        /// 的原因是每行的高度会随着子节点的个数变化而变化，而 VirtualizingStackPanel 则是使用基于 Item 的虚拟化，
        /// 因而只能实现自定义虚拟化面板，较为复杂，所以不支持。
        /// </summary>
        internal bool IsVirtualizing
        {
            get { return VirtualizingStackPanel.GetIsVirtualizing(this); }
            set
            {
                /*********************** 代码块解释 *********************************
                 * 设置该属性会触发控件模板中的 Trigger 来设置 ScrollViewer.CanContentScroll 属性。
                 * （该方案与 TreeView 默认模板模板中的方案一致）
                 * 
                 * 关于 UIV，参见：http://www.cnblogs.com/zgynhqf/archive/2011/12/12/2284335.html
                **********************************************************************/

                VirtualizingStackPanel.SetIsVirtualizing(this, value);
            }
        }

        /// <summary>
        /// 是否支持列虚拟化。
        /// 列虚拟化必须在 IsVirtualizing 属性为 true 时才起使用
        /// </summary>
        internal bool IsColumnsVirtualizingEnabled
        {
            get
            {
                return this._columnsVirtualizingEnabled && this.IsVirtualizing;
            }
        }

        /// <summary>
        /// 是否回收之间已经生成好的容器。
        /// 
        /// 此回收模式与 VirtualizingStackPanel 本身的回收模式有所区别。
        /// 这里回收的单元格不能被其它列所使用，而只是不需要再次生成了。
        /// </summary>
        internal bool IsRecycleMode
        {
            get
            {
                //目前不能解决单元格的焦点问题，所以永远打开回收模式。
                return true;
            }
        }

        internal TreeGridRowsPanel RowsPanel;

        /// <summary>
        /// 设置 UIV。
        /// </summary>
        private void SetUIV()
        {
            //表格模式下，重新绑定数据时，如果没有分组，则打开 UIV。
            if (this.OnlyGridMode && !this.HasGrouped)
            {
                this.IsVirtualizing = true;

                //当列数大于指定个数时，打开虚拟化功能
                this._columnsVirtualizingEnabled = this.Columns != null &&
                    this.Columns.Count > MinColumnsCountAsVirtualizing;
            }
        }

        /// <summary>
        /// 把指定的列滚动到视区中。
        /// </summary>
        /// <param name="column"></param>
        public void BringColumnIntoView(TreeGridColumn column)
        {
            var columnIndex = this.Columns.IndexOf(column);
            this.BringColumnIntoView(columnIndex);
        }

        /// <summary>
        /// 把某数据项对应的行滚动到视区中
        /// </summary>
        /// <param name="item"></param>
        public void BringItemIntoView(object item)
        {
            var rowIndex = this.Items.IndexOf(item);
            this.BringItemIntoView(rowIndex);
        }

        /// <summary>
        /// 把指定的列滚动到视区中。
        /// </summary>
        /// <param name="columnIndex"></param>
        public void BringColumnIntoView(int columnIndex)
        {
            if (this.RowsPanel != null)
            {
                this.RowsPanel.InternalBringColumnIntoView(columnIndex);
            }
        }

        /// <summary>
        /// 把某行滚动到视区中。
        /// </summary>
        /// <param name="rowIndex"></param>
        public void BringItemIntoView(int rowIndex)
        {
            if (this.RowsPanel != null)
            {
                this.RowsPanel.InternalBringRowIntoView(rowIndex);
            }
        }

        #endregion

        #region 合计行

        private bool _summarizing = false;

        #region ShowSummaryRow DependencyProperty

        public static readonly DependencyProperty ShowSummaryRowProperty = DependencyProperty.Register(
            "ShowSummaryRow", typeof(bool), typeof(TreeGrid),
            new PropertyMetadata(BooleanBoxes.False, (d, e) => (d as TreeGrid).OnShowSummaryRowChanged(e))
            );

        /// <summary>
        /// 用于控制是否要显示合计行的属性。
        /// </summary>
        public bool ShowSummaryRow
        {
            get { return (bool)this.GetValue(ShowSummaryRowProperty); }
            set { this.SetValue(ShowSummaryRowProperty, BooleanBoxes.Box(value)); }
        }

        private void OnShowSummaryRowChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            if (!value)
            {
                this.IsSummaryRowVisible = false;
            }
            else
            {
                this.InvalidateSummary();
            }
        }

        #endregion

        #region SummaryRowTitle DependencyProperty

        public static readonly DependencyProperty SummaryRowTitleProperty = DependencyProperty.Register(
            "SummaryRowTitle", typeof(string), typeof(TreeGrid)
            );

        /// <summary>
        /// 合计行，应该显示的行标题。
        /// </summary>
        public string SummaryRowTitle
        {
            get { return (string)this.GetValue(SummaryRowTitleProperty); }
            set { this.SetValue(SummaryRowTitleProperty, value); }
        }

        #endregion

        #region SummaryRowTitleStyle DependencyProperty

        public static readonly DependencyProperty SummaryRowTitleStyleProperty = DependencyProperty.Register(
            "SummaryRowTitleStyle", typeof(Style), typeof(TreeGrid)
            );

        public Style SummaryRowTitleStyle
        {
            get { return (Style)this.GetValue(SummaryRowTitleStyleProperty); }
            set { this.SetValue(SummaryRowTitleStyleProperty, value); }
        }

        #endregion

        #region IsSummaryRowVisible DependencyProperty

        private static readonly DependencyPropertyKey IsSummaryRowVisiblePropertyKey = DependencyProperty.RegisterReadOnly(
            "IsSummaryRowVisible", typeof(bool), typeof(TreeGrid), new PropertyMetadata(BooleanBoxes.False)
            );

        public static readonly DependencyProperty IsSummaryRowVisibleProperty = IsSummaryRowVisiblePropertyKey.DependencyProperty;

        /// <summary>
        /// 最终计算出的是否需要显示合计行。
        /// </summary>
        public bool IsSummaryRowVisible
        {
            get { return (bool)this.GetValue(IsSummaryRowVisibleProperty); }
            private set { this.SetValue(IsSummaryRowVisiblePropertyKey, BooleanBoxes.Box(value)); }
        }

        #endregion

        /// <summary>
        /// 重新计算合计值。
        /// 
        /// 此方法公开的原因：
        /// 由于目前不支持数据在属性变化时，自动刷新合计的值。所以如果需要实现该功能，则需要手动调用此方法。
        /// </summary>
        private void InvalidateSummary()
        {
            if (this._summarizing) return;
            this._summarizing = true;

            //统计合计的工作，放在系统空闲时完成。也是因为 ShowSummaryRow 属性需要在主线程才能获取。
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (this.ShowSummaryRow)
                {
                    var items = this.RootItemsControl.Items;

                    var hasSummary = false;

                    foreach (var column in this.Columns)
                    {
                        if (column.NeedSummary)
                        {
                            column.Summary = column.GetSummary(items);
                            hasSummary = true;
                        }
                    }

                    this.IsSummaryRowVisible = hasSummary;
                }

                this._summarizing = false;
            }), DispatcherPriority.ApplicationIdle);
        }

        #endregion
    }
}