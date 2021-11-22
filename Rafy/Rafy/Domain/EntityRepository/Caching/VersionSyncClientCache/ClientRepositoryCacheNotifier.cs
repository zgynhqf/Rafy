/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211122
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211122 18:20
 * 
*******************************************************/

using Rafy.MetaModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.Caching
{
    internal class ClientCacheNotifier
    {
        private IRepository _repository;

        public ClientCacheNotifier(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 本缓存对象对应的实体类型。
        /// </summary>
        public Type EntityType => _repository.EntityType;

        /// <summary>
        /// 使用的客户端缓存方案
        /// </summary>
        public ClientCacheScope ClientCacheDefinition => _repository.EntityMeta.ClientCacheDefinition;

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
    }
}
