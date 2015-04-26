/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131214 21:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain.DataPortal;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Converter;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain
{
    public abstract class PropertyQueryRepository : EntityRepository
    {
        /// <summary>
        /// 创建一个实体属性查询对象。
        /// 只能在服务端调用此方法。
        /// </summary>
        /// <returns></returns>
        protected IPropertyQuery CreatePropertyQuery()
        {
            return new PropertyQuery(Repo);
        }

        #region 数据层查询接口 - IPropertyQuery

        /// <summary>
        /// 通过托管属性查询条件来查询数据库，把满足条件的实体查询出来。
        /// </summary>
        /// <param name="propertyQuery">托管属性查询条件。</param>
        /// <returns></returns>
        protected EntityList QueryList(IPropertyQuery propertyQuery)
        {
            var args = new PropertyQueryArgs
            {
                PropertyQuery = propertyQuery
            };

            return this.QueryList(args);
        }

        /// <summary>
        /// 通过托管属性查询条件来查询数据库，把满足条件的实体查询出来。
        /// </summary>
        /// <param name="args">托管属性查询条件。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">args
        /// or
        /// args.PropertyQuery。</exception>
        /// <exception cref="System.NotSupportedException">使用内存过滤器的同时，不支持提供分页参数。</exception>
        protected EntityList QueryList(PropertyQueryArgs args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.PropertyQuery == null) throw new ArgumentNullException("args.PropertyQuery。");

            this.PrepareArgs(args);

            this.BuildDefaultQuerying(args);

            //this.OnPropertyQuerying(args);

            var dbQuery = args.PropertyQuery as PropertyQuery;

            IQuery query = this.ConvertToQuery(dbQuery);

            var entityArgs = new EntityQueryArgs
            {
                EntityList = args.EntityList,
                Filter = args.Filter,
                MemoryList = args.MemoryList,
                PagingInfo = args.PropertyQuery.PagingInfo,
                Query = query
            };
            entityArgs.SetFetchType(args.FetchType);
            if (dbQuery.EagerLoadProperties != null)
            {
                for (int i = 0, c = dbQuery.EagerLoadProperties.Count; i < c; i++)
                {
                    var item = dbQuery.EagerLoadProperties[i];
                    entityArgs.EagerLoad(item.Property as IProperty, item.Owner);
                }
            }

            return base.QueryList(entityArgs);
        }

        /// <summary>
        /// 把老的 IPropertyQuery 对象，转换为新版本的 IQuery 对象。
        /// </summary>
        /// <param name="propertyQuery">The property query.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected IQuery ConvertToQuery(IPropertyQuery propertyQuery)
        {
            var table = qf.Table(this);
            var query = qf.Query(table);

            //转换 From
            var dbQuery = propertyQuery as PropertyQuery;

            if (dbQuery.HasInnerJoin)
            {
                foreach (var refItem in dbQuery.RefItems)
                {
                    if (refItem.JoinRefType == JoinRefType.QueryData)
                    {
                        throw new NotSupportedException();
                    }

                    var refTable = qf.Table(refItem.RefTable.Repository);
                    query.From = qf.Join(query.From, refTable, refItem.RefProperty);
                }
            }

            //转换 Where
            var converter = new ConstraintConverter();
            converter.Query = query;
            converter.Convert(dbQuery.Where as Constraint);

            //转换 OrderBy
            for (int i = 0, c = dbQuery.Orders.Count; i < c; i++)
            {
                var order = dbQuery.Orders[i];
                var dir = order.Ascending ? OrderDirection.Ascending : OrderDirection.Descending;
                query.OrderBy.Add(qf.OrderBy(table.Column(order.Property), dir));
            }

            return query;
        }

        private void BuildDefaultQuerying(PropertyQueryArgs args)
        {
            //树型实体不支持修改排序规则！此逻辑不能放到 OnQueryBuilt 虚方法中，以免被重写。
            if (Repo.SupportTree)
            {
                var dbQuery = args.PropertyQuery as PropertyQuery;
                dbQuery.Orders.Clear();
                dbQuery.OrderBy(Entity.TreeIndexProperty, OrderDirection.Ascending);
            }
        }

        ///// <summary>
        ///// 所有使用 IQuery 的数据查询，在调用完应 queryBuilder 之后，都会执行此此方法。
        ///// 所以子类可以重写此方法实现统一的查询条件逻辑。
        ///// （例如，对于映射同一张表的几个子类的查询，可以使用此方法统一对所有方法都过滤）。
        ///// 
        ///// 默认实现为：
        ///// * 如果还没有进行排序，则进行默认的排序。
        ///// * 如果单一参数实现了 IPagingCriteria 接口，则使用其中的分页信息进行分页。
        ///// </summary>
        ///// <param name="args"></param>
        //protected virtual void OnPropertyQuerying(PropertyQueryArgs args)
        //{
        //    //如果没有指定 OrderNo 字段，则按照Id 排序。
        //    if (args.FetchType != FetchType.Count && !args.PropertyQuery.HasOrdered)
        //    {
        //        args.PropertyQuery.OrderBy(Entity.IdProperty, OrderDirection.Ascending);
        //    }

        //    //默认对分页进行处理。
        //    if (args.Filter == null)
        //    {
        //        var pList = CurrentIEQC.Parameters;
        //        if (pList.Length == 1)
        //        {
        //            var userCriteria = pList[0] as IPagingCriteria;
        //            if (userCriteria != null) { args.PropertyQuery.Paging(userCriteria.PagingInfo); }
        //        }
        //    }
        //}

        #endregion

        private void PrepareArgs(EntityQueryArgsBase args)
        {
            if (args.EntityList == null)
            {
                //必须使用 NewListFast，否则使用 NewList 会导致调用了 NotifyLoaded，
                //这样，不但不符合设计（这个列表需要在客户端才调用 NotifyLoaded），还会引发树型实体列表的多次关系重建。
                args.EntityList = (Repo as EntityRepository).NewListFast();
            }

            args.SetFetchType(CurrentIEQC.FetchType);
        }

        private static IEQC CurrentIEQC
        {
            get
            {
                var ieqc = FinalDataPortal.CurrentCriteria as IEQC;
                if (ieqc == null) throw new NotSupportedException("实体查询必须使用 FetchCount、FetchFirst、FetchList、FetchTable 等方法。");
                return ieqc;
            }
        }
    }
}
