﻿/*******************************************************
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
        /// 返回当前状态是否启用中。
        /// </summary>
        public abstract bool IsEnabled { get; }

        /// <summary>
        /// 使用Cache获取所有对象。
        /// </summary>
        /// <returns></returns>
        internal IEntityList FindAll()
        {
            IEntityList list = null;

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
        internal IEntityList FindByParent(Entity parent)
        {
            IEntityList children = null;

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

        internal abstract IEntityList GetCachedTable();

        internal abstract IEntityList GetCachedTableByParent(Entity parent);

        /// <summary>
        /// 把一个 table 转换为新的实体列表
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private IEntityList ConvertTable(IEntityList table)
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
                        entity = this.CloneRowData(row);
                        typeContext.Add(id, entity);
                    }

                    newList.Add(entity);
                }
            }
            else
            {
                for (int i = 0, c = table.Count; i < c; i++)
                {
                    var item = this.CloneRowData(table[i]);
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
                    entity = this.CloneRowData(row);
                    typeContext.Add(id, entity);
                }
            }
            else
            {
                entity = this.CloneRowData(row);
            }

            return entity;
        }

        /// <summary>
        /// 把一行数据转换为一个实体。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        private Entity CloneRowData(Entity row)
        {
            var entity = Entity.New(row.GetType());

            //返回的子对象的属性只是简单的完全Copy参数data的数据。
            var opt = CloneOptions.ReadDbRow();
            opt.Method = CloneValueMethod.LoadProperty;
            entity.Clone(row, opt);
            entity.PersistenceStatus = PersistenceStatus.Saved;

            (_repository as EntityRepository).SetRepo(entity);

            return entity;
        }
    }
}
