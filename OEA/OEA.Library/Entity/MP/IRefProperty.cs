/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Core;
using OEA.ManagedProperty;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 引用实体属性的 CSLA 静态属性标记
    /// </summary>
    public interface IRefProperty : IManagedProperty, IOEARefProperty
    {
        /// <summary>
        /// 引用实体的类型
        /// </summary>
        Type RefEntityType { get; }

        /// <summary>
        /// 创建某个实体的懒引用外键
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        ILazyEntityRef CreateRef(Entity owner);
    }
}
