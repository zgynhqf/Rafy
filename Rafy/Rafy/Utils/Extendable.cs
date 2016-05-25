/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130422 17:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy;
using Rafy.Reflection;

namespace Rafy
{
    /// <summary>
    /// 一个可进行简单属性扩展的类型
    /// </summary>
    [Serializable]
    [DataContract]
    public abstract class Extendable : IExtendable
    {
        private Dictionary<string, object> _properties;

        /// <summary>
        /// WCF 序列化使用。
        /// </summary>
        [DataMember]
        private Dictionary<string, object> ExtendedProperties
        {
            get { return _properties; }
            set { _properties = value; }
        }

        /// <summary>
        /// 返回当前已经扩展的属性个数。
        /// </summary>
        public int ExtendedPropertiesCount
        {
            get
            {
                if (_properties == null) return 0;
                return _properties.Count;
            }
        }

        /// <summary>
        /// 用于扩展的属性列表。
        /// 
        /// 注意，如果设置 null 值，则表示清空该属性。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object this[string property]
        {
            get
            {
                object value = null;
                if (_properties != null)
                {
                    this._properties.TryGetValue(property, out value);
                }
                return value;
            }
            set
            {
                this.OnExtendedPropertyChanging(property, value);

                if (value == null)
                {
                    if (_properties != null)
                    {
                        _properties.Remove(property);
                        if (_properties.Count == 0) _properties = null;
                    }
                }
                else
                {
                    if (_properties == null) _properties = new Dictionary<string, object>();
                    this._properties[property] = value;
                }

                this.OnExtendedPropertyChanged(property);
            }
        }

        /// <summary>
        /// 获取指定名称的扩展属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetPropertyOrDefault<T>(string property, T defaultValue = default(T))
        {
            object result;

            if (_properties != null)
            {
                if (_properties.TryGetValue(property, out result))
                {
                    return (T)TypeHelper.CoerceValue(typeof(T), result);
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置某个扩展属性为指定的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void SetExtendedProperty(string property, object value)
        {
            this[property] = value;
        }

        /// <summary>
        /// 设置某个扩展属性为指定的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public void SetExtendedProperty(string property, bool value)
        {
            this[property] = BooleanBoxes.Box(value);
        }

        /// <summary>
        /// 获取已经设置的所有的扩展属性
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, object> GetExtendedProperties()
        {
            return _properties ?? Empty;
        }

        /// <summary>
        /// 扩展属性变化前事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected virtual void OnExtendedPropertyChanging(string property, object value) { }

        /// <summary>
        /// 扩展属性变化后事件。
        /// </summary>
        /// <param name="property"></param>
        protected virtual void OnExtendedPropertyChanged(string property) { }

        internal static IReadOnlyDictionary<string, object> Empty = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
    }
}