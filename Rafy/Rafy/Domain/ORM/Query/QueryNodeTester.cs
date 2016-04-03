/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 17:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 方便测试使用的测试器。
    /// </summary>
    public static class QueryNodeTester
    {
        /// <summary>
        /// 生成 SqlServer 可用的测试 Sql 语句。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static FormattedSql GenerateTestSql(IQueryNode node)
        {
            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(node as SqlNode);
            return generator.Sql;
        }
    }
}
