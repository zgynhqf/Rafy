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
using Rafy.ManagedProperty;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 托管属性的描述器
    /// </summary>
    public class ManagedPropertyDescriptor : PropertyDescriptor
    {
        private IManagedProperty _property;

        internal protected ManagedPropertyDescriptor(IManagedProperty property)
            : base(property.Name, null)
        {
            this._property = property;
        }

        public IManagedProperty Property
        {
            get { return _property; }
        }

        public override void AddValueChanged(object component, EventHandler handler)
        {
            throw new NotSupportedException();
        }

        public override Type ComponentType
        {
            get { return typeof(ManagedPropertyObject); }
        }

        public override bool IsReadOnly
        {
            get { return this._property.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return this._property.PropertyType; }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            return (component as ManagedPropertyObject).GetProperty(this._property);
        }

        public override void ResetValue(object component)
        {
            (component as ManagedPropertyObject).ResetProperty(this._property);
        }

        public override void SetValue(object component, object value)
        {
            (component as ManagedPropertyObject).SetProperty(this._property, value);
        }

        public void SetValue(object component, object value, ManagedPropertyChangedSource source)
        {
            (component as ManagedPropertyObject).SetProperty(this._property, value, source);
        }
    }
}
