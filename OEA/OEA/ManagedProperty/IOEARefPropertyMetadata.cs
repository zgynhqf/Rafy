/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// 引用实体属性的元数据
    /// </summary>
    public interface IOEARefPropertyMetadata
    {
        /// <summary>
        /// 该外键是否可空
        /// </summary>
        bool Nullable { get; }

        /// <summary>
        /// 对应的 Id 属性的名称
        /// </summary>
        string IdProperty { get; }

        /// <summary>
        /// 对应的引用实体属性的名称
        /// </summary>
        string RefEntityProperty { get; }

        /// <summary>
        /// 实体引用的类型
        /// </summary>
        ReferenceType ReferenceType { get; }
    }
}
