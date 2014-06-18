/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121010 17:27
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121010 17:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 文本列
    /// </summary>
    public class TextTreeGridColumn : TreeGridColumn
    {
        protected override FrameworkElement GenerateEditingElementCore()
        {
            var txt = new TextBox();

            txt.SetBinding(TextBox.TextProperty, this.Binding);

            return txt;
        }

        protected internal override void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            //do nothing
        }
    }
}
