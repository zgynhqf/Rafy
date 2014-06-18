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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace DesignerEngine
{
    /// <summary>
    /// 设计器的主画布
    /// </summary>
    public class DesignerCanvas : Canvas
    {
        public DesignerCanvas()
        {
            this.AllowDrop = true;
        }

        #region 控制属性

        #region CanChangeConnection DependencyProperty

        public static readonly DependencyProperty CanChangeConnectionProperty = DependencyProperty.Register(
            "CanChangeConnection", typeof(bool), typeof(DesignerCanvas)
            );

        /// <summary>
        /// 是否允许变更连接线的两个端点。
        /// </summary>
        public bool CanChangeConnection
        {
            get { return (bool)this.GetValue(CanChangeConnectionProperty); }
            set { this.SetValue(CanChangeConnectionProperty, value); }
        }

        #endregion

        #region CanAddConnection DependencyProperty

        public static readonly DependencyProperty CanAddConnectionProperty = DependencyProperty.Register(
            "CanAddConnection", typeof(bool), typeof(DesignerCanvas),
            new PropertyMetadata(true, (d, e) => (d as DesignerCanvas).OnCanAddConnectionChanged(e))
            );

        /// <summary>
        /// 是否能添加连接线。
        /// </summary>
        public bool CanAddConnection
        {
            get { return (bool)this.GetValue(CanAddConnectionProperty); }
            set { this.SetValue(CanAddConnectionProperty, value); }
        }

        private void OnCanAddConnectionChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            foreach (var child in this.Children)
            {
                var container = child as DesignerItemContainer;
                if (container != null)
                {
                    container.CanAddConnection = value;
                }
            }
        }

        #endregion

        #endregion

        #region 选择项

        private List<IDesignerCanvasItem> _selectedItems = new List<IDesignerCanvasItem>();
        /// <summary>
        /// 当前的所有选择项
        /// </summary>
        public IList<IDesignerCanvasItem> SelectedItems
        {
            get { return new ReadOnlyCollection<IDesignerCanvasItem>(_selectedItems); }
        }

        internal bool MultiSelection
        {
            get { return (Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None; }
        }

        /// <summary>
        /// 处理用户的选择事件。
        /// </summary>
        /// <param name="item">被点击的项</param>
        internal void HandleMouseSelection(IDesignerCanvasItemInternal item)
        {
            if (this.MultiSelection)
            {
                if (item.IsSelected)
                {
                    this.RemoveSelection(item);
                }
                else
                {
                    this.AddSelection(false, item);
                }
            }
            else if (!item.IsSelected)
            {
                this.AddSelection(true, item);
            }
        }

        /// <summary>
        /// 添加项。
        /// </summary>
        /// <param name="clearOldSelection">在添加项时，是否清空原来的项。</param>
        /// <param name="items"></param>
        internal void AddSelection(bool clearOldSelection, params IDesignerCanvasItemInternal[] items)
        {
            if (clearOldSelection && _selectedItems != null)
            {
                foreach (IDesignerCanvasItemInternal old in _selectedItems) old.IsSelected = false;
                _selectedItems.Clear();
            }

            foreach (var item in items)
            {
                item.IsSelected = true;
                if (!_selectedItems.Contains(item))
                {
                    _selectedItems.Add(item);
                }
            }

            this.OnSelectionChanged();
        }

        internal void RemoveSelection(IDesignerCanvasItemInternal item)
        {
            item.IsSelected = false;
            _selectedItems.Remove(item);

            this.OnSelectionChanged();
        }

        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged()
        {
            var handler = this.SelectionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region 拖拽时画虚线多项选择框

        /// <summary>
        ///  start point of the rubberband drag operation
        /// </summary>
        private Point? _rubberbandSelectionStartPoint = null;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Source == this)
            {
                //记录拖拽点。
                this._rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

                //清空选择项。
                if (_selectedItems != null && !this.MultiSelection)
                {
                    foreach (IDesignerCanvasItemInternal item in _selectedItems) item.IsSelected = false;
                    _selectedItems.Clear();
                    this.OnSelectionChanged();
                }

                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed) this._rubberbandSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (this._rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    var adorner = new RubberbandAdorner(this, _rubberbandSelectionStartPoint);
                    adornerLayer.Add(adorner);
                }
            }

            e.Handled = true;
        }

        #endregion

        #region 拖拽线段

        /// <summary>
        /// 从一个元素到另一个元素拖拽线段的操作完成。
        /// </summary>
        public event EventHandler<CanvasDragLineCompletedEventArgs> DragLineCompleted;

        internal protected virtual void OnDragLineCompleted(DesignerItemContainer source, DesignerItemContainer sink)
        {
            //默认实现如下：
            ////从 0 开始添加，可以保证连接显示在元素之后。
            //this.Children.Insert(0, new Connection
            //{
            //    Source = source,
            //    Sink = sink
            //});

            var handler = this.DragLineCompleted;
            if (handler != null) handler(this, new CanvasDragLineCompletedEventArgs(source, sink));
        }

        #endregion

        #region 滚动条

        /// <summary>
        /// 返回一个需要的大小，这样外部的 ScrollViewer 才能正常工作。
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            var size = new Size();

            foreach (UIElement element in base.Children)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                var desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }

            // add margin 
            size.Width += 50;
            size.Height += 200;

            return size;
        }

        #endregion

        #region 工具箱拖拽

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            var dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (dragObject != null && !String.IsNullOrEmpty(dragObject.Xaml))
            {
                var content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));
                if (content != null)
                {
                    var newItem = new DesignerItemContainer();
                    newItem.Content = content;

                    var position = e.GetPosition(this);
                    if (dragObject.DesiredSize.HasValue)
                    {
                        Size desiredSize = dragObject.DesiredSize.Value;
                        newItem.Width = desiredSize.Width;
                        newItem.Height = desiredSize.Height;

                        DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X - newItem.Width / 2));
                        DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y - newItem.Height / 2));
                    }
                    else
                    {
                        DesignerCanvas.SetLeft(newItem, Math.Max(0, position.X));
                        DesignerCanvas.SetTop(newItem, Math.Max(0, position.Y));
                    }

                    this.Children.Add(newItem);

                    this.AddSelection(true, newItem);
                }

                e.Handled = true;
            }
        }

        #endregion

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null)
            {
                var item = visualAdded as IDesignerCanvasItemInternal;
                if (item != null)
                {
                    var itemContainer = item as DesignerItemContainer;
                    if (itemContainer != null)
                    {
                        itemContainer.CanAddConnection = this.CanAddConnection;
                    }
                }
            }
        }

        #region 帮助方法

        /// <summary>
        /// 在可视树中找到元素的外层 DesignerCanvas
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static DesignerCanvas GetOwnerCanvas(DependencyObject element)
        {
            return ModelingHelper.GetVisualParent<DesignerCanvas>(element);
        }

        #endregion
    }

    /// <summary>
    /// 拖拽线段完成事件参数
    /// </summary>
    public class CanvasDragLineCompletedEventArgs : EventArgs
    {
        public CanvasDragLineCompletedEventArgs(DesignerItemContainer source, DesignerItemContainer sink)
        {
            this.Source = source;
            this.Sink = sink;
        }

        /// <summary>
        /// 起始点。
        /// </summary>
        public DesignerItemContainer Source { get; private set; }

        /// <summary>
        /// 终止点。
        /// </summary>
        public DesignerItemContainer Sink { get; private set; }
    }
}