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
using System.Diagnostics;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性的字段值。
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public struct ManagedPropertyField
    {
        /// <summary>
        /// 所有字段的默认值。
        /// </summary>
        private static readonly object DefaultValue = new ManagedPropertyField_DefaultValue();

        internal IManagedProperty _property;

        internal object _value;

        /// <summary>
        /// 对应的托管属性
        /// </summary>
        public IManagedProperty Property
        {
            get { return this._property; }
            //internal set { this._property = value; }
        }

        /// <summary>
        /// 字段的值
        /// </summary>
        public object Value
        {
            get { return this._value; }
            //internal set { this._value = value; }
        }

        /// <summary>
        /// 返回字段是否已经有值，而非使用默认值。
        /// 此属性不能公布：
        /// 有时 Value 属性的值会被设置为字段的默认值，虽然这时 HasValue 为真，但是值本身还是默认值，
        /// 所以不能简单地通过这个属性来判断是否本字段的值是默认值。
        /// </summary>
        internal bool HasValue
        {
            get { return this._value != DefaultValue; }
        }

        internal void ResetValue()
        {
            this._value = DefaultValue;
        }

        internal void ResetToProperty(IManagedProperty property)
        {
            this._property = property;
            this._value = DefaultValue;
        }

        private string DebuggerDisplay
        {
            get
            {
                var display = this.Property.Name;
                if (_value != DefaultValue && !object.Equals(_value, _property.DefaultMeta.DefaultValue))
                {
                    //长度小于 30，则格式化到 30 的长度。
                    while (display.Length < 30) { display += ' '; }

                    display += "   { ";
                    if (_value == null)
                    {
                        display += "null";
                    }
                    else
                    {
                        if (_value is string) { display += "'"; }
                        display += _value;
                        if (_value is string) { display += "'"; }
                    }
                    display += " }   ";
                }

                return display;
            }
        }
    }

    internal class ManagedPropertyField_DefaultValue
    {
        public override string ToString()
        {
            return "DefaultValue";
        }
    }
}