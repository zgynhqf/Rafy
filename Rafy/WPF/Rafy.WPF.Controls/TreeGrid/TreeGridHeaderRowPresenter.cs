/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111206
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111206
 * 完整复制 GridViewHeaderRowPresenter 代码。 胡庆访 20121011 13:31
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 列头行的显示器
    /// </summary>
    [StyleTypedProperty(Property = "ColumnHeaderContainerStyle", StyleTargetType = typeof(TreeGridColumnHeader))]
    public class TreeGridHeaderRowPresenter : TreeGridRowPresenterBase
    {
        #region 静态字段与静态构造器

        private const double c_thresholdX = 4.0;

        static TreeGridHeaderRowPresenter()
        {
            InitDPList();
        }

        #endregion

        #region 字段

        private bool _gvHeadersValid;
        private List<TreeGridColumnHeader> _gvHeaders;
        private ItemsControl _itemsControl;
        private TreeGridColumnHeader _draggingSrcHeader;
        private Point _startPos;
        private Point _relativeStartPos;
        private Point _currentPos;
        private int _startColumnIndex;
        private int _desColumnIndex;
        private bool _isHeaderDragging;
        private bool _isColumnChangedOrCreated;
        private bool _prepareDragging;

        #endregion

        #region ColumnHeaderContainerStyle DependencyProperty

        public static readonly DependencyProperty ColumnHeaderContainerStyleProperty = TreeGrid.ColumnHeaderContainerStyleProperty.AddOwner(typeof(TreeGridHeaderRowPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(TreeGridHeaderRowPresenter.PropertyChanged)));

        public Style ColumnHeaderContainerStyle
        {
            get { return (Style)this.GetValue(ColumnHeaderContainerStyleProperty); }
            set { this.SetValue(ColumnHeaderContainerStyleProperty, value); }
        }

        #endregion

        #region ColumnHeaderTemplate DependencyProperty

        public static readonly DependencyProperty ColumnHeaderTemplateProperty = TreeGrid.ColumnHeaderTemplateProperty.AddOwner(typeof(TreeGridHeaderRowPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(TreeGridHeaderRowPresenter.PropertyChanged)));

        public DataTemplate ColumnHeaderTemplate
        {
            get
            {
                return (DataTemplate)base.GetValue(TreeGridHeaderRowPresenter.ColumnHeaderTemplateProperty);
            }
            set
            {
                base.SetValue(TreeGridHeaderRowPresenter.ColumnHeaderTemplateProperty, value);
            }
        }

        #endregion

        #region ColumnHeaderTemplateSelector DependencyProperty

        public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty = TreeGrid.ColumnHeaderTemplateSelectorProperty.AddOwner(typeof(TreeGridHeaderRowPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(TreeGridHeaderRowPresenter.PropertyChanged)));

        public DataTemplateSelector ColumnHeaderTemplateSelector
        {
            get
            {
                return (DataTemplateSelector)base.GetValue(TreeGridHeaderRowPresenter.ColumnHeaderTemplateSelectorProperty);
            }
            set
            {
                base.SetValue(TreeGridHeaderRowPresenter.ColumnHeaderTemplateSelectorProperty, value);
            }
        }

        #endregion

        #region ColumnHeaderStringFormat DependencyProperty

        public static readonly DependencyProperty ColumnHeaderStringFormatProperty = TreeGrid.ColumnHeaderStringFormatProperty.AddOwner(typeof(TreeGridHeaderRowPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(TreeGridHeaderRowPresenter.PropertyChanged)));

        public string ColumnHeaderStringFormat
        {
            get
            {
                return (string)base.GetValue(TreeGridHeaderRowPresenter.ColumnHeaderStringFormatProperty);
            }
            set
            {
                base.SetValue(TreeGridHeaderRowPresenter.ColumnHeaderStringFormatProperty, value);
            }
        }

        #endregion

        #region AllowsColumnReorder DependencyProperty

        public static readonly DependencyProperty AllowsColumnReorderProperty = TreeGrid.AllowsColumnReorderProperty.AddOwner(typeof(TreeGridHeaderRowPresenter));

        public bool AllowsColumnReorder
        {
            get
            {
                return (bool)base.GetValue(TreeGridHeaderRowPresenter.AllowsColumnReorderProperty);
            }
            set
            {
                base.SetValue(TreeGridHeaderRowPresenter.AllowsColumnReorderProperty, value);
            }
        }

        #endregion

        #region ColumnHeaderContextMenu DependencyProperty

        public static readonly DependencyProperty ColumnHeaderContextMenuProperty = TreeGrid.ColumnHeaderContextMenuProperty.AddOwner(typeof(TreeGridHeaderRowPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(TreeGridHeaderRowPresenter.PropertyChanged)));

        public ContextMenu ColumnHeaderContextMenu
        {
            get
            {
                return (ContextMenu)base.GetValue(TreeGridHeaderRowPresenter.ColumnHeaderContextMenuProperty);
            }
            set
            {
                base.SetValue(TreeGridHeaderRowPresenter.ColumnHeaderContextMenuProperty, value);
            }
        }

        #endregion

        #region ColumnHeaderToolTip DependencyProperty

        public static readonly DependencyProperty ColumnHeaderToolTipProperty = TreeGrid.ColumnHeaderToolTipProperty.AddOwner(typeof(TreeGridHeaderRowPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(TreeGridHeaderRowPresenter.PropertyChanged)));

        public object ColumnHeaderToolTip
        {
            get
            {
                return base.GetValue(TreeGridHeaderRowPresenter.ColumnHeaderToolTipProperty);
            }
            set
            {
                base.SetValue(TreeGridHeaderRowPresenter.ColumnHeaderToolTipProperty, value);
            }
        }

        #endregion

        internal List<TreeGridColumnHeader> ActualColumnHeaders
        {
            get
            {
                if (this._gvHeaders == null || !this._gvHeadersValid)
                {
                    this._gvHeadersValid = true;
                    this._gvHeaders = new List<TreeGridColumnHeader>();
                    UIElementCollection internalChildren = base.UIChildren;
                    int i = 0;
                    int count = base.Columns.Count;
                    while (i < count)
                    {
                        TreeGridColumnHeader gridViewColumnHeader = internalChildren[this.GetVisualIndex(i)] as TreeGridColumnHeader;
                        if (gridViewColumnHeader != null)
                        {
                            this._gvHeaders.Add(gridViewColumnHeader);
                        }
                        i++;
                    }
                }
                return this._gvHeaders;
            }
        }

        #region 记录所有列头的位置大小，方便一些事件的位置计算

        /// <summary>
        /// 记录所有列头的位置大小。
        /// </summary>
        private List<Rect> _headersPositionList;
        private List<Rect> HeadersPositionList
        {
            get
            {
                if (this._headersPositionList == null)
                {
                    this._headersPositionList = new List<Rect>();
                }
                return this._headersPositionList;
            }
        }

        private int FindIndexByPosition(Point startPos, bool findNearestColumn)
        {
            int num = -1;
            if (startPos.X < 0.0)
            {
                return 0;
            }
            int i = 0;
            var headersPosList = this.HeadersPositionList;
            while (i < headersPosList.Count)
            {
                num++;
                Rect rect = headersPosList[i];
                double x = rect.X;
                double num2 = x + rect.Width;
                if (DoubleUtil.GreaterThanOrClose(startPos.X, x) && DoubleUtil.LessThanOrClose(startPos.X, num2))
                {
                    if (!findNearestColumn)
                    {
                        break;
                    }
                    double value = (x + num2) * 0.5;
                    if (DoubleUtil.GreaterThanOrClose(startPos.X, value) && i != headersPosList.Count - 1)
                    {
                        num++;
                        break;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }
            return num;
        }

        #endregion

        #region override

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var gridViewColumnHeader = e.Source as TreeGridColumnHeader;
            if (gridViewColumnHeader != null && this.AllowsColumnReorder)
            {
                this.PrepareHeaderDrag(gridViewColumnHeader, e.GetPosition(this), e.GetPosition(gridViewColumnHeader), false);
                this.MakeParentItemsControlGotFocus();
            }
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this._prepareDragging = false;
            if (this._isHeaderDragging)
            {
                this.FinishHeaderDrag(false);
            }
            e.Handled = true;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed && this._prepareDragging)
            {
                this._currentPos = e.GetPosition(this);
                this._desColumnIndex = this.FindIndexByPosition(this._currentPos, true);
                if (!this._isHeaderDragging)
                {
                    if (this.CheckStartHeaderDrag(this._currentPos, this._startPos))
                    {
                        this.StartHeaderDrag();
                        base.InvalidateMeasure();
                    }
                }
                else
                {
                    bool flag = IsMousePositionValid(this._floatingHeader, this._currentPos, 2.0);
                    this._indicator.Visibility = (this._floatingHeader.Visibility = (flag ? Visibility.Visible : Visibility.Hidden));
                    base.InvalidateArrange();
                }
            }
            e.Handled = true;
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            if (e.LeftButton == MouseButtonState.Pressed && this._isHeaderDragging)
            {
                this.FinishHeaderDrag(true);
            }
            this._prepareDragging = false;
        }

        /// <summary>
        /// 在列的项变化时，同步变化对应的单元格列表 UIChildren。
        /// </summary>
        /// <param name="e"></param>
        internal override void OnColumnCollectionChanged(TreeGridColumnCollectionChangedEventArgs e)
        {
            base.OnColumnCollectionChanged(e);

            var internalChildren = base.UIChildren;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int visualIndex = this.GetVisualIndex(e.NewStartingIndex);
                        var column = (TreeGridColumn)e.NewItems[0];
                        this.CreateAndInsertHeader(column, visualIndex + 1);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    this.RemoveHeader(null, this.GetVisualIndex(e.OldStartingIndex));
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        int visualIndex = this.GetVisualIndex(e.OldStartingIndex);
                        this.RemoveHeader(null, visualIndex);
                        var column = (TreeGridColumn)e.NewItems[0];
                        this.CreateAndInsertHeader(column, visualIndex);
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        int visualIndex2 = this.GetVisualIndex(e.OldStartingIndex);
                        int visualIndex3 = this.GetVisualIndex(e.NewStartingIndex);
                        var element = (TreeGridColumnHeader)internalChildren[visualIndex2];
                        internalChildren.RemoveAt(visualIndex2);
                        internalChildren.Insert(visualIndex3, element);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        int count = e.ClearedColumns.Count;
                        for (int i = 0; i < count; i++)
                        {
                            this.RemoveHeader(null, 1);
                        }
                        break;
                    }
            }
            this.BuildHeaderLinks();
            this._isColumnChangedOrCreated = true;
        }

        /// <summary>
        /// 在列的属性变化时，尝试重新绘制界面
        /// </summary>
        /// <param name="column"></param>
        /// <param name="propertyName"></param>
        internal override void OnColumnPropertyChanged(TreeGridColumn column, string propertyName)
        {
            base.OnColumnPropertyChanged(column, propertyName);

            if (column.StableIndex >= 0)
            {
                var header = this.FindHeaderByColumn(column);
                if (header != null)
                {
                    if (TreeGridColumn.HeaderProperty.Name == propertyName)
                    {
                        if (!header.IsInternalGenerated || column.Header is TreeGridColumnHeader)
                        {
                            int index = base.UIChildren.IndexOf(header);
                            this.RemoveHeader(header, -1);
                            this.CreateAndInsertHeader(column, index);
                            this.BuildHeaderLinks();
                            return;
                        }
                        this.UpdateHeaderContent(header);
                    }
                    else
                    {
                        var columnDPFromName = TreeGridHeaderRowPresenter.GetColumnDPFromName(propertyName);
                        if (columnDPFromName != null)
                        {
                            this.UpdateHeaderProperty(header, columnDPFromName);
                        }
                    }
                }
            }
        }

        #endregion

        #region Measure & Arrange

        /// <summary>
        /// 是否绘制 RowHeader 控件。
        /// </summary>
        private bool ShowRowHeader
        {
            get
            {
                var grid = this.TreeGrid;
                if (grid != null) return grid.OnlyGridMode;

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
                var grid = this.TreeGrid;
                if (grid != null) return grid.RowHeaderWidth;

                return 0;
            }
        }

        /// <summary>
        /// 由于最终需要的宽度从 MeasureOverride 返回后，如果超过传入的 constraint，
        /// FrameworkElement 会把它忽略并设置为限制的大小，所以外部得到的 DesiredWidth 其实并不是真正需要的宽度。
        /// 所以，这里把这个值存储起来，其它控件可以获取这个值，来知道 Header 行真正的宽度是需要多少。
        /// </summary>
        internal double RealDesiredWith = 0;

        protected override Size MeasureOverride(Size constraint)
        {
            this.ConnectToGrid();
            this.UpdateVisualTree();

            //不论外界是否虚拟化，这里都不虚拟化，按照无限宽度来进行测量。
            //否则会造成可用的宽度被使用完，视区外的列会被测量出过高的高度。
            constraint = new Size(double.PositiveInfinity, constraint.Height);

            var internalChildren = base.UIChildren;
            double heightDesired = 0.0;
            double widthDesired = 0.0;
            double heightConstraint = constraint.Height;
            bool fristTime = true;

            var columns = base.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                var uiElement = internalChildren[this.GetVisualIndex(i)];
                if (uiElement != null)
                {
                    double widthConstraint = Math.Max(0.0, constraint.Width - widthDesired);
                    var column = columns[i];
                    if (column.State == ColumnMeasureState.Init)
                    {
                        if (fristTime)
                        {
                            base.EnsureDesiredWidthList();
                            this.LayoutUpdated += this.OnMeasureLayoutUpdated;
                            fristTime = false;
                        }
                        uiElement.Measure(new Size(widthConstraint, heightConstraint));
                        base.DesiredWidthList[column.StableIndex] = column.EnsureDataWidth(uiElement.DesiredSize.Width);
                        widthDesired += column.DesiredDataWidth;
                    }
                    else
                    {
                        var columnDesiredWidth = column.State == ColumnMeasureState.Headered || column.State == ColumnMeasureState.Data
                            ? column.DesiredDataWidth : column.Width;
                        widthConstraint = Math.Min(widthConstraint, columnDesiredWidth);
                        uiElement.Measure(new Size(widthConstraint, heightConstraint));
                        widthDesired += columnDesiredWidth;
                    }
                    heightDesired = Math.Max(heightDesired, uiElement.DesiredSize.Height);
                }
            }

            this._paddingHeader.Measure(new Size(0.0, heightConstraint));
            heightDesired = Math.Max(heightDesired, this._paddingHeader.DesiredSize.Height);
            widthDesired += c_EndPadding;

            if (this._isHeaderDragging)
            {
                this._indicator.Measure(constraint);
                this._floatingHeader.Measure(constraint);
            }

            RealDesiredWith = widthDesired;
            return new Size(widthDesired, heightDesired);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var internalChildren = base.UIChildren;

            //x 从负方向的 HorizontalOffset 开始
            double x = -this.GetHorizontalOffsetForArrange();
            double widthAvailable = arrangeSize.Width;
            var headersPosList = this.HeadersPositionList;
            headersPosList.Clear();

            var columns = base.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                UIElement uiElement = internalChildren[this.GetVisualIndex(i)];
                if (uiElement != null)
                {
                    var column = columns[i];

                    var columnDesiredWidth = column.State == ColumnMeasureState.SpecificWidth ? column.Width : column.DesiredDataWidth;
                    double width = Math.Min(widthAvailable, columnDesiredWidth);
                    var rect = new Rect(x, 0.0, width, arrangeSize.Height);

                    //如果要显示行号，则第一列要适当的缩小宽度。这样可以使得列头显示得比较好看。
                    if (i == 0 && this.ShowRowHeader)
                    {
                        var rowHeaderWidth = this.RowHeaderWidth;
                        rect.X += rowHeaderWidth;
                        rect.Width -= rowHeaderWidth;
                    }

                    uiElement.Arrange(rect);
                    headersPosList.Add(rect);

                    widthAvailable -= width;
                    x += width;
                }

                if (this._isColumnChangedOrCreated)
                {
                    for (int j = 0; j < columns.Count; j++)
                    {
                        var gridViewColumnHeader = internalChildren[this.GetVisualIndex(j)] as TreeGridColumnHeader;
                        gridViewColumnHeader.CheckWidthForPreviousHeaderGripper();
                    }
                    this._paddingHeader.CheckWidthForPreviousHeaderGripper();
                    this._isColumnChangedOrCreated = false;
                }
            }

            var headerRect = new Rect(x, 0.0, Math.Max(widthAvailable, 0.0), arrangeSize.Height);
            this._paddingHeader.Arrange(headerRect);
            headersPosList.Add(headerRect);

            if (this._isHeaderDragging)
            {
                this._floatingHeader.Arrange(new Rect(new Point(this._currentPos.X - this._relativeStartPos.X, 0.0), headersPosList[this._startColumnIndex].Size));
                Point location = new Point(headersPosList[_desColumnIndex].X, 0.0);
                this._indicator.Arrange(new Rect(location, new Size(this._indicator.DesiredSize.Width, arrangeSize.Height)));
            }

            return arrangeSize;
        }

        private void OnMeasureLayoutUpdated(object sender, EventArgs e)
        {
            var desiredWidthList = base.DesiredWidthList;
            bool flag = false;
            var columns = base.Columns;

            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var current = columns[i] as TreeGridColumn;

                if (current.State != ColumnMeasureState.SpecificWidth)
                {
                    if (current.State == ColumnMeasureState.Init)
                    {
                        current.State = ColumnMeasureState.Headered;
                    }
                    if (desiredWidthList == null || current.StableIndex >= desiredWidthList.Count)
                    {
                        flag = true;
                        break;
                    }
                    if (!DoubleUtil.AreClose(current.DesiredDataWidth, desiredWidthList[current.StableIndex]))
                    {
                        desiredWidthList[current.StableIndex] = current.DesiredDataWidth;
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                base.InvalidateMeasure();
            }

            //保证只运行一次。
            this.LayoutUpdated -= this.OnMeasureLayoutUpdated;
        }

        #endregion

        #region Focus

        internal void MakeParentItemsControlGotFocus()
        {
            if (this._itemsControl != null && !this._itemsControl.IsKeyboardFocusWithin)
            {
                //ListBox listBox = this._itemsControl as ListBox;
                //if (listBox != null && listBox.LastActionItem != null)
                //{
                //    listBox.LastActionItem.Focus();
                //    return;
                //}
                this._itemsControl.Focus();
            }
        }

        #endregion

        #region 按照优先级更新 Header 的属性

        private static DependencyProperty[][] s_DPList;

        internal void UpdateHeaderProperty(TreeGridColumnHeader header, DependencyProperty property)
        {
            DependencyProperty gvDP;
            DependencyProperty columnDP;
            DependencyProperty targetDP;
            GetMatchingDPs(property, out gvDP, out columnDP, out targetDP);
            this.UpdateHeaderProperty(header, targetDP, columnDP, gvDP);
        }

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (TreeGridHeaderRowPresenter)d;
            if (e.Property == ColumnHeaderTemplateProperty || e.Property == ColumnHeaderTemplateSelectorProperty)
            {
                TreeGridHelper.CheckTemplateAndTemplateSelector("TreeGridHeaderRowPresenter", ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, sender);
            }
            sender.UpdateAllHeaders(e.Property);
        }

        private void UpdateAllHeaders(DependencyProperty dp)
        {
            DependencyProperty gvDP;
            DependencyProperty columnDP;
            DependencyProperty targetDP;
            TreeGridHeaderRowPresenter.GetMatchingDPs(dp, out gvDP, out columnDP, out targetDP);
            int num;
            int num2;
            this.GetIndexRange(dp, out num, out num2);
            var internalChildren = base.UIChildren;
            for (int i = num; i <= num2; i++)
            {
                var gridViewColumnHeader = internalChildren[i] as TreeGridColumnHeader;
                if (gridViewColumnHeader != null)
                {
                    this.UpdateHeaderProperty(gridViewColumnHeader, targetDP, columnDP, gvDP);
                }
            }
        }

        private void UpdateHeader(TreeGridColumnHeader header)
        {
            this.UpdateHeaderContent(header);
            int i = 0;
            int num = s_DPList[0].Length;
            while (i < num)
            {
                this.UpdateHeaderProperty(header, s_DPList[2][i], s_DPList[1][i], s_DPList[0][i]);
                i++;
            }
        }

        /// <summary>
        /// 更新指定 Header 的指定依赖属性。
        /// </summary>
        /// <param name="header"></param>
        /// <param name="targetDP"></param>
        /// <param name="columnDP"></param>
        /// <param name="gvDP"></param>
        private void UpdateHeaderProperty(TreeGridColumnHeader header, DependencyProperty targetDP, DependencyProperty columnDP, DependencyProperty gvDP)
        {
            if (gvDP == TreeGridHeaderRowPresenter.ColumnHeaderContainerStyleProperty && header.Role == TreeGridColumnHeaderRole.Padding)
            {
                Style columnHeaderContainerStyle = this.ColumnHeaderContainerStyle;
                if (columnHeaderContainerStyle != null && !columnHeaderContainerStyle.TargetType.IsAssignableFrom(typeof(TreeGridColumnHeader)))
                {
                    header.Style = null;
                    return;
                }
            }

            object value = null;

            var column = header.Column;
            if (column != null && columnDP != null)
            {
                value = column.GetValue(columnDP);
            }

            if (value == null)
            {
                value = base.GetValue(gvDP);
            }

            header.UpdateProperty(targetDP, value);
        }

        private void UpdateHeaderContent(TreeGridColumnHeader header)
        {
            if (header != null && header.IsInternalGenerated)
            {
                TreeGridColumn column = header.Column;
                if (column != null)
                {
                    if (column.Header == null)
                    {
                        header.ClearValue(ContentControl.ContentProperty);
                        return;
                    }
                    header.Content = column.Header;
                }
            }
        }

        private void GetIndexRange(DependencyProperty dp, out int iStart, out int iEnd)
        {
            iStart = ((dp == TreeGridHeaderRowPresenter.ColumnHeaderTemplateProperty || dp == TreeGridHeaderRowPresenter.ColumnHeaderTemplateSelectorProperty || dp == TreeGridHeaderRowPresenter.ColumnHeaderStringFormatProperty) ? 1 : 0);
            iEnd = base.UIChildren.Count - 3;
        }

        private static void GetMatchingDPs(DependencyProperty indexDP, out DependencyProperty gvDP, out DependencyProperty columnDP, out DependencyProperty headerDP)
        {
            for (int i = 0; i < s_DPList.Length; i++)
            {
                for (int j = 0; j < s_DPList[i].Length; j++)
                {
                    if (indexDP == s_DPList[i][j])
                    {
                        gvDP = s_DPList[0][j];
                        columnDP = s_DPList[1][j];
                        headerDP = s_DPList[2][j];
                        return;
                    }
                }
            }
            headerDP = null;
            columnDP = null;
            gvDP = null;
        }

        private static void InitDPList()
        {
            var array = new DependencyProperty[3][];
            array[0] = new DependencyProperty[]
			{
				TreeGridHeaderRowPresenter.ColumnHeaderContainerStyleProperty,
				TreeGridHeaderRowPresenter.ColumnHeaderTemplateProperty,
				TreeGridHeaderRowPresenter.ColumnHeaderTemplateSelectorProperty,
				TreeGridHeaderRowPresenter.ColumnHeaderStringFormatProperty,
				TreeGridHeaderRowPresenter.ColumnHeaderContextMenuProperty,
				TreeGridHeaderRowPresenter.ColumnHeaderToolTipProperty
			};

            //column
            var array2 = new DependencyProperty[6];
            array2[0] = TreeGridColumn.HeaderContainerStyleProperty;
            array2[1] = TreeGridColumn.HeaderTemplateProperty;
            array2[2] = TreeGridColumn.HeaderTemplateSelectorProperty;
            array2[3] = TreeGridColumn.HeaderStringFormatProperty;
            array[1] = array2;

            //header
            array[2] = new DependencyProperty[]
			{
				FrameworkElement.StyleProperty,
				ContentControl.ContentTemplateProperty,
				ContentControl.ContentTemplateSelectorProperty,
				ContentControl.ContentStringFormatProperty,
				FrameworkElement.ContextMenuProperty,
				FrameworkElement.ToolTipProperty
			};

            s_DPList = array;
        }

        #endregion

        #region Drag

        private void PrepareHeaderDrag(TreeGridColumnHeader header, Point pos, Point relativePos, bool cancelInvoke)
        {
            if (header.Role == TreeGridColumnHeaderRole.Normal)
            {
                this._prepareDragging = true;
                this._isHeaderDragging = false;
                this._draggingSrcHeader = header;
                this._startPos = pos;
                this._relativeStartPos = relativePos;
                if (!cancelInvoke)
                {
                    this._startColumnIndex = this.FindIndexByPosition(this._startPos, false);
                }
            }
        }

        private void StartHeaderDrag()
        {
            this._startPos = this._currentPos;
            this._isHeaderDragging = true;
            this._draggingSrcHeader.SuppressClickEvent = true;
            base.Columns.BlockWrite();
            base.UIChildren.Remove(this._floatingHeader);
            this.AddFloatingHeader(this._draggingSrcHeader);
            this.UpdateFloatingHeader(this._draggingSrcHeader);
        }

        private void FinishHeaderDrag(bool isCancel)
        {
            this._prepareDragging = false;
            this._isHeaderDragging = false;
            this._draggingSrcHeader.SuppressClickEvent = false;
            this._floatingHeader.Visibility = Visibility.Hidden;
            this._floatingHeader.ResetFloatingHeaderCanvasBackground();
            this._indicator.Visibility = Visibility.Hidden;
            base.Columns.UnblockWrite();
            if (!isCancel)
            {
                bool flag = IsMousePositionValid(this._floatingHeader, this._currentPos, 2.0);
                int newIndex = (this._startColumnIndex >= this._desColumnIndex) ? this._desColumnIndex : (this._desColumnIndex - 1);
                if (flag)
                {
                    base.Columns.Move(this._startColumnIndex, newIndex);
                }
            }
        }

        private void UpdateFloatingHeader(TreeGridColumnHeader srcHeader)
        {
            this._floatingHeader.Style = srcHeader.Style;
            this._floatingHeader.FloatSourceHeader = srcHeader;
            this._floatingHeader.Width = srcHeader.ActualWidth;
            this._floatingHeader.Height = srcHeader.ActualHeight;
            this._floatingHeader.SetValue(TreeGridColumnHeader.ColumnPropertyKey, srcHeader.Column);
            this._floatingHeader.Visibility = Visibility.Hidden;
            this._floatingHeader.MinWidth = srcHeader.MinWidth;
            this._floatingHeader.MinHeight = srcHeader.MinHeight;
            object obj = srcHeader.ReadLocalValue(ContentControl.ContentTemplateProperty);
            if (obj != DependencyProperty.UnsetValue && obj != null)
            {
                this._floatingHeader.ContentTemplate = srcHeader.ContentTemplate;
            }
            object obj2 = srcHeader.ReadLocalValue(ContentControl.ContentTemplateSelectorProperty);
            if (obj2 != DependencyProperty.UnsetValue && obj2 != null)
            {
                this._floatingHeader.ContentTemplateSelector = srcHeader.ContentTemplateSelector;
            }
            if (!(srcHeader.Content is Visual))
            {
                this._floatingHeader.Content = srcHeader.Content;
            }
        }

        private bool CheckStartHeaderDrag(Point currentPos, Point originalPos)
        {
            return DoubleUtil.GreaterThan(Math.Abs(currentPos.X - originalPos.X), c_thresholdX);
        }

        private void RenewHeadersKeyDownEvents()
        {
            var itemsControl = this._itemsControl;
            this._itemsControl = FindItemsControlThroughTemplatedParent(this);
            if (itemsControl != this._itemsControl)
            {
                if (itemsControl != null)
                {
                    itemsControl.KeyDown -= new KeyEventHandler(this.OnColumnHeadersPresenterKeyDown);
                }
                if (this._itemsControl != null)
                {
                    this._itemsControl.KeyDown += new KeyEventHandler(this.OnColumnHeadersPresenterKeyDown);
                }
            }
        }

        private void OnColumnHeadersPresenterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && this._isHeaderDragging)
            {
                var draggingSrcHeader = this._draggingSrcHeader;

                this.FinishHeaderDrag(true);
                this.PrepareHeaderDrag(draggingSrcHeader, this._currentPos, this._relativeStartPos, true);

                base.InvalidateArrange();
            }
        }

        private static ItemsControl FindItemsControlThroughTemplatedParent(TreeGridHeaderRowPresenter presenter)
        {
            var frameworkElement = presenter.TemplatedParent as FrameworkElement;
            ItemsControl itemsControl = null;
            while (frameworkElement != null)
            {
                itemsControl = (frameworkElement as ItemsControl);
                if (itemsControl != null)
                {
                    break;
                }
                frameworkElement = (frameworkElement.TemplatedParent as FrameworkElement);
            }
            return itemsControl;
        }

        #endregion

        #region 内部控件构造

        private TreeGridColumnHeader _paddingHeader;

        private TreeGridColumnHeader _floatingHeader;

        private Separator _indicator;

        /// <summary>
        /// 如果需要，创建初始的 UI 元素。
        /// </summary>
        private void UpdateVisualTree()
        {
            if (base.NeedUpdateVisualTree)
            {
                var internalChildren = base.UIChildren;

                this.RenewEvents();

                if (internalChildren.Count == 0)
                {
                    this.AddPaddingColumnHeader();
                    this.AddIndicator();
                    this.AddFloatingHeader(null);
                }
                else if (internalChildren.Count > 3)
                {
                    int num = internalChildren.Count - 3;
                    for (int i = 0; i < num; i++)
                    {
                        this.RemoveHeader(null, 1);
                    }
                }

                this.UpdatePaddingHeader();

                var columns = base.Columns;
                //列下的格子从第二个位置开始插入
                int headerIndex = 1;
                for (int j = columns.Count - 1; j >= 0; j--)
                {
                    var column = columns[j];
                    this.CreateAndInsertHeader(column, headerIndex++);
                }

                this.BuildHeaderLinks();
                base.NeedUpdateVisualTree = false;
                this._isColumnChangedOrCreated = true;
            }
        }

        private void RenewEvents()
        {
            this.RenewHeadersKeyDownEvents();

            var grid = this._itemsControl as TreeGrid;
            if (grid != null)
            {
                grid.HeaderRowPresenter = this;
            }
        }

        private void AddPaddingColumnHeader()
        {
            var header = new TreeGridColumnHeader();
            header.IsInternalGenerated = true;
            header.SetValue(TreeGridColumnHeader.RolePropertyKey, TreeGridColumnHeaderRole.Padding);
            header.Content = null;
            header.ContentTemplate = null;
            header.ContentTemplateSelector = null;
            header.MinWidth = 0.0;
            header.Padding = new Thickness(0.0);
            header.Width = double.NaN;
            header.HorizontalAlignment = HorizontalAlignment.Stretch;

            base.UIChildren.Add(header);

            this._paddingHeader = header;
        }

        private void AddIndicator()
        {
            var separator = new Separator();
            separator.Visibility = Visibility.Hidden;
            separator.Margin = new Thickness(0.0);
            separator.Width = 2.0;

            var frameworkElementFactory = new FrameworkElementFactory(typeof(Border));
            frameworkElementFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Colors.Black));//Color.FromUInt32(4278190208u)

            var controlTemplate = new ControlTemplate(typeof(Separator));
            controlTemplate.VisualTree = frameworkElementFactory;
            controlTemplate.Seal();

            separator.Template = controlTemplate;

            base.UIChildren.Add(separator);

            this._indicator = separator;
        }

        private void AddFloatingHeader(TreeGridColumnHeader srcHeader)
        {
            Type type = srcHeader != null ? srcHeader.GetType() : typeof(TreeGridColumnHeader);
            TreeGridColumnHeader header;
            try
            {
                header = Activator.CreateInstance(type) as TreeGridColumnHeader;
            }
            catch (MissingMethodException innerException)
            {
                throw new ArgumentException("ListView_MissingParameterlessConstructor", innerException);
            }
            header.IsInternalGenerated = true;
            header.SetValue(TreeGridColumnHeader.RolePropertyKey, TreeGridColumnHeaderRole.Floating);
            header.Visibility = Visibility.Hidden;

            base.UIChildren.Add(header);

            this._floatingHeader = header;
        }

        /// <summary>
        /// 更新 _paddingHeader 的几个样式属性
        /// </summary>
        private void UpdatePaddingHeader()
        {
            var header = this._paddingHeader;
            this.UpdateHeaderProperty(header, ColumnHeaderContainerStyleProperty);
            this.UpdateHeaderProperty(header, ColumnHeaderContextMenuProperty);
            this.UpdateHeaderProperty(header, ColumnHeaderToolTipProperty);
        }

        private void RemoveHeader(TreeGridColumnHeader header, int index)
        {
            this._gvHeadersValid = false;
            if (header != null)
            {
                base.UIChildren.Remove(header);
            }
            else
            {
                header = (TreeGridColumnHeader)base.UIChildren[index];
                base.UIChildren.RemoveAt(index);
            }
            this.UnhookItemsControlKeyboardEvent(header);
        }

        private void UnhookItemsControlKeyboardEvent(TreeGridColumnHeader header)
        {
            if (this._itemsControl != null)
            {
                this._itemsControl.KeyDown -= new KeyEventHandler(header.OnColumnHeaderKeyDown);
            }
        }

        private void BuildHeaderLinks()
        {
            TreeGridColumnHeader previousVisualHeader = null;
            for (int i = 0; i < base.Columns.Count; i++)
            {
                TreeGridColumnHeader gridViewColumnHeader = (TreeGridColumnHeader)base.UIChildren[this.GetVisualIndex(i)];
                gridViewColumnHeader.PreviousVisualHeader = previousVisualHeader;
                previousVisualHeader = gridViewColumnHeader;
            }
            if (this._paddingHeader != null)
            {
                this._paddingHeader.PreviousVisualHeader = previousVisualHeader;
            }
        }

        /// <summary>
        /// 为指定列生成 header 并添加到 InternalChildren 的索引 index 处。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private TreeGridColumnHeader CreateAndInsertHeader(TreeGridColumn column, int index)
        {
            object header = column.Header;
            var gridViewColumnHeader = header as TreeGridColumnHeader;
            if (header != null)
            {
                var dependencyObject = header as DependencyObject;
                if (dependencyObject != null)
                {
                    var visual = dependencyObject as Visual;
                    if (visual != null)
                    {
                        var visual2 = VisualTreeHelper.GetParent(visual) as Visual;
                        if (visual2 != null)
                        {
                            if (gridViewColumnHeader != null)
                            {
                                var gridViewHeaderRowPresenter = visual2 as TreeGridHeaderRowPresenter;
                                if (gridViewHeaderRowPresenter != null)
                                {
                                    MSInternal.RemoveNoVerify(gridViewHeaderRowPresenter.UIChildren, gridViewColumnHeader);
                                }
                            }
                            else
                            {
                                var gridViewColumnHeader2 = visual2 as TreeGridColumnHeader;
                                if (gridViewColumnHeader2 != null)
                                {
                                    gridViewColumnHeader2.ClearValue(ContentControl.ContentProperty);
                                }
                            }
                        }
                    }
                    var parent = LogicalTreeHelper.GetParent(dependencyObject);
                    if (parent != null)
                    {
                        MSInternal.RemoveLogicalChild(parent, header);
                    }
                }
            }

            if (gridViewColumnHeader == null)
            {
                gridViewColumnHeader = new TreeGridColumnHeader();
                gridViewColumnHeader.IsInternalGenerated = true;
            }

            gridViewColumnHeader.SetValue(TreeGridColumnHeader.ColumnPropertyKey, column);

            this.HookupItemsControlKeyboardEvent(gridViewColumnHeader);

            base.UIChildren.Insert(index, gridViewColumnHeader);

            this.UpdateHeader(gridViewColumnHeader);

            this._gvHeadersValid = false;

            return gridViewColumnHeader;
        }

        private void HookupItemsControlKeyboardEvent(TreeGridColumnHeader header)
        {
            if (this._itemsControl != null)
            {
                this._itemsControl.KeyDown += new KeyEventHandler(header.OnColumnHeaderKeyDown);
            }
        }

        #endregion

        #region 其它方法

        private int GetVisualIndex(int columnIndex)
        {
            return base.UIChildren.Count - 3 - columnIndex;
        }

        private TreeGridColumnHeader FindHeaderByColumn(TreeGridColumn column)
        {
            var columns = base.Columns;
            UIElementCollection internalChildren = base.UIChildren;
            if (internalChildren.Count > columns.Count)
            {
                int num = columns.IndexOf(column);
                if (num != -1)
                {
                    int visualIndex = this.GetVisualIndex(num);
                    TreeGridColumnHeader gridViewColumnHeader = internalChildren[visualIndex] as TreeGridColumnHeader;
                    if (gridViewColumnHeader.Column == column)
                    {
                        return gridViewColumnHeader;
                    }
                    for (int i = 1; i < internalChildren.Count; i++)
                    {
                        gridViewColumnHeader = (internalChildren[i] as TreeGridColumnHeader);
                        if (gridViewColumnHeader != null && gridViewColumnHeader.Column == column)
                        {
                            return gridViewColumnHeader;
                        }
                    }
                }
            }
            return null;
        }

        private static bool IsMousePositionValid(FrameworkElement floatingHeader, Point currentPos, double arrange)
        {
            return DoubleUtil.LessThanOrClose(-floatingHeader.Height * arrange, currentPos.Y) && DoubleUtil.LessThanOrClose(currentPos.Y, floatingHeader.Height * (arrange + 1.0));
        }

        private static DependencyProperty GetColumnDPFromName(string dpName)
        {
            DependencyProperty[] array = s_DPList[1];
            for (int i = 0; i < array.Length; i++)
            {
                var dependencyProperty = array[i];
                if (dependencyProperty != null && dpName.Equals(dependencyProperty.Name))
                {
                    return dependencyProperty;
                }
            }
            return null;
        }

        #endregion
    }
}
