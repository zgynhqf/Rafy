/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130712
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130712 09:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rafy.WPF.Controls
{
    [TemplatePart(Name = "PART_Decorator", Type = typeof(FrameworkElement))]
    public class FocusTrackerControl : Control
    {
        static FocusTrackerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FocusTrackerControl), new FrameworkPropertyMetadata(typeof(FocusTrackerControl)));
        }

        private FrameworkElement _decorator;

        private Rect _dest;

        /// <summary>
        /// 目前要移动的目标位置。
        /// </summary>
        public Rect Dest
        {
            get { return _dest; }
        }

        /// <summary>
        /// 移动到指定位置。
        /// </summary>
        /// <param name="dest">目标位置</param>
        /// <param name="showAnimation">
        /// 是否使用动画来进行移动。
        /// 如果不使用动画来移动，则会直接设置控件的位置。
        /// </param>
        internal void MoveTo(Rect dest, bool showAnimation)
        {
            _dest = dest;

            this.ApplyTemplate();

            if (showAnimation)
            {
                this.StartStoryboard();
            }
            else
            {
                Canvas.SetLeft(_decorator, _dest.Left);
                Canvas.SetTop(_decorator, _dest.Top);
                _decorator.Width = _dest.Width;
                _decorator.Height = _dest.Height;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _decorator = this.Template.FindName("PART_Decorator", this) as FrameworkElement;
        }

        #region 动画移动效果

        private Storyboard _currentStory;

        internal FocusTrackerAdorner Adorner;

        private void StartStoryboard()
        {
            #region //xaml

            //<ControlTemplate.Resources>
            //    <Storyboard x:Key="MoveToTarget">
            //        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="(Canvas.Left)"
            //                Storyboard.TargetName="border">
            //            <EasingDoubleKeyFrame KeyTime="0:0:0.3" Value="{TemplateBinding ToLeft}">
            //                <EasingDoubleKeyFrame.EasingFunction>
            //                    <QuarticEase EasingMode="EaseInOut" />
            //                </EasingDoubleKeyFrame.EasingFunction>
            //            </EasingDoubleKeyFrame>
            //        </DoubleAnimationUsingKeyFrames>
            //        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="(Canvas.Top)"
            //                Storyboard.TargetName="border">
            //            <EasingDoubleKeyFrame KeyTime="0:0:0.3" Value="{TemplateBinding ToTop}">
            //                <EasingDoubleKeyFrame.EasingFunction>
            //                    <QuarticEase EasingMode="EaseInOut" />
            //                </EasingDoubleKeyFrame.EasingFunction>
            //            </EasingDoubleKeyFrame>
            //        </DoubleAnimationUsingKeyFrames>
            //        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="Width"
            //                Storyboard.TargetName="border">
            //            <EasingDoubleKeyFrame KeyTime="0:0:0.3" Value="{TemplateBinding ToWidth}">
            //                <EasingDoubleKeyFrame.EasingFunction>
            //                    <QuarticEase EasingMode="EaseInOut" />
            //                </EasingDoubleKeyFrame.EasingFunction>
            //            </EasingDoubleKeyFrame>
            //        </DoubleAnimationUsingKeyFrames>
            //        <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty="Height"
            //                Storyboard.TargetName="border">
            //            <EasingDoubleKeyFrame KeyTime="0:0:0.3" Value="{TemplateBinding ToHeight}">
            //                <EasingDoubleKeyFrame.EasingFunction>
            //                    <QuarticEase EasingMode="EaseInOut" />
            //                </EasingDoubleKeyFrame.EasingFunction>
            //            </EasingDoubleKeyFrame>
            //        </DoubleAnimationUsingKeyFrames>
            //    </Storyboard>
            //</ControlTemplate.Resources> 

            #endregion

            var story = new Storyboard();

            story.Children.Add(CreateAnimation(Canvas.LeftProperty, _dest.Left));
            story.Children.Add(CreateAnimation(Canvas.TopProperty, _dest.Top));
            story.Children.Add(CreateAnimation(FrameworkElement.WidthProperty, _dest.Width));
            story.Children.Add(CreateAnimation(FrameworkElement.HeightProperty, _dest.Height));

            //启动动画。
            story.Completed += _currentStory_Completed;
            story.Begin(_decorator);

            if (_currentStory != null) { _currentStory.Stop(); }
            _currentStory = story;
        }

        private static AnimationTimeline CreateAnimation(DependencyProperty property, double targetValue)
        {
            var animation = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new EasingDoubleKeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0.3d),
                        Value = targetValue,
                        EasingFunction = new QuarticEase
                        {
                            EasingMode = EasingMode.EaseInOut
                        }
                    }
                }
            };

            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            return animation;
        }

        void _currentStory_Completed(object sender, EventArgs e)
        {
            if (this.Adorner != null)
            {
                this.Adorner.NotifyMoveCompleted();
            }
        }

        #endregion
    }
}