/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121225 14:27
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121225 14:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using Rafy.MetaModel;
using Rafy.Utils.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 基本版本号更新方案的客户端缓存的 API
    /// </summary>
    public class ClientRepositoryCache : RepositoryCache
    {
        private ClientCacheScope _clientCacheDefinition;

        private bool _clientCacheDefinitionLoaded;

        internal ClientRepositoryCache(IRepository repository) : base(repository) { }

        /// <summary>
        /// 本缓存对象对应的实体类型。
        /// </summary>
        public Type EntityType
        {
            get { return this._repository.EntityType; }
        }

        /// <summary>
        /// 对应的仓库对象。
        /// </summary>
        public IRepository Repository
        {
            get { return _repository; }
        }

        /// <summary>
        /// 使用的客户端缓存方案
        /// </summary>
        public ClientCacheScope ClientCacheDefinition
        {
            get
            {
                if (!this._clientCacheDefinitionLoaded)
                {
                    this._clientCacheDefinition = this._repository.EntityMeta.ClientCacheDefinition;
                    this._clientCacheDefinitionLoaded = true;
                }

                return this._clientCacheDefinition;
            }
        }

        /// <summary>
        /// 指定当前的仓库是否支持Cache
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return VersionSyncMgr.IsEnabled &&
                    this.ClientCacheDefinition != null &&
                    RafyEnvironment.IsOnClient();
            }
        }

        /// <summary>
        /// 直接设置根对象为缓存
        /// </summary>
        /// <param name="entity"></param>
        public void CacheRootEntity(Entity entity)
        {
            if (this.IsEnabled)
            {
                AggregateRootCache.Instance.ModifyRootEntity(this._repository, entity);
            }
        }

        /// <summary>
        /// 使用Cache获取某个指定的对象。
        /// 
        /// 注意：
        /// 根对象和子对象分别以不同的方式进行处理：
        /// 根对象：使用单个根对象的内存缓存。
        /// 子对象：在子对象的集合中查询。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal override Entity FindById(object id)
        {
            Entity result = null;

            if (this._repository.EntityMeta.EntityCategory == EntityCategory.Root &&
                this.ClientCacheDefinition.SimpleScopeType == ClientCacheScopeType.ScopedByRoot)
            {
                result = AggregateRootCache.Instance.CacheById(this._repository, id);
            }
            else
            {
                return base.FindById(id);
            }

            return result;
        }

        /// <summary>
        /// 在某实体更新时，通知服务器更新对象整张表的版本号。
        /// </summary>
        public void UpdateServerVersion()
        {
            if (VersionSyncMgr.IsEnabled && this.ClientCacheDefinition != null)
            {
                VersionSyncMgr.Repository.UpdateVersion(this.EntityType);
            }
        }

        /// <summary>
        /// 在某实体更新时，通知服务器更新指定范围内对象的版本号。
        /// </summary>
        public void UpdateServerVersion(Entity parent)
        {
            if (VersionSyncMgr.IsEnabled)
            {
                var scope = this.ClientCacheDefinition;
                if (scope != null)
                {
                    string scopeId = null;
                    if (scope.ScopeIdGetter != null)
                    {
                        if (parent == null) throw new InvalidOperationException("此列表没有父对象，调用 NotifyVersion 方法失败。");
                        scopeId = scope.ScopeIdGetter(parent);
                    }

                    VersionSyncMgr.Repository.UpdateVersion(this.EntityType, scope.ScopeClass, scopeId);
                }
            }
        }

        /// <summary>
        /// 即刻清空该实体类型在客户端中的所有缓存对象。
        /// 
        /// 使用场景：
        /// 在服务端的数据变化后，客户端缓存会定时更新，如果此时想用这个即时刷新客户端缓存，则可以调用此方法。
        /// 
        /// 注意，不要在服务端事务中调用此方法，这是因为其中会访问 SQLCE 数据库，这会导致事务提升为分布式事务。
        /// </summary>
        public void ClearOnClient()
        {
            if (this.IsEnabled)
            {
                //同时清空 Memory 及 Disk 中的缓存。
                var region = this.ClientCacheDefinition.Class.Name;
                this.Cache.ClearRegion(region);
            }
        }

        /// <summary>
        /// 从缓存中获取整个列表。
        /// 从缓存中读取指定实体类型的所有数据。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用 GetAll 方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <returns></returns>
        internal override IList<Entity> GetCachedTable()
        {
            IList<Entity> result = null;

            var sd = this.ClientCacheDefinition;
            if (sd != null)
            {
                var className = sd.Class.Name;
                var key = CacheAllKey;

                //如果内存不存在此数据，则尝试使用硬盘缓存获取
                result = this.Cache.Get(key, className) as IList<Entity>;
                if (result == null)
                {
                    result = this._repository.GetAll();

                    var policy = new Policy() { Checker = new VersionChecker(sd.Class) };
                    this.Cache.Add(key, result, policy, className);
                }
            }

            return result;
        }

        /// <summary>
        /// 从缓存中读取指定实体类型的某个父对象下的所有子对象。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用 GetByParentId 方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        internal override IList<Entity> GetCachedTableByParent(Entity parent)
        {
            IList<Entity> result = null;

            var sd = this.ClientCacheDefinition;
            if (sd != null)
            {
                var className = sd.Class.Name;
                var key = string.Format(CacheByParentKeyFormat, parent.Id);
                result = this.Cache.Get(key, className) as IList<Entity>;

                if (result == null)
                {
                    result = this._repository.GetByParentId(parent.Id);

                    VersionChecker checker = null;
                    if (sd.ScopeById)
                    {
                        var scopeId = sd.ScopeIdGetter(parent);
                        checker = new VersionChecker(sd.Class, sd.ScopeClass, scopeId);
                    }
                    else
                    {
                        checker = new VersionChecker(sd.Class);
                    }

                    var policy = new Policy() { Checker = checker };
                    this.Cache.Add(key, result, policy, className);
                }
            }

            return result;
        }
    }
}