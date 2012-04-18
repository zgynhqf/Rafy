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

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 属性变更事件
    /// </summary>
    public interface IManagedPropertyChangedEventArgs
    {
        /// <summary>
        /// 变更后的值
        /// </summary>
        object NewValue { get; }

        /// <summary>
        /// 变更前的值
        /// </summary>
        object OldValue { get; }

        /// <summary>
        /// 对应的托管属性
        /// </summary>
        IManagedProperty Property { get; }

        /// <summary>
        /// 变更源
        /// </summary>
        ManagedPropertyChangedSource Source { get; }
    }

    /// <summary>
    /// 泛型版本的属性变更事件
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class ManagedPropertyChangedEventArgs<TPropertyType> : EventArgs, IManagedPropertyChangedEventArgs
    {
        private TPropertyType _newValue;

        private TPropertyType _oldValue;

        private ManagedProperty<TPropertyType> _property;

        private ManagedPropertyChangedSource _source;

        public ManagedPropertyChangedEventArgs(ManagedProperty<TPropertyType> property, TPropertyType oldValue, TPropertyType newValue, ManagedPropertyChangedSource source)
        {
            this._property = property;
            this._oldValue = oldValue;
            this._newValue = newValue;
            this._source = source;
        }

        /// <summary>
        /// 变更后的值
        /// 
        /// 注意，如果是只读属性，则这个值永远是默认值。
        /// </summary>
        public TPropertyType NewValue
        {
            get { return this._newValue; }
        }

        /// <summary>
        /// 变更前的值
        /// 
        /// 注意，如果是只读属性，则这个值永远是默认值。
        /// </summary>
        public TPropertyType OldValue
        {
            get { return this._oldValue; }
        }

        /// <summary>
        /// 对应的托管属性
        /// </summary>
        public ManagedProperty<TPropertyType> Property
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

        object IManagedPropertyChangedEventArgs.NewValue
        {
            get { return this._newValue; }
        }

        object IManagedPropertyChangedEventArgs.OldValue
        {
            get { return this._oldValue; }
        }

        IManagedProperty IManagedPropertyChangedEventArgs.Property
        {
            get { return this._property; }
        }
    }
}