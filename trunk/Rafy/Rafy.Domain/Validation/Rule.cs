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
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// Tracks all information for a rule.
    /// </summary>
    internal class Rule : IRule//, IComparable, IComparable<IRule>
    {
        private IValidationRule _validationRule;

        internal Rule(IValidationRule rule)
        {
            _validationRule = rule;

            if (rule is HandlerRule)
            {
                this.Key = (rule as HandlerRule).GetKeyString();
            }
            else
            {
                this.Key = rule.GetType().FullName;
            }
        }

        public string Key { get; private set; }

        public IManagedProperty Property { get; internal set; }

        public int Priority { get; internal set; }

        public RuleLevel Level { get; internal set; }

        public IValidationRule ValidationRule
        {
            get { return _validationRule; }
        }

        #region IComparable

        //int IComparable.CompareTo(object obj)
        //{
        //    return Priority.CompareTo(((IRule)obj).Priority);
        //}

        //int IComparable<IRule>.CompareTo(IRule other)
        //{
        //    return Priority.CompareTo(other.Priority);
        //}

        #endregion
    }
}