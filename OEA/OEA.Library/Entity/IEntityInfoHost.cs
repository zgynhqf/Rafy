using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.ManagedProperty;

namespace OEA.Library
{
    /// <summary>
    /// EntityInfo的驻留器
    /// </summary>
    public interface IEntityInfoHost
    {
        /// <summary>
        /// 实体属性容器
        /// </summary>
        ConsolidatedTypePropertiesContainer PropertiesContainer { get; }

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
        /// 找到本对象上层父聚合对象的外键
        /// </summary>
        /// <param name="throwOnNotFound">如果没有找到，是否需要抛出异常。</param>
        /// <returns></returns>
        EntityPropertyMeta FindParentPropertyInfo(bool throwOnNotFound);
    }
}
