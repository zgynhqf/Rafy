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
        /// <summary>
        /// 是否需要检查同一个类型中是否注册了同名的属性。
        /// </summary>
        public static bool CheckDuplicateProperties { get; set; } = true;

        #region 私有字段

        private ManagedPropertyList _compiledProperties = new ManagedPropertyList();

        private ManagedPropertyList _runtimeProperties;

        private ManagedPropertyList _availableCache;

        private ManagedPropertyList _nonReadOnlyCompiledProperties;

        #endregion

        internal ConsolidatedTypePropertiesContainer(TypePropertiesContainer simpleContainer)
        {
            this.SimpleContainer = simpleContainer;
        }

        internal TypePropertiesContainer SimpleContainer { get; private set; }

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
            return _nonReadOnlyCompiledProperties;
        }

        /// <summary>
        /// 获取编译时属性列表。
        /// 此集合包含只读属性，如果想遍历
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyList GetCompiledProperties()
        {
            return _compiledProperties;
        }

        /// <summary>
        /// 获取动态运行时属性列表。
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyList GetRuntimeProperties()
        {
            if (_runtimeProperties == null) { this.ResetRuntimeProperties(); }

            return _runtimeProperties;
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
            this.CheckPropertiesNotDuplicate();

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
            _runtimeProperties = null;
            _availableCache = null;

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

        /// <summary>
        /// 检查属性的注册过程中，没有同名的属性。
        /// </summary>
        /// <exception cref="InvalidProgramException"></exception>
        private void CheckPropertiesNotDuplicate()
        {
            if (!CheckDuplicateProperties) return;

            for (int i = 0, c = _compiledProperties.Count; i < c; i++)
            {
                for (int j = i + 1; j < c; j++)
                {
                    var a = _compiledProperties[i];
                    var b = _compiledProperties[j];
                    if (a.Name == b.Name)
                    {
                        if (a.DeclareType == b.DeclareType)
                        {
                            throw new InvalidProgramException($"不允许为实体注册同名的属性。{this.OwnerType} 中发现注册了两个 {a.Name} 属性。");
                        }
                        else
                        {
                            throw new InvalidProgramException($"不允许为实体注册同名的属性。{this.OwnerType} 中发现注册了两个 {a.Name} 属性，分别声明在以下两个类型中：{a.DeclareType} 及 {b.DeclareType}");
                        }
                    }
                }
            }
        }
    }
}
