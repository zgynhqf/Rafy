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
using System.Reflection;
using Rafy;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// CLR 属性的描述器
    /// </summary>
    public class CLRPropertyDescriptor : PropertyDescriptor
    {
        private PropertyInfo _property;

        public CLRPropertyDescriptor(PropertyInfo property)
            : base(property.Name, null)
        {
            this._property = property;
        }

        public override Type ComponentType
        {
            get { return typeof(object); }
        }

        public override bool IsReadOnly
        {
            get { return !this._property.CanWrite; }
        }

        public override Type PropertyType
        {
            get { return this._property.PropertyType; }
        }

        public override object GetValue(object component)
        {
            return this._property.GetValue(component, null);
        }

        public override void SetValue(object component, object value)
        {
            this._property.SetValue(component, value, null);
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
