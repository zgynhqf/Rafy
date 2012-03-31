/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 适配到 Entity、托管属性上。 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using OEA.ManagedProperty;

namespace OEA.Library.Validation
{
    /// <summary>
    /// Maintains rule methods for a business object
    /// or business object type.
    /// </summary>
    public class ValidationRulesManager
    {
        private RulesContainer _typeRules = new RulesContainer();

        private Dictionary<IManagedProperty, RulesContainer> _propertyRulesList;

        /// <summary>
        /// Key: IManagedProperty
        /// Value: Rules
        /// </summary>
        internal Dictionary<IManagedProperty, RulesContainer> PropertyRules
        {
            get
            {
                if (_propertyRulesList == null) _propertyRulesList = new Dictionary<IManagedProperty, RulesContainer>();
                return _propertyRulesList;
            }
        }

        /// <summary>
        /// 这些规则不与某个属性关联，是直接作用在整个实体上的。
        /// </summary>
        internal RulesContainer TypeRules
        {
            get { return this._typeRules; }
        }

        internal RulesContainer GetRulesForProperty(IManagedProperty property, bool createList)
        {
            // get the list (if any) from the dictionary
            RulesContainer list = null;
            PropertyRules.TryGetValue(property, out list);

            if (createList && list == null)
            {
                // there is no list for this name - create one
                list = new RulesContainer();
                PropertyRules.Add(property, list);
            }

            return list;
        }

        public void AddRule(RuleHandler handler, RuleArgs args, int priority)
        {
            if (args.Property != null)
            {
                // we have the list, add our new rule
                GetRulesForProperty(args.Property, true)
                    .Add(new RuleMethod(handler, args, priority));
            }
            else
            {
                this._typeRules
                    .Add(new RuleMethod(handler, args, priority));
            }
        }
    }
}