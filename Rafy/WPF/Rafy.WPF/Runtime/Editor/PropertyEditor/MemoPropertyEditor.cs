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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// “备注”属性的编辑器。
    /// </summary>
    public class MemoPropertyEditor : PropertyEditor
    {
        private TextBox _txt;
        private Panel _panel;

        protected MemoPropertyEditor() { }

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
            this._txt = new TextBox()
            {
                AcceptsReturn = true,
                Name = this.Meta.Name,
            };

            //绑定TextBox到对象属性
            this.ResetBinding(this._txt);
            this._panel.Children.Add(this._txt);

            this.AddReadOnlyComponent(this._txt, TextBox.IsReadOnlyProperty);

            this.SetAutomationElement(this._txt);

            return this._panel;
        }

        protected override void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            this._txt.Focus();
        }

        protected override DependencyProperty BindingProperty()
        {
            return TextBox.TextProperty;
        }

        private void btnPop_Click(object sender, RoutedEventArgs e)
        {
            this.OnButtonClick(e);
        }

        /// <summary>
        /// 点击按钮时，弹出一个比较大的TextBox窗口对某属性进行编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnButtonClick(RoutedEventArgs e)
        {
            var entity = this.Context.CurrentObject;
            if (entity == null) return;

            var isReadOnly = this.GetCurrentReadOnly();

            var memoTextBox = new TextBox()
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Text = (string)this.PropertyValue,
                IsReadOnly = isReadOnly
            };

            var result = App.Windows.ShowDialog(memoTextBox, w =>
            {
                w.ResizeMode = ResizeMode.CanResize;
                w.Title = "编辑".Translate() + " " + Meta.Label.Translate();
                w.Width = 600;
                w.Height = 400;
                if (isReadOnly)
                {
                    w.Buttons = ViewDialogButtons.None;
                }
            });

            if (result == WindowButton.Yes && !isReadOnly)
            {
                this.PropertyValue = memoTextBox.Text;
            }
        }
    }
}