/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170105
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170105 15:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query.Impl;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 可以为查询中的主表的 Where 条件添加一个指定条件的类。
    /// </summary>
    public abstract class EveryTableWhereAppender : QueryNodeVisitor
    {
        /// <summary>
        /// 表集合
        /// </summary>
        private List<ITableSource> _tableSourceList;

        /// <summary>
        /// 是把新的条件添加到 Where 条件的最后。
        /// true：添加到最后。
        /// false：作为第一个条件插入。
        /// 默认：false。
        /// </summary>
        public bool AddConditionToLast { get; set; }

        /// <summary>
        /// 把条件添加到查询中的主表对应的 Where 中。
        /// </summary>
        /// <param name="node"></param>
        public void Append(IQuery node)
        {
            this.Visit(node);
        }

        /// <summary>
        /// 为所有的 IQuery 对象都添加相应的多租户查询。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override IQuery VisitQuery(IQuery node)
        {
            if (node.Selection != null)
            {
                this.Visit(node.Selection);
            }

            _tableSourceList = new List<ITableSource>();
            this.Visit(node.From);
            var condition = GetTableSourceCondition(node);
            _tableSourceList = null;

            if (condition != null)
            {
                node.Where = QueryFactory.Instance.And(node.Where, condition);
            }

            if (node.Where != null)
            {
                this.Visit(node.Where);
            }

            var entityQuery = node as TableQuery;
            if (entityQuery.HasOrdered())
            {
                for (int i = 0, c = node.OrderBy.Count; i < c; i++)
                {
                    var item = node.OrderBy[i];
                    this.Visit(item);
                }
            }
            return node;
        }

        protected override ITableSource VisitEntitySource(ITableSource node)
        {
            if (_tableSourceList != null)
            {
                _tableSourceList.Add(node);
            }
            return base.VisitEntitySource(node);
        }

        /// <summary>
        /// 获取指定的主表、join表对应的条件。
        /// </summary>
        /// <param name="mainTable">The main table.</param>
        /// <param name="query">The node.</param>
        /// <returns></returns>
        protected abstract IConstraint GetCondition(ITableSource mainTable, IQuery query);

        /// <summary>
        /// 获取所有表统一的过滤条件
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private IConstraint GetTableSourceCondition(IQuery query)
        {
            IConstraint res = null;
            foreach (var tableSource in this._tableSourceList)
            {
                var condition = GetCondition(tableSource, query);
                res = QueryFactory.Instance.And(res, condition);
            }
            return res;
        }
    }
}
