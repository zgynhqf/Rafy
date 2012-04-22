/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201101
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201001
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace OEA
{
    /// <summary>
    /// 属性编辑器上下文
    /// </summary>
    public interface IPropertyEditorContext
    {
        /// <summary>
        /// 当前“激活/显示/选中”的对象
        /// </summary>
        Entity CurrentObject { get; }

        /// <summary>
        /// 是否显示在列表视图中
        /// true 为列表，false 为详细视图
        /// </summary>
        bool IsForList { get; }
    }
}
