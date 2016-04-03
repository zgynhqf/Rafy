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
using System.Linq;
using System.Collections.Generic;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// Maintains rule methods for a business object
    /// or business object type.
    /// </summary>
    internal class ValidationRulesManager
    {
        private RulesContainer _typeRules = new RulesContainer();

        private Dictionary<IManagedProperty, RulesContainer> _propertyRulesList;

        /// <summary>
        /// 这些规则不与某个属性关联，是直接作用在整个实体上的。
        /// </summary>
        internal RulesContainer TypeRules
        {
            get { return this._typeRules; }
        }

        internal Dictionary<IManagedProperty, RulesContainer> PropertyRules
        {
            get
            {
                if (_propertyRulesList == null)
                {
                    _propertyRulesList = new Dictionary<IManagedProperty, RulesContainer>();
                }
                return _propertyRulesList;
            }
        }

        /// <summary>
        /// 获取指定属性对应的属性规则容器。
        /// </summary>
        /// <param name="property">托管属性。</param>
        /// <param name="createList">如果还没有创建容器，是否需要同时创建该容器。</param>
        /// <returns></returns>
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

        /// <summary>
        /// 所有类型规则的总数。
        /// </summary>
        public int TypeRulesCount
        {
            get { return _typeRules.Count; }
        }

        /// <summary>
        /// 所有属性规则的总数。
        /// </summary>
        public int PropertyRulesCount
        {
            get
            {
                if (_propertyRulesList == null) return 0;
                return _propertyRulesList.Values.Sum(c => c.Count);
            }
        }

        public void AddRule(Rule rule)
        {
            if (rule.Property != null)
            {
                // we have the list, add our new rule
                GetRulesForProperty(rule.Property, true).Add(rule);
            }
            else
            {
                _typeRules.Add(rule);
            }
        }

        public void ClearRules()
        {
            _typeRules.Clear();
            _propertyRulesList = null;
        }

        public void ClearRules(IManagedProperty property)
        {
            _propertyRulesList.Remove(property);
        }
    }
}