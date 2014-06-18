/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120602 20:17
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120602 20:17
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Rafy.Reflection;
using Rafy.WPF.Controls;
using Xceed.Wpf.Toolkit;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 使用 DateTimePicker 的日期时间编辑器
    /// </summary>
    public class DateTimePropertyEditor : DateTimePropertyEditorBase
    {
        protected DateTimePropertyEditor() { }

        protected override FrameworkElement CreateDateTimeEditor()
        {
            return new DateTimeInputControl();
        }

        protected override DependencyProperty BindingProperty()
        {
            return DateTimePicker.ValueProperty;
        }
    }
}