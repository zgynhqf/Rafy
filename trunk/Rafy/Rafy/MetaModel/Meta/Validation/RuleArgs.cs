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
using System.Collections.Generic;
using System.Dynamic;
using Rafy;
using Rafy.Reflection;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 为业务规则验证方法提供一些必要的参数。
    /// 
    /// 该类继承自动态类型，意味着定义时可动态定义属性。
    /// </summary>
    public sealed class RuleArgs : Extendable
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
        /// </summary>
        /// <value>
        /// The broken description.
        /// </value>
        public string BrokenDescription { get; set; }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        public override string ToString()
        {
            var p = this.Property;
            if (p != null) return this.Property.Name;

            return "TypeRules";
        }

        #region DisplayProperty

        private static EntityViewMeta _lastViewMeta;

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取属性对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[属性名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <returns></returns>
        public string DisplayProperty()
        {
            if (RafyEnvironment.Location.IsWebUI || RafyEnvironment.Location.IsWPFUI)
            {
                //以线程安全的方式获取最后一次缓存的 View。
                EntityViewMeta safeView = _lastViewMeta;
                var ownerType = this.Property.OwnerType;
                if (safeView == null || safeView.EntityType != ownerType)
                {
                    safeView = UIModel.Views.CreateBaseView(ownerType);
                    _lastViewMeta = safeView;
                }

                string res = null;

                var pvm = safeView.Property(this.Property);
                if (pvm != null) res = pvm.Label;

                //如果是引用 Id 属性没有配置 Label，则尝试使用它对应的引用实体属性的 Label 来显示。
                if (string.IsNullOrEmpty(res))
                {
                    var refMP = this.Property as IRefIdProperty;
                    if (refMP != null)
                    {
                        pvm = safeView.Property(refMP.RefEntityProperty);
                        if (pvm != null) res = pvm.Label;
                    }
                }

                if (!string.IsNullOrEmpty(res))
                {
                    return res.Translate();
                }
            }

            return '[' + this.Property.Name + ']';
        }

        #endregion
    }
}