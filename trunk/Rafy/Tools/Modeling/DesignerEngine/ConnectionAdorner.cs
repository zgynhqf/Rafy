/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：20130330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130330
 * 
*******************************************************/

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignerEngine
{
    /// <summary>
    /// 连接上的两个点。
    /// </summary>
    public class ConnectionAdorner : Adorner
    {
        private DesignerCanvas _designerCanvas;
        private Connection _connection;
        /// <summary>
        /// 拖拽时，临时线的起始元素。
        /// </summary>
        private DesignerItemContainer _startItem;
        /// <summary>
        /// 拖拽时，临时线的终止元素。
        /// </summary>
        private DesignerItemContainer _draggingItem;
        /// <summary>
        /// 是否当前的操作需要变更目标元素。
        /// 
        /// 表示当前对象支持两种操作：
        /// * false: 让连接在元素上小幅度移动。
        /// * true: 变更连接对象。
        /// </summary>
        private bool _changeTarget = false;

        #region 控件构造

        private Canvas _myCanvas;
        private Thumb _sourceDragThumb, _sinkDragThumb;

        public ConnectionAdorner(DesignerCanvas designer, Connection connection)
            : base(designer)
        {
            this._designerCanvas = designer;
            this._myCanvas = new Canvas();
            this._visualChildren = new VisualCollection(this);
            this._visualChildren.Add(_myCanvas);

            this._connection = connection;

            InitializeDragThumbs();
        }

        private void InitializeDragThumbs()
        {
            Style dragThumbStyle = _connection.FindResource("ConnectionAdornerThumbStyle") as Style;

            //source drag thumb
            _sourceDragThumb = new Thumb();
            if (dragThumbStyle != null) _sourceDragThumb.Style = dragThumbStyle;
            Canvas.SetLeft(_sourceDragThumb, _connection.SourceAnchorPosition.X);
            Canvas.SetTop(_sourceDragThumb, _connection.SourceAnchorPosition.Y);
            _sourceDragThumb.DragStarted += thumbDragThumb_DragStarted;
            _sourceDragThumb.DragDelta += thumbDragThumb_DragDelta;
            _sourceDragThumb.DragCompleted += thumbDragThumb_DragCompleted;
            _sourceDragThumb.MouseDoubleClick += (o, e) => _connection.AddResetRelative(AutoPositionPoints.Source);
            this._myCanvas.Children.Add(_sourceDragThumb);

            // sink drag thumb
            _sinkDragThumb = new Thumb();
            if (dragThumbStyle != null) _sinkDragThumb.Style = dragThumbStyle;
            Canvas.SetLeft(_sinkDragThumb, _connection.SinkAnchorPosition.X);
            Canvas.SetTop(_sinkDragThumb, _connection.SinkAnchorPosition.Y);
            _sinkDragThumb.DragStarted += thumbDragThumb_DragStarted;
            _sinkDragThumb.DragDelta += thumbDragThumb_DragDelta;
            _sinkDragThumb.DragCompleted += thumbDragThumb_DragCompleted;
            _sinkDragThumb.MouseDoubleClick += (o, e) => _connection.AddResetRelative(AutoPositionPoints.Sink);
            this._myCanvas.Children.Add(_sinkDragThumb);
        }

        /// <summary>
        /// Connection 会调用此方法来通知它的位置变更
        /// </summary>
        /// <param name="e"></param>
        internal void ConnectionPositionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == Connection.SourceAnchorPositionProperty)
            {
                Canvas.SetLeft(_sourceDragThumb, _connection.SourceAnchorPosition.X);
                Canvas.SetTop(_sourceDragThumb, _connection.SourceAnchorPosition.Y);
            }

            if (e.Property == Connection.SinkAnchorPositionProperty)
            {
                Canvas.SetLeft(_sinkDragThumb, _connection.SinkAnchorPosition.X);
                Canvas.SetTop(_sinkDragThumb, _connection.SinkAnchorPosition.Y);
            }
        }

        #endregion

        void thumbDragThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.Cursor = Cursors.Cross;
            this.HitDesignerItem = null;
            this._pathGeometry = null;

            if (sender == _sourceDragThumb)
            {
                _startItem = _connection.Sink;
                _draggingItem = _connection.Source;
            }
            else if (sender == _sinkDragThumb)
            {
                _startItem = _connection.Source;
                _draggingItem = _connection.Sink;
            }
        }

        void thumbDragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Point mouse = Mouse.GetPosition(this);

            if (!_changeTarget)
            {
                //拖拽的量比较大，则进入变更目标模式
                if (_designerCanvas.CanChangeConnection && LeftDraggingItem(mouse))
                {
                    _changeTarget = true;
                    _rawStrokeDashArray = _connection.StrokeDashArray;
                    this._connection.StrokeDashArray = ChangeTargetLineStrokeDashArray;
                }
            }

            if (!_changeTarget)
            {
                //小幅度在元素的边上移动
                var rect = _draggingItem.GetRect();
                var point = RectAlgorithm.NearestPointOnLine(rect, mouse);
                var relative = RectAlgorithm.GetRectRelative(rect, point);

                if (_connection.Source == _draggingItem)
                {
                    _connection.SourceRelativePos = relative;
                }
                else
                {
                    _connection.SinkRelativePos = relative;
                }
            }
            else
            {
                this.HitTesting(mouse);

                //变换目标时，需要绘制一根临时的连接连
                var start = _connection.Source == _startItem ? _connection.SourceConnectorInfo : _connection.SinkConnectorInfo;
                this._pathGeometry = PathHelper.DarwGeometry(start, mouse);
            }

            this.InvalidateVisual();
        }

        void thumbDragThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_changeTarget)
            {
                var hitted = this.HitDesignerItem;
                if (hitted != null)
                {
                    if (_connection.Sink == _draggingItem)
                    {
                        _connection.Sink = hitted;
                    }
                    else
                    {
                        _connection.Source = hitted;
                    }

                    this.HitDesignerItem = null;
                }

                _connection.StrokeDashArray = _rawStrokeDashArray;

                _changeTarget = false;
            }

            this._pathGeometry = null;

            this.InvalidateVisual();
        }

        #region ChangeTarget

        private static readonly DoubleCollection ChangeTargetLineStrokeDashArray = new DoubleCollection(new double[] { 1, 2 });

        private const double ChangeTargetDistance = 50;

        private DesignerItemContainer _hitDesignerItem;

        private DoubleCollection _rawStrokeDashArray;

        private DesignerItemContainer HitDesignerItem
        {
            get { return _hitDesignerItem; }
            set
            {
                if (_hitDesignerItem != value)
                {
                    if (_hitDesignerItem != null)
                    {
                        _hitDesignerItem.IsDragConnectionOver = false;
                    }

                    _hitDesignerItem = value;

                    if (_hitDesignerItem != null)
                    {
                        _hitDesignerItem.IsDragConnectionOver = true;
                    }
                }
            }
        }

        private bool LeftDraggingItem(Point mouse)
        {
            Rect rect = _draggingItem.GetRect();
            rect.Inflate(ChangeTargetDistance, ChangeTargetDistance);
            return !rect.Contains(mouse);
        }

        private void HitTesting(Point hitPoint)
        {
            var hitObject = _designerCanvas.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null && hitObject != _startItem)
            {
                if (hitObject is DesignerCanvas) break;

                if (hitObject is DesignerItemContainer)
                {
                    HitDesignerItem = hitObject as DesignerItemContainer;
                    return;
                }

                hitObject = VisualTreeHelper.GetParent(hitObject);
            }

            HitDesignerItem = null;
        }

        #endregion

        #region VisualChildrenCount

        private VisualCollection _visualChildren;

        protected override int VisualChildrenCount
        {
            get { return this._visualChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this._visualChildren[index];
        }

        #endregion

        #region 渲染

        private static readonly Pen _drawingPen;

        static ConnectionAdorner()
        {
            _drawingPen = new Pen(Brushes.LightSlateGray, 1);
            _drawingPen.LineJoin = PenLineJoin.Round;
        }

        private PathGeometry _pathGeometry;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.DrawGeometry(null, _drawingPen, this._pathGeometry);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _myCanvas.Arrange(new Rect(0, 0, this._designerCanvas.ActualWidth, this._designerCanvas.ActualHeight));

            return finalSize;
        }

        #endregion
    }
}