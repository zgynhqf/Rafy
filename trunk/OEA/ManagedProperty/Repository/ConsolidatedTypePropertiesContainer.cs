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

namespace OEA.ManagedProperty
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

        private List<IManagedProperty> _compiledProperties = new List<IManagedProperty>(10);

        private List<IManagedProperty> _runtimeProperties;

        private IManagedProperty[] _availableCache;

        private IManagedProperty[] _nonReadOnlyCompiledProperties;

        #endregion

        internal TypePropertiesContainer SimpleContainer { get; set; }

        public Type OwnerType
        {
            get { return this.SimpleContainer.OwnerType; }
        }

        /// <summary>
        /// 获取编译时非只读属性的列表。
        /// </summary>
        /// <returns></returns>
        public IList<IManagedProperty> GetNonReadOnlyCompiledProperties()
        {
            return this._nonReadOnlyCompiledProperties;
        }

        /// <summary>
        /// 获取编译时属性列表。
        /// </summary>
        /// <returns></returns>
        public IList<IManagedProperty> GetCompiledProperties()
        {
            return this._compiledProperties;
        }

        /// <summary>
        /// 获取动态运行时属性列表。
        /// </summary>
        /// <returns></returns>
        public IList<IManagedProperty> GetRuntimeProperties()
        {
            if (this._runtimeProperties == null) { this.ResetRuntimeProperties(); }

            return this._runtimeProperties;
        }

        /// <summary>
        /// 获取当前可用的属性列表，包括编译时属性、动态运行时属性。
        /// </summary>
        /// <returns></returns>
        public IList<IManagedProperty> GetAvailableProperties()
        {
            if (this._availableCache == null)
            {
                if (this._runtimeProperties == null) { this.ResetRuntimeProperties(); }

                this._availableCache = this._compiledProperties.Concat(this._runtimeProperties).ToArray();
            }

            return this._availableCache;
        }

        /// <summary>
        /// 运行时属性变更事件
        /// </summary>
        public event EventHandler RuntimePropertiesChanged;

        internal void InitCompiledProperties()
        {
            this.EnumerateHierarchyContainers(item =>
            {
                this._compiledProperties.AddRange(item.GetCompiledProperties());
            });

            this._nonReadOnlyCompiledProperties = this._compiledProperties.Where(p => !p.IsReadOnly).ToArray();

            //如果最后一个还没有被初始化，则包含继承属性的整个属性列表都需要重新设置 TypeIndex
            var list = this._nonReadOnlyCompiledProperties;
            if (list.Length > 0 && list[list.Length - 1].TypeCompiledIndex == -1)
            {
                for (int i = 0, c = list.Length; i < c; i++)
                {
                    var item = list[i] as IManagedPropertyInternal;
                    if (!item.IsReadOnly) { item.TypeCompiledIndex = i; }
                }
            }
        }

        private void ResetRuntimeProperties()
        {
            this._runtimeProperties = new List<IManagedProperty>();

            this.EnumerateHierarchyContainers(item =>
            {
                this._runtimeProperties.AddRange(item.GetRuntimeProperties());
            });

            this.ChangeWithHierarchy();
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
