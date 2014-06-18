/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121019 18:03
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121019 18:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 一个可以把 false 转换为 Visibility.Hidden 的转换器。
    /// </summary>
    public class FalseToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var bValue = (bool)value;
            return bValue ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var vValue = (Visibility)value;
            return vValue == Visibility.Visible;
        }
    }
}