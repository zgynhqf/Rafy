/*******************************************************
 * 
 * 作者：李刚
 * 创建时间：20130308 16:52
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 李刚 20130308 16:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 可选择支持 Date 日期模式、DateTime 日期时间模式的输入控件。
    /// </summary>
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public class DateTimeInputControl : DateTimePicker
    {
        #region CalenderVisibility DependencyProperty

        public static readonly DependencyProperty CalenderVisibilityProperty = DependencyProperty.Register(
            "CalenderVisibility", typeof(Visibility), typeof(DateTimeInputControl),
            new PropertyMetadata(Visibility.Visible)
            );

        /// <summary>
        /// 设置 Clender 的 Visibility 属性的值
        /// </summary>
        public Visibility CalenderVisibility
        {
            get { return (Visibility)this.GetValue(CalenderVisibilityProperty); }
            set { this.SetValue(CalenderVisibilityProperty, value); }
        }

        #endregion

        #region TimePickerVisibility DependencyProperty

        public static readonly DependencyProperty TimePickerVisibilityProperty = DependencyProperty.Register(
            "TimePickerVisibility", typeof(Visibility), typeof(DateTimeInputControl),
            new PropertyMetadata(Visibility.Visible)
            );

        /// <summary>
        /// 设置TimePicker的Vibility属性的值
        /// </summary>
        public Visibility TimePickerVisibility
        {
            get { return (Visibility)this.GetValue(TimePickerVisibilityProperty); }
            set { this.SetValue(TimePickerVisibilityProperty, value); }
        }

        #endregion

        protected override void OnFormatChanged(DateTimeFormat oldValue, DateTimeFormat newValue)
        {
            base.OnFormatChanged(oldValue, newValue);

            switch (newValue)
            {
                case DateTimeFormat.LongDate:
                case DateTimeFormat.ShortDate:
                    this.CalenderVisibility = Visibility.Visible;
                    this.TimePickerVisibility = Visibility.Collapsed;
                    break;
                case DateTimeFormat.LongTime:
                case DateTimeFormat.ShortTime:
                    this.CalenderVisibility = Visibility.Collapsed;
                    this.TimePickerVisibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        #region 禁用选中部分的行为

        private static readonly PropertyInfo TextBoxProperty =
            typeof(DateTimeInputControl).BaseType.BaseType.BaseType
            .GetProperty("TextBox", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var txt = this.Template.FindName("PART_TextBox", this) as TextBox;
            if (txt != null)
            {
                TextBoxProperty.SetValue(this, txt, null);
            }
        }

        #endregion

        #region 拷贝自 Xceed.Wpf.Toolkit.DateTimePicker 的方法

        /*********************** 代码块解释 *********************************
         * 重写基类方法，解决两个问题：
         * 1. 在手动变更文本框内容后，焦点移开，再次回到之前的时间值，变更无效。
         * 2. 对中文进行分隔的时间，处理有误。
        **********************************************************************/

        private static readonly FieldInfo _processTextChangedField =
            typeof(DateTimeUpDown).GetField("_processTextChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        protected override void OnTextChanged(string previousValue, string currentValue)
        {
            var _processTextChanged = Convert.ToBoolean(_processTextChangedField.GetValue(this));
            if (!_processTextChanged) { return; }

            if (string.IsNullOrEmpty(currentValue))
            {
                base.Value = null;
                return;
            }

            DateTime currentDate = base.Value.HasValue ? base.Value.Value : DateTime.Parse(DateTime.Now.ToString(), base.CultureInfo.DateTimeFormat);
            DateTime dateTime = currentDate;
            DateTimeParser.TryParse(currentValue, this.GetFormatString(this.Format), currentDate, base.CultureInfo, out dateTime);
            base.SyncTextAndValueProperties(DateTimeInputControl.TextProperty, dateTime.ToString(base.CultureInfo));
        }

        private string GetFormatString(DateTimeFormat dateTimeFormat)
        {
            switch (dateTimeFormat)
            {
                case DateTimeFormat.Custom:
                    return this.FormatString;
                case DateTimeFormat.FullDateTime:
                    return base.CultureInfo.DateTimeFormat.FullDateTimePattern;
                case DateTimeFormat.LongDate:
                    return base.CultureInfo.DateTimeFormat.LongDatePattern;
                case DateTimeFormat.LongTime:
                    return base.CultureInfo.DateTimeFormat.LongTimePattern;
                case DateTimeFormat.MonthDay:
                    return base.CultureInfo.DateTimeFormat.MonthDayPattern;
                case DateTimeFormat.RFC1123:
                    return base.CultureInfo.DateTimeFormat.RFC1123Pattern;
                case DateTimeFormat.ShortDate:
                    return base.CultureInfo.DateTimeFormat.ShortDatePattern;
                case DateTimeFormat.ShortTime:
                    return base.CultureInfo.DateTimeFormat.ShortTimePattern;
                case DateTimeFormat.SortableDateTime:
                    return base.CultureInfo.DateTimeFormat.SortableDateTimePattern;
                case DateTimeFormat.UniversalSortableDateTime:
                    return base.CultureInfo.DateTimeFormat.UniversalSortableDateTimePattern;
                case DateTimeFormat.YearMonth:
                    return base.CultureInfo.DateTimeFormat.YearMonthPattern;
                default:
                    throw new ArgumentException("Not a supported format");
            }
        }

        internal class DateTimeParser
        {
            public static bool TryParse(string value, string format, DateTime currentDate, CultureInfo cultureInfo, out DateTime result)
            {
                bool flag = false;
                result = currentDate;
                if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(format))
                {
                    return false;
                }
                string text = value.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    flag = DateTime.TryParse(text, cultureInfo.DateTimeFormat, DateTimeStyles.None, out result);
                }
                if (!flag)
                {
                    result = currentDate;
                }
                return flag;
            }
        }

        #endregion
    }
}