/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121129 10:48
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121129 10:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 命令对应的内容控件
    /// </summary>
    public class CommandContentControl : Control
    {
        static CommandContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandContentControl), new FrameworkPropertyMetadata(typeof(CommandContentControl)));
        }

        #region Label DependencyProperty

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(CommandContentControl)
            );

        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        #endregion

        #region ImageUri DependencyProperty

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource", typeof(ImageSource), typeof(CommandContentControl),
            new PropertyMetadata((d, e) => (d as CommandContentControl).OnImageSourceChanged(e))
            );

        public ImageSource ImageSource
        {
            get { return (ImageSource)this.GetValue(ImageSourceProperty); }
            set { this.SetValue(ImageSourceProperty, value); }
        }
        private void OnImageSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (ImageSource)e.NewValue;
            if (value != null) this.UseImage = true;
        }

        #endregion

        #region UseImage DependencyProperty

        public static readonly DependencyProperty UseImageProperty = DependencyProperty.Register(
            "UseImage", typeof(bool), typeof(CommandContentControl)
            );

        public bool UseImage
        {
            get { return (bool)this.GetValue(UseImageProperty); }
            set { this.SetValue(UseImageProperty, value); }
        }

        #endregion
    }
}