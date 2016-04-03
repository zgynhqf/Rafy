/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121224 15:46
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121224 15:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 某个具体类型的托管属性。
    /// 
    /// 由于 IManagedProperty 中只有 OwnerType、DeclareType，
    /// 而当想表达该属性是从属于 OwnerType 的子类型时，则需要使用这个类来表达。
    /// </summary>
    [DebuggerDisplay("{FullName}")]
    [Serializable]
    public class ConcreteProperty : CustomSerializationObject
    {
        private IManagedProperty _property;

        private Type _owner;

        /// <summary>
        /// 使用托管属性及它的 OwnerType 作为 ConcreteType 来构造一个 ConcreteProperty。
        /// </summary>
        /// <param name="property">托管属性</param>
        public ConcreteProperty(IManagedProperty property) : this(property, property.OwnerType) { }

        /// <summary>
        /// 构造器。
        /// </summary>
        /// <param name="property">托管属性</param>
        /// <param name="owner">该属性对应的具体类型。
        /// 这个具体的类型必须是属性的拥有类型或者它的子类型。如果传入 null，则默认为属性的拥有类型。</param>
        /// <exception cref="System.ArgumentNullException">property</exception>
        public ConcreteProperty(IManagedProperty property, Type owner)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (owner == null) owner = property.OwnerType;

            this._property = property;
            this._owner = owner;
        }

        /// <summary>
        /// 属性名称。
        /// </summary>
        public string Name
        {
            get { return this._property.Name; }
        }

        /// <summary>
        /// 包含类型的全名称。
        /// </summary>
        public string FullName
        {
            get { return this._owner.Name + "." + this._property.Name; }
        }

        /// <summary>
        /// 托管属性
        /// </summary>
        public IManagedProperty Property
        {
            get { return this._property; }
        }

        /// <summary>
        /// 该属性对应的具体类型。
        /// 这个具体的类型必须是属性的拥有类型或者它的子类型。
        /// </summary>
        public Type Owner
        {
            get { return this._owner; }
        }

        #region Serialization

        /// <summary>
        /// 反序列化构造函数。
        /// 
        /// 需要更高安全性，加上 SecurityPermissionAttribute 标记。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        private ConcreteProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var propertyName = info.GetString("PropertyName");
            var ownerType = info.GetValue("OwnerType", typeof(Type)) as Type;
            var container = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(ownerType);
            var property = container.GetAvailableProperties().Find(propertyName);

            _owner = ownerType;
            _property = property;
        }

        protected override void Serialize(SerializationInfo info, StreamingContext context)
        {
            base.Serialize(info, context);

            info.AddValue("PropertyName", _property.Name);
            info.AddValue("OwnerType", _owner);
        }

        #endregion
    }
}