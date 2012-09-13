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
using System.Runtime.Serialization;
using OEA.ManagedProperty;
using OEA;
using OEA.Serialization.Mobile;

namespace OEA.Library.Validation
{
    /// <summary>
    /// Stores details about a specific broken business rule.
    /// </summary>
    public class BrokenRule
    {
        internal BrokenRule(IRuleMethod rule)
        {
            RuleLabel = rule.RuleLabel;
            Description = rule.RuleArgs.BrokenDescription;
            Property = rule.RuleArgs.Property;
            Level = rule.RuleArgs.Level;
        }

        //internal BrokenRule(string source, BrokenRule rule)
        //{
        //    RuleLabel = string.Format("rule://{0}.{1}", source, rule.RuleLabel.Replace("rule://", string.Empty));
        //    Description = rule.Description;
        //    Property = rule.Property;
        //    Level = rule.Level;
        //}

        /// <summary>
        /// Provides access to the name of the broken rule.
        /// </summary>
        /// <value>The name of the rule.</value>
        public string RuleLabel { get; private set; }

        /// <summary>
        /// Provides access to the description of the broken rule.
        /// </summary>
        /// <value>The description of the rule.</value>
        public string Description { get; private set; }

        /// <summary>
        /// 如果这是某个属性关联的规则参数，则这个属性表示关联的托管属性
        /// </summary>
        public IManagedProperty Property { get; private set; }

        /// <summary>
        /// Gets the severity of the broken rule.
        /// </summary>
        public RuleLevel Level { get; private set; }
    }
}