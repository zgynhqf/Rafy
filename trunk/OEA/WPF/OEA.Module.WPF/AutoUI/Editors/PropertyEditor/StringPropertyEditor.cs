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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;

namespace OEA.Module.WPF.Editors
{
    public class StringPropertyEditor : WPFPropertyEditor
    {
        public StringPropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var propertyName = this.PropertyViewInfo.Name;

            var textbox = new TextBox()
            {
                Name = propertyName,
                Style = OEAStyles.StringPropertyEditor_TextBox
            };

            this.ResetBinding(textbox);

            //回车移动焦点c
            textbox.KeyDown += (o, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            };

            this.BindElementReadOnly(textbox, TextBox.IsReadOnlyProperty);

            this.SetAutomationElement(textbox);

            return textbox;
        }

        protected override void ResetBinding(FrameworkElement editingControl)
        {
            //绑定TextBox到对象属性
            var binding = this.CreateBinding();
            //可空类型需要指定空值时的显示值，否则可能会出现转换错误，如double?
            binding.TargetNullValue = string.Empty;
            editingControl.SetBinding(TextBox.TextProperty, this.CreateBinding());
        }

        #region 以下代码拷贝自：DataGridTextColumn 类。

        protected override void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            TextBox textBox = editingElement as TextBox;
            if (textBox == null) return;

            textBox.Focus();

            var args = editingEventArgs as TextCompositionEventArgs;
            if (args != null)
            {
                string str2 = args.Text;
                textBox.Text = str2;
                textBox.Select(str2.Length, 0);
                return;
            }

            if (!(editingEventArgs is MouseButtonEventArgs) || !PlaceCaretOnTextBox(textBox, Mouse.GetPosition(textBox)))
            {
                textBox.SelectAll();
            }
        }

        private static bool PlaceCaretOnTextBox(TextBox textBox, Point position)
        {
            int characterIndexFromPoint = textBox.GetCharacterIndexFromPoint(position, false);
            if (characterIndexFromPoint >= 0)
            {
                textBox.Select(characterIndexFromPoint, 0);
                return true;
            }
            return false;
        }

        #endregion
    }
}