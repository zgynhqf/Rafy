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
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Windows.Media.Animation;
using System.Windows;
using System.ComponentModel;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Layout
{
    /// <summary>
    /// 滑动布局动画定义。
    /// </summary>
    public class ResizingPanelSlippingAnimation : DependencyPropertyChangedListener
    {
        /// <summary>
        /// 初始化整个布局
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="parentChildProportion"></param>
        public static void Initialize(FrameworkElement left, FrameworkElement right, ParentChildProportion parentChildProportion = null)
        {
            //layout.AggtBlocks.Layout.ParentChildProportion
            parentChildProportion = parentChildProportion ?? ParentChildProportion.Default;

            ResizingPanelExt.SetStarGridLength(left, parentChildProportion.Parent);
            ResizingPanelExt.SetStarGridLength(right, parentChildProportion.Children);

            var animation = new ResizingPanelSlippingAnimation(parentChildProportion.Children);
            animation.Attach(right);
        }

        private AnimationTimeline _show, _hide;

        private ResizingPanelSlippingAnimation(double childrenPartition)
        {
            _show = new DoubleAnimationUsingKeyFrames
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
                        Value = childrenPartition,
                        EasingFunction = new ElasticEase
                        {
                            EasingMode = EasingMode.EaseOut,
                            Springiness = 100  //子滑进秒数
                        }
                    }
                }
            };
            _hide = new DoubleAnimation()
            {
                From = childrenPartition,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                To = 0
            };
        }

        private void Attach(FrameworkElement children)
        {
            var propertyDescriptor = DependencyPropertyDescriptor.FromProperty(UIElement.VisibilityProperty, typeof(UIElement));
            this.Attach(propertyDescriptor, children);
        }

        protected override void OnPropertyChanged(object sender, EventArgs e)
        {
            _element.BeginAnimation(
                ResizingPanelExt.StarGridLengthProperty,
                _element.Visibility == Visibility.Visible ? _show : _hide
                );
        }
    }

    public abstract class DependencyPropertyChangedListener
    {
        protected FrameworkElement _element;

        public void Attach(DependencyPropertyDescriptor property, FrameworkElement element)
        {
            _element = element;

            EventHandler eventHandler = OnPropertyChanged;

            property.RemoveValueChanged(element, eventHandler);
            property.AddValueChanged(element, eventHandler);

            element.Unloaded += (o, e) =>
            {
                property.RemoveValueChanged(element, eventHandler);
            };
        }

        protected abstract void OnPropertyChanged(object sender, EventArgs e);
    }
}
