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
        /// <summary>
        /// 获取指定实体类型对应的验证规则声明器。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal static ValidationDeclarer For(Type entityType)
        {
            return new ValidationDeclarer { EntityType = entityType };
        }

        private ValidationRulesManager _rules;

        private ValidationDeclarer() { }

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

        public ValidationRulesManager Rules
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
        /// <param name="parameters">The parameters.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        public void AddRule(RuleHandler handler,
           object parameters = null, RuleLevel level = RuleLevel.Error, int priority = 0
           )
        {
            EnsureHandler(handler);

            var args = new RuleArgs { Level = level };

            if (parameters != null)
            {
                var properties = TypeDescriptor.GetProperties(parameters);
                foreach (PropertyDescriptor propertyDescriptor in properties)
                {
                    object value = propertyDescriptor.GetValue(parameters);
                    args[propertyDescriptor.Name] = value;
                }
            }

            Rules.AddRule(handler, args, priority);
        }

        /// <summary>
        /// 为某个属性添加一个业务验证规则。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        public void AddRule(IManagedProperty property, RuleHandler handler,
           object parameters = null, RuleLevel level = RuleLevel.Error, int priority = 0
           )
        {
            EnsureHandler(handler);

            var args = new RuleArgs
            {
                Property = property,
                Level = level
            };

            if (parameters != null)
            {
                var properties = TypeDescriptor.GetProperties(parameters);
                foreach (PropertyDescriptor propertyDescriptor in properties)
                {
                    object value = propertyDescriptor.GetValue(parameters);
                    args[propertyDescriptor.Name] = value;
                }
            }

            Rules.AddRule(handler, args, priority);
        }

        public void ClearRules()
        {
            Rules.ClearRules();
        }

        public void ClearRules(IManagedProperty property)
        {
            Rules.ClearRules(property);
        }

        private static void EnsureHandler(RuleHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var method = handler.Method;
            if (!method.IsStatic) throw new InvalidOperationException(string.Format("InvalidRuleMethodException: {0}", method.Name));
            //if (!method.IsStatic && method.DeclaringType.IsInstanceOfType(_target))
            //    throw new InvalidOperationException(string.Format("{0}: {1}", "Properties.Resources.InvalidRuleMethodException", method.Name));
        }
    }
}