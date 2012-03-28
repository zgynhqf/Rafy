/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100612
 * 说明：可以过滤SQL关键字的TextBox
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100612
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace OEA.Module.WPF.Controls
{
    public class SafeSqlTextBox : TextBox
    {
        static SafeSqlTextBox()
        {
            TextProperty.OverrideMetadata(typeof(SafeSqlTextBox),
                new FrameworkPropertyMetadata(
                    new PropertyChangedCallback(TextPropertyChangedCallBack),
                    new CoerceValueCallback(TextPropertyCoerceValueCallback)
                    )
                );
        }

        private static object TextPropertyCoerceValueCallback(DependencyObject d, object baseValue)
        {
            string value = baseValue.ToString();
            return value.Replace("'", "\"");
        }

        private static void TextPropertyChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
    }
}