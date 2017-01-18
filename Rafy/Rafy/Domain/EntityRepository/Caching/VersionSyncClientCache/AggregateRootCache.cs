/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101020
 * 说明：聚合根对象的内存缓存对象
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101020
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using Rafy.Utils.Caching;
using Rafy.MetaModel;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 聚合根对象的内存缓存对象
    /// </summary>
    internal class AggregateRootCache
    {
        internal static readonly AggregateRootCache Instance = new AggregateRootCache();

        private AggregateRootCache() { }

        private ICache Cache => Utils.Caching.Cache.Default;

        //public EntityList CacheAll(Type entityType, Func<EntityList> ifNotExists = null)
        //{
        //    EntityList result = null;

        //    //目前只是在客户端使用了缓存。
        //    if (RafyEnvironment.IsOnClient())
        //    {
        //        CacheScope sd = null;
        //        if (CacheDefinition.Instance.TryGetScope(entityType, out sd))
        //        {
        //            var className = entityType.Name;
        //            var key = "AggregateRootCache_CacheAll";
        //            result = this._cache.Get(key, className) as EntityList;

        //            if (result == null && ifNotExists != null)
        //            {
        //                result = ifNotExists();

        //                var checkers = new AggregateChecker();
        //                checkers.Add(new VersionChecker(entityType));

        //                var childrenTypes = new List<Type>();
        //                GetAggregateChildrenTypes(entityType, childrenTypes);

        //                for (int i = 0, c = childrenTypes.Count; i < c; i++)
        //                {
        //                    var childType = childrenTypes[i];
        //                    if (CacheDefinition.Instance.TryGetScope(childType, out sd))
        //                    {
        //                        if (sd.ScopeClass != entityType) throw new InvalidOperationException("此方法暂时只支持“所有的范围定义为根对象”！");
        //                        for (int j = 0, c2 = result.Count; j < c2; j++)
        //                        {
        //                            var entity = result[j];
        //                            var entityCheckers = new AggregateChecker();
        //                            checkers.Add(new VersionChecker(childType, sd.ScopeClass, entity.Id.ToString()));
        //                        }
        //                    }
        //                }

        //                var policy = new Policy()
        //                {
        //                    Checker = checkers
        //                };
        //                this._cache.Add(key, result, policy, className);
        //            }
        //        }
        //    }

        //    return result;
        //}

        /// <summary>
        /// 直接设置根对象为缓存
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="entity">The entity.</param>
        internal void ModifyRootEntity(IRepository repository, Entity entity)
        {
            if (entity == null) return;

            var sd = repository.EntityMeta.ClientCacheDefinition;
            if (sd != null)
            {
                var entityType = repository.EntityType;

                var entityId = entity.Id;
                var className = entityType.Name;
                var key = GetCacheKey(entityId);

                this.Cache.Remove(key, className);

                var policy = CreatePolicy(entityType, entityId);
                this.Cache.Add(key, entity, policy, className);
            }
        }

        /// <summary>
        /// 从缓存中读取指定实体类型的所有数据。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用 GetByIdCore 方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="id">The unique identifier.</param>
        /// <returns></returns>
        internal Entity CacheById(IRepository repository, object id)
        {
            Entity result = null;

            var sd = repository.EntityMeta.ClientCacheDefinition;
            if (sd != null)
            {
                var entityType = repository.EntityType;

                var className = entityType.Name;
                var key = GetCacheKey(id);

                //AggregateChecker 不支持序列化，所以这里只用内在缓存即可。
                result = this.Cache.Get(key, className) as Entity;

                if (result == null)
                {
                    result = repository.GetById(id);
                    if (result != null)
                    {
                        var policy = CreatePolicy(entityType, id);
                        this.Cache.Add(key, result, policy, className);
                    }
                }
            }

            return result;
        }

        private static string GetCacheKey(object id)
        {
            return "AggregateRootCache_CacheById_" + id;
        }

        private static Policy CreatePolicy(Type entityType, object id)
        {
            //根对象的检测条件由所有子对象的所有条件组合而成。
            var checkers = new AggregateChecker();
            checkers.Add(new VersionChecker(entityType));

            var childrenTypes = new List<Type>();
            GetAggregateChildrenTypes(entityType, childrenTypes);
            for (int i = 0, c = childrenTypes.Count; i < c; i++)
            {
                var childType = childrenTypes[i];

                var childScope = CommonModel.Entities.Get(childType).ClientCacheDefinition;
                if (childScope != null)
                {
                    if (childScope.ScopeClass != entityType) throw new InvalidOperationException("此方法暂时只支持“所有的范围定义为根对象”！");
                    checkers.Add(new VersionChecker(childType, childScope.ScopeClass, id.ToString()));
                }
            }

            var policy = new Policy() { Checker = checkers };
            return policy;
        }

        /// <summary>
        /// 递归获取指定类型下所有的子类型
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="types"></param>
        private static void GetAggregateChildrenTypes(Type entityType, IList<Type> types)
        {
            var entityInfo = CommonModel.Entities.Get(entityType);
            foreach (var child in entityInfo.ChildrenProperties)
            {
                var childType = child.ChildType.EntityType;

                types.Add(childType);

                GetAggregateChildrenTypes(childType, types);
            }
        }
    }
}
