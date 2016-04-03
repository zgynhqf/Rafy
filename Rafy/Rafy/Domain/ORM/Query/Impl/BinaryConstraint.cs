/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 12:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class BinaryConstraint : SqlBinaryConstraint, IBinaryConstraint
    {
        IConstraint IBinaryConstraint.Left
        {
            get
            {
                return base.Left as IConstraint;
            }
            set
            {
                base.Left = value as SqlConstraint;
            }
        }

        BinaryOperator IBinaryConstraint.Opeartor
        {
            get
            {
                return (BinaryOperator)base.Opeartor;
            }
            set
            {
                base.Opeartor = (SqlBinaryConstraintType)value;
            }
        }

        IConstraint IBinaryConstraint.Right
        {
            get
            {
                return base.Right as IConstraint;
            }
            set
            {
                base.Right = value as SqlConstraint;
            }
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.BinaryConstraint; }
        }
    }
}
