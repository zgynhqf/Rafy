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
using System.ComponentModel;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 属性变更前事件
    /// </summary>
    public interface IManagedPropertyChangingEventArgs
    {
        /// <summary>
        /// 设置本属性的值可以取消本次属性设置操作
        /// </summary>
        bool Cancel { get; set; }

        /// <summary>
        /// 设置的属性值
        /// </summary>
        object Value { get; }

        /// <summary>
        /// 是否已经变更了 CoercedValue 属性
        /// </summary>
        bool HasCoercedValue { get; }

        /// <summary>
        /// 设置本属性的值可以强制更改本次属性设置的最终值
        /// </summary>
        object CoercedValue { get; set; }

        /// <summary>
        /// 对应的属性
        /// </summary>
        IManagedProperty Property { get; }

        /// <summary>
        /// 变更源
        /// </summary>
        ManagedPropertyChangedSource Source { get; }
    }

    /// <summary>
    /// 泛型版本的属性变更前事件
    /// </summary>
    /// <typeparam name="TPropertyType">属性类型</typeparam>
    public class ManagedPropertyChangingEventArgs<TPropertyType> : CancelEventArgs, IManagedPropertyChangingEventArgs
    {
        private bool _hasCoercedValue;

        private TPropertyType _coercedValue;

        private TPropertyType _value;

        private IManagedProperty _property;

        private ManagedPropertyChangedSource _source;

        public ManagedPropertyChangingEventArgs(IManagedProperty property, TPropertyType value, ManagedPropertyChangedSource source)
        {
            this._property = property;
            this._source = source;
            this._value = value;
        }

        /// <summary>
        /// 设置的属性值
        /// </summary>
        public TPropertyType Value
        {
            get { return this._value; }
        }

        /// <summary>
        /// 设置本属性的值可以强制更改本次属性设置的最终值
        /// </summary>
        public TPropertyType CoercedValue
        {
            get { return this._coercedValue; }
            set
            {
                this._hasCoercedValue = true;
                this._coercedValue = value;
            }
        }

        /// <summary>
        /// 是否已经变更了 CoercedValue 属性
        /// </summary>
        public bool HasCoercedValue
        {
            get { return this._hasCoercedValue; }
        }

        /// <summary>
        /// 对应的属性
        /// </summary>
        public IManagedProperty Property
        {
            get { return this._property; }
        }

        /// <summary>
        /// 变更源
        /// </summary>
        public ManagedPropertyChangedSource Source
        {
            get { return this._source; }
        }

        object IManagedPropertyChangingEventArgs.Value
        {
            get { return this._value; }
        }

        object IManagedPropertyChangingEventArgs.CoercedValue
        {
            get { return this._coercedValue; }
            set { this.CoercedValue = (TPropertyType)value; }
        }
    }
}