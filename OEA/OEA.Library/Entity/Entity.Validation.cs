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
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 实体类的一些不太重要的实现代码。
    /// </summary>
    public partial class Entity
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

        internal virtual void AddInstanceValidations() { }

        /// <summary>
        /// 重写此方法添加实体验证。
        /// 
        /// 基类中已经实现了：
        /// 为所有不可空的引用属性加上 Required 验证规则。
        /// </summary>
        internal protected virtual void AddValidations()
        {
            var rules = this.ValidationRules;

            //为所有不可空的引用属性加上 Required 验证规则。
            var properties = this.GetRepository().GetAvailableIndicators();
            foreach (var p in properties)
            {
                if (p is IRefProperty)
                {
                    var meta = p.GetMeta(this) as IRefPropertyMetadata;
                    if (!meta.Nullable)
                    {
                        rules.AddRule(p, CommonRules.Required);
                    }
                }
                //else
                //{
                //    if (p.PropertyType == typeof(DateTime))
                //    {

                //    }
                //}
            }
        }

        protected override void OnDeserialized(DesirializedArgs context)
        {
            this.ValidationRules.SetTarget(this);

            base.OnDeserialized(context);

            this.SetChildrenParent_OnDeserializaion();
        }
    }
}