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
using System.Collections;

namespace Rafy.Domain
{
    /// <summary>
    /// 所有实体查询的参数类型的基类。
    /// </summary>
    public abstract class EntityQueryArgs : QueryArgs, IEntityQueryArgs
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
            this.MergePagingAndFirst();
        }

        bool IEntityQueryArgs.FetchingFirst
        {
            get { return this.QueryType == RepositoryQueryType.First; }
        }

        #endregion

        /// <summary>
        /// 实体查询对应的方法和参数信息。
        /// </summary>
        public IEntityQueryInvocation Invocation { get; internal set; }

        /// <summary>
        /// 如果是内存加载，则使用这个列表。
        /// </summary>
        internal List<Entity> MemoryList;

        /// <summary>
        /// 加载的列表对象
        /// </summary>
        public IEntityList EntityList { get; internal set; }

        /// <summary>
        /// 对查询出来的对象进行内存级别的过滤器，默认为 null。
        /// </summary>
        public Predicate<Entity> Filter { get; set; }

        /// <summary>
        /// 如果某次查询结果是一棵完整的子树，那么必须设置此属性为 true ，才可以把整个树标记为完整加载。
        /// 否则，所有节点的子节点集合 TreeChildren 处在未加载完全的状态（IsLoaded = false）。
        /// </summary>
        public bool MarkTreeFullLoaded { get; set; } = true;

        private PagingInfo _pagingInfo = PagingInfo.Empty;

        /// <summary>
        /// 要对结果进行分页的分页信息。
        /// 默认为 PagingInfo.Empty。
        /// </summary>
        public PagingInfo PagingInfo
        {
            get { return _pagingInfo; }
            set
            {
                _pagingInfo = value;
                this.MergePagingAndFirst();
            }
        }

        /// <summary>
        /// 如果 <see cref="IEntityQueryArgs.FetchingFirst"/> 为真，则重新修改 <see cref="PagingInfo"/>。
        /// </summary>
        internal virtual void MergePagingAndFirst()
        {
            if (this.QueryType == RepositoryQueryType.First)
            {
                if (PagingInfo.IsNullOrEmpty(_pagingInfo))
                {
                    _pagingInfo = new PagingInfo(1, 1);
                }
                else
                {
                    _pagingInfo.PageNumber = 1;
                    _pagingInfo.PageSize = 1;
                }
            }
        }

        IList IEntityQueryArgs.List
        {
            get { return this.MemoryList as IList ?? this.EntityList; }
        }

        /// <summary>
        /// 数据加载选项
        /// </summary>
        public LoadOptions LoadOptions { get; set; }

        internal void SetDataLoadOptions(PagingInfo paging = null, LoadOptions loadOptions = null)
        {
            if (!PagingInfo.IsNullOrEmpty(paging))
            {
                this.PagingInfo = paging;
            }

            this.LoadOptions = loadOptions;
        }
    }
}
