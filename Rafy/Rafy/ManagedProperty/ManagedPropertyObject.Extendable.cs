/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160519
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160519 21:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Reflection;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 运行时动态扩展属性的实现。
    /// </summary>
    /// <remarks>大部分代码拷贝自：<see cref="Extendable"/>。</remarks>
    public abstract partial class ManagedPropertyObject //: IExtendable
    {
        /// <summary>
        /// 由于运行时属性不会很多，所以使用 Dictionary 类来进行更快速的检索。
        /// </summary>
        private Dictionary<string, object> _dynamics;

        /// <summary>
        /// 返回当前已经扩展的属性个数。
        /// </summary>
        public int DynamicPropertiesCount
        {
            get
            {
                if (_dynamics == null) return 0;
                return _dynamics.Count;
            }
        }

        /// <summary>
        /// 用于扩展的属性列表。
        /// 
        /// 注意，如果设置 null 值，则表示清空该属性。
        /// </summary>
        /// <param name="dynamicProperty"></param>
        /// <returns></returns>
        public object this[string dynamicProperty]
        {
            get
            {
                object value = null;
                if (_dynamics != null)
                {
                    this._dynamics.TryGetValue(dynamicProperty, out value);
                }
                return value;
            }
            private set
            {
                //this.OnDynamicPropertyChanging(dynamicProperty, value);

                if (value == null)
                {
                    if (_dynamics != null)
                    {
                        _dynamics.Remove(dynamicProperty);
                        if (_dynamics.Count == 0) _dynamics = null;
                    }
                }
                else
                {
                    if (_dynamics == null) _dynamics = new Dictionary<string, object>();
                    this._dynamics[dynamicProperty] = value;
                }

                this.OnPropertyChanged(dynamicProperty);
            }
        }

        /// <summary>
        /// 获取指定名称的扩展属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynamicProperty"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetDynamicPropertyOrDefault<T>(string dynamicProperty, T defaultValue = default(T))
        {
            object result;

            if (_dynamics != null)
            {
                if (_dynamics.TryGetValue(dynamicProperty, out result))
                {
                    return (T)TypeHelper.CoerceValue(typeof(T), result);
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 设置某个扩展属性为指定的值。
        /// </summary>
        /// <param name="dynamicProperty"></param>
        /// <param name="value"></param>
        public void SetDynamicProperty(string dynamicProperty, object value)
        {
            this[dynamicProperty] = value;
        }

        /// <summary>
        /// 设置某个扩展属性为指定的值。
        /// </summary>
        /// <param name="dynamicProperty"></param>
        /// <param name="value"></param>
        public void SetDynamicProperty(string dynamicProperty, bool value)
        {
            this[dynamicProperty] = BooleanBoxes.Box(value);
        }

        /// <summary>
        /// 获取已经设置的所有的扩展属性
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, object> GetDynamicProperties()
        {
            return _dynamics ?? Extendable.Empty;
        }

        ///// <summary>
        ///// 扩展属性变化前事件。
        ///// </summary>
        ///// <param name="dynamicProperty"></param>
        ///// <param name="value"></param>
        //protected virtual void OnDynamicPropertyChanging(string dynamicProperty, object value) { }

        ///// <summary>
        ///// 扩展属性变化后事件。
        ///// </summary>
        ///// <param name="dynamicProperty"></param>
        //protected virtual void OnDynamicPropertyChanged(string dynamicProperty)
        //{
        //    this.OnPropertyChanged(dynamicProperty);
        //}
    }
}
