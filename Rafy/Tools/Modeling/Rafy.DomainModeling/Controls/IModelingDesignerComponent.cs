/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130410 14:58
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130410 14:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// BlockControl、BlockRelation 都实现这个接口。
    /// </summary>
    public interface IModelingDesignerComponent
    {
        /// <summary>
        /// 用于底层显示的图形控件。
        /// </summary>
        Control EngineControl { get; }

        /// <summary>
        /// 组件的类型
        /// </summary>
        DesignerComponentKind Kind { get; }
    }

    /// <summary>
    /// 设计器元素类型。
    /// </summary>
    public enum DesignerComponentKind
    {
        /// <summary>
        /// 元素块
        /// </summary>
        Block,
        /// <summary>
        /// 关系连线。
        /// </summary>
        Relation
    }
}
