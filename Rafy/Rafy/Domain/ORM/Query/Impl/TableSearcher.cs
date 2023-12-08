/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20231208
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20231208 13:10
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.ORM.Query
{
    internal class TableSearcher : QueryNodeVisitor
    {
        private List<ITableSource> _tables = new List<ITableSource>();
        public static List<ITableSource> GetAllTables(ISource source)
        {
            var counter = new TableSearcher();
            counter.Visit(source);
            return counter._tables;
        }
        protected override IQueryNode VisitTableSource(ITableSource node)
        {
            _tables.Add(node);
            return node;
        }
    }
}
