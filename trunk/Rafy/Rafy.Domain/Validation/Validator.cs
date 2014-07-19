/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140310
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140310 12:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 规则验证器
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// threshold for short-circuiting to kick in
        /// </summary>
        private static int _processThroughPriority;
        /// <summary>
        /// Gets or sets the priority through which
        /// CheckRules should process before short-circuiting
        /// processing on broken rules.
        /// </summary>
        /// <value>Defaults to 0.</value>
        /// <remarks>
        /// All rules for each property are processed by CheckRules
        /// though this priority. Rules with lower priorities are
        /// only processed if no previous rule has been marked as
        /// broken.
        /// </remarks>
        public static int ProcessThroughPriority
        {
            get { return _processThroughPriority; }
            set { _processThroughPriority = value; }
        }

        /// <summary>
        /// 检查某个属性是否满足规则
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="property">托管属性</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// target
        /// or
        /// property
        /// </exception>
        public static BrokenRulesCollection Validate(this Entity target, IManagedProperty property)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (property == null) throw new ArgumentNullException("property");

            var res = new BrokenRulesCollection();

            // get the rules dictionary
            var rules = ValidationHelper.GetTypeRules(target.FindRepository() as ITypeValidationsHost, target.GetType());
            if (rules != null)
            {
                // get the rules list for this property
                RulesContainer rulesList = rules.GetRulesForProperty(property, false);
                if (rulesList != null)
                {
                    // get the actual list of rules (sorted by priority)
                    CheckRules(target, rulesList, res);
                }
            }

            return res;
        }

        /// <summary>
        /// 检查整个实体对象是否满足规则
        /// </summary>
        public static BrokenRulesCollection Validate(this Entity target)
        {
            if (target == null) throw new ArgumentNullException("target");

            var res = new BrokenRulesCollection();

            Validate(target, res);

            return res;
        }

        private static void Validate(Entity target, BrokenRulesCollection res)
        {
            var rules = ValidationHelper.GetTypeRules(target.FindRepository() as ITypeValidationsHost, target.GetType());
            if (rules != null)
            {
                foreach (var de in rules.PropertyRules) { CheckRules(target, de.Value, res); }

                CheckRules(target, rules.TypeRules, res);
            }

            foreach (var child in target.GetLoadedChildren())
            {
                var list = child.Value as EntityList;
                if (list != null)
                {
                    list.EachNode(childEntity =>
                    {
                        Validate(childEntity, res);
                        return false;
                    });
                }
                else
                {
                    Validate(child.Value as Entity, res);
                }
            }
        }

        private static void CheckRules(Entity target, ValidationRulesManager rules, IManagedProperty property, BrokenRulesCollection brokenRulesList)
        {
            // get the rules list for this property
            RulesContainer rulesList = rules.GetRulesForProperty(property, false);
            if (rulesList != null)
            {
                // get the actual list of rules (sorted by priority)
                CheckRules(target, rulesList, brokenRulesList);
            }
        }

        /// <summary>
        /// Given a list
        /// containing IRuleMethod objects, this
        /// method executes all those rule methods.
        /// </summary>
        private static void CheckRules(Entity target, RulesContainer rules, BrokenRulesCollection brokenRulesList)
        {
            var list = rules.GetList(true);

            bool previousRuleBroken = false;

            // Lock the rules here to ensure that all rules are run before allowing
            // async rules to notify that they have completed.

            for (int index = 0; index < list.Count; index++)
            {
                IRule rule = list[index];
                // see if short-circuiting should kick in
                if (previousRuleBroken && rule.Priority > _processThroughPriority) continue;

                var args = new RuleArgs(rule);

                // we're not short-circuited, so check rule
                try
                {
                    args.BrokenDescription = null;
                    rule.ValidationRule.Validate(target, args);
                }
                catch (Exception ex)
                {
                    throw new ValidationException("Properties.Resources.ValidationRulesException" + args.Property.Name + rule.Key, ex);
                }

                if (args.IsBroken)
                {
                    // the rule is broken
                    brokenRulesList.Add(rule, args);
                    if (rule.Level == RuleLevel.Error)
                    {
                        previousRuleBroken = true;
                    }
                }
            }
        }
    }
}