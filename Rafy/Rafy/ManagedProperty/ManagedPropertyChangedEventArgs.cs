/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 2.0版本，由泛型加接口的方式变为完全的结构体，以 object 类型作为属性的类型。 胡庆访 20121118
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
    /// 泛型版本的属性变更事件
    /// </summary>
    public struct ManagedPropertyChangedEventArgs
    {
        private object _newValue;

        private object _oldValue;

        private IManagedProperty _property;

        private ManagedPropertyChangedSource _source;

        public ManagedPropertyChangedEventArgs(IManagedProperty property, object oldValue, object newValue, ManagedPropertyChangedSource source)
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
        public object NewValue
        {
            get { return this._newValue; }
        }

        /// <summary>
        /// 变更前的值
        /// 
        /// 注意，如果是只读属性，则这个值永远是默认值。
        /// </summary>
        public object OldValue
        {
            get { return this._oldValue; }
        }

        /// <summary>
        /// 对应的托管属性
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
    }
}