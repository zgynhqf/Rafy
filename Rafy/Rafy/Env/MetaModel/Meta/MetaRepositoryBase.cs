using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Rafy.MetaModel.Attributes;
using Rafy.ManagedProperty;
using System.Reflection;

namespace Rafy.MetaModel
{
    public class MetaRepositoryBase<TMeta> : IEnumerable<TMeta>
        where TMeta : MetaBase
    {
        private bool _froozen = false;

        private List<TMeta> _allPrimes = new List<TMeta>();

        protected void AddPrime(TMeta meta)
        {
            lock (this._allPrimes) this._allPrimes.Add(meta);

            //如果在冻结后再添加的元素，需要被冻结。
            if (this._froozen) { meta.Freeze(); }
        }

        /// <summary>
        /// 冻结所有的命令元数据。
        /// 
        /// 只冻结内部元素，并不冻结集合的行为。
        /// </summary>
        internal void FreezeItems()
        {
            foreach (var v in this._allPrimes) v.Freeze();
            this._froozen = true;
        }

        internal List<TMeta> GetCurrentInnerList()
        {
            return this._allPrimes;
        }

        #region IEnumerable<TMeta>

        IEnumerator<TMeta> IEnumerable<TMeta>.GetEnumerator()
        {
            return this._allPrimes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._allPrimes.GetEnumerator();
        }

        #endregion
    }

    internal static class EntityMetaHelper
    {
        /// <summary>
        /// 通过托管属性获取所有的实体属性列表
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <returns></returns>
        internal static IList<IManagedProperty> GetEntityProperties(EntityMeta entityMeta)
        {
            return entityMeta.ManagedProperties.GetCompiledProperties()
                .Where(mp => !(mp is IListProperty))
                .ToArray();
        }

        /// <summary>
        /// 通过托管属性获取所有的子属性列表
        /// </summary>
        /// <param name="entityMeta"></param>
        /// <returns></returns>
        internal static IList<IManagedProperty> GetChildrenProperties(EntityMeta entityMeta)
        {
            return entityMeta.ManagedProperties.GetCompiledProperties()
                .Where(mp => mp is IListProperty)
                .ToArray();
        }
    }
}