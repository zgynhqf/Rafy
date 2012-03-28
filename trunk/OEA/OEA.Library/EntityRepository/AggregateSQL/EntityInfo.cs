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
    interface IEntityInfoHost
    {
        /// <summary>
        /// 获取所有的CSLA属性信息
        /// </summary>
        /// <returns></returns>
        IList<IManagedProperty> GetAvailableIndicators();

        EntityMeta EntityMeta { get; }

        /// <summary>
        /// 找到本对象上层父聚合对象的外键
        /// </summary>
        /// <returns></returns>
        EntityPropertyMeta GetParentPropertyInfo();
    }
}
