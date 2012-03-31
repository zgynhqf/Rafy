/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Linq.Expressions;
using OEA.ManagedProperty;
using OEA.Serialization;
using OEA.Library.Validation;
using System.ComponentModel;

namespace OEA.Library
{
    /// <summary>
    /// 实体类的一些不太重要的实现代码。
    /// </summary>
    public partial class Entity : IValidatable, IDataErrorInfo
    {
        private ValidationRules _validationRules;

        /// <summary>
        /// Provides access to the broken rules functionality.
        /// </summary>
        /// <remarks>
        /// This property is used within your business logic so you can
        /// easily call the AddRule() method to associate validation
        /// rules with your object's properties.
        /// </remarks>
        public ValidationRules ValidationRules
        {
            get
            {
                if (_validationRules == null)
                {
                    _validationRules = new ValidationRules(this);
                }
                else if (_validationRules.Target == null)
                {
                    _validationRules.SetTarget(this);
                }

                return _validationRules;
            }
        }

        /// <summary>
        /// 检查整个实体对象是否满足规则
        /// </summary>
        public BrokenRulesCollection Validate()
        {
            return this.ValidationRules.CheckRules();
        }

        /// <summary>
        /// 检查某个属性是否满足规则
        /// </summary>
        /// <param name="property">托管属性</param>
        public BrokenRulesCollection Validate(IManagedProperty property)
        {
            return this.ValidationRules.CheckRules(property);
        }

        internal virtual void AddInstanceValidations() { }

        /// <summary>
        /// 重写此方法添加实体验证。
        /// </remarks>
        internal protected virtual void AddValidations() { }

        protected override void OnDeserialized(DesirializedArgs context)
        {
            this.ValidationRules.SetTarget(this);

            base.OnDeserialized(context);
        }

        #region IDataErrorInfo Memebers

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                var allIndicators = this.FindRepository().GetAvailableIndicators();
                var property = allIndicators.FirstOrDefault(p => p.Name == columnName);
                if (property != null)
                {
                    var broken = this.Validate(property);
                    if (broken.Count > 0) return broken.ToString();
                }

                return null;
            }
        }

        #endregion
    }
}