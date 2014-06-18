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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignerEngine
{
    /// <summary>
    /// 连续线对象。
    /// </summary>
    public class Connection : Control, IDesignerCanvasItem, IDesignerCanvasItemInternal
    {
        static Connection()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Connection), new FrameworkPropertyMetadata(typeof(Connection)));
        }

        /// <summary>
        /// 所在的画布
        /// </summary>
        public DesignerCanvas OwnerCanvas
        {
            get { return DesignerCanvas.GetOwnerCanvas(this); }
        }

        #region 两个端点，及事件监听

        public Connection()
        {
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        #region Source DependencyProperty

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(DesignerItemContainer), typeof(Connection),
            new PropertyMetadata((d, e) => (d as Connection).OnSourceChanged(e))
            );

        public DesignerItemContainer Source
        {
            get { return (DesignerItemContainer)this.GetValue(SourceProperty); }
            set { this.SetValue(SourceProperty, value); }
        }

        private void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            var old = (DesignerItemContainer)e.OldValue;
            var value = (DesignerItemContainer)e.NewValue;

            if (old != null)
            {
                old.PositionChanged -= OnConnectorPositionChanged;
                old.Connections.Remove(this);
            }

            if (value != null)
            {
                value.Connections.Add(this);
                if (this.IsLoaded)
                {
                    value.PositionChanged += OnConnectorPositionChanged;
                }
            }

            OnItemChanged();

            this.OnSourceChanged();
        }

        public event EventHandler SourceChanged;

        protected virtual void OnSourceChanged()
        {
            var handler = this.SourceChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Sink DependencyProperty

        public static readonly DependencyProperty SinkProperty = DependencyProperty.Register(
            "Sink", typeof(DesignerItemContainer), typeof(Connection),
            new PropertyMetadata((d, e) => (d as Connection).OnSinkChanged(e))
            );

        public DesignerItemContainer Sink
        {
            get { return (DesignerItemContainer)this.GetValue(SinkProperty); }
            set { this.SetValue(SinkProperty, value); }
        }

        private void OnSinkChanged(DependencyPropertyChangedEventArgs e)
        {
            var old = (DesignerItemContainer)e.OldValue;
            var value = (DesignerItemContainer)e.NewValue;

            if (old != null)
            {
                old.PositionChanged -= OnConnectorPositionChanged;
                old.Connections.Remove(this);
            }

            if (value != null)
            {
                value.Connections.Add(this);
                if (this.IsLoaded)
                {
                    value.PositionChanged += OnConnectorPositionChanged;
                }
            }

            OnItemChanged();

            this.OnSinkChanged();
        }

        public event EventHandler SinkChanged;

        protected virtual void OnSinkChanged()
        {
            var handler = this.SinkChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void OnItemChanged()
        {
            _autoPositionPoints = AutoPositionPoints.All;
            this.UpdatePathGeometry();
        }

        #endregion

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var source = this.Source;
            if (source != null) { source.PositionChanged += this.OnConnectorPositionChanged; }
            var sink = this.Sink;
            if (sink != null) { sink.PositionChanged += this.OnConnectorPositionChanged; }

            this.UpdatePathGeometry();
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // remove event handler
            var source = this.Source;
            if (source != null) source.PositionChanged -= OnConnectorPositionChanged;

            var sink = this.Sink;
            if (sink != null) sink.PositionChanged -= OnConnectorPositionChanged;

            RemoveAdorner();
        }

        #endregion

        #region Label DependencyProperty

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(Connection)
            );

        /// <summary>
        /// 连接线上显示的标签
        /// </summary>
        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        #endregion

        #region LabelPosition DependencyProperty

        public static readonly DependencyProperty LabelPositionProperty = DependencyProperty.Register(
            "LabelPosition", typeof(Point), typeof(Connection)
            );

        /// <summary>
        /// Label 所在的位置
        /// </summary>
        public Point LabelPosition
        {
            get { return (Point)this.GetValue(LabelPositionProperty); }
            set { this.SetValue(LabelPositionProperty, value); }
        }

        #endregion

        #region LabelVisibility DependencyProperty

        public static readonly DependencyProperty LabelVisibilityProperty = DependencyProperty.Register(
            "LabelVisibility", typeof(Visibility), typeof(Connection)
            );

        /// <summary>
        /// Label 的可见性
        /// </summary>
        public Visibility LabelVisibility
        {
            get { return (Visibility)this.GetValue(LabelVisibilityProperty); }
            set { this.SetValue(LabelVisibilityProperty, value); }
        }

        #endregion

        #region StrokeDashArray DependencyProperty

        public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
            "StrokeDashArray", typeof(DoubleCollection), typeof(Connection)
            );

        /// <summary>
        /// 连接线的虚线点集合。
        /// </summary>
        public DoubleCollection StrokeDashArray
        {
            get { return (DoubleCollection)this.GetValue(StrokeDashArrayProperty); }
            set { this.SetValue(StrokeDashArrayProperty, value); }
        }

        #endregion

        #region Stroke DependencyProperty

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(Connection)
            );

        /// <summary>
        /// 连接线的连线颜色。
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)this.GetValue(StrokeProperty); }
            set { this.SetValue(StrokeProperty, value); }
        }

        #endregion

        #region 选择

        #region IsSelected DependencyProperty

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsSelected", typeof(bool), typeof(Connection),
            new PropertyMetadata(false, (d, e) => (d as Connection).OnIsSelectedChanged(e))
            );

        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        /// <summary>
        /// 如果被选中，则显示 ConnectionAdorner。
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            internal set { this.SetValue(IsSelectedPropertyKey, value); }
        }

        private void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            if (value)
            {
                ShowAdorner();
            }
            else
            {
                HideAdorner();
            }
        }

        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            var canvas = OwnerCanvas;
            if (canvas != null) { canvas.HandleMouseSelection(this); }

            e.Handled = false;
        }

        #endregion

        #region 路径绘制

        /// <summary>
        /// 起始点、终止点的相对位置。
        /// 
        /// 把 DesignerItem 看成一个 (0,0) 到 (1,1) 的矩形，这个属性描述连接点在这个矩形的位置。
        /// 这个位置应该只在四条边上。
        /// </summary>
        private Point _sourceRelativePos, _sinkRelativePos;

        /// <summary>
        /// 指定当前需要自动计算位置的端点。
        /// </summary>
        private AutoPositionPoints _autoPositionPoints = AutoPositionPoints.All;

        internal AutoPositionPoints AutoPositionPoints
        {
            get { return _autoPositionPoints; }
        }

        private bool IsConnected
        {
            get { return Source != null && Sink != null; }
        }

        /// <summary>
        /// 起始点在起始元素的相对位置
        /// </summary>
        public Point SourceRelativePos
        {
            get { return _sourceRelativePos; }
            set
            {
                if (_sourceRelativePos != value)
                {
                    _sourceRelativePos = value;
                    _autoPositionPoints &= ~AutoPositionPoints.Source;
                    this.OnSourceRelativePosChanged();

                    this.UpdatePathGeometry();
                }
            }
        }

        /// <summary>
        /// 起始点相对位置变更事件。
        /// </summary>
        public event EventHandler SourceRelativePosChanged;

        protected virtual void OnSourceRelativePosChanged()
        {
            var handler = this.SourceRelativePosChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// 终止点在终止元素的相对位置
        /// </summary>
        public Point SinkRelativePos
        {
            get { return _sinkRelativePos; }
            set
            {
                if (_sinkRelativePos != value)
                {
                    _sinkRelativePos = value;
                    _autoPositionPoints &= ~AutoPositionPoints.Sink;
                    this.OnSinkRelativePosChanged();

                    this.UpdatePathGeometry();
                }
            }
        }

        /// <summary>
        /// 终止点相对位置变更事件。
        /// </summary>
        public event EventHandler SinkRelativePosChanged;

        protected virtual void OnSinkRelativePosChanged()
        {
            var handler = this.SinkRelativePosChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #region PathGeometry DependencyProperty

        private static readonly DependencyPropertyKey PathGeometryPropertyKey = DependencyProperty.RegisterReadOnly(
            "PathGeometry", typeof(PathGeometry), typeof(Connection),
            new PropertyMetadata((d, e) => (d as Connection).OnPathGeometryChanged(e))
            );

        public static readonly DependencyProperty PathGeometryProperty = PathGeometryPropertyKey.DependencyProperty;

        public PathGeometry PathGeometry
        {
            get { return (PathGeometry)this.GetValue(PathGeometryProperty); }
            set { this.SetValue(PathGeometryPropertyKey, value); }
        }

        private void OnPathGeometryChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (PathGeometry)e.NewValue;
            if (value != null)
            {
                UpdateAnchorPosition();
            }
        }

        #endregion

        internal ConnectorInfo SourceConnectorInfo
        {
            get { return GetConnectorInfo(this.Source, _sourceRelativePos); }
        }

        internal ConnectorInfo SinkConnectorInfo
        {
            get { return GetConnectorInfo(this.Sink, _sinkRelativePos); }
        }

        internal void AddResetRelative(AutoPositionPoints autoResetPoints)
        {
            _autoPositionPoints |= autoResetPoints;
            this.UpdatePathGeometry();
        }

        void OnConnectorPositionChanged(object sender, EventArgs e)
        {
            this.UpdatePathGeometry();
        }

        /// <summary>
        /// 更新路径，并指定需要自动计算端点位置的点。
        /// </summary>
        /// <param name="autoResetPoints"></param>
        private void UpdatePathGeometry()
        {
            if (this.IsLoaded && this.IsConnected)
            {
                //始终自动计算上次设置的自动计算点。
                this.AutoResetRelativePos();

                this.PathGeometry = PathHelper.DarwGeometry(this.SourceConnectorInfo.Position, this.SinkConnectorInfo.Position);
                //this.PathGeometry = PathHelper.DarwGeometry(this.SourceConnectorInfo, this.SinkConnectorInfo);
            }
        }

        /// <summary>
        /// 自动计算指定端点的相对位置。
        /// </summary>
        private void AutoResetRelativePos()
        {
            if (_autoPositionPoints == AutoPositionPoints.None) return;

            var from = Source.GetRect();
            var to = Sink.GetRect();

            if ((_autoPositionPoints & AutoPositionPoints.Source) != 0)
            {
                var toCenter = to.GetCenterPoint();
                if (!from.Contains(toCenter))
                {
                    var point = RectAlgorithm.CalcCrossPoint(from, toCenter);
                    this._sourceRelativePos = RectAlgorithm.GetRectRelative(from, point);
                }
            }

            if ((_autoPositionPoints & AutoPositionPoints.Sink) != 0)
            {
                var fromCenter = from.GetCenterPoint();
                if (!to.Contains(fromCenter))
                {
                    var point = RectAlgorithm.CalcCrossPoint(to, fromCenter);
                    this._sinkRelativePos = RectAlgorithm.GetRectRelative(to, point);
                }
            }
        }

        private static ConnectorInfo GetConnectorInfo(DesignerItemContainer item, Point relative)
        {
            var info = new ConnectorInfo();
            info.DesignerItemRect = item.GetRect();
            info.Position = RectAlgorithm.GetRectAbsolute(info.DesignerItemRect, relative);
            return info;
        }

        #endregion

        #region Anchor

        /// <summary>
        /// 连接线的中点
        /// </summary>
        internal Point CenterPoint { get; private set; }

        #region SourceAnchorPosition DependencyProperty

        public static readonly DependencyProperty SourceAnchorPositionProperty = DependencyProperty.Register(
            "SourceAnchorPosition", typeof(Point), typeof(Connection)
            );

        /// <summary>
        /// 起始位置图形的位置
        /// </summary>
        public Point SourceAnchorPosition
        {
            get { return (Point)this.GetValue(SourceAnchorPositionProperty); }
            set { this.SetValue(SourceAnchorPositionProperty, value); }
        }

        #endregion

        #region SourceAnchorAngle DependencyProperty

        public static readonly DependencyProperty SourceAnchorAngleProperty = DependencyProperty.Register(
            "SourceAnchorAngle", typeof(double), typeof(Connection)
            );

        /// <summary>
        /// 起始位置图形的角度
        /// </summary>
        public double SourceAnchorAngle
        {
            get { return (double)this.GetValue(SourceAnchorAngleProperty); }
            set { this.SetValue(SourceAnchorAngleProperty, value); }
        }

        #endregion

        #region SourceAnchorStyle DependencyProperty

        public static readonly DependencyProperty SourceAnchorStyleProperty = DependencyProperty.Register(
            "SourceAnchorStyle", typeof(Style), typeof(Connection)
            );

        /// <summary>
        /// 起始位置图形的样式
        /// </summary>
        public Style SourceAnchorStyle
        {
            get { return (Style)this.GetValue(SourceAnchorStyleProperty); }
            set { this.SetValue(SourceAnchorStyleProperty, value); }
        }

        #endregion

        #region SinkAnchorPosition DependencyProperty

        public static readonly DependencyProperty SinkAnchorPositionProperty = DependencyProperty.Register(
            "SinkAnchorPosition", typeof(Point), typeof(Connection)
            );

        /// <summary>
        /// 终止位置图形的位置
        /// </summary>
        public Point SinkAnchorPosition
        {
            get { return (Point)this.GetValue(SinkAnchorPositionProperty); }
            set { this.SetValue(SinkAnchorPositionProperty, value); }
        }

        #endregion

        #region SinkAnchorAngle DependencyProperty

        public static readonly DependencyProperty SinkAnchorAngleProperty = DependencyProperty.Register(
            "SinkAnchorAngle", typeof(double), typeof(Connection)
            );

        /// <summary>
        /// 终止位置图形的角度
        /// </summary>
        public double SinkAnchorAngle
        {
            get { return (double)this.GetValue(SinkAnchorAngleProperty); }
            set { this.SetValue(SinkAnchorAngleProperty, value); }
        }

        #endregion

        #region SinkAnchorScale DependencyProperty

        public static readonly DependencyProperty SinkAnchorScaleProperty = DependencyProperty.Register(
            "SinkAnchorScale", typeof(double), typeof(Connection),
            new PropertyMetadata(1d)
            );

        /// <summary>
        /// 目标箭头的缩放大小。
        /// </summary>
        public double SinkAnchorScale
        {
            get { return (double)this.GetValue(SinkAnchorScaleProperty); }
            set { this.SetValue(SinkAnchorScaleProperty, value); }
        }

        #endregion

        #region SinkAnchorStyle DependencyProperty

        public static readonly DependencyProperty SinkAnchorStyleProperty = DependencyProperty.Register(
            "SinkAnchorStyle", typeof(Style), typeof(Connection)
            );

        /// <summary>
        /// 目标位置箭头的样式。
        /// </summary>
        public Style SinkAnchorStyle
        {
            get { return (Style)this.GetValue(SinkAnchorStyleProperty); }
            set { this.SetValue(SinkAnchorStyleProperty, value); }
        }

        #endregion

        private void UpdateAnchorPosition()
        {
            Point point, tangent;

            var path = this.PathGeometry;

            path.GetPointAtFractionLength(0, out point, out tangent);
            this.SourceAnchorAngle = ModelingHelper.ToAngle(-tangent.Y, -tangent.X);
            this.SourceAnchorPosition = point;

            path.GetPointAtFractionLength(1, out point, out tangent);
            this.SinkAnchorAngle = ModelingHelper.ToAngle(tangent.Y, tangent.X);
            this.SinkAnchorPosition = point;

            path.GetPointAtFractionLength(0.5, out point, out tangent);
            this.LabelPosition = this.CenterPoint = point;
        }

        #endregion

        #region ConnectionAdorner

        /// <summary>
        /// 本连接线正在被拖拽时，显示的指示连接线。
        /// </summary>
        private ConnectionAdorner _connectionAdorner;

        private void ShowAdorner()
        {
            // the ConnectionAdorner is created once for each Connection
            if (_connectionAdorner == null)
            {
                var designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

                var adornerLayer = AdornerLayer.GetAdornerLayer(designer);
                if (adornerLayer != null)
                {
                    _connectionAdorner = new ConnectionAdorner(designer, this);
                    adornerLayer.Add(_connectionAdorner);
                }
            }
            _connectionAdorner.Visibility = Visibility.Visible;
        }

        private void HideAdorner()
        {
            if (_connectionAdorner != null)
            {
                _connectionAdorner.Visibility = Visibility.Collapsed;
            }
        }

        private void RemoveAdorner()
        {
            if (_connectionAdorner != null)
            {
                var canvas = OwnerCanvas;
                if (canvas != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        adornerLayer.Remove(_connectionAdorner);
                        _connectionAdorner = null;
                    }
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            //任何的依赖属性变更时，就通知 Adorner 位置已经变更。
            if (_connectionAdorner != null)
            {
                if (e.Property == Connection.SourceAnchorPositionProperty ||
                    e.Property == Connection.SinkAnchorPositionProperty)
                {
                    _connectionAdorner.ConnectionPositionChanged(e);
                }
            }
        }

        #endregion

        #region IDesignerCanvasItemInternal

        bool IDesignerCanvasItemInternal.IsSelected
        {
            get { return IsSelected; }
            set { IsSelected = value; }
        }

        #endregion
    }

    /// <summary>
    /// 需要自动计算连接位置的端点
    /// </summary>
    internal enum AutoPositionPoints
    {
        None = 0,
        Source = 1,
        Sink = 2,
        All = 3,
    }
}