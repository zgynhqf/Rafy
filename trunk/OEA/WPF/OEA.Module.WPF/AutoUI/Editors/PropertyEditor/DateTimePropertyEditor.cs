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
using Xceed.Wpf.Toolkit;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 使用 DateTimePicker 的日期时间编辑器
    /// </summary>
    public class DateTimePropertyEditor : WPFPropertyEditor
    {
        protected DateTimePropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var picker = new DateTimePicker() { Name = this.Meta.Name, };

            this.ResetBinding(picker);

            this.SetAutomationElement(picker);

            return picker;
        }

        protected override DependencyProperty BindingProperty()
        {
            return DateTimePicker.ValueProperty;
        }
    }
}