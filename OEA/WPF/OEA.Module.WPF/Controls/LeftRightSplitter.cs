/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110307
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100307
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 可收缩的分隔控件
    /// </summary>
    [TemplatePart(Name = "PART_Left", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_Right", Type = typeof(ContentControl))]
    public class LeftRightSplitter : Control
    {
        static LeftRightSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LeftRightSplitter),
                new FrameworkPropertyMetadata(typeof(LeftRightSplitter))
                );
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var left = this.Template.FindName("PART_Left", this) as ContentControl;
            var right = this.Template.FindName("PART_Right", this) as ContentControl;

            left.Content = this.Left;
            right.Content = this.Right;
        }

        #region LeftProperty

        public object Left
        {
            get { return (object)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(object), typeof(LeftRightSplitter), new UIPropertyMetadata(LeftChangedCallback));

        private static void LeftChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var s = d as SimulatedSplitter;
            //s._left.Content = e.NewValue;
        }

        #endregion

        #region RightProperty

        public object Right
        {
            get { return (object)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }

        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register("Right", typeof(object), typeof(LeftRightSplitter), new UIPropertyMetadata(RightChangedCallback));

        private static void RightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //var s = d as SimulatedSplitter;
            //s._right.Content = e.NewValue;
        }

        #endregion

        protected override IEnumerator LogicalChildren
        {
            get
            {
                return new List<object>{
                    this.Left, this.Right
                } as IEnumerator;
            }
        }
    }
}
