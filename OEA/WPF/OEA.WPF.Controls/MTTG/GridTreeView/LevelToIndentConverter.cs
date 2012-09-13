using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// Convert Level to left margin
    /// Pass a prarameter if you want a unit length other than 19.0.
    /// </summary>
    public class LevelToIndentConverter : IValueConverter
    {
        public object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            int level = (int)o;
            int indentLevel = level;

            Double indentSize = 0;
            if (parameter != null) double.TryParse(parameter.ToString(), out indentSize);

            return indentLevel * indentSize;
        }

        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}