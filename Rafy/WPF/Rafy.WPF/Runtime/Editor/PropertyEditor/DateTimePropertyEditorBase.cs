/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120918 15:39
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120918 15:39
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Rafy.Reflection;
using Xceed.Wpf.Toolkit;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 三个日期时间编辑器的基类。
    /// </summary>
    public abstract class DateTimePropertyEditorBase : PropertyEditor
    {
        protected override sealed FrameworkElement CreateEditingElement()
        {
            var picker = this.CreateDateTimeEditor();
            picker.Name = this.Meta.Name;

            //三个日期时间编辑器控件，都需要对删除作以下处理：
            //如果属性支持可空，则直接设置为空属性；如果属性不可空，则屏蔽这两个按键。
            picker.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Delete || e.Key == Key.Back)
                {
                    if (TypeHelper.IsNullable(this.Meta.PropertyMeta.ManagedProperty.PropertyType))
                    {
                        this.PropertyValue = null;
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            };

            this.ResetBinding(picker);

            this.SetAutomationElement(picker);

            return picker;
        }

        protected abstract FrameworkElement CreateDateTimeEditor();

        protected override void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            //进入编辑状态时，把焦点放到文本框上。
            var picker = editingElement as Control;
            if (picker != null)
            {
                var textBox = picker.Template.FindName("TextBox", picker) as TextBox;
                if (textBox != null) textBox.Focus();
            }
            //picker.IsOpen = true;
        }
    }
}
