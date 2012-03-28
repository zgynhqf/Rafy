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

namespace OEA.Module.WPF.Editors
{
    class CommonTreeColumn : TreeColumn
    {
        protected override IWPFPropertyEditor CreateEditorCore(PropertyEditorFactory factory)
        {
            return factory.Create(this.PropertyInfo);
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
