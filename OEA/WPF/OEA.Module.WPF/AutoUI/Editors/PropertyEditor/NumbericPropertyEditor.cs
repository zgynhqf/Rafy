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
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Automation;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 使用文本框进行录入的数字编辑器
    /// </summary>
    public class NumbericPropertyEditor : StringPropertyEditor
    {
        protected NumbericPropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var textbox = base.CreateEditingElement() as TextBox;

            textbox.KeyDown += On_Textbox_KeyDown;

            return textbox;
        }

        /// <summary>
        /// 在 KeyDown 事件中限制用户输出非数据的按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            //如果按数据键，不需要处理
            var key = e.Key;
            if (key >= Key.D0 && key <= Key.D9 ||
                key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                return;
            }

            //如果按下 '.' 键，则只在第一次有效。
            if (key == Key.OemPeriod || key == Key.Decimal)
            {
                var t = sender as TextBox;
                if (!t.Text.Contains('.')) return;
            }

            e.Handled = true;
        }
    }
}