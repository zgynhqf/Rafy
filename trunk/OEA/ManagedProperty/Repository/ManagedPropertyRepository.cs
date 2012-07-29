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
    /// 托管属性仓库
    /// </summary>
    public class ManagedPropertyRepository : IEnumerable<KeyValuePair<Type, TypePropertiesContainer>>
    {
        #region SingleTon

        public static readonly ManagedPropertyRepository Instance = new ManagedPropertyRepository();

        private ManagedPropertyRepository() { }

        #endregion

        #region 私有字段

        private object _lock = new object();

        private int _nextGlobalIndex;

        private Dictionary<Type, TypePropertiesContainer> _allProperties = new Dictionary<Type, TypePropertiesContainer>();

        private ManagedPropertyLifeCycle _curLifeCycle = ManagedPropertyLifeCycle.CompileOrSetup;

        #endregion

        /// <summary>
        /// 注销某个类型所对应的所有运行时属性
        /// </summary>
        /// <param name="ownerType"></param>
        public void UnRegisterAllRuntimeProperties(Type ownerType)
        {
            var container = this.GetOrCreateTypeProperties(ownerType);

            IEnumerable<IManagedProperty> removed = null;

            lock (container) { removed = container.ClearRuntimeProperties(); }

            foreach (IManagedPropertyInternal item in removed) { item.IsUnregistered = true; }
        }

        /// <summary>
        /// 注销指定的属性集合
        /// </summary>
        /// <param name="properties"></param>
        public void UnRegister(params IManagedProperty[] properties)
        {
            if (properties.Length == 0) { return; }

            var property = properties.FirstOrDefault();

            var lifeCycle = property.LifeCycle;
            if (properties.Any(p => p.LifeCycle != lifeCycle)) throw new ArgumentException("属性列表必须是同一类托管属性。");
            var ownerType = property.OwnerType;
            if (properties.Any(p => p.OwnerType != ownerType)) throw new InvalidOperationException("所有属性都必须是注册在同一个托管属性对象类型下。");

            TypePropertiesContainer container = this.GetOrCreateTypeProperties(ownerType);
            lock (container)
            {
                if (lifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
                {
                    //如果已经到达运行时，则 container.ConsolidatedContainer 已经生成完毕，此时注销无法成功。
                    if (this._curLifeCycle == ManagedPropertyLifeCycle.Runtime) throw new InvalidOperationException("编译期属性不能在运行时被反注册。");
                    container.RemoveCompiledProperties(properties);
                }
                else
                {
                    container.RemoveRuntimeProperties(properties);
                }
            }

            foreach (IManagedPropertyInternal item in properties) { item.IsUnregistered = true; }
        }

        /// <summary>
        /// 注册某个属性到容器中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public ManagedProperty<T> RegisterProperty<T>(ManagedProperty<T> property)
        {
            if (property.GlobalIndex >= 0)
            {
                throw new InvalidOperationException("同一个属性只能注册一次。");
            }

            lock (this._lock)
            {
                property.GlobalIndex = this._nextGlobalIndex++;
            }

            property.LifeCycle = this._curLifeCycle;

            var typeProperties = this.GetOrCreateTypePropertiesRecur(property.OwnerType);
            lock (typeProperties)
            {
                if (this._curLifeCycle == ManagedPropertyLifeCycle.CompileOrSetup)
                {
                    typeProperties.AddCompiledProperty(property);
                }
                else
                {
                    typeProperties.AddRuntimeProperty(property);
                }
            }

            return property;
        }

        /// <summary>
        /// 获取某个类型的已经注册的所有可用属性
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        public ConsolidatedTypePropertiesContainer GetTypePropertiesContainer(Type ownerType)
        {
            var container = this.GetOrCreateTypeProperties(ownerType);
            if (container != null)
            {
                var solidateContainer = container.ConsolidatedContainer;
                if (solidateContainer == null) { throw new NotSupportedException(string.Format("动态类型 {0} 没有生成继承托管属性容器，可能是生命期不支持。", ownerType)); }
                return solidateContainer;
            }
            return null;
        }

        /// <summary>
        /// 获取某个类型的已经注册的所有可用属性
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        private TypePropertiesContainer GetOrCreateTypePropertiesRecur(Type ownerType)
        {
            var baseType = ownerType.BaseType;

            if (baseType != typeof(ManagedPropertyObject) && baseType != null)
            {
                this.GetOrCreateTypePropertiesRecur(baseType);
            }

            return this.GetOrCreateTypeProperties(ownerType);
        }

        private Type _lastOwnerType;
        private TypePropertiesContainer _lastResultCache;
        internal TypePropertiesContainer GetOrCreateTypeProperties(Type ownerType)
        {
            //由于经常是对同一类型的实体进行大量的构造操作，所以这里对最后一次使用的类型进行缓存
            if (this._lastOwnerType == ownerType) { return this._lastResultCache; }

            TypePropertiesContainer list = null;

            if (!this._allProperties.TryGetValue(ownerType, out list))
            {
                list = new TypePropertiesContainer(ownerType);
                this._allProperties.Add(ownerType, list);
            }

            this._lastOwnerType = ownerType;
            this._lastResultCache = list;

            return list;
        }

        /// <summary>
        /// 注册编译属性完成后，需要调用此方法。
        /// </summary>
        internal void NotifyCompilePropertiesCompleted()
        {
            //设置 container.BaseType
            foreach (var kv in this._allProperties.ToArray())
            {
                var baseType = kv.Key.BaseType;
                if (baseType != null && baseType != typeof(ManagedPropertyObject))
                {
                    var container = kv.Value;
                    container.BaseType = this.GetOrCreateTypeProperties(baseType);
                }
            }

            //设置 ConsolidatedContainer
            foreach (var kv in this._allProperties)
            {
                var type = kv.Key;
                var container = kv.Value;

                var consolidatedContainer = new ConsolidatedTypePropertiesContainer();
                consolidatedContainer.SimpleContainer = container;
                container.ConsolidatedContainer = consolidatedContainer;

                consolidatedContainer.InitCompiledProperties();
            }

            this._curLifeCycle = ManagedPropertyLifeCycle.Runtime;
        }

        //private ConsolidatedTypePropertiesContainer CreateCompileConsolidatedList(Type type)
        //{
        //    var result = new ConsolidatedTypePropertiesContainer();

        //    // get inheritance hierarchy
        //    List<Type> hierarchy = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject));

        //    // walk from top to bottom to build consolidated list
        //    for (int index = hierarchy.Count - 1; index >= 0; index--)
        //    {
        //        var source = this.GetTypeProperties(hierarchy[index]);
        //        result.CompiledProperties.AddRange(source.GetCompiledProperties());
        //    }

        //    //如果最后一个还没有被初始化，则包含继承属性的整个属性列表都需要重新设置 TypeIndex
        //    var list = result.CompiledProperties;
        //    if (list.Count > 0 && list[list.Count - 1].TypeCompiledIndex == -1)
        //    {
        //        for (int i = 0, c = list.Count; i < c; i++)
        //        {
        //            var item = list[i] as ITypeCompiledIndexOwner;
        //            item.TypeCompiledIndex = i;
        //        }
        //    }

        //    return result;
        //}

        public IEnumerator<KeyValuePair<Type, TypePropertiesContainer>> GetEnumerator()
        {
            return this._allProperties.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}