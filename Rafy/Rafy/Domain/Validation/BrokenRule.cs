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
using Rafy.ManagedProperty;
using Rafy;
using Rafy.Serialization.Mobile;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 表示触犯的规则的明细。
    /// </summary>
    public class BrokenRule
    {
        internal BrokenRule(IRule rule, RuleArgs args)
        {
            this.Rule = rule;
            this.Property = rule.Property;
            this.Description = args.BrokenDescription;
        }

        //internal BrokenRule(string source, BrokenRule rule)
        //{
        //    RuleLabel = string.Format("rule://{0}.{1}", source, rule.RuleLabel.Replace("rule://", string.Empty));
        //    Description = rule.Description;
        //    Property = rule.Property;
        //    Level = rule.Level;
        //}

        /// <summary>
        /// 触犯的规则。
        /// </summary>
        public IRule Rule { get; private set; }

        /// <summary>
        /// 触发规则的描述信息。
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 如果这是某个属性关联的规则参数，则这个属性表示关联的托管属性
        /// </summary>
        public IManagedProperty Property { get; private set; }
    }
}