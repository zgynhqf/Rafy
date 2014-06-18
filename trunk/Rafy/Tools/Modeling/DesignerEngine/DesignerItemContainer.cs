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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignerEngine
{
    /// <summary>
    /// 设计器元素容器元素。
    /// </summary>
    public class DesignerItemContainer : ContentControl, IDesignerCanvasItem, IDesignerCanvasItemInternal
    {
        static DesignerItemContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItemContainer), new FrameworkPropertyMetadata(typeof(DesignerItemContainer)));
        }

        private List<Connection> _connections;

        /// <summary>
        /// 所在的画布
        /// </summary>
        public DesignerCanvas OwnerCanvas
        {
            get { return DesignerCanvas.GetOwnerCanvas(this); }
        }

        /// <summary>
        /// 本元素拥有的所有连接点
        /// </summary>
        public IList<Connection> Connections
        {
            get
            {
                if (_connections == null)
                {
                    _connections = new List<Connection>();
                }
                return _connections;
            }
        }

        #region CanAddConnection DependencyProperty

        private static readonly DependencyPropertyKey CanAddConnectionPropertyKey = DependencyProperty.RegisterReadOnly(
            "CanAddConnection", typeof(bool), typeof(DesignerItemContainer),
            new PropertyMetadata(true)
            );

        public static readonly DependencyProperty CanAddConnectionProperty = CanAddConnectionPropertyKey.DependencyProperty;

        /// <summary>
        /// 是否能添加连接线。
        /// </summary>
        public bool CanAddConnection
        {
            get { return (bool)this.GetValue(CanAddConnectionProperty); }
            internal set { this.SetValue(CanAddConnectionPropertyKey, value); }
        }

        #endregion

        #region IsSelected DependencyProperty

        private static readonly DependencyPropertyKey IsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsSelected", typeof(bool), typeof(DesignerItemContainer),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty IsSelectedProperty = IsSelectedPropertyKey.DependencyProperty;

        /// <summary>
        /// 是否已经被用户选中。
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)this.GetValue(IsSelectedProperty); }
            internal set { this.SetValue(IsSelectedPropertyKey, value); }
        }

        #endregion

        #region IsDragConnectionOver DependencyProperty

        public static readonly DependencyProperty IsDragConnectionOverProperty = DependencyProperty.Register(
            "IsDragConnectionOver", typeof(bool), typeof(DesignerItemContainer),
            new PropertyMetadata((d, e) => (d as DesignerItemContainer).OnIsDragConnectionOverChanged(e))
            );

        /// <summary>
        /// 如果临时连接线正在拖动到这个元素上，则这个值返回真。此时，所有的连接点都将会被显示。
        /// </summary>
        public bool IsDragConnectionOver
        {
            get { return (bool)this.GetValue(IsDragConnectionOverProperty); }
            set { this.SetValue(IsDragConnectionOverProperty, value); }
        }

        private void OnIsDragConnectionOverChanged(DependencyPropertyChangedEventArgs e)
        {
            //var value = (bool)e.NewValue;
        }

        #endregion

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            //更新选择项。
            var designer = DesignerCanvas.GetOwnerCanvas(this);
            if (designer != null)
            {
                designer.HandleMouseSelection(this);
            }

            e.Handled = false;
        }

        #region PositionChanged

        internal Rect GetRect()
        {
            return new Rect(
                ModelingHelper.Round(DesignerCanvas.GetLeft(this)),
                ModelingHelper.Round(DesignerCanvas.GetTop(this)),
                ModelingHelper.Round(this.ActualWidth),
                ModelingHelper.Round(this.ActualHeight)
                );
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            var property = e.Property;
            if (property == Canvas.LeftProperty || property == Canvas.TopProperty ||
                property == ActualWidthProperty || property == ActualHeightProperty
                )
            {
                this.OnPositionChanged();
            }
        }

        /// <summary>
        /// 位置变更事件。
        /// </summary>
        public event EventHandler PositionChanged;

        protected virtual void OnPositionChanged()
        {
            var handler = this.PositionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region IDesignerCanvasItemInternal

        bool IDesignerCanvasItemInternal.IsSelected
        {
            get { return IsSelected; }
            set { IsSelected = value; }
        }

        #endregion

        #region 帮助方法

        /// <summary>
        /// 从可视树上获取某个元素的外层 DesignerItemContainer。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static DesignerItemContainer GetItemContainer(DependencyObject element)
        {
            DesignerItemContainer designerItem = null;

            DependencyObject parent = element;
            while (true)
            {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent == null) break;

                designerItem = parent as DesignerItemContainer;
                if (designerItem != null) { break; }

                //如果已经找到 DesignerCanvas，则退出。
                if (parent is DesignerCanvas) break;
            }

            return designerItem;
        }

        #endregion
    }
}