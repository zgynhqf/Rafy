/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 属性元数据
    /// </summary>
    public abstract class PropertyMeta : Meta
    {
        #region 字段

        private IPropertyRuntime _runtimeProperty;

        private Type _propertyType;

        [UnAutoFreeze]
        private EntityMeta _owner;

        #endregion

        /// <summary>
        /// 如果本元数据对应的实体属性是由托管属性编写的，那么这里返回它所对应的托管属性。
        /// </summary>
        public IManagedProperty ManagedProperty { get; internal set; }

        /// <summary>
        /// 返回这个托管属性对应的 CLR 属性，如果没有对应的 CLR 属性（或者找到多个），则返回 null。
        /// </summary>
        public PropertyInfo CLRProperty
        {
            get
            {
                if (this.Owner != null)
                {
                    var entityType = _owner.EntityType;
                    var name = this.ManagedProperty.Name;
                    try
                    {
                        return entityType.GetProperty(name, this.ManagedProperty.PropertyType);
                    }
                    catch (AmbiguousMatchException)
                    {
                        //找到多个同名属性，返回 null
                        return null;
                    }
                }

                return null;
            }
        }

        public override string Name
        {
            get { return this._runtimeProperty.Name; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// 属性的类型。
        /// 这个类型并不一定与托管属性的类型一致。例如 Id 属性，声明的是 object 类型，但是实际可能是 int、string 等。
        /// </summary>
        public Type PropertyType
        {
            get { return this._propertyType; }
            set { this.SetValue(ref this._propertyType, value); }
        }

        /// <summary>
        /// 对应的属性
        /// </summary>
        public IPropertyRuntime Runtime
        {
            get
            {
                if (this._runtimeProperty == null) throw new ArgumentNullException("this._runtimeProperty");
                return this._runtimeProperty;
            }
            set { this.SetValue(ref this._runtimeProperty, value); }
        }

        /// <summary>
        /// 所在类型的元数据
        /// </summary>
        public EntityMeta Owner
        {
            get { return this._owner; }
            set { this.SetValue(ref this._owner, value); }
        }
    }

    public interface IPropertyRuntime
    {
        string Name { get; }
        Type PropertyType { get; }
        bool CanWrite { get; }

        object Core { get; }
    }

    public class CLRPropertyRuntime : IPropertyRuntime
    {
        private PropertyInfo _core;

        public CLRPropertyRuntime(PropertyInfo core)
        {
            this._core = core;
        }

        public object Core
        {
            get { return this._core; }
        }

        public string Name
        {
            get { return this._core.Name; }
        }

        public Type PropertyType
        {
            get { return this._core.PropertyType; }
        }

        public bool CanWrite
        {
            get { return this._core.CanWrite; }
        }
    }

    public class ManagedPropertyRuntime : IPropertyRuntime
    {
        private IManagedProperty _core;

        public ManagedPropertyRuntime(IManagedProperty core)
        {
            _core = core;
        }

        public string Name
        {
            get { return this._core.Name; }
        }

        public Type PropertyType
        {
            get { return _core.PropertyType; }
        }

        public object Core
        {
            get { return this._core; }
        }

        public bool CanWrite
        {
            get { return !this._core.IsReadOnly; }
        }
    }
}