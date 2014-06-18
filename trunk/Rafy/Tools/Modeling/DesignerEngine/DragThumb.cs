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
    /// 用于拖动位置的控件
    /// 
    /// 在 DesignerItem 中任意位置放置这个控件，即可实现对 DesignerItem 的拖拽。
    /// </summary>
    public class DragThumb : Thumb
    {
        static DragThumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragThumb), new FrameworkPropertyMetadata(typeof(DragThumb)));
        }

        public DragThumb()
        {
            base.DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var designerItem = DesignerItemContainer.GetItemContainer(this);
            if (designerItem != null && designerItem.IsSelected)
            {
                var designer = DesignerCanvas.GetOwnerCanvas(designerItem);
                if (designer != null)
                {
                    //只移动 DesignerItem
                    var designerItems = designer.SelectedItems.OfType<DesignerItemContainer>();

                    //以下代码不知何意，暂留。
                    //double minLeft = double.MaxValue;
                    //double minTop = double.MaxValue;

                    //foreach (var item in designerItems)
                    //{
                    //    double left = GetDouble(item, Canvas.LeftProperty);
                    //    double top = GetDouble(item, Canvas.TopProperty);

                    //    minLeft = Math.Min(left, minLeft);
                    //    minTop = Math.Min(top, minTop);
                    //}

                    //double deltaHorizontal = Math.Max(-minLeft, e.HorizontalChange);
                    //double deltaVertical = Math.Max(-minTop, e.VerticalChange);

                    double deltaHorizontal = e.HorizontalChange;
                    double deltaVertical = e.VerticalChange;

                    foreach (var item in designerItems)
                    {
                        double left = GetDouble(item, Canvas.LeftProperty);
                        double top = GetDouble(item, Canvas.TopProperty);

                        Canvas.SetLeft(item, left + deltaHorizontal);
                        Canvas.SetTop(item, top + deltaVertical);
                    }

                    designer.InvalidateMeasure();

                    e.Handled = true;
                }
            }
        }

        private static double GetDouble(DependencyObject item, DependencyProperty property)
        {
            var res = (double)item.GetValue(property);
            if (double.IsNaN(res)) res = 0;
            return res;
        }
    }
}
