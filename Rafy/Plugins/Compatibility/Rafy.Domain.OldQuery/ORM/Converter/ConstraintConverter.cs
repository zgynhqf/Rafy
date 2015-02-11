/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131215 00:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain.ORM.Converter
{
    class ConstraintConverter : ConstraintVisitor
    {
        internal IQuery Query;
        private QueryFactory f = QueryFactory.Instance;

        public void Convert(Constraint node)
        {
            base.Visit(node);

            Query.Where = f.And(Query.Where, _whereResult);
        }

        private IConstraint _whereResult;

        protected override void VisitAndOrConstraint(AndOrConstraint node)
        {
            this.Visit(node.Left);
            var leftWhere = _whereResult;

            this.Visit(node.Right);
            var rightWhere = _whereResult;

            _whereResult = f.Binary(leftWhere, node.IsAnd ? BinaryOperator.And : BinaryOperator.Or, rightWhere);
        }

        protected override void VisitProperty(PropertyConstraint node)
        {
            var op = ConvertOperator(node.Operator);

            var source = Query.From.FindTable(node.ConcreteType);
            var column = source.Column(node.Property);
            _whereResult = f.Constraint(column, op, node.Value);
        }

        private static PropertyOperator ConvertOperator(PropertyCompareOperator pco)
        {
            var op = PropertyOperator.Equal;
            switch (pco)
            {
                case PropertyCompareOperator.Equal:
                    op = PropertyOperator.Equal;
                    break;
                case PropertyCompareOperator.NotEqual:
                    op = PropertyOperator.NotEqual;
                    break;
                case PropertyCompareOperator.Greater:
                    op = PropertyOperator.Greater;
                    break;
                case PropertyCompareOperator.GreaterEqual:
                    op = PropertyOperator.GreaterEqual;
                    break;
                case PropertyCompareOperator.Less:
                    op = PropertyOperator.Less;
                    break;
                case PropertyCompareOperator.LessEqual:
                    op = PropertyOperator.LessEqual;
                    break;
                case PropertyCompareOperator.Like:
                    op = PropertyOperator.Like;
                    break;
                case PropertyCompareOperator.Contains:
                    op = PropertyOperator.Contains;
                    break;
                case PropertyCompareOperator.StartWith:
                    op = PropertyOperator.StartsWith;
                    break;
                case PropertyCompareOperator.EndWith:
                    op = PropertyOperator.EndsWith;
                    break;
                case PropertyCompareOperator.In:
                    op = PropertyOperator.In;
                    break;
                case PropertyCompareOperator.NotIn:
                    op = PropertyOperator.NotIn;
                    break;
                default:
                    break;
            }
            return op;
        }

        protected override void VisitGroup(ConstraintGroup node)
        {
            IConstraint where = null;

            for (int i = 0, c = node.Items.Count; i < c; i++)
            {
                this.Visit(node.Items[i]);

                if (i == 0)
                {
                    where = _whereResult;
                }
                else
                {
                    string op = node.Operators[i - 1];
                    if (op == Constraint.AndOperator)
                    {
                        where = f.And(where, _whereResult);
                    }
                    else if (op == Constraint.OrOperator)
                    {
                        where = f.Or(where, _whereResult);
                    }
                }
            }

            _whereResult = where;
        }

        protected override void VisitSqlWhereConstraint(SqlWhereConstraint node)
        {
            _whereResult = f.Literal(node.FormatSql, node.Parameters);
        }

        protected override void VisitTwoPropertiesComparison(TwoPropertiesConstraint node)
        {
            var leftTable = Query.From.FindTable(node.LeftPropertyOwner ?? node.LeftProperty.OwnerType);
            var rightTable = Query.From.FindTable(node.RightPropertyOwner ?? node.RightProperty.OwnerType);
            _whereResult = f.Constraint(
                leftTable.Column(node.LeftProperty),
                ConvertOperator(node.Operator),
                rightTable.Column(node.RightProperty)
                );
        }
    }
}
