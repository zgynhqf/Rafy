/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130603
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130603 16:32
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
    /// 两个属性进行对比
    /// </summary>
    internal class TwoPropertiesConstraint : Constraint
    {
        public override ConstraintType Type
        {
            get { return ConstraintType.TwoPropertiesComparison; }
        }

        /// <summary>
        /// 操作符左边的属性。
        /// </summary>
        public IManagedProperty LeftProperty;

        /// <summary>
        /// 操作符左边的属性对应的实体类型。
        /// </summary>
        public Type LeftPropertyOwner;

        /// <summary>
        /// 操作符
        /// </summary>
        public PropertyCompareOperator Operator;

        /// <summary>
        /// 操作符右边的属性。
        /// </summary>
        public IManagedProperty RightProperty;

        /// <summary>
        /// 操作符右边的属性对应的实体类型。
        /// </summary>
        public Type RightPropertyOwner;

        //public override void GetSql(TextWriter sql, FormattedSqlParameters parameters)
        //{
        //    AppendColumn(sql, LeftProperty, LeftPropertyOwner);

        //    //根据不同的操作符，来生成不同的 sql。
        //    switch (Operator)
        //    {
        //        case PropertyCompareOperator.Equal:
        //            sql.Write(" = ");
        //            break;
        //        case PropertyCompareOperator.NotEqual:
        //            sql.Write(" != ");
        //            break;
        //        case PropertyCompareOperator.Greater:
        //            sql.Write(" > ");
        //            break;
        //        case PropertyCompareOperator.GreaterEqual:
        //            sql.Write(" >= ");
        //            break;
        //        case PropertyCompareOperator.Less:
        //            sql.Write(" < ");
        //            break;
        //        case PropertyCompareOperator.LessEqual:
        //            sql.Write(" <= ");
        //            break;
        //        default:
        //            throw new NotSupportedException(string.Format("两个属性对比时，不能进行 {0} 操作。", Operator));
        //    }

        //    AppendColumn(sql, RightProperty, RightPropertyOwner);
        //}

        //private void AppendColumn(TextWriter sql, IManagedProperty property, Type propertyType)
        //{
        //    var leftDbTable = Context.GetPropertyTable(property, propertyType);
        //    sql.AppendQuoteName(leftDbTable).Write('.');
        //    sql.AppendQuote(leftDbTable, leftDbTable.Translate(property));
        //}
    }
}