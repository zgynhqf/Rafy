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
using Rafy.Utils;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 运行时动态属性的实现。
    /// 
    /// 概念：动态属性表示可以在运行时为单个实体随意添加的属性。同一类型的各个实体实例间的动态属性是互不相关的。
    /// </summary>
    /// <remarks>大部分代码拷贝自：<see cref="Extendable"/>。</remarks>
    public abstract partial class ManagedPropertyObject //: IExtendable
    {
        /// <summary>
        /// 由于运行时动态属性不会很多，所以使用 Dictionary 类来进行更快速的检索。
        /// </summary>
        private Dictionary<string, object> _dynamics;

        /// <summary>
        /// 返回当前已经添加的动态属性的个数。
        /// 概念：动态属性表示可以在运行时为单个实体随意添加的属性。同一类型的各个实体实例间的动态属性是互不相关的。
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
        /// 获取或设置动态属性的值。
        /// 
        /// 注意，如果设置 null 值，则表示清空该属性。
        /// </summary>
        /// <param name="dynamicProperty">对应的动态属性的名称。</param>
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
        /// 获取指定名称的动态属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dynamicProperty">对应的动态属性的名称。</param>
        /// <param name="defaultValue">如果属性还没有值，则返回这个默认值。</param>
        /// <returns></returns>
        public T GetDynamicPropertyOrDefault<T>(string dynamicProperty, T defaultValue = default(T))
        {
            object result;

            if (_dynamics != null)
            {
                if (_dynamics.TryGetValue(dynamicProperty, out result))
                {
                    var desiredType = typeof(T);
                    if (desiredType.IsEnum)
                    {
                        result = EnumViewModel.Parse(result as string, desiredType);
                    }
                    return (T)TypeHelper.CoerceValue(desiredType, result);
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 获取动态属性的值。
        /// </summary>
        /// <param name="dynamicProperty">对应的动态属性的名称。</param>
        /// <param name="desiredType">需要转换的类型。</param>
        /// <returns></returns>
        public object GetDynamicProperty(string dynamicProperty, Type desiredType = null)
        {
            var result = this[dynamicProperty];

            if (desiredType != null && result != null)
            {
                if (desiredType.IsEnum)
                {
                    result = EnumViewModel.Parse(result as string, desiredType);
                }
                result = TypeHelper.CoerceValue(desiredType, result);
            }

            return result;
        }

        /// <summary>
        /// 设置某个动态属性为指定的值。
        /// </summary>
        /// <param name="dynamicProperty">对应的动态属性的名称。</param>
        /// <param name="value">要设置的值。</param>
        public void SetDynamicProperty(string dynamicProperty, object value)
        {
            this[dynamicProperty] = value;
        }

        /// <summary>
        /// 设置某个动态属性为指定的值。
        /// </summary>
        /// <param name="dynamicProperty">对应的动态属性的名称。</param>
        /// <param name="value">要设置的值。</param>
        public void SetDynamicProperty(string dynamicProperty, bool value)
        {
            this[dynamicProperty] = BooleanBoxes.Box(value);
        }

        /// <summary>
        /// 获取已经设置的所有的动态属性
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
