/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121017 18:12
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121017 18:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 每一行中的单元格虚拟化容器。
    /// 
    /// 部分代码来自于 MS.DataGridCellsPanel 类。
    /// 
    /// 每一行的第一列应该是：RowHeader + Expander + ColumnUI
    /// </summary>
    public class TreeGridCellsPanel : VirtualizingPanel, IWeakEventListener
    {
        /**********************************************************************
         * 
         * 说明：
         * 每一行的第一列应该是：RowHeader + Expander + ColumnUI
         * 位置都安排在第一列，
         * 把这两个多余的元素作为子 Visual 列表的最后两个，Index 则是 RowHeader 在前，Expander 在后。
         * （TreeGridRow 中也会使用到这个排序。）
         * 
        **********************************************************************/

        /// <summary>
        /// 需要定义一个 UIElementCollection 元素，并调用它的 Add 方法，来间接实现设置子控件的 LogicalParent 和 VisualParent。
        /// </summary>
        private UIElementCollection _additionalChildren;

        public TreeGridCellsPanel()
        {
            this._additionalChildren = new UIElementCollection(this, this);
        }

        #region RowHeaderProperty

        public static DependencyProperty RowHeaderProperty = DependencyProperty.Register(
            "RowHeader", typeof(UIElement), typeof(TreeGridCellsPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => (d as TreeGridCellsPanel).OnRowHeaderChanged(e))
            );

        /// <summary>
        /// 这个属性主要用于绑定 TreeGridRow 中的 RowHeader
        /// </summary>
        public UIElement RowHeader
        {
            get { return (UIElement)this.GetValue(RowHeaderProperty); }
            set { this.SetValue(RowHeaderProperty, value); }
        }

        private void OnRowHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldElement = e.OldValue as UIElement;
            if (oldElement != null) { this._additionalChildren.Remove(oldElement); }

            var newElement = e.NewValue as UIElement;
            if (newElement != null) { this._additionalChildren.Insert(0, newElement); }
        }

        #endregion

        #region FirstColumnIndentProperty

        public static DependencyProperty FirstColumnIndentProperty = DependencyProperty.Register(
            "FirstColumnIndent", typeof(double), typeof(TreeGridCellsPanel),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender)
            );

        /// <summary>
        /// 当前行的第一列的左缩进量
        /// 
        /// 这个值需要绑定到 TreeGridRow 的 Level 上。
        /// </summary>
        public double FirstColumnIndent
        {
            get { return (double)this.GetValue(FirstColumnIndentProperty); }
            set { this.SetValue(FirstColumnIndentProperty, value); }
        }

        #endregion

        #region ExpanderProperty

        public static DependencyProperty ExpanderProperty = DependencyProperty.Register(
            "Expander", typeof(UIElement), typeof(TreeGridCellsPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, (d, e) => (d as TreeGridCellsPanel).OnExpanderChanged(e))
            );

        public UIElement Expander
        {
            get { return (UIElement)this.GetValue(ExpanderProperty); }
            set { this.SetValue(ExpanderProperty, value); }
        }

        private void OnExpanderChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldElement = e.OldValue as UIElement;
            if (oldElement != null) { this._additionalChildren.Remove(oldElement); }

            var newElement = e.NewValue as UIElement;
            if (newElement != null) { this._additionalChildren.Add(newElement); }
        }

        #endregion

        #region EnsureConnected

        /// <summary>
        /// 当前是否采用虚拟化布局
        /// </summary>
        private bool _isVirtualizing;

        /// <summary>
        /// 是否回收之间已经生成好的容器。
        /// 
        /// 此回收模式与 VirtualizingStackPanel 本身的回收模式有所区别。
        /// 这里回收的单元格不能被其它列所使用，而只是不需要再次生成了。
        /// </summary>
        private bool _isRecycleMode = true;

        private TreeGridCellsPresenter _cellsPresenter;

        private TreeGridRowsPanel _rowsPanel;

        private TreeGrid _treeGrid;

        private TreeGridColumnCollection Columns
        {
            get
            {
                if (this._treeGrid != null) { return this._treeGrid.Columns; }

                return null;
            }
        }

        #region 使用 TreeGrid 上的几个属性来控制汇制过程。

        /// <summary>
        /// 是否绘制 RowHeader 控件。
        /// </summary>
        private bool ShowRowHeader
        {
            get
            {
                if (this._treeGrid != null) return this._treeGrid.OnlyGridMode;

                return false;
            }
        }

        /// <summary>
        /// 行号的左缩进量
        /// </summary>
        private int RowHeaderWidth
        {
            get
            {
                if (this._treeGrid != null) return this._treeGrid.RowHeaderWidth;

                return 0;
            }
        }

        /// <summary>
        /// 是否需要绘制 Expander。
        /// 
        /// 注意：
        /// 由于需要显示层级的缩进，就算当前行没有子行，它的 Expander 也需要占位，即 Hidden 而不是 Collapsed。
        /// 这里，只有当完全不显示层级缩进时，这个值才会是 true。
        /// </summary>
        private bool ShowExpander
        {
            get
            {
                if (this._treeGrid != null) return !this._treeGrid.OnlyGridMode;

                return true;
            }
        }

        #endregion

        private bool IsConnected
        {
            get { return this._treeGrid != null; }
        }

        /// <summary>
        /// 连接到可视父元素，连接成功，则返回 true。
        /// </summary>
        /// <returns></returns>
        private bool TryConnect()
        {
            if (!this.IsConnected)
            {
                this._cellsPresenter = ItemsControl.GetItemsOwner(this) as TreeGridCellsPresenter;
                if (this._cellsPresenter != null)
                {
                    this._cellsPresenter.InternalItemsHost = this;

                    var row = this._cellsPresenter.Row;
                    if (row != null)
                    {
                        row.CellsPanel = this;

                        this._treeGrid = row.TreeGrid;
                        if (this._treeGrid != null)
                        {
                            InternalCollectionChangedEventManager.AddListener(this._treeGrid.Columns, this);

                            this._rowsPanel = this._treeGrid.RowsPanel;

                            //分组显示时，RowsPanel 为 null，也不能进行虚拟化操作。
                            this._isVirtualizing = this._rowsPanel != null && this._treeGrid.IsColumnsVirtualizingEnabled;
                            this._isRecycleMode = this._treeGrid.IsRecycleMode;
                        }
                    }
                }
            }

            return this.IsConnected;
        }

        private void Disconnect()
        {
            if (this.IsConnected)
            {
                InternalCollectionChangedEventManager.RemoveListener(this._treeGrid.Columns, this);

                this._rowsPanel = null;
                this._treeGrid = null;

                var row = this._cellsPresenter.Row;
                if (row != null) { row.CellsPanel = null; }

                this._cellsPresenter.InternalItemsHost = null;
                this._cellsPresenter = null;
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            this.Disconnect();
        }

        #endregion

        protected override Visual GetVisualChild(int index)
        {
            var showRowHeader = this.ShowRowHeader;
            var showExpander = this.ShowExpander;

            //多了一个元素，则优先使用 RowHeader
            if (index == 0)
            {
                if (showRowHeader) { return this.RowHeader; }
                if (showExpander) { return this.Expander; }
            }
            //多了两个元素，则第二个元素应该是 Expander
            else if (index == 1 && showExpander && showRowHeader) { return this.Expander; }

            if (showRowHeader) index--;
            if (showExpander) index--;

            return base.GetVisualChild(index);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                var count = base.VisualChildrenCount;

                if (this.ShowRowHeader) count++;

                if (this.ShowExpander) count++;

                return count;
            }
        }

        #region Measure & Arrange & Realize

        /// <summary>
        /// 每个列所需要的宽度列表。
        /// 
        /// 注意索引是列的 ActualIndex。
        /// </summary>
        private List<double> _desiredWidthList;

        /// <summary>
        /// 可视单元格的起始、终止索引号
        /// 
        /// 范围内的单元格将被生成，其它单元格全部虚拟化。
        /// </summary>
        private int _from, _to;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (!this.TryConnect()) { return default(Size); }

            var columns = this._treeGrid.Columns;

            var totalColumnsWidth = this.CalcVisibleRange(columns);

            this.RealizeChildren();

            var size = this.MeasureChild(columns, availableSize);

            if (_isVirtualizing)
            {
                size.Width = totalColumnsWidth;
            }

            return size;
        }

        /// <summary>
        /// 计算 from，to，并返回总长度。
        /// 
        /// 如果在虚拟化模式下，总长度返回 0.
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        private double CalcVisibleRange(TreeGridColumnCollection columns)
        {
            this._from = 0;
            this._to = columns.Count - 1;

            if (!this._isVirtualizing) { return 0; }

            double allWidth = 0.0;                                 //所有列的宽度总和
            double hOffset = this._rowsPanel.HorizontalOffset;     //水平滚动条位置
            double viewPortWidth = this._rowsPanel.ViewportWidth;  //视窗宽度
            double leftInvisibleSum = 0.0;                         //左边不可见的宽度，逐列累加，直到超过 xOffset
            double visibleSum = 0.0;                               //当前可见的宽度，逐列累加，直到超过 viewPortWidth
            bool isVisible = false;                                //是否当前正在统计不可见的宽度
            bool stop = false;                                     //是否停止计算可见列，而开始统计所有宽度。

            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var column = columns[i];
                var width = column.CalculateActualWidth();
                allWidth += width;
                if (stop) continue;

                if (!isVisible)
                {
                    if (leftInvisibleSum + width > hOffset)
                    {
                        this._from = i;
                        visibleSum = leftInvisibleSum + width - hOffset;
                        isVisible = true;
                    }
                    else
                    {
                        leftInvisibleSum += width;
                    }
                }
                else
                {
                    visibleSum += width;
                    if (visibleSum > viewPortWidth)
                    {
                        this._to = i;
                        stop = true;
                    }
                }
            }

            return allWidth;
        }

        /// <summary>
        /// 实例化需要显示的单元格，
        /// 同时虚拟化不需要显示的单元格。
        /// </summary>
        private void RealizeChildren()
        {
            //注意，需要先访问 InternalChildren，再访问 ItemContainerGenerator，否则 generator 会是 null。
            var children = this.InternalChildren;
            var generator = this.ItemContainerGenerator;

            if (this._isVirtualizing)
            {
                if (this._isRecycleMode)
                {
                    this.Realize(children, generator);
                }
                else
                {
                    this.RealizeWithCleanup(children, generator);
                }
            }
            else
            {
                //非虚拟化模式下，也需要把所有的孩子移除后再重新添加到 InternalChildren 中。
                this.RemoveInternalChildRange(0, children.Count);

                using (generator.StartAt(new GeneratorPosition(-1, 0), GeneratorDirection.Forward))
                {
                    bool isNewlyRealized;
                    UIElement uiElement;
                    while ((uiElement = (generator.GenerateNext(out isNewlyRealized) as UIElement)) != null)
                    {
                        this.AddInternalChild(uiElement);

                        if (isNewlyRealized) generator.PrepareItemContainer(uiElement);
                    }
                }
            }
        }

        private void Realize(UIElementCollection children, IItemContainerGenerator generator)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 虚拟化模式下，为了保证编辑状态的焦点不会丢失，生成单元格过程中不再移除已经之前生成好的单元格。
             * 所有已经生成好的单元格都会被 generator 存储起来，
             * 而以下过程只是修改 InternalChildren，即可视树中的元素：
             * 一开始，先把所有可视树子节点移除，然后不论是否刚生成的单元格，都直接加入到可视树中。
             * 
            **********************************************************************/

            foreach (var child in children)
            {
                var cell = child as TreeGridCell;
                if (cell == null) break;

                cell.VisibleOnVirtualizing = false;
            }

            this.RemoveInternalChildRange(0, children.Count);

            // 获取第一个可视元素位置信息
            var start = generator.GeneratorPositionFromIndex(this._from);
            using (generator.StartAt(start, GeneratorDirection.Forward, true))
            {
                for (int i = this._from; i <= this._to; i++)
                {
                    bool isNewlyRealized;
                    var container = generator.GenerateNext(out isNewlyRealized) as UIElement;

                    this.AddInternalChild(container);

                    var cell = container as TreeGridCell;
                    if (cell != null) cell.VisibleOnVirtualizing = true;

                    if (isNewlyRealized)
                    {
                        generator.PrepareItemContainer(container);
                    }
                    else
                    {
                        //正在编辑的有焦点的控件，需要还原其焦点。
                        if (cell != null)
                        {
                            this._cellsPresenter.RestoreEditingCellFocus(cell);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 测量所有已经生成的单元格。
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        private Size MeasureChild(TreeGridColumnCollection columns, Size availableSize)
        {
            double maxWidth = availableSize.Width;
            double maxHeight = availableSize.Height;
            double rowDesiredHeight = 0.0;                 //行最终需要的高度
            double rowDesiredWidth = 0.0;                  //行最终需要的宽度
            bool firstTime = true;                         //是否在 Init 状态下还没有初始化过 DesiredWidthList。
            var cells = this.InternalChildren;

            for (int i = this._from, j = 0; i <= this._to; i++, j++)
            {
                var column = columns[i];

                //当前行还可用的宽度
                double rowAvaiableWidth = Math.Max(0, maxWidth - rowDesiredWidth);

                //第一列额外元素需要的宽度
                double headersDesiredWidth = 0;

                #region 测量第一列额外元素需要的宽度

                if (i == 0)
                {
                    //测量 RowHeader
                    if (this.ShowRowHeader)
                    {
                        headersDesiredWidth += this.RowHeaderWidth;
                        rowAvaiableWidth -= this.RowHeaderWidth;
                        //this._rowHeaderContainer.Measure(new Size(columnWidth, maxHeight));
                        //x += this._rowHeaderContainer.DesiredSize.Width;
                        //columnWidth -= this._rowHeaderContainer.DesiredSize.Width;
                    }

                    //留下 FirstColumnIndent
                    var firstColumnIndent = Math.Min(this.FirstColumnIndent, rowAvaiableWidth);
                    headersDesiredWidth += firstColumnIndent;
                    rowAvaiableWidth -= firstColumnIndent;

                    //测量 Expander
                    if (this.ShowExpander)
                    {
                        var expander = this.Expander;
                        expander.Measure(new Size(rowAvaiableWidth, maxHeight));
                        headersDesiredWidth += expander.DesiredSize.Width;
                        rowAvaiableWidth -= expander.DesiredSize.Width;
                    }
                }

                #endregion

                var cell = cells[j];

                //测量单元格
                var state = column.State;
                if (state == ColumnMeasureState.Init || state == ColumnMeasureState.Headered)
                {
                    if (firstTime)
                    {
                        TreeGridHelper.EnsureDesiredWidthList(ref this._desiredWidthList, this._treeGrid.Columns);
                        this.LayoutUpdated += this.OnMeasureLayoutUpdated;
                        firstTime = false;
                    }

                    cell.Measure(new Size(rowAvaiableWidth, maxHeight));

                    //只有在虚拟化后的当前页面时，才影响动态宽度。
                    if (this._cellsPresenter.IsOnCurrentPage)
                    {
                        //当前列需要的宽度应该是列的宽度加额外元素的宽度。
                        column.EnsureDataWidth(cell.DesiredSize.Width + headersDesiredWidth);
                    }
                    this._desiredWidthList[column.StableIndex] = column.DesiredDataWidth;

                    rowDesiredWidth += column.DesiredDataWidth;
                }
                else
                {
                    var actualWidth = column.CalculateActualWidth();

                    rowAvaiableWidth = Math.Min(rowAvaiableWidth, actualWidth - headersDesiredWidth);

                    cell.Measure(new Size(rowAvaiableWidth, maxHeight));

                    rowDesiredWidth += actualWidth;
                }

                rowDesiredHeight = Math.Max(rowDesiredHeight, cell.DesiredSize.Height);
            }

            rowDesiredWidth += TreeGridHeaderRowPresenter.c_EndPadding;

            this._cellsPresenter.IsOnCurrentPageValid = false;

            return new Size(rowDesiredWidth, rowDesiredHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (!this.TryConnect()) { return finalSize; }

            var columns = this._treeGrid.Columns;

            //计算左边不可见的列的宽度
            double x = 0.0;
            for (int i = 0; i < this._from; i++)
            {
                x += columns[i].CalculateActualWidth();
            }

            var children = this.InternalChildren;
            double rowWidth = finalSize.Width;
            for (int i = this._from, j = 0; i <= this._to; i++, j++)
            {
                var column = columns[i];

                var columnWidth = Math.Min(column.CalculateActualWidth(), rowWidth);

                #region 画上第一列额外的元素

                if (i == 0)
                {
                    //第一列中的布局：RowHeaderWidth + FirstColumnIndent(Level) + ExpanderWidth + ColumnUI

                    //画上 RowHeader
                    if (this.ShowRowHeader)
                    {
                        var rowHeaderWidth = Math.Min(columnWidth, this.RowHeaderWidth);

                        var header = this.RowHeader;
                        if (header != null)
                        {
                            header.Arrange(new Rect(0, 0, rowHeaderWidth, double.MaxValue));
                        }

                        x += rowHeaderWidth;
                        columnWidth -= rowHeaderWidth;
                    }

                    //留下 FirstColumnIndent
                    var firstColumnIndent = Math.Min(columnWidth, this.FirstColumnIndent);
                    x += firstColumnIndent;
                    columnWidth -= firstColumnIndent;

                    //画上 Expander
                    if (this.ShowExpander)
                    {
                        var expander = this.Expander;
                        double expanderWidth = Math.Min(columnWidth, expander.DesiredSize.Width);

                        expander.Arrange(new Rect(x, 0, expanderWidth, expander.DesiredSize.Height));

                        x += expanderWidth;
                        columnWidth -= expanderWidth;
                    }
                }

                #endregion

                var cell = children[j];
                cell.Arrange(new Rect(x, 0, columnWidth, finalSize.Height));

                x += columnWidth;

                rowWidth -= columnWidth;
                if (rowWidth == 0) break;
            }

            return finalSize;
        }

        /// <summary>
        /// 此方法会设置所有列的状态为 Data。
        /// 并在双击列头而导致重新计算动态列宽的情况下，触发第二次测量过程。
        /// </summary>
        private void OnMeasureLayoutUpdated(object sender, EventArgs e)
        {
            var desiredWidthList = this._desiredWidthList;
            bool invalidateDesiredWidth = false;
            var columns = this.Columns;

            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var column = columns[i] as TreeGridColumn;

                //所有动态列宽的列的状态都更新为 Data，表示已经计算出最大列宽。
                if (column.State != ColumnMeasureState.SpecificWidth)
                {
                    column.State = ColumnMeasureState.Data;

                    var index = column.StableIndex;

                    //如果 desiredWidthList 大小不一致，已经过期，则需要重新测量。
                    if (desiredWidthList == null || index >= desiredWidthList.Count)
                    {
                        invalidateDesiredWidth = true;
                        break;
                    }

                    //如果 desiredWidth 与 desiredWidthList 中的大小不一致，也表示已经过期，需要重新测量。
                    var desiredWidth = column.DesiredDataWidth;
                    if (!DoubleUtil.AreClose(desiredWidth, desiredWidthList[index]))
                    {
                        desiredWidthList[index] = desiredWidth;
                        invalidateDesiredWidth = true;
                    }
                }
            }

            //触发第二次测量过程。
            if (invalidateDesiredWidth)
            {
                base.InvalidateMeasure();
            }

            //保证只运行一次。
            this.LayoutUpdated -= this.OnMeasureLayoutUpdated;
        }

        #endregion

        #region 响应列的变化

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(InternalCollectionChangedEventManager))
            {
                this.ColumnCollectionChanged(sender, e as NotifyCollectionChangedEventArgs);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 当本行对应的 TreeGridColumnCollection 列集合变化时，发生此事件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            var columnArgs = arg as TreeGridColumnCollectionChangedEventArgs;
            if (columnArgs != null)
            {
                if (base.IsInitialized)
                {
                    if (columnArgs.Column != null)
                    {
                        this.OnColumnPropertyChanged(columnArgs.Column, columnArgs.PropertyName);
                    }
                    else
                    {
                        this.OnColumnCollectionChanged(columnArgs);
                    }
                }
            }
        }

        /// <summary>
        /// 在列的项变化时，同步变化对应的单元格列表 UIChildren。
        /// </summary>
        /// <param name="e"></param>
        private void OnColumnCollectionChanged(TreeGridColumnCollectionChangedEventArgs e)
        {
            TreeGridHelper.UpdateDesiredWidthListOnColumnChanged(ref this._desiredWidthList, e);

            if (this.TryConnect())
            {
                this._cellsPresenter.UpdateItemsSourceOnColumnsChanged(e);

                base.InvalidateMeasure();
            }
        }

        /// <summary>
        /// 当某个 TreeGridColumn 的属性变更时，把这个变更同步到相应的单元格上。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="propertyName"></param>
        private void OnColumnPropertyChanged(TreeGridColumn column, string propertyName)
        {
            if (TreeGridColumn.c_ActualWidthName == propertyName) { return; }

            //列宽变化时，重新测量
            if (TreeGridColumn.WidthProperty.Name == propertyName)
            {
                base.InvalidateMeasure();
                return;
            }

            int actualIndex = column.StableIndex;
            if (actualIndex >= 0 && actualIndex < base.InternalChildren.Count)
            {
                var cell = base.InternalChildren[actualIndex] as TreeGridCell;
                if (cell != null)
                {
                    if (TreeGridColumn.BindingProperty.Name == propertyName)
                    {
                        var textBlock = cell.Content as TextBlock;
                        if (textBlock != null)
                        {
                            var displayMemberBinding = column.Binding;
                            if (displayMemberBinding != null)
                            {
                                textBlock.SetBinding(TextBlock.TextProperty, displayMemberBinding);
                            }
                        }
                    }
                    else if (TreeGridColumn.CellStyleProperty.Name == propertyName)
                    {
                        cell.Style = column.CellStyle;
                    }
                    else if (TreeGridColumn.CellContentTemplateProperty.Name == propertyName)
                    {
                        cell.ContentTemplate = column.GetDisplayCellTemplate();
                    }
                    else if (TreeGridColumn.CellContentTemplateSelectorProperty.Name == propertyName)
                    {
                        cell.ContentTemplateSelector = column.CellContentTemplateSelector;
                    }
                }
            }
        }

        #endregion

        #region 暂时用不到的容器生成方案

        //以下代码只有解决焦点的恢复问题后才可继续使用。

        /// <summary>
        /// 生成容器的同时，删除不必要的容器。
        /// </summary>
        /// <param name="children"></param>
        /// <param name="generator"></param>
        private void RealizeWithCleanup(UIElementCollection children, IItemContainerGenerator generator)
        {
            //注意，此模式下会有拖动列头顺序时，单元格子不变化的问题。由于暂时不使用此模式，所以暂时先不完成。

            // 获取第一个可视元素位置信息
            var start = generator.GeneratorPositionFromIndex(this._from);
            // 根据元素位置信息计算子元素索引
            int childIndex = start.Offset == 0 ? start.Index : start.Index + 1;
            using (generator.StartAt(start, GeneratorDirection.Forward, true))
            {
                for (int i = this._from; i <= this._to; i++, childIndex++)
                {
                    bool isNewlyRealized;
                    var container = generator.GenerateNext(out isNewlyRealized) as UIElement;
                    if (isNewlyRealized)
                    {
                        if (childIndex >= children.Count)
                        {
                            this.AddInternalChild(container);
                        }
                        else
                        {
                            this.InsertInternalChild(childIndex, container);
                        }
                        generator.PrepareItemContainer(container);
                    }
                }
            }

            this.Cleanup(children, generator);
        }

        /// <summary>
        /// 清除不显示的单元格。
        /// </summary>
        /// <param name="children"></param>
        /// <param name="generator"></param>
        private void Cleanup(UIElementCollection children, IItemContainerGenerator generator)
        {
            //清除不需要显示的子元素，注意从集合后向前操作，以免造成操作过程中元素索引发生改变
            for (int i = children.Count - 1; i > -1; i--)
            {
                // 通过已显示的子元素的位置信息得出元素索引
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);

                // 移除不再显示的元素
                if (itemIndex < this._from || itemIndex > this._to)
                {
                    generator.Remove(childGeneratorPos, 1);
                    this.RemoveInternalChildRange(i, 1);
                }
            }
        }

        #endregion
    }
}