/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：聚合实体的加载器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 聚合实体的加载器
    /// </summary>
    internal class AggregateEntityLoader
    {
        private AggregateDescriptor _aggregateInfo;

        internal AggregateEntityLoader(AggregateDescriptor aggregate)
        {
            if (aggregate == null) throw new ArgumentNullException("aggregate");
            if (aggregate.Items.Count < 1) throw new InvalidOperationException("aggregate.Items.Count < 1 must be false.");

            this._aggregateInfo = aggregate;
        }

        /// <summary>
        /// 通过聚合SQL加载整个聚合对象列表。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="sql">The SQL.</param>
        internal void Query(EntityList list, string sql)
        {
            IDataTable dataTable = null;

            var repo = this._aggregateInfo.Items.First.Value.OwnerRepository;
            using (var db = RdbDataProvider.Get(repo).CreateDbAccesser())
            {
                var table = db.RawAccesser.QueryDataTable(sql, CommandType.Text);

                dataTable = new RawTable(table);
            }

            //使用dataTable中的数据 和 AggregateDescriptor 中的描述信息，读取整个聚合列表。
            this.ReadFromTable(list, dataTable, this._aggregateInfo.Items.First);
        }

        /// <summary>
        /// 根据 optionNode 中的描述信息，读取 table 中的数据组装为对象列表并返回。
        /// 如果 optionNode 中指定要加载更多的子/引用对象，则会递归调用自己实现聚合加载。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="table">The table.</param>
        /// <param name="optionNode">The option node.</param>
        private void ReadFromTable(EntityList list, IDataTable table, LinkedListNode<LoadOptionItem> optionNode)
        {
            var option = optionNode.Value;
            AggregateEntityLoaderHelper.ReadFromTable(list, table, (entity, subTable) =>
            {
                EntityList listResult = null;

                //是否还有后继需要加载的对象？如果是，则递归调用自己进行子对象的加载。
                var nextNode = optionNode.Next;
                if (nextNode != null)
                {
                    listResult = nextNode.Value.OwnerRepository.NewList();
                    this.ReadFromTable(listResult, subTable, nextNode);
                }
                else
                {
                    listResult = option.PropertyEntityRepository.NewList();
                    AggregateEntityLoaderHelper.ReadFromTable(listResult, subTable, null);
                }

                //是否需要排序？
                if (listResult.Count > 1 && option.OrderBy != null)
                {
                    listResult = option.PropertyEntityRepository.NewListOrderBy(listResult, option.OrderBy);
                }

                //当前对象是加载类型的子对象还是引用的外键
                if (option.LoadType == AggregateLoadType.Children)
                {
                    listResult.SetParentEntity(entity);
                    entity.LoadProperty(option.PropertyMeta.ManagedProperty, listResult);
                }
                else
                {
                    if (listResult.Count > 0)
                    {
                        option.SetReferenceEntity(entity, listResult[0]);
                    }
                }
            });
        }

        private static class AggregateEntityLoaderHelper
        {
            /// <summary>
            /// 这个方法把table中的数据全部读取并转换为对象存入对象列表中。
            /// 
            /// 算法简介：
            /// 由于子对象的数据都是存储在这个IGTable中，所以每一个TEntity可能对应多个行，
            /// 每一行数据其实就是一个子对象的数据，而TEntity的属性值是重复的。
            /// 所以这里找到每个TEntity对应的第一行和最后一行，把它封装为一个子表格，传给子对象集合进行加载。
            /// 这样的设计是为了实现重用这个方法：集合加载IGTable中的数据。
            /// </summary>
            /// <param name="list">转换的对象存入这个列表中</param>
            /// <param name="table">表格数据，数据类型于以下形式：
            /// TableA  TableB  TableC  TableD...
            /// a1      b1      c1
            /// a1      b1      c2
            /// a2      b2      NULL
            /// a3      NULL    NULL
            /// ...</param>
            /// <param name="relationLoader">为每个TEntity调用此方法，从IGTable中加载它对应的孩子对象。
            /// 加载完成后的对象会被加入到list中，所以此方法有可能返回一个全新的TEntity。</param>
            public static void ReadFromTable(EntityList list, IDataTable table, Action<Entity, IDataTable> relationLoader)
            {
                var entityType = list.EntityType;
                var repo = RepositoryFactoryHost.Factory.FindByEntity(entityType);
                string idName = RdbDataProvider.Get(repo).SQLColumnsGenerator.GetReadableIdColumnSql();

                object lastId = null;
                //每个TEntity对象对应的第一行数据
                int startRow = 0;
                for (int i = 0, c = table.Count; i < c; i++)
                {
                    var row = table[i];

                    var objId = row[idName];
                    object id = objId != DBNull.Value ? objId : null;

                    //如果 id 改变，表示已经进入到下一个 TEntity 对象的开始行了。
                    if (id != lastId)
                    {
                        //不是第一次 或者 全是NULL值
                        if (lastId != null)
                        {
                            //前一行就是最后一行。
                            int endRow = i - 1;

                            Entity item = CreateEntity(entityType, table, startRow, endRow, relationLoader);

                            list.Add(item);
                        }

                        //重置 startRow 为下一个 TEntity
                        startRow = i;
                    }

                    lastId = id;
                }

                //加入最后一个 Entity
                if (lastId != null)
                {
                    Entity lastEntity = CreateEntity(entityType, table, startRow, table.Count - 1, relationLoader);
                    list.Add(lastEntity);
                }
            }

            /// <summary>
            /// 把 table 从 startRow 到 endRow 之间的数据，都转换为一个 TEntity 并返回。
            /// </summary>
            /// <param name="entityType">Type of the entity.</param>
            /// <param name="table">The table.</param>
            /// <param name="startRow">The start row.</param>
            /// <param name="endRow">The end row.</param>
            /// <param name="relationLoader">The relation loader.</param>
            /// <returns></returns>
            private static Entity CreateEntity(Type entityType, IDataTable table, int startRow, int endRow, Action<Entity, IDataTable> relationLoader)
            {
                //新的TEntity
                var repo = RepositoryFactoryHost.Factory.FindByEntity(entityType);
                var entity = RdbDataProvider.Get(repo).SQLColumnsGenerator.ReadDataDirectly(table[startRow]);
                if (entity == null)
                {
                    throw new InvalidProgramException("id不为空，对象也不应该为空。");
                }

                var childTable = new SubTable(table, startRow, endRow);
                if (relationLoader != null) { relationLoader(entity, childTable); }
                return entity;
            }
        }
    }
}