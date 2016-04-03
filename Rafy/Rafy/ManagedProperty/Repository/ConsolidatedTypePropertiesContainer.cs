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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 联合属性容器
    /// 
    /// 此容器中存放了类型及其基类的所有托管属性。
    /// 外部一般使用此类
    /// </summary>
    public class ConsolidatedTypePropertiesContainer
    {
        #region 私有字段

        private ManagedPropertyList _compiledProperties = new ManagedPropertyList();

        private ManagedPropertyList _runtimeProperties;

        private ManagedPropertyList _availableCache;

        private ManagedPropertyList _nonReadOnlyCompiledProperties;

        #endregion

        internal TypePropertiesContainer SimpleContainer { get; set; }

        /// <summary>
        /// 为界面层使用反射提供属性描述器集合。
        /// 
        /// 直接在此申明一个字段保存该值，提高查询的效率。
        /// 所有的逻辑都在 PropertyDescriptorFactory 类中。
        /// </summary>
        internal PropertyDescriptorCollection PropertyDescriptors;

        public Type OwnerType
        {
            get { return this.SimpleContainer.OwnerType; }
        }

        /// <summary>
        /// 获取编译时非只读属性的列表。
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyList GetNonReadOnlyCompiledProperties()
        {
            return this._nonReadOnlyCompiledProperties;
        }

        /// <summary>
        /// 获取编译时属性列表。
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyList GetCompiledProperties()
        {
            return this._compiledProperties;
        }

        /// <summary>
        /// 获取动态运行时属性列表。
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyList GetRuntimeProperties()
        {
            if (this._runtimeProperties == null) { this.ResetRuntimeProperties(); }

            return this._runtimeProperties;
        }

        /// <summary>
        /// 获取当前可用的属性列表，包括编译时属性、动态运行时属性。
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyList GetAvailableProperties()
        {
            if (_availableCache == null)
            {
                if (_runtimeProperties == null) { this.ResetRuntimeProperties(); }

                _availableCache = new ManagedPropertyList();
                _availableCache.AddRange(
                    _compiledProperties.Concat(_runtimeProperties)
                    );
            }

            return _availableCache;
        }

        /// <summary>
        /// 运行时属性变更事件
        /// </summary>
        public event EventHandler RuntimePropertiesChanged;

        internal void InitCompiledProperties()
        {
            this.EnumerateHierarchyContainers(item =>
            {
                _compiledProperties.AddRange(item.GetCompiledProperties());
            });

            _nonReadOnlyCompiledProperties = new ManagedPropertyList();
            _nonReadOnlyCompiledProperties.AddRange(
                _compiledProperties.Where(p => !p.IsReadOnly)
                );

            //如果最后一个还没有被初始化，则包含继承属性的整个属性列表都需要重新设置 TypeIndex
            var list = _nonReadOnlyCompiledProperties;
            if (list.Count > 0 && list[list.Count - 1].TypeCompiledIndex == -1)
            {
                for (int i = 0, c = list.Count; i < c; i++)
                {
                    var item = list[i] as IManagedPropertyInternal;
                    if (!item.IsReadOnly) { item.TypeCompiledIndex = i; }
                }
            }
        }

        private void ResetRuntimeProperties()
        {
            _runtimeProperties = new ManagedPropertyList();

            this.EnumerateHierarchyContainers(item =>
            {
                _runtimeProperties.AddRange(item.GetRuntimeProperties());
            });

            ChangeWithHierarchy();
        }

        #region ChangeWithHierarchy

        private bool _hierarchyChangedAttached;

        /// <summary>
        /// 父类中任何一个在改变时，子类都需要变化。
        /// </summary>
        private void ChangeWithHierarchy()
        {
            if (!this._hierarchyChangedAttached)
            {
                this.EnumerateHierarchyContainers(item =>
                {
                    item.RuntimePropertiesChanged += (o, e) => this.NotifyRuntimeChanged();
                });

                this._hierarchyChangedAttached = true;
            }
        }

        private void NotifyRuntimeChanged()
        {
            this._runtimeProperties = null;
            this._availableCache = null;

            var handler = this.RuntimePropertiesChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region EnumerateHierarchyContainers

        private void EnumerateHierarchyContainers(Action<TypePropertiesContainer> action)
        {
            // get inheritance hierarchy
            var hierarchy = this.GetHierarchyContainers();

            // walk from top to bottom to build consolidated list
            for (int index = hierarchy.Count - 1; index >= 0; index--)
            {
                action(hierarchy[index]);
            }
        }

        private List<TypePropertiesContainer> GetHierarchyContainers()
        {
            TypePropertiesContainer current = this.SimpleContainer;
            var hierarchy = new List<TypePropertiesContainer>();
            do
            {
                hierarchy.Add(current);
                current = current.BaseType;
            } while (current != null);
            return hierarchy;
        }

        #endregion
    }
}
