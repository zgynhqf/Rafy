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
    /// 特定模板列
    /// </summary>
    public class TemplateTreeGridColumn : TreeGridColumn
    {
        public DataTemplate EditingTemplate { get; set; }

        protected override FrameworkElement GenerateEditingElementCore()
        {
            var control = new ContentPresenter
            {
                ContentTemplate = this.EditingTemplate
            };

            control.SetBinding(ContentPresenter.ContentProperty, this.Binding);

            return control;
        }

        protected internal override void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            //do nothing
        }
    }
}
