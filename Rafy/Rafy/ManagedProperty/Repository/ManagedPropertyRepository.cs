/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.5.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 修改托管属性的注册行为，不再在程序启动时加载所有属性，而是在运行时按需注册需要的属性。 胡庆访 20120108
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Rafy.Reflection;

namespace Rafy.ManagedProperty
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

        private object _statusLock = new object();
        private object _allPropertiesLock = new object();

        private int _nextGlobalIndex;

        private Dictionary<Type, TypePropertiesContainer> _allProperties = new Dictionary<Type, TypePropertiesContainer>();

        #endregion

        #region 初始化扩展属性。

        private bool _isExtensionRegistered = false;

        /// <summary>
        /// 是否所有扩展属性都已经被注册完成。
        /// </summary>
        public bool IsExtensionRegistered
        {
            get { return _isExtensionRegistered; }
        }

        /// <summary>
        /// 是否正在扩展属性注册过程中。
        /// </summary>
        private bool _isRegisteringExtension = false;

        /// <summary>
        /// 通过所有包含扩展属性的程序集来初始化所有的扩展属性。
        /// 
        /// 注意，扩展属性需要在程序启动时就完全注册完成，否则托管属性类型并不知道自己最终会有哪些属性。
        /// </summary>
        /// <param name="assemblies"></param>
        public void IntializeExtension(IEnumerable<Assembly> assemblies)
        {
            if (this._isExtensionRegistered) throw new InvalidProgramException("此方法只允许在程序启动时被调用一次。");

            try
            {
                this._isRegisteringExtension = true;

                var entityTypes = SearchAllExtensionDeclarers(assemblies);
                foreach (var type in entityTypes)
                {
                    RunPropertyResigtry(type);
                }

                var extRegisterTypes = SearchAllExtensionRegisters(assemblies);
                foreach (var type in extRegisterTypes)
                {
                    var register = Activator.CreateInstance(type) as ExtensionPropertiesRegister;
                    register.Register();
                }
            }
            finally
            {
                this._isRegisteringExtension = false;
                this._isExtensionRegistered = true;
            }
        }

        /// <summary>
        /// 获取所有标记了 CompiledPropertyDeclarerAttribute 的类型。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> SearchAllExtensionDeclarers(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (Attribute.IsDefined(type, typeof(CompiledPropertyDeclarerAttribute), true))
                    {
                        //不从 ManagedPropertyObject 类继承而又标记了 CompiledPropertyDeclarerAttribute 的类型，
                        //被认为是用于注册扩展属性的。
                        if (!type.IsSubclassOf(typeof(ManagedPropertyObject)))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有 ExtensionPropertiesRegister 的子类型。
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Type> SearchAllExtensionRegisters(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(ExtensionPropertiesRegister)))
                    {
                        yield return type;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 注销某个类型所对应的所有运行时属性
        /// </summary>
        /// <param name="ownerType"></param>
        public void UnRegisterAllRuntimeProperties(Type ownerType)
        {
            var container = this.GetOrCreateTypeProperties(ownerType);

            IEnumerable<IManagedProperty> removed = null;

            lock (container.Lock)
            {
                removed = container.ClearRuntimeProperties();
            }

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

            var container = this.GetOrCreateTypeProperties(ownerType);
            lock (container.Lock)
            {
                if (lifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    //如果已经到达运行时，则 container.ConsolidatedContainer 已经生成完毕，此时注销无法成功。
                    if (container.CurLifeCycle == ManagedPropertyLifeCycle.Runtime) throw new InvalidOperationException("编译期属性不能在运行时被反注册。");
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
        /// 
        /// 线程安全。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public ManagedProperty<T> RegisterProperty<T>(ManagedProperty<T> property)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (property.GlobalIndex >= 0) { throw new InvalidOperationException("同一个属性只能注册一次。"); }

            var ownerType = property.OwnerType;

            /*********************** 代码块解释 *********************************
             * 
             * 属性注册的顺序是：
             * * 启动时，需要先注册所有的编译期扩展属性。
             * * 注册该类型自身定义的所有编译期正常属性。
             * * 运行时注册的运行期动态属性。
             * 
             * 所以：
             * 1. 如果不是注册扩展属性，需要保证该类型的静态构造函数首先被执行。
             * 2. 如果不是注册扩展属性，并且声明的类型并不是属性的拥有者，则可以理解该属性为运行期动态属性。
             * 
            **********************************************************************/
            property.LifeCycle = ManagedPropertyLifeCycle.Compile;
            if (this._isExtensionRegistered)
            {
                RunPropertyResigtry(ownerType);

                if (ownerType != property.DeclareType)
                {
                    property.LifeCycle = ManagedPropertyLifeCycle.Runtime;
                }
            }
            else
            {
                if (!this._isRegisteringExtension)
                {
                    throw new InvalidProgramException("在注册所有扩展属性前，不能使用 RegisterProperty 方法注册任何其它属性。(例如：在控制台 Main 函数中除了调用 Rafy 初始化，后面还加载了 Rafy 实体，则会抛出此异常。)");
                }
            }

            //找到或创建 TypePropertiesContainer
            lock (_statusLock)
            {
                property.GlobalIndex = _nextGlobalIndex++;
            }

            var typeProperties = this.GetOrCreateTypeProperties(ownerType);
            lock (typeProperties.Lock)
            {
                //生命周期从编译变化到 Runtime 时，需要调用 CompleteCompileProperties
                if (typeProperties.CurLifeCycle == ManagedPropertyLifeCycle.Compile &&
                    property.LifeCycle == ManagedPropertyLifeCycle.Runtime)
                {
                    CompleteCompileProperties(typeProperties);
                }

                //以下代码把托管属性加入到容器对应的集合中。
                if (typeProperties.CurLifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    typeProperties.AddCompiledProperty(property, this._sortAsAdding);

                    if (!this._sortAsAdding)
                    {
                        if (_changedList == null) _changedList = new List<TypePropertiesContainer>(5000);
                        _changedList.Add(typeProperties);
                    }
                }
                else
                {
                    if (property.LifeCycle == ManagedPropertyLifeCycle.Compile)
                    {
                        throw new InvalidProgramException(string.Format(
                            @"类型 {0} 的托管属性已经进入运行期，不能再注册编译期属性：{1}。
可能的原因：在 {0} 类型以外的类型中注册了 {1} 属性。"
                            , property.OwnerType, property.Name));
                    }

                    typeProperties.AddRuntimeProperty(property);
                }
            }

            property.DefaultMeta.Freeze();

            return property;
        }

        /// <summary>
        /// 获取某个类型的已经注册的所有可用属性
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        public ConsolidatedTypePropertiesContainer GetTypePropertiesContainer(Type ownerType)
        {
            //由于当前方法是一个应用层调用的公有 API，所以当调用此方法时，它的所有编译属性必须已经注册完成。
            var container = this.GetOrCreateTypeProperties(ownerType);
            if (container != null)
            {
                //调用本方法时，必须注册完毕，如果还没有变更生成期，则需要主动完成此次变更。
                if (container.CurLifeCycle == ManagedPropertyLifeCycle.Compile)
                {
                    RunPropertyResigtry(ownerType);

                    //主动完成注册。
                    lock (container.Lock)
                    {
                        if (container.CurLifeCycle == ManagedPropertyLifeCycle.Compile)
                        {
                            CompleteCompileProperties(container);
                        }
                    }
                }

                return container.ConsolidatedContainer;
            }

            return null;
        }

        /// <summary>
        /// 最后一次使用的类型缓存，此字段不需要同步锁。
        /// </summary>
        private TypePropertiesContainer _lastResultCache;

        /// <summary>
        /// 获取某个类型的已经注册的所有可用属性。
        /// 
        /// 如果该类型还没有创建相应的容器，则递归为该类型及其基类型创建相应的属性容器。
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        private TypePropertiesContainer GetOrCreateTypeProperties(Type ownerType)
        {
            //由于经常是对同一类型的实体进行大量的构造操作，所以这里对最后一次使用的类型进行缓存
            var lrc = _lastResultCache;
            if (lrc != null && lrc.OwnerType == ownerType) { return lrc; }

            TypePropertiesContainer list = null;
            if (!_allProperties.TryGetValue(ownerType, out list))
            {
                lock (_allPropertiesLock)
                {
                    if (!_allProperties.TryGetValue(ownerType, out list))
                    {
                        list = new TypePropertiesContainer(ownerType);

                        var baseType = ownerType.BaseType;
                        if (baseType != typeof(ManagedPropertyObject))
                        {
                            if (baseType == typeof(object))
                            {
                                throw new InvalidProgramException(string.Format("托管属性类型 {0} 必须继承自 ManagedPropertyObject 类。", ownerType));
                            }

                            //初始化基类的 Container，并设置 container.BaseType
                            list.BaseType = this.GetOrCreateTypeProperties(baseType);
                        }

                        this._allProperties.Add(ownerType, list);
                    }
                }
            }

            _lastResultCache = list;

            return list;
        }

        /// <summary>
        /// 完成所有编译期属性的注册。此时会变更 container 的对应生成周期，并为其设置 ConsolidatedContainer 属性。
        /// </summary>
        private static void CompleteCompileProperties(TypePropertiesContainer container)
        {
            //设置 ConsolidatedContainer
            var consolidatedContainer = new ConsolidatedTypePropertiesContainer();
            consolidatedContainer.SimpleContainer = container;
            consolidatedContainer.InitCompiledProperties();

            //consolidatedContainer 初始化编译期属性成功（无异常），才设置 container 的连接。
            container.ConsolidatedContainer = consolidatedContainer;
            //变更生命期到“运行时”。
            container.CurLifeCycle = ManagedPropertyLifeCycle.Runtime;
        }

        /// <summary>
        /// 执行某个托管属性声明类型的静态构造函数，以保证其中的所有静态字段都被初始化完成。
        /// 如果已经执行过，则不会再次执行。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RunPropertyResigtry(Type type)
        {
            //泛型类在绑定具体类型前，是无法初始化它的静态字段的，所以这里直接退出，而留待子类来进行初始化。
            if (type.ContainsGenericParameters)
            {
                if (!type.IsAbstract)
                {
                    throw new InvalidOperationException(string.Format(
                        "声明托管属性的泛型类型 {0}，必须声明为 abstract，否则无法正常使用托管属性！",
                        type.FullName
                        ));
                }
                return false;
            }

            //同时运行基类及它本身的所有静态构造函数
            var types = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject), typeof(object)).ToArray();
            for (int i = types.Length - 1; i >= 0; i--)
            {
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(types[i].TypeHandle);
            }

            return true;

            //var ci = type.TypeInitializer;
            //if (ci != null) ci.Invoke(null, null);

            //var attr =
            //    System.Reflection.BindingFlags.Static |
            //    System.Reflection.BindingFlags.Public |
            //    System.Reflection.BindingFlags.DeclaredOnly |
            //    System.Reflection.BindingFlags.NonPublic;
            //var t = type;
            //while (t != null)
            //{
            //    var fields = t.GetFields(attr);
            //    if (fields.Length > 0)
            //    {
            //        //只需要获取一个，即可强制整个静态构造函数运行。
            //        fields[0].GetValue(null);
            //    }
            //    t = t.BaseType;
            //}
        }

        #region DeferSortingCompiledProperties

        /// <summary>
        /// 在 Using 语句最后释放时，才排序所有字段，提高效率。
        /// 
        /// 但是暂时没有使用，没有提高明显的效率。待验证。
        /// </summary>
        /// <returns></returns>
        public IDisposable DeferSortingCompiledProperties()
        {
            return new DeferSorter(this);
        }

        private bool _sortAsAdding = true;

        private List<TypePropertiesContainer> _changedList;

        private class DeferSorter : IDisposable
        {
            private ManagedPropertyRepository _repo;

            public DeferSorter(ManagedPropertyRepository repo)
            {
                _repo = repo;

                Monitor.Enter(_repo._statusLock);
                _repo._sortAsAdding = false;
            }

            public void Dispose()
            {
                var list = _repo._changedList;
                _repo._changedList = null;
                _repo._sortAsAdding = true;
                Monitor.Exit(_repo._statusLock);

                foreach (var typeProperties in list)
                {
                    typeProperties.ResortCompiledProperties();
                }
            }
        }

        #endregion

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