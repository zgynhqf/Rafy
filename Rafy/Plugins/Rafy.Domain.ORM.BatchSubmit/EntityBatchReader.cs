/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101028
 * 说明：根实体的所有孩子的读取器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101028
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM.BatchSubmit
{
    /// <summary>
    /// 根实体的所有孩子的读取器
    /// </summary>
    internal class EntityBatchReader
    {
        private IDomainComponent _entityOrList;

        private IList<EntityBatch> _batches;

        /// <summary>
        /// 为指定的根实体构造一个读取器
        /// </summary>
        /// <param name="entityOrList">The entity or list.</param>
        /// <exception cref="System.ArgumentNullException">entityOrList</exception>
        internal EntityBatchReader(IDomainComponent entityOrList)
        {
            if (entityOrList == null) throw new ArgumentNullException("entityOrList");

            this._entityOrList = entityOrList;
        }

        /// <summary>
        /// 执行读取操作，返回读取到的实体批处理列表。
        /// 
        /// 注意，本列表中实体类型的排序和聚合对象的父子关系顺序一致。即父实体在前，子实体在后。
        /// </summary>
        /// <returns></returns>
        internal IList<EntityBatch> Read()
        {
            _batches = new List<EntityBatch>();

            if (_entityOrList is Entity)
            {
                var batch = this.FindBatch(_entityOrList.GetType());
                ReadToBatchRecur(_entityOrList as Entity, batch);
            }
            else
            {
                var list = _entityOrList as EntityList;
                var batch = this.FindBatch(list.EntityType);
                ReadToBatchRecur(list, batch);
            }

            return _batches;
        }

        private void ReadToBatchRecur(EntityList entityList, EntityBatch batch)
        {
            var deletedList = entityList.DeletedListField;
            if (deletedList != null)
            {
                for (int i = 0, c = deletedList.Count; i < c; i++)
                {
                    var entity = deletedList[i];
                    this.ReadToBatchRecur(entity, batch);
                }
            }

            for (int i = 0, c = entityList.Count; i < c; i++)
            {
                var entity = entityList[i];
                this.ReadToBatchRecur(entity, batch);
            }
        }

        private void ReadToBatchRecur(Entity entity, EntityBatch batch)
        {
            switch (entity.PersistenceStatus)
            {
                case PersistenceStatus.Unchanged:
                    break;
                case PersistenceStatus.Modified:
                    batch.UpdateBatch.Add(entity);
                    break;
                case PersistenceStatus.New:
                    batch.InsertBatch.Add(entity);
                    break;
                case PersistenceStatus.Deleted:

                    batch.DeleteBatch.Add(entity);

                    //如果本类启用了假删除，那么它下面的所有实体都需要加载到内存中，这样在读取聚合时，它的聚合子也会读取到待删除列表中。
                    if (batch.Repository.EntityMeta.IsPhantomEnabled)
                    {
                        //不论这个列表属性是否已经加载，都必须获取其所有的数据行，并标记为删除。
                        batch.Repository.LoadAllChildren(entity);

                        foreach (var child in entity.GetLoadedChildren())
                        {
                            var list = child.Value as EntityList;
                            if (list != null && list.Count > 0) { list.Clear(); }
                        }
                    }
                    break;
                default:
                    break;
            }

            this.ReadChildrenRecur(entity, batch);
        }

        /// <summary>
        /// 递归读取指定父对象中的所有子对象
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="batch">The batch.</param>
        private void ReadChildrenRecur(Entity entity, EntityBatch batch)
        {
            //遍历所有子属性，读取孩子列表
            foreach (var child in entity.GetLoadedChildren())
            {
                var children = child.Value as EntityList;
                //所有孩子列表中的实体，都加入到对应的实体列表中。
                //并递归读取孩子的孩子实体。
                var childBatch = this.FindBatch(children.EntityType);
                ReadToBatchRecur(children, childBatch);
            }
        }

        /// <summary>
        /// 找到对应实体的全部对象列表
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private EntityBatch FindBatch(Type entityType)
        {
            for (int i = 0, c = _batches.Count; i < c; i++)
            {
                var batch = _batches[i];
                if (batch.EntityType == entityType)
                {
                    return batch;
                }
            }

            var newBatch = new EntityBatch
            {
                EntityType = entityType,
                Repository = RF.Find(entityType)
            };
            _batches.Add(newBatch);

            return newBatch;
        }
    }
}