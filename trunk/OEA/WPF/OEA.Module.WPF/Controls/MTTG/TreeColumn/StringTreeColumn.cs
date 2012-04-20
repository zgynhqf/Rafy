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
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 一般的文本列
    /// </summary>
    public class StringTreeColumn : TreeColumn
    {
        protected StringTreeColumn() { }

        protected override IWPFPropertyEditor CreateEditorCore(PropertyEditorFactory factory)
        {
            return factory.Create<StringPropertyEditor>(this.Meta);
        }

        //protected override FrameworkElement GenerateEditingElementCore()
        //{
        //    var control = base.GenerateEditingElementCore();

        //    if (!this.Editor.IsReadonly)
        //    {
        //        //当类库属性PropertyInfo.Name的set赋值过程更改了value时，需要强制将数据从绑定源属性传输到绑定目标属性。
        //        control.LostFocus += (s, e) =>
        //        {
        //            control.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        //        };
        //    }

        //    return control;
        //}

        public override void CommitEdit()
        {
            this.Editor.Control.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
