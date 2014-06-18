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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesignerEngine
{
    /// <summary>
    /// 用于拖动大小的控件
    /// </summary>
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            this.DragDelta += ResizeThumb_DragDelta;

            //双击块的边线时，需要恢复为自动计算大小模式。
            this.MouseDoubleClick += ResizeThumb_MouseDoubleClick;
        }

        void ResizeThumb_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedDesignerItems = GetSelectedContainers();
            if (selectedDesignerItems != null)
            {
                foreach (var container in selectedDesignerItems)
                {
                    //尽量直接设置内容控件的大小。
                    var item = (container.Content as FrameworkElement) ?? container;

                    if (this.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        item.Height = double.NaN;
                    }
                    if (this.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        item.Width = double.NaN;
                    }
                }

                e.Handled = true;
            }
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var selectedDesignerItems = GetSelectedContainers();
            if (selectedDesignerItems != null)
            {
                double minLeft, minTop, minDeltaHorizontal, minDeltaVertical;
                double dragDeltaVertical, dragDeltaHorizontal;

                CalculateDragLimits(selectedDesignerItems, out minLeft, out minTop, out minDeltaHorizontal, out minDeltaVertical);

                foreach (var container in selectedDesignerItems)
                {
                    //尽量直接设置内容控件的大小。
                    var item = (container.Content as FrameworkElement) ?? container;

                    switch (base.VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                        case VerticalAlignment.Top:
                            double top = Canvas.GetTop(item);
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            Canvas.SetTop(item, top + dragDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                        default:
                            break;
                    }

                    switch (base.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            double left = Canvas.GetLeft(item);
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            Canvas.SetLeft(item, left + dragDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                        default:
                            break;
                    }
                }

                e.Handled = true;
            }
        }

        private DesignerItemContainer[] GetSelectedContainers()
        {
            DesignerItemContainer[] selectedDesignerItems = null;

            var designerItem = this.DataContext as DesignerItemContainer;
            if (designerItem != null && designerItem.IsSelected)
            {
                var designer = DesignerCanvas.GetOwnerCanvas(designerItem);
                if (designer != null)
                {
                    // only resize DesignerItems
                    selectedDesignerItems = designer.SelectedItems.OfType<DesignerItemContainer>().ToArray();
                }
            }
            return selectedDesignerItems;
        }

        private static void CalculateDragLimits(
            IEnumerable<DesignerItemContainer> selectedDesignerItems,
            out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical
            )
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (var item in selectedDesignerItems)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);

                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
            }
        }
    }
}