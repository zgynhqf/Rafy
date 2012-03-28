/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110630
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110630
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Media.Animation;
using System.Windows;
using System.ComponentModel;

namespace OEA.Module.WPF.Layout
{
    /// <summary>
    /// 滑动布局动画定义。
    /// </summary>
    public static class LayoutSlippingAnimation
    {
        /// <summary>
        /// 初始化整个布局
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static void Initialize(TraditionalLayout layout, FrameworkElement left, FrameworkElement right)
        {
            var leftRightPercent = layout.AggtBlocks.Layout.ParentChildProportion ?? ParentChildProportion.Default;

            ResizingPanelExt.SetStarGridLength(left, leftRightPercent.Parent);
            ResizingPanelExt.SetStarGridLength(right, leftRightPercent.Children);

            AnimationTimeline show = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames = new DoubleKeyFrameCollection()
                {
                    new EasingDoubleKeyFrame
                    {
                        KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))
                    },
                    new EasingDoubleKeyFrame
                    {
                        KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1)),
                        Value = leftRightPercent.Children,
                        EasingFunction = new ElasticEase
                        {
                            EasingMode = EasingMode.EaseOut,
                            Springiness = 10
                        }
                    }
                }
            };
            AnimationTimeline hide = new DoubleAnimation()
            {
                From = leftRightPercent.Children,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                To = 0
            };

            //监听 childrenTab.VisiblityChanged 事件，播放动画效果
            var childrenTab = layout.TryGetChildrenTab();
            if (childrenTab != null)
            {
                DependencyPropertyDescriptor.FromProperty(UIElement.VisibilityProperty, typeof(UIElement))
                    .AddValueChanged(childrenTab, (o, e) =>
                    {
                        right.BeginAnimation(
                            ResizingPanelExt.StarGridLengthProperty,
                            childrenTab.Visibility == Visibility.Visible ? show : hide
                            );
                    });
            }
        }
    }
}
