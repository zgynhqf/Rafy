/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101214
 * 说明：列表数据量到是否显示的转换器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101214
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Collections;
using System.Windows;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 列表数据量到是否显示的转换器
    /// </summary>
    public class ItemsControlNoDataConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool noData = false;

            var itemsSource = values[0];
            var itemsCount = values[1];

            if (itemsSource != null)
            {
                if (itemsSource is IList)
                {
                    noData = (itemsSource as IList).Count == 0;
                }
                else if (itemsSource is IEnumerable)
                {
                    noData = !(itemsSource as IEnumerable).OfType<object>().Any();
                }
                else if (itemsSource == null)
                {
                    noData = true;
                }
            }
            //else
            //{
            //    noData = (int)itemsCount == 0;
            //}

            return noData ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class NoCountVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        bool noData = (int)value == 0;

    //        return noData ? Visibility.Visible : Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
