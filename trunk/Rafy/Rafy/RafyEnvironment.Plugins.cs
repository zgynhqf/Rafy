/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110331
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100331
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;
using Rafy;
using Rafy.Reflection;
using Rafy.ComponentModel;

namespace Rafy
{
    /// <summary>
    /// Library Module Plugins
    /// </summary>
    public partial class RafyEnvironment
    {
        private const string DomainPluginFolder = "Domain";
        private const string UIPluginFolder = "UI";

        #region 启动 Plugins

        /// <summary>
        /// 启动所有的 实体插件
        /// </summary>
        internal static void StartupDomainPlugins()
        {
            var libraries = GetDomainPlugins();

            foreach (var pluginAssembly in libraries)
            {
                //调用 ILibrary
                var library = pluginAssembly.Instance as DomainPlugin;
                if (library != null) library.Initialize(_appCore);
            }
        }

        /// <summary>
        /// 启动所有的 模块插件
        /// </summary>
        internal static void StartupUIPlugins()
        {
            var libraries = GetUIPlugins();
            foreach (var pluginAssembly in libraries)
            {
                //调用 IModule
                var module = pluginAssembly.Instance as UIPlugin;
                if (module != null) module.Initialize(_appCore);
            }
        }

        #endregion

        #region 初始化托管属性

        /// <summary>
        /// 注册所有插件中的扩展托管属性
        /// </summary>
        internal static void InitExtensionProperties()
        {
            if (!ManagedPropertyRepository.Instance.IsExtensionRegistered)
            {
                var allAssemblies = RafyEnvironment.GetAllPlugins().Select(p => p.Assembly);

                ManagedPropertyRepository.Instance.IntializeExtension(allAssemblies);
            }
        }

        #endregion

        #region 获取所有 Plugins

        private static IEnumerable<PluginAssembly> _libraries;

        private static IEnumerable<PluginAssembly> _modules;

        /// <summary>
        /// 找到当前程序所有可运行的领域实体插件。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginAssembly> GetDomainPlugins()
        {
            if (_libraries == null)
            {
                var assemblies = EnumerateAllDomainAssemblies().Union(PluginTable.DomainLibraries).ToArray();
                _libraries = LoadPlugins(assemblies);

                PluginTable.DomainLibraries.Lock();
            }
            return _libraries;
        }

        /// <summary>
        /// 找到当前程序所有可运行的界面插件程序集。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginAssembly> GetUIPlugins()
        {
            if (_modules == null)
            {
                //如果是界面应用程序，则加载所有的 UI 文件。否则返回空集合。
                if (_location.IsWebUI || _location.IsWPFUI)
                {
                    var assemblies = EnumerateAllUIAssemblies().Union(PluginTable.UILibraries).ToList();
                    _modules = LoadPlugins(assemblies);

                    PluginTable.UILibraries.Lock();
                }
                else
                {
                    _modules = Enumerable.Empty<PluginAssembly>();
                }
            }
            return _modules;
        }

        /// <summary>
        /// 获取当前环境被初始化的所有插件。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginAssembly> GetAllPlugins()
        {
            if (_location.IsWPFUI || _location.IsWebUI)
            {
                return GetDomainPlugins().Union(GetUIPlugins()).OrderBy(a => a.Instance.SetupLevel);
            }

            return GetDomainPlugins();
        }

        internal static void Reset()
        {
            _libraries = null;
            _modules = null;
            ResetLocation();
        }

        private static List<PluginAssembly> LoadPlugins(IEnumerable<Assembly> assemblies)
        {
            return assemblies.Select(assembly =>
            {
                var pluginType = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                IPlugin pluginInstance = null;
                if (pluginType != null)
                {
                    pluginInstance = Activator.CreateInstance(pluginType) as IPlugin;
                    //throw new NotSupportedException("所有插件包中必须有且仅有一个实现 IPlugin 接口的类型！" + Environment.NewLine + "文件路径：" + file);
                }
                else
                {
                    pluginInstance = new EmptyPlugin(assembly);
                }

                return new PluginAssembly(assembly, pluginInstance);
            })
                //这里按照产品 721 进行排序。
            .OrderBy(a => a.Instance.SetupLevel)
            .ToList();
        }

        private static IEnumerable<Assembly> EnumerateAllDomainAssemblies()
        {
            yield return LoadRafyAssembly("Rafy.Domain");

            foreach (var file in EnumerateAllDomainPluginFiles())
            {
                yield return Assembly.LoadFrom(file);
            }
        }

