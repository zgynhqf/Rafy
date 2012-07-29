/*******************************************************
 * 
 * 作者：Glodon
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 Glodon 2009
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using OEA.Module.WPF.Editors;

using System.Globalization;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Utils;

namespace OEA.Module.WPF.Converter
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { return string.Empty; }

            return new EnumViewModel((Enum)value).Label;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value.ToString();

            if (string.IsNullOrEmpty(strValue)) return null;

            return EnumViewModel.LabelToEnum(strValue, targetType);
        }
    }
}