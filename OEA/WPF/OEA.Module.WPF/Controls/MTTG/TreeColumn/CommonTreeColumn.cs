/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110713
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201101
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
{
    class CommonTreeColumn : TreeColumn
    {
        protected override IWPFPropertyEditor CreateEditorCore(PropertyEditorFactory factory)
        {
            return factory.Create(this.Meta);
        }

        protected override Binding GenerateBindingFormat(string name, string stringformat)
        {
            //使用 PropertyEditor 来生成 Binding 的原因是：
            //如果是下拉框、则不能直接使用默认的绑定方案。
            var binding = (this.Editor as WPFPropertyEditor).CreateBindingInternal();

            binding.StringFormat = stringformat;

            return binding;
        }
    }

    class ReadonlyTreeColumn : TreeColumn
    {
        protected override bool TryCheckIsReadonly(object dataItem)
        {
            return true;
        }
    }
}