        private static IEnumerable<string> EnumerateAllDomainPluginFiles()
        {
            //查找Library目录下的所有程序集
            string libraryPath = MapDllPath(DomainPluginFolder + "\\");
            if (Directory.Exists(libraryPath))
            {
                foreach (var dll in Directory.GetFiles(libraryPath, "*.dll"))
                {
                    yield return dll;
                }
            }

            //加入所有客户化的DLL
            foreach (var dir in GetCustomerEntityDlls())
            {
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.GetFiles(dir, "*.dll"))
                    {
                        yield return file;
                    }
                }
            }
        }

        private static IEnumerable<Assembly> EnumerateAllUIAssemblies()
        {
            //如果是 WPF 客户端，则需要添加 Rafy.WPF 程序集。
            if (_location.IsWPFUI)
            {
                yield return LoadRafyAssembly("Rafy.WPF");
            }
            //else if (_locInfo.IsWebUI)
            //{
            //    yield return LoadRafyAssembly("Rafy.Web");
            //}

            foreach (var file in EnumerateAllUIPluginFiles())
            {
                yield return Assembly.LoadFrom(file);
            }
        }

        private static IEnumerable<string> EnumerateAllUIPluginFiles()
        {
            //查找 Module 目录下的所有程序集
            string modulePath = MapDllPath(UIPluginFolder + "\\");
            if (Directory.Exists(modulePath))
            {
                foreach (var dll in Directory.GetFiles(modulePath, "*.dll"))
                {
                    yield return dll;
                }
            }

            //加入所有客户化的 DLL
            foreach (var dir in GetCustomerModuleDlls())
            {
                if (Directory.Exists(dir))
                {
                    foreach (var file in Directory.GetFiles(dir, "*.dll"))
                    {
                        yield return file;
                    }
                }
            }
        }

        private static Assembly LoadRafyAssembly(string name)
        {
            var aName = typeof(RafyEnvironment).Assembly.GetName();
            aName.Name = name;
            return Assembly.Load(aName);
        }

        #endregion

        #region RootType

        /// <summary>
        /// 遍历系统中存在的所有聚合根类型
        /// </summary>
        /// <returns></returns>
        public static IList<Type> SearchAllRootTypes()
        {
            var result = new List<Type>();

            foreach (var plugin in GetAllPlugins())
            {
                foreach (var type in plugin.Assembly.GetTypes())
                {
                    if (IsConcreteRootType(type)) result.Add(type);
                }
            }

            return result;
        }

        internal static bool IsConcreteRootType(Type entityType)
        {
            return !entityType.IsAbstract && !entityType.IsGenericType &&
                (entityType.HasMarked<RootEntityAttribute>() ||
                entityType.HasMarked<QueryEntityAttribute>());
        }

        /// <summary>
        /// 获取一个实体类型的根类型。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal static Type GetRootType(Type entityType)
        {
            //实体类必须从 ManagedPropertyObject 上继承。
            var root = entityType;
            while (root != null && !IsConcreteRootType(root))
            {
                root = GetParentType(root);
            }

            return root;
        }

        private static Type GetParentType(Type entityType)
        {
            var container = ManagedPropertyRepository.Instance.GetTypePropertiesContainer(entityType);

            var parentProperty = container.GetNonReadOnlyCompiledProperties().OfType<IRefProperty>()
                .FirstOrDefault(p => p.ReferenceType == ReferenceType.Parent);

            if (parentProperty != null) { return parentProperty.RefEntityType; }

            return null;
        }

        #endregion

        #region EntityConfig

        private static Dictionary<Type, List<EntityConfig>> _typeConfigurations;

        /// <summary>
        /// 获取所有的实体配置列表。
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<Type, List<EntityConfig>> GetTypeConfigurations()
        {
            InitTypeConfigurations();

            return _typeConfigurations;
        }

        private static void InitTypeConfigurations()
        {
            if (_typeConfigurations == null)
            {
                /*********************** 代码块解释 *********************************
                 * 查找所有 EntityConfig 类型，并根据是否为扩展视图的配置类，
                 * 分别加入到两个不同的列表中。
                **********************************************************************/

                var defaultRepo = new Dictionary<Type, List<EntityConfig>>(100);
                var entityType = typeof(EntityConfig);

                //实体配置一般只放在领域插件中。但是，一些只存在于客户端的实体，则会放到界面插件中，所以这里需要检查所有的插件。
                foreach (var p in RafyEnvironment.GetAllPlugins())
                {
                    foreach (var type in p.Assembly.GetTypes())
                    {
                        if (!type.IsGenericTypeDefinition && entityType.IsAssignableFrom(type))
                        {
                            var config = Activator.CreateInstance(type) as EntityConfig;
                            config.PluginSetupLevel = p.Instance.SetupLevel;
                            config.InheritanceCount = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject)).Count();

                            List<EntityConfig> typeList = null;

                            if (!defaultRepo.TryGetValue(config.EntityType, out typeList))
                            {
                                typeList = new List<EntityConfig>(2);
                                defaultRepo.Add(config.EntityType, typeList);
                            }

                            typeList.Add(config);
                        }
                    }
                }

                _typeConfigurations = defaultRepo;
            }
        }

        /// <summary>
        /// 获取某个实体视图的所有配置类实例
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal static IEnumerable<EntityConfig> FindConfigurations(Type entityType)
        {
            InitTypeConfigurations();

            var hierachy = TypeHelper.GetHierarchy(entityType, typeof(ManagedPropertyObject)).Reverse();
            foreach (var type in hierachy)
            {
                List<EntityConfig> configList = null;
                if (_typeConfigurations.TryGetValue(type, out configList))
                {
                    var orderedList = configList.OrderBy(o => o.PluginSetupLevel).ThenBy(o => o.InheritanceCount);
                    foreach (var config in orderedList) { yield return config; }
                }
            }
        }

        #endregion

        #region EntityViewConfig

        internal static ViewConfigFinder<WebViewConfig> WebConfigurations = new ViewConfigFinder<WebViewConfig>();

        internal static ViewConfigFinder<WPFViewConfig> WPFConfigurations = new ViewConfigFinder<WPFViewConfig>();

        internal class ViewConfigFinder<TViewConfig>
            where TViewConfig : ViewConfig
        {
            private Dictionary<Type, List<TViewConfig>> _configurations;

            private Dictionary<ExtendTypeKey, List<TViewConfig>> _extendConfigurations;

            private void InitConfigurations()
            {
                if (_extendConfigurations == null)
                {
                    /*********************** 代码块解释 *********************************
                     * 查找所有 EntityConfig 类型，并根据是否为扩展视图的配置类，
                     * 分别加入到两个不同的列表中。
                    **********************************************************************/

                    var defaultRepo = new Dictionary<Type, List<TViewConfig>>(100);
                    var extendRepo = new Dictionary<ExtendTypeKey, List<TViewConfig>>(100);
                    var entityType = typeof(TViewConfig);

                    //视图配置可以放在所有插件中。
                    foreach (var p in RafyEnvironment.GetAllPlugins())
                    {
                        foreach (var type in p.Assembly.GetTypes())
                        {
                            if (!type.IsGenericTypeDefinition && entityType.IsAssignableFrom(type))
                            {
                                var config = Activator.CreateInstance(type) as TViewConfig;
                                config.PluginSetupLevel = p.Instance.SetupLevel;
                                config.InheritanceCount = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject)).Count();

                                List<TViewConfig> typeList = null;

                                if (config.ExtendView == null)
                                {
                                    if (!defaultRepo.TryGetValue(config.EntityType, out typeList))
                                    {
                                        typeList = new List<TViewConfig>(2);
                                        defaultRepo.Add(config.EntityType, typeList);
                                    }
                                }
                                else
                                {
                                    var key = new ExtendTypeKey { EntityType = config.EntityType, ExtendView = config.ExtendView };
                                    if (!extendRepo.TryGetValue(key, out typeList))
                                    {
                                        typeList = new List<TViewConfig>(2);
                                        extendRepo.Add(key, typeList);
                                    }
                                }

                                typeList.Add(config);
                            }
                        }
                    }

                    _configurations = defaultRepo;
                    _extendConfigurations = extendRepo;
                }
            }

            /// <summary>
            /// 获取某个实体视图的所有配置类实例
            /// </summary>
            /// <param name="entityType"></param>
            /// <param name="extendView">如果想获取扩展视图列表，则需要传入指定的扩展视图列表</param>
            /// <returns></returns>
            internal IEnumerable<TViewConfig> FindViewConfigurations(Type entityType, string extendView = null)
            {
                InitConfigurations();

                var hierachy = TypeHelper.GetHierarchy(entityType, typeof(ManagedPropertyObject)).Reverse();
                if (extendView == null)
                {
                    foreach (var type in hierachy)
                    {
                        List<TViewConfig> configList = null;
                        if (_configurations.TryGetValue(type, out configList))
                        {
                            var orderedList = configList.OrderBy(o => o.PluginSetupLevel).ThenBy(o => o.InheritanceCount);
                            foreach (var config in orderedList) { yield return config; }
                        }
                    }
                }
                else
                {
                    foreach (var type in hierachy)
                    {
                        var key = new ExtendTypeKey { EntityType = type, ExtendView = extendView };

                        List<TViewConfig> configList = null;
                        if (_extendConfigurations.TryGetValue(key, out configList))
                        {
                            var orderedList = configList.OrderBy(o => o.PluginSetupLevel).ThenBy(o => o.InheritanceCount);
                            foreach (var config in orderedList) { yield return config; }
                        }
                    }
                }
            }

            private struct ExtendTypeKey
            {
                public Type EntityType;

                public string ExtendView;

                public override int GetHashCode()
                {
                    return this.EntityType.GetHashCode() ^ this.ExtendView.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    var other = (ExtendTypeKey)obj;
                    return other.EntityType == this.EntityType && other.ExtendView == this.ExtendView;
                }
            }
        }

        #endregion
    }
}