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

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 规则验证器
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// 检查某个属性是否满足规则
        /// </summary>
        /// <param name="target">要验证的实体。</param>
        /// <param name="property">托管属性</param>
        /// <param name="actions">验证时的行为。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">target
        /// or
        /// property</exception>
        public static BrokenRulesCollection Validate(this Entity target, IManagedProperty property, ValidatorActions actions = ValidatorActions.None)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (property == null) throw new ArgumentNullException("property");

            var res = new BrokenRulesCollection();

            //获取指定实体的规则容器
            var rulesManager = ValidationHelper.GetTypeRules(target.FindRepository() as ITypeValidationsHost, target.GetType());
            if (rulesManager != null)
            {
                // get the rules list for this property
                RulesContainer rulesList = rulesManager.GetRulesForProperty(property, false);
                if (rulesList != null)
                {
                    // get the actual list of rules (sorted by priority)
                    CheckRules(target, rulesList, res, actions);
                }
            }

            return res;
        }

        /// <summary>
        /// 检查整个实体对象是否满足规则
        /// </summary>
        /// <param name="target">要验证的实体。</param>
        /// <param name="actions">验证时的行为。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">target</exception>
        public static BrokenRulesCollection Validate(this Entity target, ValidatorActions actions = ValidatorActions.None)
        {
            if (target == null) throw new ArgumentNullException("target");

            var res = new BrokenRulesCollection();

            ValidateEntity(target, res, actions);

            return res;
        }

        private static void ValidateEntity(Entity target, BrokenRulesCollection res, ValidatorActions actions)
        {
            var rules = ValidationHelper.GetTypeRules(target.FindRepository() as ITypeValidationsHost, target.GetType());
            if (rules != null)
            {
                //先验证所有属性。
                foreach (var de in rules.PropertyRules)
                {
                    CheckRules(target, de.Value, res, actions);
                }

                //再验证整个实体。
                CheckRules(target, rules.TypeRules, res, actions);
            }

            if (HasAction(actions, ValidatorActions.ValidateChildren))
            {
                foreach (var child in target.GetLoadedChildren())
                {
                    var list = child.Value as EntityList;
                    if (list != null)
                    {
                        list.EachNode(childEntity =>
                        {
                            ValidateEntity(childEntity, res, actions);
                            return false;
                        });
                    }
                    else
                    {
                        ValidateEntity(child.Value as Entity, res, actions);
                    }
                }
            }
        }

        private static void CheckRules(
            Entity target,
            RulesContainer rules, BrokenRulesCollection brokenRulesList,
            ValidatorActions actions, int processThroughPriority = 0
            )
        {
            var list = rules.GetList(true);

            bool previousRuleBroken = false;

            // Lock the rules here to ensure that all rules are run before allowing
            // async rules to notify that they have completed.

            var ignoreDataSourceValidations = HasAction(actions, ValidatorActions.IgnoreDataSourceValidations);
            var stopOnFirstBroken = HasAction(actions, ValidatorActions.StopOnFirstBroken);

            for (int index = 0; index < list.Count; index++)
            {
                IRule rule = list[index];

                //连接数据源的验证规则可能需要被过滤掉。
                if (ignoreDataSourceValidations && rule.ValidationRule.ConnectToDataSource)
                {
                    continue;
                }

                // see if short-circuiting should kick in
                if (previousRuleBroken && rule.Priority > processThroughPriority) continue;

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
                    brokenRulesList.Add(rule, args);

                    if (stopOnFirstBroken)
                    {
                        break;
                    }

                    if (rule.Level == RuleLevel.Error)
                    {
                        previousRuleBroken = true;
                    }
                }
            }
        }

        private static bool HasAction(ValidatorActions actions, ValidatorActions toCheck)
        {
            return (actions & toCheck) == toCheck;
        }
    }

    /// <summary>
    /// 验证时的行为
    /// </summary>
    [Flags]
    public enum ValidatorActions
    {
        None = 0,
        /// <summary>
        /// 验证整个实体时，是否需要验证该实体的组合子实体。
        /// </summary>
        ValidateChildren = 1,
        /// <summary>
        /// 是否需要验证连接数据源的验证规则。
        /// </summary>
        IgnoreDataSourceValidations = 2,
        /// <summary>
        /// 是否在第一规则被破坏时，即刻停止。
        /// </summary>
        StopOnFirstBroken = 4
    }
}