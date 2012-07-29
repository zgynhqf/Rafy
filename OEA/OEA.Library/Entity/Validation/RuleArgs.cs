/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120330
 * 
*******************************************************/

using System;
using OEA.ManagedProperty;
using System.Dynamic;
using OEA.MetaModel.View;
using OEA.MetaModel;
using System.Collections.Generic;
using OEA.Reflection;

namespace OEA.Library.Validation
{
    /// <summary>
    /// 为业务规则验证方法提供一些必要的参数。
    /// 
    /// 该类继承自动态类型，意味着定义时可动态定义属性。
    /// </summary>
    public sealed class RuleArgs : ICustomParamsHolder
    {
        /// <summary>
        /// 如果这是某个属性关联的规则参数，则这个属性表示关联的托管属性
        /// </summary>
        public IManagedProperty Property { get; internal set; }

        /// <summary>
        /// Gets or sets the severity of the broken rule.
        /// </summary>
        /// <value>The severity of the broken rule.</value>
        /// <remarks>
        /// Setting this property only has an effect if
        /// the rule method returns <see langword="false" />.
        /// </remarks>
        public RuleLevel Level { get; internal set; }

        /// <summary>
        /// 返回是否执行打破了规则。
        /// </summary>
        public bool IsBroken
        {
            get { return !string.IsNullOrEmpty(this.BrokenDescription); }
        }

        /// <summary>
        /// 在规则检查函数中描述当前的错误信息。
        /// </remarks>
        public string BrokenDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this
        /// broken rule should stop the processing of subsequent
        /// rules for this property.
        /// </summary>
        /// <value><see langword="true" /> if no further
        /// rules should be process for this property.</value>
        /// <remarks>
        /// Setting this property only has an effect if
        /// the rule method returns <see langword="false" />.
        /// </remarks>
        public bool StopProcessing { get; set; }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var p = this.Property;
            if (p != null) return this.Property.Name;

            return "TypeRules";
        }

        #region GetPropertyDisplay

        private static EntityViewMeta _lastViewMeta;

        /// <summary>
        /// 获取当前属性的显示名称。
        /// 
        /// 并尝试以一种线程安全的方式去缓存最近一次使用的 EVM。
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public string GetPropertyDisplay()
        {
            EntityViewMeta safeView = _lastViewMeta;

            var ownerType = this.Property.OwnerType;
            if (safeView == null || safeView.EntityType != ownerType)
            {
                safeView = UIModel.Views.CreateDefaultView(ownerType);
                _lastViewMeta = safeView;
            }

            string res = null;

            var pvm = safeView.Property(this.Property);
            if (pvm != null) res = pvm.Label;

            return res ?? this.Property.Name;
        }

        #endregion

        #region ICustomParamsHolder Members

        private Dictionary<string, object> _customParams = new Dictionary<string, object>();

        /// <summary>
        /// 获取指定参数的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public T TryGetCustomParams<T>(string paramName)
        {
            object result;

            if (_customParams.TryGetValue(paramName, out result))
            {
                return (T)TypeHelper.CoerceValue(typeof(T), result);
            }

            return default(T);
        }

        /// <summary>
        /// 设置自定义参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        public void SetCustomParams(string paramName, object value)
        {
            this._customParams[paramName] = value;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllCustomParams()
        {
            return this._customParams;
        }

        #endregion
    }
}