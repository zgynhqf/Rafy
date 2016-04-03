/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150822
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150822 14:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 在 Sql 树中找到第一个表。
    /// </summary>
    internal class FirstTableFinder : SqlNodeVisitor
    {
        private SqlTable _table;

        public SqlTable Find(ISqlNode node)
        {
            this.Visit(node);

            return _table;
        }

        protected override ISqlNode Visit(ISqlNode node)
        {
            if (_table != null) { return node; }

            return base.Visit(node);
        }

        protected override SqlTable VisitSqlTable(SqlTable sqlTable)
        {
            if (_table == null)
            {
                _table = sqlTable;
                return sqlTable;
            }

            return base.VisitSqlTable(sqlTable);
        }
    }
}
