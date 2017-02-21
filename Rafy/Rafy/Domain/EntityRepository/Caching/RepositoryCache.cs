/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130319 15:05
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130319 15:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM;
using Rafy.Utils.Caching;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// ServerCache，ClientCache 的基类。
    /// </summary>
    public abstract class RepositoryCache
    {
        protected const string CacheAllKey = "All";

        protected const string CacheByParentKeyFormat = "ByParentId_{0}";

        internal IRepository _repository;

        internal RepositoryCache(IRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// 获取或设置当前仓库使用的缓存对象。
        /// </summary>
        public ICache Cache { get; set; } = Utils.Caching.Cache.Default;

        /// <summary>
        /// 使用Cache获取所有对象。
        /// </summary>
        /// <returns></returns>
        internal EntityList FindAll()
        {
            EntityList list = null;

            var table = this.GetCachedTable();
            if (table != null)
            {
                list = this.ConvertTable(table);
            }

            return list;
        }

        /// <summary>
        /// 使用Cache获取某个父对象下的所有子对象。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        internal EntityList FindByParent(Entity parent)
        {
            EntityList children = null;

            var smallTable = this.GetCachedTableByParent(parent);
            if (smallTable != null)
            {
                children = this.ConvertTable(smallTable);
            }

            return children;
        }

        /// <summary>
        /// 使用Cache获取某个指定的对象。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal virtual Entity FindById(object id)
        {
            var table = this.GetCachedTable();
            for (int i = 0, c = table.Count; i < c; i++)
            {
                var row = table[i];
                if (row.Id.Equals(id))
                {
                    var result = this.ConvertRow(row);
                    return result;
                }
            }

            return null;
        }

        internal abstract IList<Entity> GetCachedTable();

        internal abstract IList<Entity> GetCachedTableByParent(Entity parent);

        /// <summary>
        /// 把一个 table 转换为新的实体列表
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private EntityList ConvertTable(IList<Entity> table)
        {
            var converter = this._repository as IRepositoryInternal;

            var newList = this._repository.NewList();

            //在把行转换为对象时，如果已经使用了 EntityContext，则尽量尝试使用 EntityContext 中的对象。
            var current = EntityContext.Current;
            if (current != null)
            {
                var typeContext = current.GetOrCreateTypeContext(this._repository.EntityType);
                for (int i = 0, c = table.Count; i < c; i++)
                {
                    var row = table[i];
                    Entity entity = null;

                    var id = row.Id;
                    var item = typeContext.TryGetById(id) as Entity;
                    if (item != null)
                    {
                        entity = item;
                    }
                    else
                    {
                        entity = converter.ConvertRow(row);
                        typeContext.Add(id, entity);
                    }

                    newList.Add(entity);
                }
            }
            else
            {
                for (int i = 0, c = table.Count; i < c; i++)
                {
                    var item = converter.ConvertRow(table[i]);
                    newList.Add(item);
                }
            }

            return newList;
        }

        private Entity ConvertRow(Entity row)
        {
            Entity entity = null;

            var converter = this._repository as IRepositoryInternal;

            //在把行转换为对象时，如果已经使用了 EntityContext，则尽量尝试使用 EntityContext 中的对象。
            var current = EntityContext.Current;
            if (current != null)
            {
                var typeContext = current.GetOrCreateTypeContext(this._repository.EntityType);

                var id = row.Id;
                var item = typeContext.TryGetById(id) as Entity;
                if (item != null)
                {
                    entity = item;
                }
                else
                {
                    entity = converter.ConvertRow(row);
                    typeContext.Add(id, entity);
                }
            }
            else
            {
                entity = converter.ConvertRow(row);
            }

            return entity;
        }
    }
}
