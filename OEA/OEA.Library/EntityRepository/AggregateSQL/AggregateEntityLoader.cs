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

namespace OEA.Library
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
            if (aggregate.Items.Count < 1) throw new InvalidOperationException("aggregate.Items.Count < 2 must be false.");

            this._aggregateInfo = aggregate;
        }

        /// <summary>
        /// 通过聚合SQL加载整个聚合对象列表。
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        internal void Query(EntityList list, string sql)
        {
            IDataTable dataTable = null;

            IDbFactory dbFactory = this._aggregateInfo.Items.First.Value.OwnerRepository;
            using (var db = dbFactory.CreateDb())
            {
                var table = db.DBA.QueryDataTableNormal(sql, CommandType.Text);

                dataTable = new RawTable(table);
            }

            //使用dataTable中的数据 和 AggregateDescriptor 中的描述信息，读取整个聚合列表。
            this.ReadFromTable(list, dataTable, this._aggregateInfo.Items.First);
        }

        /// <summary>
        /// 根据 optionNode 中的描述信息，读取 table 中的数据组装为对象列表并返回。
        /// 
        /// 如果 optionNode 中指定要加载更多的子/引用对象，则会递归调用自己实现聚合加载。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="optionNode"></param>
        /// <returns></returns>
        private void ReadFromTable(EntityList list, IDataTable table, LinkedListNode<LoadOptionItem> optionNode)
        {
            var option = optionNode.Value;
            list.ReadFromTable(table, (entity, subTable) =>
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
                    listResult.ReadFromTable(subTable);
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
                    entity.LoadProperty(option.PropertyInfo.ManagedProperty, listResult);
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
    }
}