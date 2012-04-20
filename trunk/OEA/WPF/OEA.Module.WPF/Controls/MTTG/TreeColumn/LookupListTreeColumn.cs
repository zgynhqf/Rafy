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
    /// 下拉列
    /// </summary>
    public class LookupListTreeColumn : TreeColumn
    {
        protected LookupListTreeColumn() { }

        protected override IWPFPropertyEditor CreateEditorCore(PropertyEditorFactory factory)
        {
            return factory.Create<LookupListPropertyEditor>(this.Meta);
        }

        protected override FrameworkElement GenerateEditingElementCore()
        {
            var combo = base.GenerateEditingElementCore().CastTo<ComboBox>();

            //表格中下拉控件在生成时，立刻被打开。
            combo.IsDropDownOpen = true;

            return combo;
        }

        protected override FrameworkElementFactory GenerateDisplayTemplateInCell()
        {
            var refInfo = this.Meta.ReferenceViewInfo;

            //如果是可输入下拉列表,非编辑状态下是绑定到自身属性上
            if (!string.IsNullOrEmpty(refInfo.RefEntityProperty))
            {
                var textBlock = new FrameworkElementFactory(typeof(TextBlock));
                var binding = new Binding(this.Meta.BindingPath());
                textBlock.SetBinding(TextBlock.TextProperty, binding);
                return textBlock;
            }

            return base.GenerateDisplayTemplateInCell();
        }
    }
}
