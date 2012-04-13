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

using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 根实体的所有孩子的读取器
    /// </summary>
    internal class EntityChldrenBatchReader
    {
        private Entity _entity;

        private IDictionary<Type, IList<Entity>> _allChildren;

        /// <summary>
        /// 为指定的根实体构造一个读取器
        /// </summary>
        /// <param name="entity"></param>
        internal EntityChldrenBatchReader(Entity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            this._entity = entity;
        }

        /// <summary>
        /// 执行读取操作，返回读取到的实体字典。
        /// 
        /// 实体字典：
        /// Key：实体的类型
        /// Value：对应实体类型的所有实体。
        /// 
        /// 注意，本字典中Key的类型排序和聚合对象的父子关系一致。
        /// </summary>
        /// <returns></returns>
        internal IDictionary<Type, IList<Entity>> Read()
        {
            this._allChildren = new Dictionary<Type, IList<Entity>>();

            this.ReadChildren(this._entity);

            return this._allChildren;
        }

        /// <summary>
        /// 递归读取根对象的所有子对象
        /// </summary>
        /// <param name="entity"></param>
        private void ReadChildren(Entity entity)
        {
            var provider = RepositoryFactoryHost.Factory.Create(entity.GetType());
            var allProperties = provider.GetAvailableIndicators();

            var childrenList = new List<IList<Entity>>();

            //遍历所有子属性，读取孩子列表
            for (int i = 0, c = allProperties.Count; i < c; i++)
            {
                var property = allProperties[i];
                if (property is IListProperty)
                {
                    var children = entity.GetProperty(property) as IList<Entity>;
                    if (children != null && children.Count > 0)
                    {
                        //所有孩子列表中的实体，都加入到对应的实体列表中。
                        //并递归读取孩子的孩子实体。
                        var entityType = children[0].GetType();
                        var list = this.FindAggregateList(entityType);
                        childrenList.Add(list);
                        for (int j = 0, c2 = children.Count; j < c2; j++)
                        {
                            var child = children[j];

                            list.Add(child);
                            this.ReadChildren(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 找到对应实体的全部对象列表
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private IList<Entity> FindAggregateList(Type entityType)
        {
            IList<Entity> result = null;
            if (!this._allChildren.TryGetValue(entityType, out result))
            {
                result = new List<Entity>();
                this._allChildren.Add(entityType, result);
            }
            return result;
        }
    }
}
