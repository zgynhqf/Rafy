/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120412
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120412
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    /// <summary>
    /// OEA 列表属性
    /// </summary>
    public interface IListProperty : IManagedProperty
    {
        /// <summary>
        /// 列表对应的实体类型
        /// </summary>
        Type ListEntityType { get; }

        /// <summary>
        /// 为某个对象获取本属性的元数据
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        new IOEAListPropertyMetadata GetMeta(object owner);

        /// <summary>
        /// 为某个类型获取本属性的元数据
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        new IOEAListPropertyMetadata GetMeta(Type ownerType);
    }
}