/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 非泛型的接口实现使得值的获取和设置都可以由框架直接在 object 层面上进行调用。
    /// </summary>
    public interface IManagedPropertyField
    {
        /// <summary>
        /// 对应的托管属性
        /// </summary>
        IManagedProperty Property { get; }

        /// <summary>
        /// 字段的值
        /// </summary>
        object Value { get; set; }
    }

    /// <summary>
    /// 泛型的实现使得值的存储不再需要装箱拆箱，更快。
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    [Serializable]
    internal class ManagedPropertyField<TPropertyType> : IManagedPropertyField
    {
        private ManagedProperty<TPropertyType> _property;

        private TPropertyType _value;

        internal ManagedPropertyField(ManagedProperty<TPropertyType> property)
        {
            this._property = property;
        }

        /// <summary>
        /// 对应的托管属性
        /// </summary>
        public ManagedProperty<TPropertyType> Property
        {
            get { return this._property; }
        }

        /// <summary>
        /// 字段的值
        /// </summary>
        public TPropertyType Value
        {
            get { return this._value; }
            set { this._value = value; }
        }

        IManagedProperty IManagedPropertyField.Property
        {
            get { return this._property; }
        }

        object IManagedPropertyField.Value
        {
            get { return this._value; }
            set { this._value = (TPropertyType)value; }
        }
    }
}