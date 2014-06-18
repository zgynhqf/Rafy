/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121030 17:30
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121030 17:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGrid 合计行中的单元格
    /// </summary>
    public class TreeGridColumnSummary : Control
    {
        static TreeGridColumnSummary()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridColumnSummary), new FrameworkPropertyMetadata(typeof(TreeGridColumnSummary)));
        }

        #region SummaryText DependencyProperty

        public static readonly DependencyProperty SummaryTextProperty = DependencyProperty.Register(
            "SummaryText", typeof(string), typeof(TreeGridColumnSummary)
            );

        public string SummaryText
        {
            get { return (string)this.GetValue(SummaryTextProperty); }
            set { this.SetValue(SummaryTextProperty, value); }
        }

        #endregion

        #region IsSummaryVisible DependencyProperty

        public static readonly DependencyProperty IsSummaryVisibleProperty = DependencyProperty.Register(
            "IsSummaryVisible", typeof(bool), typeof(TreeGridColumnSummary)
            );

        public bool IsSummaryVisible
        {
            get { return (bool)this.GetValue(IsSummaryVisibleProperty); }
            set { this.SetValue(IsSummaryVisibleProperty, BooleanBoxes.Box(value)); }
        }

        #endregion

        internal TreeGridColumn Column;
    }
}