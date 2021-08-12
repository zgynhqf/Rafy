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
    }
}
