/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140116
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140116 12:01
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 用于声明验证规则的类型。
    /// </summary>
    internal class ValidationDeclarer : IValidationDeclarer
    {
        private ValidationRulesManager _rules;

        /// <summary>
        /// 获取指定实体类型对应的验证规则声明器。
        /// </summary>
        internal ValidationDeclarer(Type entityType)
        {
            this.EntityType = entityType;
        }

        /// <summary>
        /// 对应的实体类型。
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 获取当前已经声明的规则的个数。
        /// </summary>
        public int RulesCount
        {
            get
            {
                if (_rules == null) return 0;
                return _rules.TypeRulesCount + _rules.PropertyRulesCount;
            }
        }

        internal ValidationRulesManager Rules
        {
            get
            {
                if (_rules == null)
                {
                    _rules = ValidationHelper.GetTypeRules(this.EntityType);
                }
                return _rules;
            }
        }

        /// <summary>
        /// 为整个实体添加一个业务验证规则。
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="System.ArgumentNullException">handler</exception>
        public void AddRule(RuleHandler handler,
           RuleLevel level = RuleLevel.Error, int priority = 0
           )
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var valiRule = new HandlerRule { Handler = handler };

            this.AddRule(valiRule, level, priority);
        }

        /// <summary>
        /// 为某个属性添加一个业务验证规则。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="System.ArgumentNullException">handler</exception>
        public void AddRule(IManagedProperty property, RuleHandler handler,
           RuleLevel level = RuleLevel.Error, int priority = 0
           )
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var valiRule = new HandlerRule { Handler = handler };

            this.AddRule(property, valiRule, level, priority);
        }

        /// <summary>
        /// 为整个实体添加一个业务验证规则。
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="System.NotSupportedException">验证规则必须从 ValidationRule 类型继承。</exception>
        public void AddRule(IValidationRule rule, RuleLevel level = RuleLevel.Error, int priority = 0)
        {
            //var internalRule = rule as ValidationRule;
            //if (internalRule == null) throw new NotSupportedException("验证规则必须从 ValidationRule 类型继承。");

            //this.AddRule(internalRule.RuleHandler, null, level, priority);

            //if (parameters != null)
            //{
            //    var properties = TypeDescriptor.GetProperties(parameters);
            //    foreach (PropertyDescriptor propertyDescriptor in properties)
            //    {
            //        object value = propertyDescriptor.GetValue(parameters);
            //        args[propertyDescriptor.Name] = value;
            //    }
            //}

            if (rule == null) throw new ArgumentNullException("rule");

            var innerRule = new Rule(rule);
            innerRule.Level = level;
            innerRule.Priority = priority;

            Rules.AddRule(innerRule);
        }

        /// <summary>
        /// 为某个属性添加一个业务验证规则。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="rule">The rule.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="System.NotSupportedException">验证规则必须从 ValidationRule 类型继承。</exception>
        public void AddRule(IManagedProperty property, IValidationRule rule, RuleLevel level = RuleLevel.Error, int priority = 0)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (rule == null) throw new ArgumentNullException("rule");

            var innerRule = new Rule(rule);
            innerRule.Property = property;
            innerRule.Level = level;
            innerRule.Priority = priority;

            Rules.AddRule(innerRule);
        }

        public void ClearRules()
        {
            Rules.ClearRules();
        }

        public void ClearRules(IManagedProperty property)
        {
            Rules.ClearRules(property);
        }
    }
}