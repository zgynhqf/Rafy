/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121120 19:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121120 19:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 引用 Id 属性或者是引用实体属性。
    /// 两类属性，都可以转换为此接口，方便统一使用方法。
    /// </summary>
    public interface IRefProperty : IManagedProperty
    {
        /// <summary>
        /// 引用实体的类型
        /// </summary>
        Type RefEntityType { get; }

        /// <summary>
        /// 实体引用的类型
        /// </summary>
        ReferenceType ReferenceType { get; }

        /// <summary>
        /// 该引用属性是否可空。
        /// 如果引用Id属性的类型是引用类型（字符串）或者是一个 Nullable 类型，则这个属性返回 true。
        /// </summary>
        bool Nullable { get; }

        /// <summary>
        /// 返回对应的引用 Id 属性。
        /// </summary>
        IRefIdProperty RefIdProperty { get; }

        /// <summary>
        /// 返回对应的引用实体属性。
        /// </summary>
        IRefEntityProperty RefEntityProperty { get; }
    }

    /// <summary>
    /// 引用实体属性的静态属性 Id 标记
    /// </summary>
    public interface IRefIdProperty : IRefProperty
    {
        /// <summary>
        /// 引用的实体的主键的算法程序。
        /// </summary>
        IKeyProvider KeyProvider { get; }
    }

    /// <summary>
    /// 引用实体属性的静态属性实体标记
    /// </summary>
    public interface IRefEntityProperty : IRefProperty { }
}