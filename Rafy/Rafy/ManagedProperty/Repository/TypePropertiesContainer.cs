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
using System.Collections.ObjectModel;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 属性容器
    /// 类型本身的托管属性的容器
    /// </summary>
    public class TypePropertiesContainer
    {
        /// <summary>
        /// 在变更本容器时，都应该执行加锁操作。
        /// </summary>
        internal readonly object Lock = new object();

        private List<IManagedProperty> _compiledProperties = new List<IManagedProperty>(10);

        private List<IManagedProperty> _runtimeProperties;

        public TypePropertiesContainer(Type ownerType)
        {
            this.OwnerType = ownerType;
        }

        internal ManagedPropertyLifeCycle CurLifeCycle = ManagedPropertyLifeCycle.Compile;

        /// <summary>
        /// 对应的类型
        /// </summary>
        public Type OwnerType { get; private set; }

        /// <summary>
        /// 基类的属性容器
        /// </summary>
        public TypePropertiesContainer BaseType { get; internal set; }

        /// <summary>
        /// 对应的联合属性容器
        /// </summary>
        public ConsolidatedTypePropertiesContainer ConsolidatedContainer { get; internal set; }

        /// <summary>
        /// 当前类型声明的编译期属性
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IManagedProperty> GetCompiledProperties()
        {
            return this._compiledProperties;
        }

        /// <summary>
        /// 当前类型声明的运行时属性
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IManagedProperty> GetRuntimeProperties()
        {
            if (this._runtimeProperties == null)
            {
                return Enumerable.Empty<IManagedProperty>();
            }

            return this._runtimeProperties;
        }

        /// <summary>
        /// 运行时属性变更事件
        /// </summary>
        public event EventHandler RuntimePropertiesChanged;

        internal void AddCompiledProperty(IManagedProperty property, bool withSort)
        {
            this._compiledProperties.Add(property);

            if (withSort)
            {
                this.ResortCompiledProperties();
            }
        }

        internal void RemoveCompiledProperties(IEnumerable<IManagedProperty> properties)
        {
            foreach (var property in properties)
            {
                this._compiledProperties.Remove(property);
            }

            this.ResortCompiledProperties();
        }

        internal void AddRuntimeProperty(IManagedProperty property)
        {
            if (this._runtimeProperties == null)
            {
                this._runtimeProperties = new List<IManagedProperty>();
            }

            this._runtimeProperties.Add(property);

            this.ResortRuntimeProperties();

            this.NotifyRuntimeChanged();
        }

        internal void RemoveRuntimeProperties(IEnumerable<IManagedProperty> properties)
        {
            if (this._runtimeProperties != null)
            {
                foreach (var property in properties)
                {
                    this._runtimeProperties.Remove(property);
                }

                this.ResortRuntimeProperties();

                this.NotifyRuntimeChanged();
            }
        }

        internal IEnumerable<IManagedProperty> ClearRuntimeProperties()
        {
            if (this._runtimeProperties == null) return Enumerable.Empty<IManagedProperty>();

            var list = this._runtimeProperties;

            this._runtimeProperties = null;

            this.NotifyRuntimeChanged();

            return list;
        }

        private void ResortRuntimeProperties()
        {
            if (this._runtimeProperties != null)
            {
                this._runtimeProperties.Sort((m1, m2) =>
                {
                    return m1.Name.CompareTo(m2.Name);
                });
            }
        }

        internal void ResortCompiledProperties()
        {
            this._compiledProperties.Sort((m1, m2) =>
            {
                return m1.Name.CompareTo(m2.Name);
            });
        }

        /// <summary>
        /// 使用这个事件，通知本类的子类也应该更新动态属性列表。
        /// </summary>
        private void NotifyRuntimeChanged()
        {
            var handler = this.RuntimePropertiesChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
