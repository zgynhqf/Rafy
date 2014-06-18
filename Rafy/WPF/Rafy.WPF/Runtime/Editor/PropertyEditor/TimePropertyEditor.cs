/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120602 20:20
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120602 20:20
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 使用 TimePicker 的时间编辑器
    /// </summary>
    public class TimePropertyEditor : DateTimePropertyEditorBase
    {
        protected TimePropertyEditor() { }

        protected override FrameworkElement CreateDateTimeEditor()
        {
            return new TimePicker();
        }

        protected override DependencyProperty BindingProperty()
        {
            return TimePicker.ValueProperty;
        }
    }
}