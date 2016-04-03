/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 13:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class SubQueryRef : SqlSubSelect, ISubQuery
    {
        IQuery ISubQuery.Query
        {
            get
            {
                return base.Select as IQuery;
            }
            set
            {
                base.Select = value as SqlSelect;
            }
        }

        string ISubQuery.Alias
        {
            get
            {
                return base.Alias;
            }
            set
            {
                base.Alias = value;
            }
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.SubQuery; }
        }

        IColumnNode ISubQuery.Column(IColumnNode rawProperty)
        {
            var raw = rawProperty as ColumnNode;

            var property = new ColumnNode
            {
                ColumnName = raw.ColumnName,
                Property = raw.Property,
                Table = this,
            };

            return property;
        }

        IColumnNode ISubQuery.Column(IColumnNode rawProperty, string alias)
        {
            var raw = rawProperty as ColumnNode;

            var property = new ColumnNode
            {
                ColumnName = raw.ColumnName,
                Property = raw.Property,
                Table = this,
                Alias = alias
            };

            return property;
        }

        string INamedSource.GetName()
        {
            return base.GetName();
        }

        private TableSourceFinder _finder;

        ITableSource ISource.FindTable(IRepository repo, string alias)
        {
            if (_finder == null) { _finder = new TableSourceFinder(this); }
            return _finder.Find(repo, alias);
        }
    }
}
