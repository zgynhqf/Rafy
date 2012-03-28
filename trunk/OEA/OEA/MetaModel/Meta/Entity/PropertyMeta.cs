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
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    /// <summary>
    /// 属性元数据
    /// </summary>
    public abstract class PropertyMeta : Meta
    {
        #region 字段

        private IPropertyRuntime _runtimeProperty;

        private EntityMeta _entityInfo;

        #endregion

        /// <summary>
        /// 如果本元数据对应的实体属性是由托管属性编写的，那么这里返回它所对应的托管属性。
        /// 
        /// 一般来说，
        /// 孩子属性都是用 MP 编写；
        /// 实体属性则会有一些是直接使用 CLR 编写并标记 EntityPropertyAttribute（例如视图属性），这时，它就没有对应的托管属性。
        /// </summary>
        public IManagedProperty ManagedProperty { get; internal set; }

        public override string Name
        {
            get { return this._runtimeProperty.Name; }
            set { throw new NotSupportedException(); }
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
            get { return this._entityInfo; }
            set { this.SetValue(ref this._entityInfo, value); }
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

        private string _name;

        public ManagedPropertyRuntime(string name, IManagedProperty core)
        {
            this._name = name;
            this._core = core;
        }

        public string Name
        {
            get { return this._name; }
        }

        public Type PropertyType
        {
            get { return this._core.PropertyType; }
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