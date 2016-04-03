/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2012
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130523 16:36
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rafy.Domain.ORM;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 所有实体查询的参数类型的基类。
    /// </summary>
    public abstract class EntityQueryArgsBase : QueryArgs, ISelectArgs
    {
        #region QueryType

        private RepositoryQueryType _queryType = RepositoryQueryType.List;

        /// <summary>
        /// 当前查询数据的类型。
        /// 实体查询时，不会对应 <see cref="Rafy.Domain.RepositoryQueryType.Table" /> 类型。
        /// </summary>
        public override RepositoryQueryType QueryType
        {
            get { return _queryType; }
        }

        internal void SetQueryType(RepositoryQueryType value)
        {
            _queryType = value;
        }

        bool ISelectArgs.FetchingFirst
        {
            get { return this.QueryType == RepositoryQueryType.First; }
        }

        #endregion

        /// <summary>
        /// 如果是内存加载，则使用这个列表。
        /// </summary>
        internal IList<Entity> MemoryList;

        /// <summary>
        /// 加载的列表对象
        /// </summary>
        public EntityList EntityList { get; set; }

        /// <summary>
        /// 对查询出来的对象进行内存级别的过滤器，默认为 null。
        /// </summary>
        public Predicate<Entity> Filter { get; set; }

        /// <summary>
        /// 如果某次查询结果是一棵完整的子树，那么必须设置此属性为 true ，才可以把整个树标记为完整加载。
        /// 否则，所有节点的子节点集合 TreeChildren 处在未加载完全的状态（IsLoaded = false）。
        /// </summary>
        public bool MarkTreeFullLoaded { get; set; }

        private PagingInfo _pagingInfo = PagingInfo.Empty;

        /// <summary>
        /// 要对结果进行分页的分页信息。
        /// 默认为 PagingInfo.Empty。
        /// </summary>
        public PagingInfo PagingInfo
        {
            get { return _pagingInfo; }
            set { _pagingInfo = value; }
        }

        IList<Entity> ISelectArgs.List
        {
            get { return this.MemoryList ?? this.EntityList; }
        }

        internal EagerLoadOptions EagerLoadOptions;

        /// <summary>
        /// 贪婪加载某个属性
        /// </summary>
        /// <param name="property">需要贪婪加载的托管属性。可以是一个引用属性，也可以是一个组合子属性。</param>
        /// <param name="propertyOwner">这个属性的拥有者类型。</param>
        public void EagerLoad(IProperty property, Type propertyOwner = null)
        {
            this.EagerLoad(new ConcreteProperty(property, propertyOwner ?? property.OwnerType));
        }

        /// <summary>
        /// 贪婪加载某个属性
        /// </summary>
        /// <param name="property">需要贪婪加载的托管属性。可以是一个引用属性，也可以是一个组合子属性。</param>
        private void EagerLoad(ConcreteProperty property)
        {
            if (this.EagerLoadOptions == null)
            {
                this.EagerLoadOptions = new EagerLoadOptions();
            }

            this.EagerLoadOptions.CoreList.Add(property);
        }

        internal void SetDataLoadOptions(PagingInfo paging = null, EagerLoadOptions eagerLoad = null)
        {
            if (!PagingInfo.IsNullOrEmpty(paging))
            {
                this.PagingInfo = paging;
            }

            if (eagerLoad != null && eagerLoad.CoreList.Count > 0)
            {
                if (this.EagerLoadOptions != null)
                {
                    for (int i = 0, c = eagerLoad.CoreList.Count; i < c; i++)
                    {
                        var item = eagerLoad.CoreList[i];
                        this.EagerLoad(item);
                    }
                }
                else
                {
                    this.EagerLoadOptions = eagerLoad;
                }
            }
        }
    }

    /// <summary>
    /// 使用 IQuery 进行查询的参数。
    /// </summary>
    public class EntityQueryArgs : EntityQueryArgsBase, IEntitySelectArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryArgs"/> class.
        /// </summary>
        public EntityQueryArgs() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryArgs"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public EntityQueryArgs(IQuery query)
        {
            this.Query = query;
        }

        /// <summary>
        /// 对应的查询条件定义。
        /// </summary>
        public IQuery Query { get; set; }
    }

    ///// <summary>
    ///// 使用 Linq 进行查询的参数。
    ///// </summary>
    //public class LinqQueryArgs : EntityQueryArgs
    //{
    //    /// <summary>
    //    /// 对应的 Linq 查询条件表达式。
    //    /// 此条件在内部会被转换为 IQuery 对象来描述整个查询。
    //    /// </summary>
    //    public IQueryable Queryable { get; set; }
    //}
}
