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
using System.Runtime;

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

        /// <summary>
        /// 重写此方法添加该实体特定的验证规则。
        /// 
        /// 此方法中只能调用<see cref="ValidationRules.AddInstanceRule"/> 方法。
        /// </summary>
        internal virtual void AddInstanceValidations() { }

        /// <summary>
        /// 重写此方法添加实体验证。
        /// 
        /// 此方法中只能调用<see cref="ValidationRules.AddRule"/>方法。
        /// 
        /// 基类中已经实现了：
        /// 为所有不可空的引用属性加上 Required 验证规则。
        /// </summary>
        internal protected virtual void AddValidations()
        {
            var rules = this.ValidationRules;

            //为所有不可空的引用属性加上 Required 验证规则。
            var properties = this.PropertiesContainer.GetAvailableProperties();
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

        /// <summary>
        /// 通知上层应用，需要重新验证某个指定的属性。
        /// 
        /// 一般在某个属性变更时调用此方法来通知另一属性需要进行验证。
        /// </summary>
        /// <param name="properties"></param>
        public void Revalidate(params IProperty[] properties)
        {
            //目前直接用属性变更事件来通知上层的 Binding 重新
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    this.Revalidate(property);
                }
            }
        }

        /// <summary>
        /// 通知上层应用，需要重新验证某个指定的属性。
        /// 
        /// 一般在某个属性变更时调用此方法来通知另一属性需要进行验证。
        /// </summary>
        /// <param name="property"></param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Revalidate(IProperty property)
        {
            //目前直接用属性变更事件来通知上层的 Binding 重新
            this.OnPropertyChanged(property.Name);
        }
    }
}