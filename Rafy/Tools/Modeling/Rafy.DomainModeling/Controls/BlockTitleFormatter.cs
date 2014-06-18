/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 19:43
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 19:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 一个默认的块状标题格式化器
    /// </summary>
    internal class BlockTitleFormatter : ITitleFormatter
    {
        public string Format(IModelingDesignerComponent component)
        {
            var block = component as BlockControl;
            var title = block.TypeName;

            //在设计器中检测，如果有命名冲突，则显示全类型名。
            var designer = ModelingDesigner.GetDesigner(component as DependencyObject);
            if (designer != null)
            {
                if (designer.Blocks.Any(a => a != block && a.TypeName == title))
                {
                    title = block.TypeFullName;
                }
            }

            //如果有 label，则附加显示 Label。
            var label = block.Label;
            if (!string.IsNullOrWhiteSpace(label))
            {
                title += "(" + label + ")";
            }

            return title;
        }
    }
}