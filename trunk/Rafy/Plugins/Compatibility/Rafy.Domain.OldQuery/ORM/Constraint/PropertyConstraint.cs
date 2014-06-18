/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 17:27
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 属性对比的约束条件。
    /// </summary>
    internal class PropertyConstraint : Constraint
    {
        internal PropertyConstraint() { }

        internal PropertyConstraint(PropertyComparisonExpression exp)
        {
            this.Property = exp.Property;
            this.ConcreteType = exp.Owner;
            this.Operator = exp.Operator;
            this.Value = exp.Value;
        }

        public IManagedProperty Property;
        public Type ConcreteType;
        public PropertyCompareOperator Operator;
        public object Value;

        public override ConstraintType Type
        {
            get { return ConstraintType.Property; }
        }

        //public override void GetSql(TextWriter sql, FormattedSqlParameters parameters)
        //{
        //    //使用属性所在实体类对应的表来生成 Sql，可以处理不同数据库的 Sql 生成。
        //    var dbTable = Context.GetPropertyTable(Property, ConcreteType);
        //    dbTable.WritePropertyConstraintSql(this, sql, parameters);
        //}
    }
}