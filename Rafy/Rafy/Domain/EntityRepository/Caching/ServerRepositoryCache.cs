/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130319 15:02
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130319 15:02
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Utils.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// 服务端使用的缓存。
    /// </summary>
    public class ServerRepositoryCache : RepositoryCache
    {
        private bool? _enabled;

        internal ServerRepositoryCache(IRepository repository) : base(repository) { }

        /// <summary>
        /// 是否已经被启用。
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                //需要使用懒加载来使用 _repository.EntityMeta 属性，否则会造成过早加载 EntityMeta 属性。
                if (this._enabled == null)
                {
                    this._enabled = this._repository.EntityMeta.ServerCacheEnabled;
                }
                return this._enabled.Value;
            }
            set
            {
                this._enabled = value;
            }
        }

        internal override IList<Entity> GetCachedTable()
        {
            var className = this._repository.EntityType.Name;

            return this.Cache.Get(CacheAllKey, () => this._repository.GetAll(), className);
        }

        internal override IList<Entity> GetCachedTableByParent(Entity parent)
        {
            var className = this._repository.EntityType.Name;
            var parentId = parent.Id;
            var key = string.Format(CacheByParentKeyFormat, parentId);

            return this.Cache.Get(key, () => this._repository.GetByParentId(parentId), className);
        }
    }
}
