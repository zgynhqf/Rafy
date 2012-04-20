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
using OEA.Editors;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// “备注”属性的编辑器。
    /// </summary>
    public class MemoPropertyEditor : WPFPropertyEditor
    {
        private TextBox _tb;
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
            this._tb = new TextBox()
            {
                AcceptsReturn = true,
                Name = this.Meta.Name,
            };

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
            if (this.Context.CurrentObject == null) return;

            TextBox edtInfo = new TextBox()
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Text = (string)this.PropertyValue,
                IsReadOnly = this.IsReadonly
            };

            var result = App.Windows.ShowDialog(edtInfo, w =>
            {
                w.ResizeMode = ResizeMode.CanResize;
                w.Title = "编辑" + Meta.Label;
                w.Width = 600;
                w.Height = 400;
                if (this.IsReadonly)
                {
                    w.Buttons = ViewDialogButtons.None;
                }
            });

            if (result == WindowButton.Yes && !this.IsReadonly)
            {
                PropertyValue = edtInfo.Text;
            }
        }
    }
}