/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 10:47
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 10:47
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignerEngine
{
    /// <summary>
    /// 这个 Adorner 用于绘制从某个 Connector 出发的一条线。
    /// </summary>
    public class ConnectorAdorner : Adorner
    {
        //private static readonly DoubleCollection ChangeTargetLineStrokeDashArray = new DoubleCollection(new double[] { 1, 2 });

        private DesignerCanvas _canvas;
        /// <summary>
        /// 从这个连接点出发。
        /// </summary>
        private Connector _connector;
        private Pen _drawingPen;
        private PathGeometry _pathGeometry;
        private DesignerItemContainer _hitDesignerItem;

        public ConnectorAdorner(DesignerCanvas designer, Connector connector)
            : base(designer)
        {
            this._canvas = designer;
            this._connector = connector;
            _drawingPen = new Pen(Brushes.LightSlateGray, 1);
            _drawingPen.LineJoin = PenLineJoin.Round;
            this.Cursor = Cursors.Cross;
        }

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

        private void HitTesting(Point hitPoint)
        {
            var hitObject = _canvas.InputHitTest(hitPoint) as DependencyObject;
            while (hitObject != null && hitObject != _connector.ParentDesignerItem)
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

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_hitDesignerItem != null)
            {
                _canvas.OnDragLineCompleted(_connector.ParentDesignerItem, _hitDesignerItem);
                _hitDesignerItem.IsDragConnectionOver = false;
            }

            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            var adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
            if (adornerLayer != null) { adornerLayer.Remove(this); }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured) this.CaptureMouse();

                var pos = e.GetPosition(this);
                HitTesting(pos);
                var from = _connector.ParentDesignerItem.GetRect().GetCenterPoint();
                //var from = _connector.Position;
                this._pathGeometry = PathHelper.DarwGeometry(from, pos);

                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawGeometry(null, _drawingPen, this._pathGeometry);

            // without a background the OnMouseMove event would not be fired
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }
    }
}
