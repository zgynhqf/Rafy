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
using SimpleCsla.Wpf;
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
    public class MemoPropertyEditor : ButtonPropertyEditor
    {
        protected MemoPropertyEditor() { }

        /// <summary>
        /// 点击按钮时，弹出一个比较大的TextBox窗口对某属性进行编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnButtonClick(RoutedEventArgs e)
        {
            if (this.Context.CurrentObject == null) return;

            TextBox edtInfo = new TextBox()
            {
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap
            };
            edtInfo.Text = (string)this.PropertyValue;
            edtInfo.IsReadOnly = this.IsReadonly;

            var result = App.Current.Windows.ShowDialog(edtInfo, w =>
            {
                w.ResizeMode = ResizeMode.CanResize;
                w.Title = "编辑" + PropertyViewInfo.Label;
                w.Width = 600;
                w.Height = 400;
                if (this.IsReadonly)
                {
                    w.Buttons = ViewDialogButtons.None;
                }
            });

            if (result == WindowButton.Yes && !IsReadonly)
            {
                PropertyValue = edtInfo.Text;
            }
        }
    }
}