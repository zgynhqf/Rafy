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

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 托管属性元数据
    /// </summary>
    public interface IManagedPropertyMetadata
    {
        /// <summary>
        /// 默认值
        /// </summary>
        object DefaultValue { get; }
    }

    /// <summary>
    /// 内部使用的 托管属性元数据
    /// </summary>
    internal interface IManagedPropertyMetadataInternal : IManagedPropertyMetadata
    {
        bool RaisePropertyChangingMetaEvent(ManagedPropertyObject sender, ref object value, ManagedPropertyChangedSource source);

        object CoerceGetValue(ManagedPropertyObject sender, object value);

        IManagedPropertyChangedEventArgs RaisePropertyChangedMetaEvent(ManagedPropertyObject sender, object oldValue, object newValue, ManagedPropertyChangedSource source);
    }

    /// <summary>
    /// 泛型版本的托管属性元数据
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class ManagedPropertyMetadata<TPropertyType> : Freezable, IManagedPropertyMetadata, IManagedPropertyMetadataInternal
    {
        #region 字段

        private ManagedProperty<TPropertyType> _property;

        private ManagedPropertyCoerceGetValueCallBack<TPropertyType> _coerceGetValueCallBack;

        private ManagedPropertyChangingCallBack<TPropertyType> _propertyChangingCallBack;

        private ManagedPropertyChangedCallBack<TPropertyType> _propertyChangedCallBack;

        #endregion

        /// <summary>
        /// 默认值
        /// get { return (TPropertyType)this._provider.GetDefaultValue(); }
        /// </summary>
        public TPropertyType DefaultValue { get; set; }

        /// <summary>
        /// 属性获取时的强制逻辑回调
        /// </summary>
        public ManagedPropertyCoerceGetValueCallBack<TPropertyType> CoerceGetValueCallBack
        {
            get { return this._coerceGetValueCallBack; }
            set { this._coerceGetValueCallBack = value; }
        }

        /// <summary>
        /// 属性变更前回调
        /// </summary>
        public ManagedPropertyChangingCallBack<TPropertyType> PropertyChangingCallBack
        {
            get { return this._propertyChangingCallBack; }
            set { this._propertyChangingCallBack = value; }
        }

        /// <summary>
        /// 属性变更后回调
        /// </summary>
        public ManagedPropertyChangedCallBack<TPropertyType> PropertyChangedCallBack
        {
            get { return this._propertyChangedCallBack; }
            set { this._propertyChangedCallBack = value; }
        }

        internal void SetProperty(ManagedProperty<TPropertyType> property)
        {
            this._property = property;
        }

        #region CoerceGetValue

        internal TPropertyType CoerceGetValue(ManagedPropertyObject sender, TPropertyType value)
        {
            if (this._coerceGetValueCallBack != null)
            {
                return this._coerceGetValueCallBack(sender, value);
            }

            return value;
        }

        object IManagedPropertyMetadataInternal.CoerceGetValue(ManagedPropertyObject sender, object value)
        {
            if (this._coerceGetValueCallBack != null)
            {
                return this._coerceGetValueCallBack(sender, (TPropertyType)value);
            }

            return value;
        }

        #endregion

        #region RaisePropertyChanging

        internal bool RaisePropertyChangingMetaEvent(ManagedPropertyObject sender, ref TPropertyType value, ManagedPropertyChangedSource source)
        {
            if (this._propertyChangingCallBack != null)
            {
                var e = new ManagedPropertyChangingEventArgs<TPropertyType>(this._property, value, source);

                this._propertyChangingCallBack(sender, e);

                if (e.HasCoercedValue) { value = e.CoercedValue; }

                return e.Cancel;
            }

            return false;
        }

        bool IManagedPropertyMetadataInternal.RaisePropertyChangingMetaEvent(
            ManagedPropertyObject sender, ref object value, ManagedPropertyChangedSource source
            )
        {
            if (this._propertyChangingCallBack != null)
            {
                var e = new ManagedPropertyChangingEventArgs<TPropertyType>(this._property, (TPropertyType)value, source);

                this._propertyChangingCallBack(sender, e);

                if (e.HasCoercedValue) { value = e.CoercedValue; }

                return e.Cancel;
            }

            return false;
        }

        #endregion

        #region RaisePropertyChanged

        internal ManagedPropertyChangedEventArgs<TPropertyType> RaisePropertyChangedMetaEvent(
            ManagedPropertyObject sender,
            TPropertyType oldValue, TPropertyType newValue, ManagedPropertyChangedSource source
            )
        {
            var args = new ManagedPropertyChangedEventArgs<TPropertyType>(this._property, oldValue, newValue, source);

            if (this._propertyChangedCallBack != null) { this._propertyChangedCallBack(sender, args); }

            return args;
        }

        IManagedPropertyChangedEventArgs IManagedPropertyMetadataInternal.RaisePropertyChangedMetaEvent(
            ManagedPropertyObject sender,
            object oldValue, object newValue, ManagedPropertyChangedSource source
            )
        {
            return this.RaisePropertyChangedMetaEvent(sender, (TPropertyType)oldValue, (TPropertyType)newValue, source);
        }

        #endregion

        #region 隐式接口实现

        object IManagedPropertyMetadata.DefaultValue
        {
            get { return this.DefaultValue; }
        }

        #endregion
    }

    /// <summary>
    /// 属性获取时的强制逻辑回调
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="o"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public delegate TPropertyType ManagedPropertyCoerceGetValueCallBack<TPropertyType>(ManagedPropertyObject o, TPropertyType value);

    /// <summary>
    /// 属性变更前回调
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="o"></param>
    /// <param name="e"></param>
    public delegate void ManagedPropertyChangingCallBack<TPropertyType>(ManagedPropertyObject o, ManagedPropertyChangingEventArgs<TPropertyType> e);

    /// <summary>
    /// 属性变更后回调
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    /// <param name="o"></param>
    /// <param name="e"></param>
    public delegate void ManagedPropertyChangedCallBack<TPropertyType>(ManagedPropertyObject o, ManagedPropertyChangedEventArgs<TPropertyType> e);
}