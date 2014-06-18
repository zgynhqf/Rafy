/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 适配到 Entity、托管属性上。 胡庆访 20120330
 * 
*******************************************************/

using System;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// Tracks all information for a rule.
    /// </summary>
    internal class RuleMethod : IRuleMethod, IComparable, IComparable<IRuleMethod>
    {
        private RuleHandler _handler;

        /// <summary>
        /// Creates and initializes the rule.
        /// </summary>
        /// <param name="handler">The address of the method implementing the rule.</param>
        /// <param name="args">A RuleArgs object.</param>
        /// <param name="priority">
        /// Priority for processing the rule (smaller numbers have higher priority, default=0).
        /// </param>
        public RuleMethod(RuleHandler handler, RuleArgs args, int priority)
        {
            this._handler = handler;

            this.RuleArgs = args;
            this.RuleLabel = string.Format(@"rule://{0}/{1}/{2}",
                Uri.EscapeDataString(_handler.Method.DeclaringType.FullName),
                _handler.Method.Name,
                RuleArgs.ToString()
                );

            this.Priority = priority;
        }

        /// <summary>
        /// Gets the priority of the rule method.
        /// </summary>
        /// <value>The priority value</value>
        /// <remarks>
        /// Priorities are processed in descending
        /// order, so priority 0 is processed
        /// before priority 1, etc.
        /// </remarks>
        public int Priority { get; private set; }

        /// <summary>
        /// Gets the name of the rule.
        /// </summary>
        /// <remarks>
        /// The rule's name must be unique and is used
        /// to identify a broken rule in the BrokenRules
        /// collection.
        /// </remarks>
        public string RuleLabel { get; private set; }

        /// <summary>
        /// Returns the name of the field, property or column
        /// to which the rule applies.
        /// </summary>
        public RuleArgs RuleArgs { get; private set; }

        /// <summary>
        /// Invokes the rule to validate the data.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the data is valid, 
        /// <see langword="false" /> if the data is invalid.
        /// </returns>
        public void Check(Entity target)
        {
            _handler.Invoke(target, this.RuleArgs);
        }

        /// <summary>
        /// Returns the name of the method implementing the rule
        /// and the property, field or column name to which the
        /// rule applies.
        /// </summary>
        public override string ToString()
        {
            return this.RuleLabel;
        }

        #region IComparable

        int IComparable.CompareTo(object obj)
        {
            return Priority.CompareTo(((IRuleMethod)obj).Priority);
        }

        int IComparable<IRuleMethod>.CompareTo(IRuleMethod other)
        {
            return Priority.CompareTo(other.Priority);
        }

        #endregion
    }
}