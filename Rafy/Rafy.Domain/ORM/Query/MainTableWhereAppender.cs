/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151022
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151022 17:33
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 可以为查询中的主表的 Where 条件添加一个指定条件的类。
    /// </summary>
    public abstract class MainTableWhereAppender : QueryNodeVisitor
    {
        private bool _mainTableHandled;

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
            var query = base.VisitQuery(node);

            if (!_mainTableHandled)
            {
                var mainTable = query.MainTable;
                var condition = this.GetCondition(mainTable, node);
                if (condition != null)
                {
                    if (this.AddConditionToLast)
                    {
                        query.Where = QueryFactory.Instance.And(query.Where, condition);
                    }
                    else
                    {
                        query.Where = QueryFactory.Instance.And(condition, query.Where);
                    }
                }

                _mainTableHandled = true;
            }

            return query;
        }

        /// <summary>
        /// 获取指定的主表对应的条件。
        /// </summary>
        /// <param name="mainTable">The main table.</param>
        /// <param name="query">The node.</param>
        /// <returns></returns>
        protected abstract IConstraint GetCondition(ITableSource mainTable, IQuery query);
    }
}
