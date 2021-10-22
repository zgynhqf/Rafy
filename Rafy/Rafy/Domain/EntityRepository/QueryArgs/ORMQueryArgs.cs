using Rafy.Domain.ORM.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 使用 <see cref="ORM.Query.IQuery"/> 进行查询的参数。
    /// </summary>
    public class ORMQueryArgs : EntityQueryArgs, IORMQueryArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ORMQueryArgs"/> class.
        /// </summary>
        public ORMQueryArgs() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ORMQueryArgs"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        public ORMQueryArgs(IQuery query)
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
