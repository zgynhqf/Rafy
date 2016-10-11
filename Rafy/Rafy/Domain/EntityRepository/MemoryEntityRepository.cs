/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 本地实体的仓库基类。
    /// 
    /// 提供了为本地实体生成 Id 的功能。
    /// 本仓库会把所有的生成的实体的数据都存储在内存中。
    /// （
    /// 不用存储到数据库，只存在于内存中。
    /// 不论是客户端还是服务端，在哪调用仓库的查询接口，就存在哪的内存中。
    /// 在内存中，只存在实体的数据，在查询时，会把这些数据转换为实体。
    /// ）
    /// </summary>
    public abstract class MemoryEntityRepository : EntityRepository
    {
        private new MemoryRepositoryDataProvider DataProvider
        {
            get { return base.DataProvider as MemoryRepositoryDataProvider; }
        }

        /// <summary>
        /// 获取给定实体的真实键。
        /// 
        /// 由于这些实体的 Id 是自动生成的，所以子类需要提供真实的字符串类型的主键。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract string GetRealKey(Entity entity);

        protected sealed override long DoCountAll()
        {
            DataProvider.EnsureStore();
            return DataProvider._memoryRows.Count;
        }

        protected sealed override EntityList DoGetAll(PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            if (!PagingInfo.IsNullOrEmpty(paging)) { throw new NotSupportedException(); }

            DataProvider.EnsureStore();
            var items = DataProvider._memoryRows.Values.Select(v => DataProvider.FromRow(v));
            var list = this.CreateList(items);
            TreeHelper.MarkTreeFullLoaded(list);

            return list;
        }

        /// <summary>
        /// 通过本地 Id 查找实体。
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eagerLoad">需要贪婪加载的属性。</param>
        /// <returns></returns>
        protected sealed override Entity DoGetById(object id, EagerLoadOptions eagerLoad)
        {
            DataProvider.EnsureStore();

            var row = DataProvider._memoryRows.Values.FirstOrDefault(e => e.Id.Equals(id));
            if (row == null) return null;
            //if (row == null)
            //{
            //    //如果还没有加载，则主动加载所有实体，然后再找到其中的那个。
            //    row = this.GetAll().FirstOrDefault(e => e.Id.Equals(id));
            //    if (row == null) { throw new InvalidOperationException("没有找到对应 id 的实体，可能是还没有通过仓库获取该实体。"); }
            //}

            return DataProvider.FromRow(row);
        }

        internal override void NotifyLoadedIfMemory(Entity entity)
        {
            base.NotifyLoadedIfMemory(entity);

            (this.DataProvider as MemoryRepositoryDataProvider).Saver.InsertInternal(entity);
        }

        public abstract class MemoryRepositoryDataProvider : RepositoryDataProvider
        {
            public MemoryRepositoryDataProvider()
            {
                this.Saver = new MemorySaver();

                this.DataSaver = this.Saver;
            }

            private MemoryEntityRepository _Repository
            {
                get { return base.Repository as MemoryEntityRepository; }
            }

            internal MemorySaver Saver { get; private set; }

            /// <summary>
            /// 当前已经生成 Id
            /// </summary>
            internal Dictionary<string, Entity> _memoryRows;

            /// <summary>
            /// 清除本地内存数据库
            /// </summary>
            public void Clear()
            {
                this.ClearCore();
            }

            protected virtual void ClearCore()
            {
                _memoryRows = null;
            }

            /// <summary>
            /// 通过真实的键查找目标实体。
            /// </summary>
            /// <param name="realKey"></param>
            /// <returns></returns>
            protected Entity FindByRealKey(string realKey)
            {
                EnsureStore();

                Entity res = null;
                _memoryRows.TryGetValue(realKey, out res);
                return res;
            }

            private string GetRealKey(Entity entity)
            {
                return _Repository.GetRealKey(entity);
            }

            internal Entity FromRow(Entity row)
            {
                var entity = Entity.New(row.GetType());

                this.MemoryClone(row, entity);

                entity.PersistenceStatus = PersistenceStatus.Unchanged;

                return entity;

                //return row;
            }

            internal Entity ToRow(Entity item)
            {
                //暂时实现一致。
                return FromRow(item);
            }

            internal void EnsureStore()
            {
                if (_memoryRows == null)
                {
                    _memoryRows = new Dictionary<string, Entity>();

                    var items = this.LoadAll();

                    foreach (var item in items)
                    {
                        this.Saver.InsertInternal(item);
                    }
                }
            }

            /// <summary>
            /// 子类重写此方法来实现一次性加载所有对象到内存中的逻辑。
            /// </summary>
            /// <returns></returns>
            protected abstract IEnumerable<Entity> LoadAll();

            public override LiteDataTable GetEntityValue(object entityId, string property)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 从指定的对象中拷贝所有数据到另一对象中。
            /// 
            /// 默认实体只拷贝所有数据属性。
            /// 子类可重写此方法来拷贝更多一般字段。
            /// </summary>
            /// <param name="src">数据源对象。</param>
            /// <param name="dst">目标对象。</param>
            protected virtual void MemoryClone(Entity src, Entity dst)
            {
                //同时由于以下代码只是对托管属性进行了拷贝，会导致一些一般字段无法复制。（参见 Rafy.RBAC.Old.ModuleAC 实体类型。）

                //返回的子对象的属性只是简单的完全Copy参数data的数据。
                var opt = CloneOptions.ReadDbRow();
                opt.Method = CloneValueMethod.LoadProperty;
                dst.Clone(src, opt);
            }

            public class MemorySaver : DataSaver
            {
                private MemoryRepositoryDataProvider _dp;

                protected internal override void Init(RepositoryDataProvider dataProvider)
                {
                    base.Init(dataProvider);

                    _dp = dataProvider as MemoryRepositoryDataProvider;
                }

                #region 重写数据访问方法

                internal protected override void Submit(SubmitArgs e)
                {
                    _dp.EnsureStore();

                    base.Submit(e);
                }

                internal void InsertInternal(Entity entity)
                {
                    _dp.Insert(entity);
                }

                public override void InsertToPersistence(Entity entity)
                {
                    _dp.EnsureStore();

                    string key = _dp.GetRealKey(entity);

                    //在生成 Id 时，为某个模型生成临时使用的本地 Id。
                    var idProvider = (entity as IEntityWithId).IdProvider;
                    if (!idProvider.IsAvailable(entity.Id))
                    {
                        var found = _dp.FindByRealKey(key);
                        if (found != null)
                        {
                            //如果这个实体已经存在于内存中，则更新新对象的 Id，并在最后把新对象的数据存储起来即可。
                            entity.LoadProperty(Entity.IdProperty, found.Id);
                        }
                        else
                        {
                            var newId = idProvider.NewLocalValue();
                            entity.LoadProperty(Entity.IdProperty, newId);
                        }
                    }

                    //有些实体并不一定通过 CDU 接口保存到 _memoryRows 中，而是在查询时临时生成。
                    //这时，也需要把这些实体都加入到 _memoryRows 中。
                    _dp._memoryRows[key] = _dp.ToRow(entity);
                }

                public override void UpdateToPersistence(Entity entity)
                {
                    entity = _dp.ToRow(entity);
                    _dp._memoryRows[_dp.GetRealKey(entity)] = entity;
                }

                public override void DeleteFromPersistence(Entity entity)
                {
                    _dp._memoryRows.Remove(_dp.GetRealKey(entity));
                }

                //…… 其它查询方法的重写暂缓 

                #endregion

                /// <summary>
                /// 未实现
                /// </summary>
                /// <returns></returns>
                public override RedundanciesUpdater CreateRedundanciesUpdater()
                {
                    throw new NotImplementedException();
                }

                protected override void DeleteRefCore(Entity entity, IRefProperty refProperty)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}