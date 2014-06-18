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

namespace Rafy.Domain.ORM.Converter
{
    class ConstraintVisitor
    {
        protected virtual void Visit(Constraint constraint)
        {
            switch (constraint.Type)
            {
                case ConstraintType.Empty:
                    this.VisitEmpty(constraint as EmptyConstraint);
                    break;
                case ConstraintType.Property:
                    this.VisitProperty(constraint as PropertyConstraint);
                    break;
                case ConstraintType.TwoPropertiesComparison:
                    this.VisitTwoPropertiesComparison(constraint as TwoPropertiesConstraint);
                    break;
                case ConstraintType.Sql:
                    this.VisitSqlWhereConstraint(constraint as SqlWhereConstraint);
                    break;
                case ConstraintType.AndOr:
                    this.VisitAndOrConstraint(constraint as AndOrConstraint);
                    break;
                case ConstraintType.Group:
                    this.VisitGroup(constraint as ConstraintGroup);
                    break;
                default:
                    break;
            }
        }

        protected virtual void VisitGroup(ConstraintGroup node)
        {
        }

        protected virtual void VisitAndOrConstraint(AndOrConstraint node)
        {
        }

        protected virtual void VisitEmpty(EmptyConstraint node)
        {
        }

        protected virtual void VisitProperty(PropertyConstraint node)
        {
        }

        protected virtual void VisitTwoPropertiesComparison(TwoPropertiesConstraint node)
        {
        }

        protected virtual void VisitSqlWhereConstraint(SqlWhereConstraint node)
        {
        }
    }
}
