using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// EntityInfo的驻留器
    /// </summary>
    public interface IEntityInfoHost
    {
        /// <summary>
        /// 实体元数据
        /// </summary>
        EntityMeta EntityMeta { get; }

        /// <summary>
        /// 所有本实体中所有声明的冗余属性。
        /// </summary>
        /// <returns></returns>
        IList<IProperty> GetPropertiesInRedundancyPath();

        /// <summary>
        /// 所有本实体中所有声明的子属性。
        /// 
        /// 每一个子属性值可能是一个列表，也可能是一个单一实体。
        /// </summary>
        /// <returns></returns>
        IList<IProperty> GetChildProperties();

        /// <summary>
        /// 找到本对象上层聚合父实体的实体引用属性元数据。
        /// 
        /// 注意，此函数返回的是引用实体属性，而非引用 Id 属性。
        /// </summary>
        /// <param name="throwOnNotFound">如果没有找到，是否需要抛出异常。</param>
        /// <returns></returns>
        EntityPropertyMeta FindParentPropertyInfo(bool throwOnNotFound);
    }
}
