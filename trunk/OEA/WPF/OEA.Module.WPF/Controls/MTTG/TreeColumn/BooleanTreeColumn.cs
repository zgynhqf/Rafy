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
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using OEA.MetaModel;
using OEA.MetaModel.View;


namespace OEA.Module.WPF.Editors
{
    public class BooleanTreeColumn : TreeColumn
    {
        protected BooleanTreeColumn() { }

        protected override IWPFPropertyEditor CreateEditorCore(PropertyEditorFactory factory)
        {
            return factory.Create<BooleanPropertyEditor>(this.PropertyInfo);
        }

        protected override FrameworkElementFactory GenerateDisplayTemplateInCell()
        {
            var cb = new FrameworkElementFactory(typeof(CheckBox));

            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding(this.PropertyInfo.Name));

            return cb;
        }
    }
}