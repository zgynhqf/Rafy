using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;

namespace Rafy.WPF
{
    /// <summary>
    /// ItemColletion 转换为 Visibility
    /// 不能直接传 Count，因为有的子页签是被设置为 Collapsed 不显示的
    /// TabControl 中的 TabItem 不能被设置为 Collapsed，否则自动化无法识别，所以这里把 TabItem 的大小变到很小。
    /// </summary>
    public class ItemColletionToScaleYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(double)) throw new NotSupportedException();

            var items = value as ItemCollection;
            return items.Count == 1 ? 0.001 : 1d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
