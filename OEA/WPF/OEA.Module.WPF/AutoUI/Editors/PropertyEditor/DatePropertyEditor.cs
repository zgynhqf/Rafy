/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 使用DatePicker的日期编辑器
    /// </summary>
    public class DatePropertyEditor : WPFPropertyEditor
    {
        public DatePropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var datePicker = new DatePicker()
            {
                Name = PropertyViewInfo.Name,
            };

            this.ResetBinding(datePicker);

            this.SetAutomationElement(datePicker);

            return datePicker;
        }

        protected override void ResetBinding(FrameworkElement editingControl)
        {
            //绑定TextBox到对象属性
            var binding = this.CreateBinding();
            binding.StringFormat = "yyyy-MM-dd";
            binding.Converter = new AnyToDateConverter();
            editingControl.SetBinding(DatePicker.SelectedDateProperty, binding);
        }

        class AnyToDateConverter : IValueConverter
        {
            #region IValueConverter Members

            public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                DateTime? result = null;

                var strValue = value as string;
                if (!string.IsNullOrWhiteSpace(strValue))
                {
                    DateTime tmp;
                    if (DateTime.TryParse(strValue, out tmp))
                    {
                        result = tmp;
                    }
                }
                else if (value is DateTime)
                {
                    result = (DateTime)value;
                }

                return result;
            }

            public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var time = ((DateTime?)value).GetValueOrDefault(DateTime.Now);

                object targetValue = time;

                if (targetType == typeof(string))
                {
                    targetValue = time.ToShortDateString();
                }

                return targetValue;
            }

            #endregion
        }
    }
}