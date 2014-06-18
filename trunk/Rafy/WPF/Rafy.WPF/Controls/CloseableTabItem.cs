/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120426
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rafy.WPF.Controls
{
    public class CloseableTabItem : TabItem
    {
        static CloseableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableTabItem), new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
        }

        #region ClosingButtonTooltip DependencyProperty

        public static readonly DependencyProperty ClosingButtonTooltipProperty = DependencyProperty.Register(
            "ClosingButtonTooltip", typeof(object), typeof(CloseableTabItem)
            );

        /// <summary>
        /// 关闭按钮的 ToolTip
        /// </summary>
        public object ClosingButtonTooltip
        {
            get { return (object)this.GetValue(ClosingButtonTooltipProperty); }
            set { this.SetValue(ClosingButtonTooltipProperty, value); }
        }

        #endregion
    }
}
