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
        /// 获取所有的静态的托管属性标记器。
        /// </summary>
        /// <returns></returns>
        IList<IManagedProperty> GetAvailableIndicators();

        /// <summary>
        /// 实体元数据
        /// </summary>
        EntityMeta EntityMeta { get; }

        /// <summary>
        /// 找到本对象上层父聚合对象的外键
        /// </summary>
        /// <param name="throwOnNotFound">如果没有找到，是否需要抛出异常。</param>
        /// <returns></returns>
        EntityPropertyMeta FindParentPropertyInfo(bool throwOnNotFound);
    }
}
