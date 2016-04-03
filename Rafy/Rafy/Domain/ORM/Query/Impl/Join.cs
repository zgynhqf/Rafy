/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 12:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class Join : SqlJoin, IJoin
    {
        ISource IJoin.Left
        {
            get
            {
                return base.Left as ISource;
            }
            set
            {
                base.Left = value as SqlSource;
            }
        }

        JoinType IJoin.JoinType
        {
            get
            {
                return (JoinType)base.JoinType;
            }
            set
            {
                base.JoinType = (SqlJoinType)value;
            }
        }

        ITableSource IJoin.Right
        {
            get
            {
                return base.Right as ITableSource;
            }
            set
            {
                base.Right = value as SqlTable;
            }
        }

        IConstraint IJoin.Condition
        {
            get
            {
                return base.Condition as IConstraint;
            }
            set
            {
                base.Condition = value as SqlConstraint;
            }
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.Join; }
        }

        private TableSourceFinder _finder;

        ITableSource ISource.FindTable(IRepository repo, string alias)
        {
            if (_finder == null) { _finder = new TableSourceFinder(this); }
            return _finder.Find(repo, alias);
        }
    }
}
