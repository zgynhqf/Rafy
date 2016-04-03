/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150202
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150202 16:21
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
    /// 生成 Sql 时的上下文对象。用于生成过程的上下文共享。
    /// </summary>
    internal class QueryGenerationContext
    {
        private int _tablesCount = 0;

        /// <summary>
        /// 当前已经使用过 <see cref="NextTableAlias"/> 生成的表的个数。
        /// </summary>
        public int TablesCount
        {
            get { return _tablesCount; }
        }

        /// <summary>
        /// 自动生成的 SQL 需要使用这个方法来统一生成表名。
        /// </summary>
        /// <returns></returns>
        public string NextTableAlias()
        {
            return "T" + _tablesCount++;
        }

        public static QueryGenerationContext Get(IQuery query)
        {
            var tq = query as TableQuery;
            if (tq.GenerationContext == null)
            {
                tq.GenerationContext = new QueryGenerationContext();
            }
            return tq.GenerationContext;
        }

        public void Bind(IQuery query)
        {
            (query as TableQuery).GenerationContext = this;
        }
    }
}