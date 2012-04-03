using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using OEA.Module.WPF.Editors;
using OEA.Core;
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
            return new EnumViewModel((Enum)value).Label;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return EnumViewModel.LabelToEnum(value.ToString(), targetType);
        }
    }
}