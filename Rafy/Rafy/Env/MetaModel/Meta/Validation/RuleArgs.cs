﻿/*******************************************************
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

namespace Rafy.MetaModel
{
    /// <summary>
    /// 为业务规则验证方法提供一些必要的参数。
    /// 
    /// 该类继承自动态类型，意味着定义时可动态定义属性。
    /// </summary>
    public sealed class RuleArgs
    {
        internal RuleArgs(IRule rule)
        {
            this.Rule = rule;
        }

        /// <summary>
        /// 对应的规则。
        /// </summary>
        public IRule Rule { get; private set; }

        /// <summary>
        /// 如果这是某个属性关联的规则参数，则这个属性表示关联的托管属性
        /// </summary>
        public IManagedProperty Property
        {
            get { return this.Rule.Property; }
        }

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

        #region Display

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取属性对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[属性名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <returns></returns>
        public string DisplayProperty(object entity)
        {
            return Display(this.Property, entity);
        }

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取属性对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[属性名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string Display(IManagedProperty property, object entity)
        {
            return RafyEnvironment.Provider.GetLabelForDisplay(property, entity.GetType());
        }

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取实体对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[实体类型名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string Display(Type entityType)
        {
            return RafyEnvironment.Provider.GetLabelForDisplay(entityType);
        }

        #endregion
    }
}