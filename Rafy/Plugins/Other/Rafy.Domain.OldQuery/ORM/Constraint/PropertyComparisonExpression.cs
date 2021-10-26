/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 18:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 属性对比操作“表达式”
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    internal class PropertyComparisonExpression : ConcreteProperty
    {
        internal PropertyComparisonExpression(IManagedProperty property, Type concreteType, PropertyCompareOperator op, object value)
            : base(property, concreteType)
        {
            this.Operator = op;
            this.Value = value;

            switch (op)
            {
                case PropertyCompareOperator.Equal:
                case PropertyCompareOperator.NotEqual:
                    break;
                default:
                    if (value == null)
                    {
                        throw new InvalidOperationException("null 值只能进行相等、不相等两种对比。");
                    }
                    break;
            }
        }

        /// <summary>
        /// 操作符
        /// </summary>
        public PropertyCompareOperator Operator { get; private set; }

        /// <summary>
        /// 对比的值。
        /// </summary>
        public object Value { get; private set; }

        private string DebuggerDisplay
        {
            get { return this.FullName + " " + this.Operator + " " + this.Value; }
        }
    }
}