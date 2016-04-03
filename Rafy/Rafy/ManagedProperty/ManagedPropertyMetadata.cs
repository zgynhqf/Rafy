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

namespace Rafy.ManagedProperty
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

        /// <summary>
        /// 是否支持序列化
        /// </summary>
        bool Serializable { get; }
    }

    /// <summary>
    /// 内部使用的 托管属性元数据
    /// </summary>
    internal interface IManagedPropertyMetadataInternal : IManagedPropertyMetadata
    {
        object CoerceGetValue(ManagedPropertyObject sender, object value);

        bool RaisePropertyChangingMetaEvent(ManagedPropertyObject sender, ref object value, ManagedPropertyChangedSource source);

        void RaisePropertyChangedMetaEvent(ManagedPropertyObject sender, ManagedPropertyChangedEventArgs e);
    }

    /// <summary>
    /// 泛型版本的托管属性元数据
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class ManagedPropertyMetadata<TPropertyType> : FreezableMeta, IManagedPropertyMetadata, IManagedPropertyMetadataInternal
    {
        #region 字段

        private ManagedProperty<TPropertyType> _property;

        private ManagedPropertyCoerceGetValueCallBack<TPropertyType> _coerceGetValueCallBack;

        private ManagedPropertyChangingCallBack<TPropertyType> _propertyChangingCallBack;

        private ManagedPropertyChangedCallBack _propertyChangedCallBack;

        private bool _serializable = true;

        private TPropertyType _defaultValue;

        private object _defaultValueBox;

        #endregion

        /// <summary>
        /// 默认值
        /// get { return (TPropertyType)this._provider.GetDefaultValue(); }
        /// </summary>
        public TPropertyType DefaultValue
        {
            get { return this._defaultValue; }
            set
            {
                this.CheckUnFrozen();
                this._defaultValue = value;
                this._defaultValueBox = value;
            }
        }

        /// <summary>
        /// 是否支持序列化。
        /// 默认为 true。
        /// </summary>
        public bool Serializable
        {
            get { return this._serializable; }
            set
            {
                this.CheckUnFrozen();
                this._serializable = value;
            }
        }

        /// <summary>
        /// 属性获取时的强制逻辑回调
        /// </summary>
        public ManagedPropertyCoerceGetValueCallBack<TPropertyType> CoerceGetValueCallBack
        {
            get { return this._coerceGetValueCallBack; }
            set
            {
                this.CheckUnFrozen();
                this._coerceGetValueCallBack = value;
            }
        }

        /// <summary>
        /// 属性变更前回调
        /// </summary>
        public ManagedPropertyChangingCallBack<TPropertyType> PropertyChangingCallBack
        {
            get { return this._propertyChangingCallBack; }
            set
            {
                this.CheckUnFrozen();
                this._propertyChangingCallBack = value;
            }
        }

        /// <summary>
        /// 属性变更后回调
        /// </summary>
        public ManagedPropertyChangedCallBack PropertyChangedCallBack
        {
            get { return this._propertyChangedCallBack; }
            set
            {
                this.CheckUnFrozen();
                this._propertyChangedCallBack = value;
            }
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

        void IManagedPropertyMetadataInternal.RaisePropertyChangedMetaEvent(ManagedPropertyObject sender, ManagedPropertyChangedEventArgs e)
        {
            if (this._propertyChangedCallBack != null) { this._propertyChangedCallBack(sender, e); }
        }

        #endregion

        #region 隐式接口实现

        object IManagedPropertyMetadata.DefaultValue
        {
            get { return this._defaultValueBox; }
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
    /// <param name="o">The automatic.</param>
    /// <param name="e">The <see cref="ManagedPropertyChangedEventArgs"/> instance containing the event data.</param>
    public delegate void ManagedPropertyChangedCallBack(ManagedPropertyObject o, ManagedPropertyChangedEventArgs e);
}