/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130402 13:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130402 13:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DesignerEngine;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 此控件从 DependencyObject 继承，不会显示在可视树中，只承载必要的数据。
    /// 最终使用 Connection 控件进行显示。
    /// </summary>
    public class BlockRelation : FrameworkElement, IModelingDesignerComponent, IConnectionKey
    {
        #region FromBlock DependencyProperty

        public static readonly DependencyProperty FromBlockProperty = DependencyProperty.Register(
            "FromBlock", typeof(string), typeof(BlockRelation),
            new PropertyMetadata((d, e) => (d as BlockRelation).OnFromBlockChanged(e))
            );

        /// <summary>
        /// 起始元素主键
        /// </summary>
        public string FromBlock
        {
            get { return (string)this.GetValue(FromBlockProperty); }
            set { this.SetValue(FromBlockProperty, value); }
        }

        private void OnFromBlockChanged(DependencyPropertyChangedEventArgs e)
        {
            _from = null;
            ResetSource();
        }

        #endregion

        #region ToBlock DependencyProperty

        public static readonly DependencyProperty ToBlockProperty = DependencyProperty.Register(
            "Block", typeof(string), typeof(BlockRelation),
            new PropertyMetadata((d, e) => (d as BlockRelation).OnToBlockChanged(e))
            );

        /// <summary>
        /// 终止元素主键
        /// </summary>
        public string ToBlock
        {
            get { return (string)this.GetValue(ToBlockProperty); }
            set { this.SetValue(ToBlockProperty, value); }
        }

        private void OnToBlockChanged(DependencyPropertyChangedEventArgs e)
        {
            _to = null;
            ResetSink();
        }

        #endregion

        #region Label DependencyProperty

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(BlockRelation)
            );

        /// <summary>
        /// 连线显示的 Label
        /// </summary>
        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        #endregion

        #region LabelVisibility DependencyProperty

        public static readonly DependencyProperty LabelVisibilityProperty = DependencyProperty.Register(
            "LabelVisibility", typeof(Visibility), typeof(BlockRelation)
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

        #region FromPointPos DependencyProperty

        public static readonly DependencyProperty FromPointPosProperty = DependencyProperty.Register(
            "FromPointPos", typeof(Point?), typeof(BlockRelation),
            new PropertyMetadata((d, e) => (d as BlockRelation).OnFromPointPosChanged(e))
            );

        /// <summary>
        /// 连接线连接到起始元素的点的相对位置。
        /// </summary>
        public Point? FromPointPos
        {
            get { return (Point?)this.GetValue(FromPointPosProperty); }
            set { this.SetValue(FromPointPosProperty, value); }
        }

        private void OnFromPointPosChanged(DependencyPropertyChangedEventArgs e)
        {
            ResetFromPointPos();
        }

        #endregion

        #region ToPointPos DependencyProperty

        public static readonly DependencyProperty ToPointPosProperty = DependencyProperty.Register(
            "ToPointPos", typeof(Point?), typeof(BlockRelation),
            new PropertyMetadata((d, e) => (d as BlockRelation).OnToPointPosChanged(e))
            );

        /// <summary>
        /// 连接线连接到终止元素的点的相对位置。
        /// </summary>
        public Point? ToPointPos
        {
            get { return (Point?)this.GetValue(ToPointPosProperty); }
            set { this.SetValue(ToPointPosProperty, value); }
        }

        private void OnToPointPosChanged(DependencyPropertyChangedEventArgs e)
        {
            ResetToPointPos();
        }

        #endregion

        #region ConnectionType DependencyProperty

        public static readonly DependencyProperty ConnectionTypeProperty = DependencyProperty.Register(
            "ConnectionType", typeof(ConnectionType), typeof(BlockRelation),
            new PropertyMetadata((d, e) => (d as BlockRelation).OnConnectionTypeChanged(e))
            );

        /// <summary>
        /// 连接的类型。
        /// </summary>
        public ConnectionType ConnectionType
        {
            get { return (ConnectionType)this.GetValue(ConnectionTypeProperty); }
            set { this.SetValue(ConnectionTypeProperty, value); }
        }

        private void OnConnectionTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this._connection != null)
            {
                this.ResetConnectionType();
            }
        }

        #endregion

        #region Hidden DependencyProperty

        public static readonly DependencyProperty HiddenProperty = DependencyProperty.Register(
            "Hidden", typeof(bool), typeof(BlockRelation),
            new PropertyMetadata((d, e) => (d as BlockRelation).OnHiddenChanged(e))
            );

        /// <summary>
        /// 是否该连接已经设置为隐藏
        /// </summary>
        public bool Hidden
        {
            get { return (bool)this.GetValue(HiddenProperty); }
            set { this.SetValue(HiddenProperty, value); }
        }

        private void OnHiddenChanged(DependencyPropertyChangedEventArgs e)
        {
            ResetVisibility();
        }

        internal void ResetVisibility()
        {
            if (_designer != null)
            {
                this.Visibility = _designer.ShowHiddenRelations || !this.Hidden ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        private ModelingDesigner _designer;
        internal ModelingDesigner Designer
        {
            get { return _designer; }
            set
            {
                if (_designer != value)
                {
                    _designer = value;

                    if (value != null)
                    {
                        ResetVisibility();
                    }
                }
            }
        }

        #region 创建引擎控件，并进行双向绑定。

        /// <summary>
        /// 用于显示的控件。
        /// </summary>
        private Connection _connection;

        private BlockControl _from, _to;

        private BlockControl From
        {
            get
            {
                if (_from == null && _designer != null)
                {
                    var fromId = this.FromBlock;
                    if (!string.IsNullOrEmpty(fromId))
                    {
                        _from = _designer.Blocks.Find(fromId) as BlockControl;
                    }
                }

                return _from;
            }
        }

        private BlockControl To
        {
            get
            {
                if (_to == null && _designer != null)
                {
                    var toId = this.ToBlock;
                    if (!string.IsNullOrEmpty(toId))
                    {
                        _to = _designer.Blocks.Find(toId) as BlockControl;
                    }
                }

                return _to;
            }
        }

        /// <summary>
        /// 创建一个连接线对象。
        /// </summary>
        /// <returns></returns>
        internal Connection CreateConnection()
        {
            _connection = new Connection();

            _connection.DataContext = this;
            _connection.SetBinding(Connection.LabelProperty, new Binding() { Path = new PropertyPath(LabelProperty), Mode = BindingMode.TwoWay });
            _connection.SetBinding(Connection.LabelVisibilityProperty, new Binding() { Path = new PropertyPath(LabelVisibilityProperty), Mode = BindingMode.TwoWay });
            _connection.SetBinding(Connection.VisibilityProperty, new Binding() { Path = new PropertyPath(VisibilityProperty), Mode = BindingMode.TwoWay });

            BindBlocks();
            BindRelativePos();
            ResetConnectionType();

            return _connection;
        }

        /// <summary>
        /// 绑定 From、To
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void BindBlocks()
        {
            ResetSource();
            ResetSink();
            _connection.SourceChanged += (o, e) =>
            {
                var sourceBlock = BlockControl.GetBlockControl(_connection.Source);
                this.FromBlock = sourceBlock.TypeFullName;
            };
            _connection.SinkChanged += (o, e) =>
            {
                var sinkBlock = BlockControl.GetBlockControl(_connection.Sink);
                this.ToBlock = sinkBlock.TypeFullName;
            };
        }

        private void ResetSource()
        {
            if (this.From != null && _connection != null)
            {
                _connection.Source = this.From.Container;
            }
        }

        private void ResetSink()
        {
            if (this.To != null && _connection != null)
            {
                _connection.Sink = this.To.Container;
            }
        }

        /// <summary>
        /// 绑定 FromPointPos、ToPointPos
        /// </summary>
        private void BindRelativePos()
        {
            ResetFromPointPos();
            ResetToPointPos();
            _connection.SourceRelativePosChanged += (o, e) =>
            {
                var autoPoints = _connection.AutoPositionPoints;
                if ((autoPoints & AutoPositionPoints.Source) == 0)
                {
                    this.FromPointPos = _connection.SourceRelativePos;
                }
                else
                {
                    this.FromPointPos = null;
                }
            };
            _connection.SinkRelativePosChanged += (o, e) =>
            {
                var autoPoints = _connection.AutoPositionPoints;
                if ((autoPoints & AutoPositionPoints.Sink) == 0)
                {
                    this.ToPointPos = _connection.SinkRelativePos;
                }
                else
                {
                    this.ToPointPos = null;
                }
            };
        }

        private void ResetToPointPos()
        {
            if (this.ToPointPos.HasValue && _connection != null)
            {
                _connection.SinkRelativePos = this.ToPointPos.Value;
            }
        }

        private void ResetFromPointPos()
        {
            if (this.FromPointPos.HasValue && _connection != null)
            {
                _connection.SourceRelativePos = this.FromPointPos.Value;
            }
        }

        private void ResetConnectionType()
        {
            if (_connection != null)
            {
                var visual = this._connection;

                switch (this.ConnectionType)
                {
                    case ConnectionType.Reference:
                        visual.SinkAnchorStyle = _designer.FindResource("AnchorStyle_FillArrow") as Style;
                        visual.SinkAnchorScale = 0.5;
                        visual.StrokeDashArray = null;
                        break;
                    case ConnectionType.NullableReference:
                        visual.SinkAnchorStyle = _designer.FindResource("AnchorStyle_FillArrow") as Style;
                        visual.SinkAnchorScale = 0.5;
                        visual.StrokeDashArray = new DoubleCollection(new double[] { 1, 1, 4, 1 });
                        break;
                    case ConnectionType.Composition:
                        visual.SinkAnchorStyle = _designer.FindResource("AnchorStyle_FillDiamond") as Style;
                        visual.SinkAnchorScale = 1;
                        visual.StrokeDashArray = null;
                        break;
                    case ConnectionType.Aggregation:
                        visual.SinkAnchorStyle = _designer.FindResource("AnchorStyle_EmptyDiamond") as Style;
                        visual.SinkAnchorScale = 1;
                        visual.StrokeDashArray = null;
                        break;
                    case ConnectionType.Inheritance:
                        visual.SinkAnchorStyle = _designer.FindResource("AnchorStyle_EmptyArrow") as Style;
                        visual.SinkAnchorScale = 1;
                        visual.StrokeDashArray = null;
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion

        /// <summary>
        /// 用于底层显示的图形控件。
        /// </summary>
        public Control EngineControl
        {
            get { return _connection; }
        }

        DesignerComponentKind IModelingDesignerComponent.Kind
        {
            get { return DesignerComponentKind.Relation; }
        }

        string IConnectionKey.From
        {
            get { return this.FromBlock; }
        }

        string IConnectionKey.To
        {
            get { return this.ToBlock; }
        }
    }
}