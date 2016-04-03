/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 13:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class ColumnsComparison : SqlColumnsComparisonConstraint, IColumnsComparison
    {
        IColumnNode IColumnsComparison.LeftColumn
        {
            get
            {
                return base.LeftColumn as IColumnNode;
            }
            set
            {
                base.LeftColumn = value as SqlColumn;
            }
        }

        IColumnNode IColumnsComparison.RightColumn
        {
            get
            {
                return base.RightColumn as IColumnNode;
            }
            set
            {
                base.RightColumn = value as SqlColumn;
            }
        }

        PropertyOperator IColumnsComparison.Operator
        {
            get
            {
                return (PropertyOperator)base.Operator;
            }
            set
            {
                base.Operator = (SqlColumnConstraintOperator)value;
            }
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.ColumnsComparisonConstraint; }
        }
    }
}
