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
using Xceed.Wpf.Toolkit;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 使用 TimePicker 的时间编辑器
    /// </summary>
    public class TimePropertyEditor : WPFPropertyEditor
    {
        protected TimePropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var datePicker = new TimePicker() { Name = this.Meta.Name, };

            this.ResetBinding(datePicker);

            this.SetAutomationElement(datePicker);

            return datePicker;
        }

        protected override DependencyProperty BindingProperty()
        {
            return TimePicker.ValueProperty;
        }
    }
}