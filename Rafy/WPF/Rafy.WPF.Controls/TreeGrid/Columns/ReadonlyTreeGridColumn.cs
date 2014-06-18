/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121101 15:42
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121101 15:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 只读列
    /// </summary>
    public class ReadonlyTreeGridColumn : TreeGridColumn
    {
        protected override bool CanEnterEditing(object dataItem)
        {
            return false;
        }

        protected override System.Windows.FrameworkElement GenerateEditingElementCore()
        {
            throw new NotImplementedException();
        }

        protected internal override void PrepareElementForEdit(System.Windows.FrameworkElement editingElement, System.Windows.RoutedEventArgs editingEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}
