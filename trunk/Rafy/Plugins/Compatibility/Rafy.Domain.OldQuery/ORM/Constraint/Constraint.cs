/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 10:59
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 内部使用的约束条件项
    /// 
    /// 从 IConstraintGroup 继承的原因是：
    /// 任何一个约束（不论是约束组对象、还是单一的属性约束），都可以被看成一组约束，并向其继续添加新的约束。
    /// </summary>
    internal abstract class Constraint : IConstraintGroup
    {
        public const string AndOperator = "AND";
        public const string OrOperator = "OR";

        internal PropertyQuery Context;

        public bool IsEmpty
        {
            get { return this.Type == ConstraintType.Empty; }
        }

        public abstract ConstraintType Type { get; }

        public IConstraintGroup And(PropertyComparisonExpression exp)
        {
            var constraint = ConstrainProperty(exp);

            return this.And(constraint);
        }

        public IConstraintGroup Or(PropertyComparisonExpression exp)
        {
            var constraint = ConstrainProperty(exp);

            return this.Or(constraint);
        }

        public virtual IConstraintGroup And(IConstraintGroup constraint)
        {
            return Add(constraint as Constraint, AndOperator);
        }

        public virtual IConstraintGroup Or(IConstraintGroup constraint)
        {
            return Add(constraint as Constraint, OrOperator);
        }

        private IConstraintGroup Add(Constraint constraint, string op)
        {
            //如果目标是空的，那么还是只返回当前对象为约束组。
            if (constraint.IsEmpty) { return this; }

            var group = new ConstraintGroup { Context = Context };

            if (this.IsEmpty)
            {
                //就算是空的条件，也应该加到一个组中再返回；否则会造成 constraint 变得跟接后面的条件不是一组的。
                //例如：EmptyConstrint.And(q1).And(q2); q1 跟 q2 应该是同一组的，它们应该有括号分隔开。
                group.Items.Add(constraint);
            }
            else
            {
                group.Items.Add(this);
                group.Items.Add(constraint);
                group.Operators.Add(op);
            }

            return group;
        }

        private PropertyConstraint ConstrainProperty(PropertyComparisonExpression exp)
        {
            var constraint = new PropertyConstraint(exp)
            {
                Context = Context,
            };
            return constraint;
        }

        //public string GetSql(FormattedSqlParameters paramaters)
        //{
        //    var sql = new StringWriter();
        //    this.GetSql(sql, paramaters);
        //    return sql.ToString();
        //}

        ///// <summary>
        ///// 获取 Where 条件对应的 SQL。
        ///// </summary>
        ///// <param name="sql">输入的字符串。</param>
        ///// <param name="parameters">最终的参数列表。这个条件对应的参数都需要添加到这个列表中。</param>
        ///// <returns></returns>
        //public abstract void GetSql(TextWriter sql, FormattedSqlParameters parameters);

        //private void Prepare(PropertyCompareExpression exp, Type propertyOwner)
        //{
        //    if (propertyOwner != null)
        //    {
        //        exp.OwnerType = propertyOwner;
        //    }
        //    else
        //    {
        //        if (!exp.Property.OwnerType.IsAssignableFrom(Repo.EntityType))
        //        {
        //            ORMHelper.ThrowBasePropertyNotMappedException(exp.Property.Name, exp.Property.OwnerType);
        //        }

        //        exp.OwnerType = Repo.EntityType;
        //    }
        //}
    }

    internal enum ConstraintType
    {
        Empty,
        Property,
        TwoPropertiesComparison,
        Sql,
        AndOr,
        Group
    }
}