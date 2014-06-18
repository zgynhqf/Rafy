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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignerEngine
{
    /// <summary>
    /// 画布在拖拽时的虚线框。
    /// </summary>
    internal class RubberbandAdorner : Adorner
    {
        private Point? _startPoint;
        private Point? _endPoint;
        private Pen _rubberbandPen;

        private DesignerCanvas _designerCanvas;

        public RubberbandAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            this._designerCanvas = designerCanvas;
            this._startPoint = dragStartPoint;
            _rubberbandPen = new Pen(Brushes.LightSlateGray, 1);
            _rubberbandPen.DashStyle = new DashStyle(new double[] { 2 }, 1);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!this.IsMouseCaptured)
                    this.CaptureMouse();

                _endPoint = e.GetPosition(this);
                UpdateSelection();
                this.InvalidateVisual();
            }
            else
            {
                if (this.IsMouseCaptured) this.ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // release mouse capture
            if (this.IsMouseCaptured) this.ReleaseMouseCapture();

            // remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this._designerCanvas);
            if (adornerLayer != null)
                adornerLayer.Remove(this);

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired !
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (this._startPoint.HasValue && this._endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rubberbandPen, new Rect(this._startPoint.Value, this._endPoint.Value));
        }

        private void UpdateSelection()
        {
            var items = new List<IDesignerCanvasItemInternal>();

            var rubberBand = new Rect(_startPoint.Value, _endPoint.Value);
            foreach (Visual item in _designerCanvas.Children)
            {
                if (item is IDesignerCanvasItemInternal)
                {
                    Point center = new Point();
                    if (item is DesignerItemContainer)
                    {
                        Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);
                        Rect itemBounds = item.TransformToAncestor(_designerCanvas).TransformBounds(itemRect);

                        center = itemBounds.GetCenterPoint();
                    }
                    else if (item is Connection)
                    {
                        //如果是连接，则只需要选中中点就可以了。
                        var con = item as Connection;
                        center = con.CenterPoint;
                    }

                    if (rubberBand.Contains(center))
                    {
                        items.Add(item as IDesignerCanvasItemInternal);
                    }
                }
            }

            _designerCanvas.AddSelection(!_designerCanvas.MultiSelection, items.ToArray());
        }
    }
}