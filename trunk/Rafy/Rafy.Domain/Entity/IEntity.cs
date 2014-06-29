using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Rafy.MetaModel;
using System.Collections;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体
    /// </summary>
    public interface IEntity : IDomainComponent, IRafyEntity { }

    /// <summary>
    /// 实体列表
    /// </summary>
    public interface IEntityList : IList<Entity>, IList, IDomainComponent
    {
        #region 解决 IList<Entity> 与 IList 的冲突

        new int Count { get; }

        new Entity this[int index] { get; set; }

        #endregion
    }
}