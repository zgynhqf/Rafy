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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace DesignerEngine
{
    /// <summary>
    /// 一个可用于添加新的连接线的连接点。
    /// </summary>
    public class Connector : Control
    {
        static Connector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Connector), new FrameworkPropertyMetadata(typeof(Connector)));
        }

        private DesignerItemContainer _parentDesignerItem;
        /// <summary>
        /// 这个连接器所属的设计元素。
        /// </summary>
        internal DesignerItemContainer ParentDesignerItem
        {
            get
            {
                if (_parentDesignerItem == null)
                {
                    _parentDesignerItem = DesignerItemContainer.GetItemContainer(this);
                }

                return _parentDesignerItem;
            }
        }

        #region Position

        private Point _position;
        /// <summary>
        /// 相对于 DesignerCanvas 的本连接器的中心点位置。
        /// </summary>
        public Point Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Connector()
        {
            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        // when the layout changes we update the position property
        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            var canvas = FindCanvas();
            if (canvas != null)
            {
                //get centre position of this Connector relative to the DesignerCanvas
                this.Position = this.TransformToAncestor(canvas).Transform(new Point(this.Width / 2, this.Height / 2));
            }
        }

        #endregion

        #region 鼠标拖拽时，产生临时的连接线。

        private Point? _dragStartPoint = null;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var canvas = FindCanvas();
            if (canvas != null)
            {
                // position relative to DesignerCanvas
                this._dragStartPoint = e.GetPosition(canvas);
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed) this._dragStartPoint = null;

            // but if mouse button is pressed and start point value is set we do have one
            if (this._dragStartPoint.HasValue)
            {
                // create connection adorner 
                var canvas = FindCanvas();
                if (canvas != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(canvas);
                    if (adornerLayer != null)
                    {
                        var adorner = new ConnectorAdorner(canvas, this);
                        adornerLayer.Add(adorner);

                        e.Handled = true;
                    }
                }
            }
        }

        #endregion

        private DesignerCanvas FindCanvas()
        {
            var item = this.ParentDesignerItem;
            if (item != null)
            {
                return item.OwnerCanvas;
            }
            return null;
        }

        internal ConnectorInfo GetInfo()
        {
            var info = new ConnectorInfo();

            info.DesignerItemRect = this.ParentDesignerItem.GetRect();
            info.Position = this.Position;

            return info;
        }
    }
}
