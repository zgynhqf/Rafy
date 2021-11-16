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
using Rafy.Serialization;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性的字段状态及存储状态（只读结构体）
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public struct ManagedPropertyField
    {
        internal IManagedProperty _property;

        /// <summary>
        /// 由于本类型是结构体，所以使用以下三种方式来表示值：
        /// * null 表示默认值；
        /// * 其它值表示本地值；
        /// </summary>
        private object _value;

        /// <summary>
        /// 内部存储的状态。
        /// </summary>
        private MPFStatus _status;

        /// <summary>
        /// 对应的托管属性
        /// </summary>
        public IManagedProperty Property { get { return _property; } }

        /// <summary>
        /// 字段的值
        /// 该属性的值是内部存储的值，未进行类型转换。如果需要默认值，请重新调用 <see cref="ManagedPropertyObject.GetProperty(IManagedProperty)"/> 方法。
        /// 例如，Enum 属性在未设置值时，Value 会返回 null，这时会造成调用者在类型转换时出错。
        /// 不能公开此属性，无法直接使用。这是因为这个值是还没有进行过类型转换、也没有经过 <see cref="IManagedPropertyMetadataInternal.CoerceGetValue(ManagedPropertyObject, object)"/> 方法进行处理的、还可能是 DefaultValue 的值。
        /// </summary>
        internal object Value
        {
            get => _value;
            set
            {
                _value = value;
                this.HasLocalValue = true;
            }
        }

        /// <summary>
        /// 当前的属性是否为变更状态。（相对于最近一次调用 <see cref="ManagedPropertyObject.MarkPropertiesUnchanged"/> 方法之后）
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return (_status & MPFStatus.Changed) == MPFStatus.Changed;
            }
            internal set
            {
                if (value)
                {
                    _status |= MPFStatus.Changed;
                }
                else
                {
                    _status &= ~MPFStatus.Changed;
                }
            }
        }

        /// <summary>
        /// 返回属性是否处于禁用状态。
        /// 属性一旦进入禁用状态，则对这个属性调用 Get、Set 都将会出错，除非重新解禁。
        /// </summary>
        public bool IsDisabled
        {
            get
            {
                return (_status & MPFStatus.Disabled) == MPFStatus.Disabled;
            }
            internal set
            {
                if (value)
                {
                    _status |= MPFStatus.Disabled;
                }
                else
                {
                    _status &= ~MPFStatus.Disabled;
                }
            }
        }

        /// <summary>
        /// 返回是否存在本地值。（开发者是否存在主动设置/加载的字段值（本地值）。）
        /// 没有本地值的属性，是不占用过多的内存的，在序列化、反序列化的过程中也将被忽略，网络传输时，也不需要传输值。
        /// * 一个属性，如果调用过 LoadProperty、SetProperty 更新了值后，字段都会有本地值。
        /// * 如果调用过 <see cref="ManagedPropertyObject.ResetProperty(IManagedProperty)"/>  来设置默认值，都会清空其本地值；那么，此时这个方法也会返回 false。
        /// </summary>
        public bool HasLocalValue
        {
            get
            {
                return (_status & MPFStatus.HasLocalValue) == MPFStatus.HasLocalValue;
            }
            private set
            {
                if (value)
                {
                    _status |= MPFStatus.HasLocalValue;
                }
                else
                {
                    _status &= ~MPFStatus.HasLocalValue;
                }
            }
        }

        /// <summary>
        /// 将值清空到表示默认值的状态。
        /// </summary>
        internal void ResetValue()
        {
            _value = null;
            this.HasLocalValue = false;
        }

        #region 序列化

        internal bool TrySerialize(object defaultValue, out object result)
        {
            result = null;
            if (_status == MPFStatus.Default) return false;//默认状态的字段，不需要序列化。
            if (object.Equals(_value, defaultValue)) return false;//默认值，不需要进行序列化。

            if (_status == MPFStatus.HasLocalValue)
            {
                //大部分的属性都只是这个状态，所以尽量不使用 MPFV 类型进行序列化。
                result = _value;
            }
            else
            {
                result = new MPFV
                {
                    Value = _value,
                    Status = (byte)_status,
                };
            }
            return true;
        }

        internal void Deserialize(object value)
        {
            if (value is MPFV)
            {
                var mpfValues = (MPFV)value;

                _value = mpfValues.Value;
                _status = (MPFStatus)mpfValues.Status;
            }
            else
            {
                _value = value;
                _status = MPFStatus.HasLocalValue;
            }
        }

        #endregion

        private string DebuggerDisplay
        {
            get
            {
                var display = this.Property.Name;
                if (this.HasLocalValue && !object.Equals(this.Value, _property.DefaultMeta.DefaultValue))
                {
                    //长度小于 30，则格式化到 30 的长度。
                    while (display.Length < 30) { display += ' '; }

                    display += "   { ";
                    if (Value == null)
                    {
                        display += "null";
                    }
                    else
                    {
                        if (this.Value is string) { display += "'"; }
                        display += Value;
                        if (this.Value is string) { display += "'"; }
                    }
                    display += " }   ";
                }

                return display;
            }
        }
    }

    /// <summary>
    /// Status of <see cref="ManagedPropertyField"/>.
    /// </summary>
    [Flags]
    internal enum MPFStatus : byte
    {
        /// <summary>
        /// 默认状态
        /// </summary>
        Default = 0,
        /// <summary>
        /// 属性是否有本地值。
        /// </summary>
        HasLocalValue = 1,
        /// <summary>
        /// 属性被禁用。
        /// </summary>
        Disabled = 2,
        /// <summary>
        /// 在加载了持久化的值之后，还被变更为新的值。
        /// </summary>
        Changed = 4
    }
}
namespace Rafy
{
    /// <summary>
    /// Values of <see cref="Rafy.ManagedProperty.ManagedPropertyField"/>
    /// </summary>
    [Serializable]
    internal class MPFV : ICustomSerializationObject
    {
        public object Value;

        public byte Status;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (Value != null)
            {
                info.AddValue("v", Value, Value.GetType());
            }
            info.AddValue("s", Status);
        }

        public void SetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var item in info)
            {
                if (item.Name == "v")
                {
                    Value = item.Value;
                }
                else if (item.Name == "s")
                {
                    Status = (byte)item.Value;
                }
            }
        }
    }
}