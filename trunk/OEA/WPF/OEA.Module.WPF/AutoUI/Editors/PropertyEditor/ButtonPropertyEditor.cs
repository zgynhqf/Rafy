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
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using AvalonDock;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 含有Button的属性编辑器
    /// 
    /// 子类重写OnButtonClick事件即可直接写具体事件代码。
    /// </summary>
    public abstract class ButtonPropertyEditor : WPFPropertyEditor
    {
        private TextBox _tb;
        private Panel _panel;

        protected ButtonPropertyEditor() { }

        /// <summary>
        /// 创建一个DockPanel
        /// 其中包含了一个Button和一个TextBox
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement CreateEditingElement()
        {
            this._panel = new DockPanel();

            //create a button
            Button btnPop = new Button() { Content = "..." };
            btnPop.SetValue(DockPanel.DockProperty, Dock.Right);
            btnPop.Click += new RoutedEventHandler(btnPop_Click);
            this._panel.Children.Add(btnPop);

            //create a textbox
            this._tb = new TextBox()
            {
                Name = this.PropertyViewInfo.Name
            };
            this._tb.MaxHeight = this._tb.FontSize * 1.6;

            //绑定TextBox到对象属性
            this.ResetBinding(this._tb);
            this._panel.Children.Add(this._tb);

            this.BindElementReadOnly(this._tb, TextBox.IsReadOnlyProperty);

            this.SetAutomationElement(this._tb);

            return this._panel;
        }

        protected override void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            base.PrepareElementForEditCore(editingElement, editingEventArgs);

            this._tb.Focus();
        }

        protected override void ResetBinding(FrameworkElement editingControl)
        {
            //绑定TextBox到对象属性
            editingControl.SetBinding(TextBox.TextProperty, this.CreateBinding());
        }

        /// <summary>
        /// 点击按钮后发生的事件。
        /// 子类实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnButtonClick(RoutedEventArgs e) { }

        private void btnPop_Click(object sender, RoutedEventArgs e)
        {
            this.OnButtonClick(e);
        }
    }
}