/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 12:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class ColumnConstraint : SqlColumnConstraint, IColumnConstraint
    {
        IColumnNode IColumnConstraint.Column
        {
            get
            {
                return base.Column as IColumnNode;
            }
            set
            {
                base.Column = value as SqlColumn;
            }
        }

        PropertyOperator IColumnConstraint.Operator
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

        object IColumnConstraint.Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
            }
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.ColumnConstraint; }
        }
    }
}
