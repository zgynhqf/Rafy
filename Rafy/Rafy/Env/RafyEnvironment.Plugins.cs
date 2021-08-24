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
 * 编辑文件 崔化栋 20180424 09:50
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
using Rafy.Configuration;
using System.Collections;

namespace Rafy
{
    /// <summary>
    /// Library Module Plugins
    /// </summary>
    public partial class RafyEnvironment
    {
        #region 初始化托管属性

        /// <summary>
        /// 注册所有插件中的扩展托管属性
        /// </summary>
        internal static void InitExtensionProperties()
        {
            if (!ManagedPropertyRepository.Instance.IsExtensionRegistered)
            {
                var allAssemblies = _allPlugins.Select(p => p.Assembly);

                ManagedPropertyRepository.Instance.IntializeExtension(allAssemblies);
            }
        }

        #endregion

        #region 获取所有 Plugins

        private static bool _allPluginsLoaded = false;
        private static PluginCollection _domainPlugins;
        private static PluginCollection _uiPlugins;
        private static PluginCollection _allPlugins;

        /// <summary>
        /// 是否支持插件的按需加载。
        /// 默认是 false。
        /// </summary>
        public static bool EnablePluginLoadAsRequired { get; set; }

        /// <summary>
        /// 当前程序所有可运行的领域实体插件。
        /// 在 <see cref="AppImplementationBase.InitEnvironment"/> 中通过代码加入本集合中的插件，都会在启动时全部加载。
        /// </summary>
        public static PluginCollection DomainPlugins
        {
            get { return _domainPlugins; }
        }

        /// <summary>
        /// 当前程序所有可运行的界面插件程序集。
        /// 在 <see cref="AppImplementationBase.InitEnvironment"/> 中通过代码加入本集合中的插件，都会在启动时全部加载。
        /// </summary>
        public static PluginCollection UIPlugins
        {
            get { return _uiPlugins; }
        }

        /// <summary>
        /// 获取当前环境已经被加载的所有插件。
        /// </summary>
        public static PluginCollection AllPlugins
        {
            get { return _allPlugins; }
        }

        /// <summary>
        /// 保证已经配置的所有插件（启动插件、按需加载插件），都全部被正确加载。
        /// </summary>
        public static void EnsureAllPluginsLoaded()
        {
            if (!_allPluginsLoaded)
            {
                LoadAllRuntimePlugins();

                _allPluginsLoaded = true;
            }
        }

        /// <summary>
        /// 在运行时，根据需要加载某个程序集对应的插件。
        /// 该插件需要在配置文件中提前进行配置。
        /// 该方法会防止重入。
        /// </summary>
        /// <param name="assembly"></param>
        public static IPlugin LoadPlugin(Assembly assembly)
        {
            return LoadPlugin(assembly.GetName().Name);
        }

        /// <summary>
        /// 在运行时，根据需要加载某个程序集对应的插件。
        /// 该插件需要在配置文件中提前进行配置。
        /// 该方法会防止重入。
        /// </summary>
        /// <param name="assemblyName"></param>
        public static IPlugin LoadPlugin(string assemblyName)
        {
            //已经加载过的插件，不再加载。
            var exists = _allPlugins.Find(assemblyName);
            if (exists != null) return exists;

            PluginType pluginType = PluginType.Domain;
            IPluginConfig pluginFound = null;

            //domain plugins.
            IPluginConfig[] configPlugins = GetDomainPluginsConfig();
            for (int i = 0, c = configPlugins.Length; i < c; i++)
            {
                var pluginSection = configPlugins[i];
                if (pluginSection.LoadType == PluginLoadType.AtStartup) continue;

                if (IsPluginOfAssembly(pluginSection, assemblyName))
                {
                    pluginFound = pluginSection;
                    break;
                }
            }

            //ui plugins.
            if (pluginFound == null && _location.IsUI)
            {
                configPlugins = GetUIPluginsConfig();
                for (int i = 0, c = configPlugins.Length; i < c; i++)
                {
                    var pluginSection = configPlugins[i];
                    if (pluginSection.LoadType == PluginLoadType.AtStartup) continue;

                    if (IsPluginOfAssembly(pluginSection, assemblyName))
                    {
                        pluginFound = pluginSection;
                        pluginType = PluginType.UI;
                        break;
                    }
                }
            }

            if (pluginFound == null) throw new InvalidProgramException($"插件 { assemblyName } 需要在配置文件中提前进行配置。");

            return LoadRuntimePlugin(pluginFound, pluginType);
        }

        /// <summary>
        /// 运行时，新加载了一个插件的事件。
        /// 注意，此时，这个插件还没有被初始化。
        /// </summary>
        public static event EventHandler<PluginEventArgs> RuntimePluginLoaded;

        internal static void Reset()
        {
            _allPluginsLoaded = false;
            _domainPlugins = new PluginCollection();
            _uiPlugins = new PluginCollection();
            _allPlugins = new PluginCollection();

            ResetLocation();
        }

        internal static void CreateStartupPlugins()
        {
            //所有插件（其中，DomainPlugins 在列表的前面，UIPlugins 在列表的后面。）
            //domain plugins.
            IPluginConfig[] configPlugins = GetDomainPluginsConfig();
            CreateStartupPluginsByConfig(_domainPlugins, configPlugins);
            _domainPlugins.Insert(0, new Rafy.Domain.RafyDomainPlugin());//其实这里不应该使用上层的类，但是内部为了简单实现，且效率更高。
            _domainPlugins.Lock();

            foreach (var item in _domainPlugins) { _allPlugins.Add(item); }

            //ui plugins.
            if (_location.IsUI)
            {
                configPlugins = GetUIPluginsConfig();
                CreateStartupPluginsByConfig(_uiPlugins, configPlugins);
                //if (_location.IsWPFUI)
                //{
                //    _uiPlugins.Insert(0, CreatePlugin("Rafy.WPF.RafyWPFPlugin, Rafy.WPF"));
                //    //_uiPlugins.Insert(0, LoadRafyPlugin("Rafy.WPF"));
                //}
                _uiPlugins.Lock();

                foreach (var item in _uiPlugins) { _allPlugins.Add(item); }
            }

            CheckDuplucatePlugins();

            _allPlugins.Lock();
        }

        /// <summary>
        /// 启动所有的插件
        /// </summary>
        internal static void InitializeStartupPlugins()
        {
            //先初始化实体插件，再初始化界面插件。
            foreach (var plugin in _allPlugins)
            {
                plugin.Initialize(_appCore);
            }

            //switch (RafyEnvironment.Location)
            //{
            //    case RafyLocation.WPFClient:
            //    case RafyLocation.LocalVersion:
            //        //初始化界面插件。
            //        StartupUIPlugins();
            //        break;

            //    case RafyLocation.WCFServer:
            //    case RafyLocation.WebServer:
            //    default:

            //        //在配置文件中配置好 <probing privatePath="bin/Library;bin/Module"/> 即可，不需要使用代码加载。
            //        ////虽然服务端不需要初始化所有 Module 插件，但是也需要把它们的 dll 加载到程序中，
            //        ////这是因为有一些实体插件并没有按照严格的分层，是直接把 WPF 界面层代码也放在其中的。
            //        ////这种情况下，如果不加载这些 Module 对应的 dll，则这些插件无法正常启动。
            //        //foreach (var m in RafyEnvironment.GetAllModules()) { }
            //        break;
            //}
        }

        /// <summary>
        /// 将配置文件中配置的所有插件，都加载进内存中。
        /// </summary>
        private static void LoadAllRuntimePlugins()
        {
            //所有插件（其中，DomainPlugins 在列表的前面，UIPlugins 在列表的后面。）
            //domain plugins.
            IPluginConfig[] configPlugins = GetDomainPluginsConfig();
            for (int i = 0, c = configPlugins.Length; i < c; i++)
            {
                var pluginSection = configPlugins[i];
                if (pluginSection.LoadType == PluginLoadType.AtStartup) continue;

                LoadRuntimePlugin(pluginSection, PluginType.Domain);
            }

            //ui plugins.
            if (_location.IsUI)
            {
                configPlugins = GetUIPluginsConfig();
                for (int i = 0, c = configPlugins.Length; i < c; i++)
                {
                    var pluginSection = configPlugins[i];
                    if (pluginSection.LoadType == PluginLoadType.AtStartup) continue;

                    LoadRuntimePlugin(pluginSection, PluginType.UI);
                }
            }
        }

        /// <summary>
        /// 在运行时，按需加载指定的插件
        /// </summary>
        /// <param name="pluginConfig">要加载的插件</param>
        /// <param name="pluginType">该插件的类型</param>
        private static IPlugin LoadRuntimePlugin(IPluginConfig pluginConfig, PluginType pluginType)
        {
            var plugin = CreatePlugin(pluginConfig);

            //已经加载过的插件，不再加载。
            var existsPlugin = _allPlugins.Find(plugin.Assembly);
            if (existsPlugin != null) return existsPlugin;

            if (pluginType == PluginType.Domain)
            {
                _domainPlugins.Unlock();
                _domainPlugins.Add(plugin);
                _domainPlugins.Lock();
            }
            else
            {
                _uiPlugins.Unlock();
                _uiPlugins.Add(plugin);
                _uiPlugins.Lock();
            }

            _allPlugins.Unlock();
            _allPlugins.Add(plugin);
            _allPlugins.Lock();

            //实体类型对应的集合需要重建。
            EntityConfigRepository.ClearCache();

            var handler = RuntimePluginLoaded;
            if (handler != null)
            {
                handler(null, new PluginEventArgs
                {
                    Plugin = plugin,
                    PluginType = pluginType
                });
            }

            //加载完成后，再初始化。
            plugin.Initialize(_appCore);

            return plugin;
        }

        private static IPlugin LoadRafyPlugin(string assebmlyName)
        {
            var aName = typeof(RafyEnvironment).Assembly.GetName();
            aName.Name = assebmlyName;
            aName.Version = null;//忽略版本号
            var assembly = Assembly.Load(aName);
            return CreatePluginFromAssembly(assembly);
        }

        private static IPlugin CreatePluginFromAssembly(Assembly assembly)
        {
            var pluginType = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (pluginType == null)
            {
                throw new NotSupportedException("所有插件包中必须有且仅有一个实现 IPlugin 接口的类型！" + Environment.NewLine + "程序集：" + assembly);
            }
            var pluginInstance = Activator.CreateInstance(pluginType) as IPlugin;

            return pluginInstance;
        }

        private static void CreateStartupPluginsByConfig(PluginCollection pluginList, IPluginConfig[] sortedPlugins)
        {
            if (sortedPlugins.Length == 0) return;

            for (int i = 0, c = sortedPlugins.Length; i < c; i++)
            {
                var pluginSection = sortedPlugins[i];
                if (pluginSection.LoadType == PluginLoadType.AsRequired && EnablePluginLoadAsRequired) continue;

                IPlugin plugin = CreatePlugin(pluginSection);

                pluginList.Add(plugin);
            }
        }

        private static IPlugin CreatePlugin(IPluginConfig pluginConfig)
        {
            string pluginClassOrAssembly = pluginConfig.Plugin;//可以只填写程序集名称，也可以写出插件类型的全名称。

            if (string.IsNullOrEmpty(pluginClassOrAssembly)) throw new ArgumentNullException(nameof(pluginClassOrAssembly));

            IPlugin plugin = null;

            try
            {
                //按照插件类型名称来加载插件
                var pluginType = Type.GetType(pluginClassOrAssembly);
                if (pluginType != null)
                {
                    plugin = Activator.CreateInstance(pluginType, true) as IPlugin;
                }
            }
            catch { }

            if (plugin == null)
            {
                //按照程序集名称来加载插件
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(pluginClassOrAssembly);
                }
                catch (Exception ex)
                {
                    throw new InvalidProgramException(string.Format("无法加载配置文件中指定的插件：{0}。", pluginClassOrAssembly), ex);
                }

                plugin = CreatePluginFromAssembly(assembly);
            }

            return plugin;
        }

        /// <summary>
        /// 通过兼容的算法，来匹配配置的插件名是否与对应的程序集匹配。
        /// </summary>
        /// <param name="pluginConfig"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private static bool IsPluginOfAssembly(IPluginConfig pluginConfig, string assemblyName)
        {
            string pluginClassOrAssembly = pluginConfig.Plugin;//可以只填写程序集名称，也可以写出插件类型的全名称。
            if (string.IsNullOrEmpty(pluginClassOrAssembly)) throw new ArgumentNullException(nameof(pluginClassOrAssembly));

            //Version 后面的内部需要截断，只需要前面的部分。
            var versionIndex = pluginClassOrAssembly.IndexOf("Version");
            if (versionIndex > 0)
            {
                pluginClassOrAssembly = pluginClassOrAssembly.Substring(0, versionIndex);
            }
            pluginClassOrAssembly = pluginClassOrAssembly.Trim();

            //截取出程序集的名称。（如果是两个部分，则前一部分是类名，后一部分是程序集名；否则全是程序集名。）
            string pluginAssemblyName = null;
            var commaIndex = pluginClassOrAssembly.IndexOf(',');
            if (commaIndex > 0)
            {
                pluginAssemblyName = pluginClassOrAssembly.Substring(commaIndex + 1).Trim();
            }
            else
            {
                pluginAssemblyName = pluginClassOrAssembly;
            }

            return assemblyName == pluginAssemblyName;
        }

        private static void CheckDuplucatePlugins()
        {
            foreach (var a in _allPlugins)
            {
                foreach (var b in _allPlugins)
                {
                    if (a != b && a.Assembly == b.Assembly)
                    {
                        throw new InvalidProgramException($"编写的代码有误，一个程序集只能声明一个插件类型！在 {a.Assembly} 程序集中，声明了 {a.GetType()} 和 {b.GetType()} 两个插件类型。");
                    }
                }
            }
        }

        /// <summary>
        /// 插件事件类型
        /// </summary>
        public class PluginEventArgs : EventArgs
        {
            /// <summary>
            /// 对应的运行时插件。
            /// </summary>
            public IPlugin Plugin { get; internal set; }

            /// <summary>
            /// 该插件所配置的插件类型。
            /// </summary>
            public PluginType PluginType { get; internal set; }
        }

        private static IPluginConfig[] GetDomainPluginsConfig()
        {
#if NET45
            return Configuration.Section.DomainPlugins.OfType<PluginElement>().ToArray();
#endif
#if NS2
            return Configuration.Section.DomainPlugins;
#endif
        }

        private static IPluginConfig[] GetUIPluginsConfig()
        {
#if NET45
            return Configuration.Section.UIPlugins.OfType<PluginElement>().ToArray();
#endif
#if NS2
            return Configuration.Section.UIPlugins;
#endif
        }

        //private static void EnsureAppMetaPrepared()
        //{
        //    if (_appCore == null) throw new InvalidProgramException("应用程序还没有启动！");
        //    if (_appCore.Phase < AppPhase.MetaPrepared) throw new InvalidProgramException("应用程序还没有进入运行时！");
        //}

        #endregion

        #region RootType

        /// <summary>
        /// 遍历系统中所有已经加载的插件中的所有聚合根类型
        /// </summary>
        /// <returns></returns>
        public static IList<Type> SearchAllRootTypes()
        {
            var result = new List<Type>();

            foreach (var plugin in _allPlugins)
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

        public static IEntityConfigRepository EntityConfigRepository { get; set; } = new EntityConfigFinder();

        /// <summary>
        /// 获取某个实体视图的所有配置类实例
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        internal static IEnumerable<EntityConfig> FindConfigurations(Type entityType)
        {
            return EntityConfigRepository.FindConfigurations(entityType);
        }

        public class EntityConfigFinder : IEntityConfigRepository
        {
            private Dictionary<Type, List<EntityConfig>> _typeConfigurations;

            /// <summary>
            /// 获取所有的实体配置列表。
            /// </summary>
            /// <returns></returns>
            internal Dictionary<Type, List<EntityConfig>> GetTypeConfigurations()
            {
                InitTypeConfigurations();

                return _typeConfigurations;
            }

            /// <summary>
            /// 获取某个实体视图的所有配置类实例
            /// </summary>
            /// <param name="entityType"></param>
            /// <returns></returns>
            public IEnumerable<EntityConfig> FindConfigurations(Type entityType)
            {
                InitTypeConfigurations();

                var hierachy = TypeHelper.GetHierarchy(entityType, typeof(ManagedPropertyObject)).Reverse();
                foreach (var type in hierachy)
                {
                    List<EntityConfig> configList = null;
                    if (_typeConfigurations.TryGetValue(type, out configList))
                    {
                        var orderedList = configList.OrderBy(o => o.PluginIndex).ThenBy(o => o.InheritanceCount);
                        foreach (var config in orderedList) { yield return config; }
                    }
                }
            }

            private void InitTypeConfigurations()
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
                    var allPlugins = RafyEnvironment.AllPlugins;
                    for (int index = 0, c = allPlugins.Count; index < c; index++)
                    {
                        var plugin = allPlugins[index];
                        foreach (var type in plugin.Assembly.GetTypes())
                        {
                            if (!type.IsGenericTypeDefinition && !type.IsAbstract && entityType.IsAssignableFrom(type))
                            {
                                var config = Activator.CreateInstance(type) as EntityConfig;
                                config.PluginIndex = index;
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

            public void ClearCache()
            {
                _typeConfigurations = null;
            }
        }

        #endregion

        #region EntityViewConfig

        /// <summary>
        /// Web 视图配置仓库。
        /// </summary>
        public static IViewConfigRepository WebConfigurations { get; set; } = new ViewConfigFinder(typeof(WebViewConfig));

        /// <summary>
        /// WPF 视图配置仓库。
        /// </summary>
        public static IViewConfigRepository WPFConfigurations { get; set; } = new ViewConfigFinder(typeof(WPFViewConfig));

        public class ViewConfigFinder : IViewConfigRepository
        {
            private Type _viewConfigType;

            public ViewConfigFinder(Type viewConfigType)
            {
                _viewConfigType = viewConfigType;
            }

            private Dictionary<Type, List<ViewConfig>> _configurations;

            private Dictionary<ExtendTypeKey, List<ViewConfig>> _extendConfigurations;

            public IEnumerable<ViewConfig> FindViewConfigurations(Type entityType, string extendView = null)
            {
                InitConfigurations();

                var hierachy = TypeHelper.GetHierarchy(entityType, typeof(ManagedPropertyObject)).Reverse();
                if (extendView == null)
                {
                    foreach (var type in hierachy)
                    {
                        List<ViewConfig> configList = null;
                        if (_configurations.TryGetValue(type, out configList))
                        {
                            var orderedList = configList.OrderBy(o => o.PluginIndex).ThenBy(o => o.InheritanceCount);
                            foreach (var config in orderedList) { yield return config; }
                        }
                    }
                }
                else
                {
                    foreach (var type in hierachy)
                    {
                        var key = new ExtendTypeKey { EntityType = type, ExtendView = extendView };

                        List<ViewConfig> configList = null;
                        if (_extendConfigurations.TryGetValue(key, out configList))
                        {
                            var orderedList = configList.OrderBy(o => o.PluginIndex).ThenBy(o => o.InheritanceCount);
                            foreach (var config in orderedList) { yield return config; }
                        }
                    }
                }
            }

            private void InitConfigurations()
            {
                if (_extendConfigurations == null)
                {
                    /*********************** 代码块解释 *********************************
                     * 查找所有 EntityConfig 类型，并根据是否为扩展视图的配置类，
                     * 分别加入到两个不同的列表中。
                    **********************************************************************/

                    var defaultRepo = new Dictionary<Type, List<ViewConfig>>(100);
                    var extendRepo = new Dictionary<ExtendTypeKey, List<ViewConfig>>(100);

                    //视图配置可以放在所有插件中。
                    for (int index = 0, c = _allPlugins.Count; index < c; index++)
                    {
                        var plugin = _allPlugins[index];
                        foreach (var type in plugin.Assembly.GetTypes())
                        {
                            if (!type.IsGenericTypeDefinition && !type.IsAbstract && _viewConfigType.IsAssignableFrom(type))
                            {
                                var config = Activator.CreateInstance(type) as ViewConfig;
                                config.PluginIndex = index;
                                config.InheritanceCount = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject)).Count();

                                List<ViewConfig> typeList = null;

                                if (config.ExtendView == null)
                                {
                                    if (!defaultRepo.TryGetValue(config.EntityType, out typeList))
                                    {
                                        typeList = new List<ViewConfig>(2);
                                        defaultRepo.Add(config.EntityType, typeList);
                                    }
                                }
                                else
                                {
                                    var key = new ExtendTypeKey { EntityType = config.EntityType, ExtendView = config.ExtendView };
                                    if (!extendRepo.TryGetValue(key, out typeList))
                                    {
                                        typeList = new List<ViewConfig>(2);
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

        #region IOC & SOA

        /// <summary>
        /// 组件的 IOC 容器。
        /// </summary>
        public static IObjectContainer ObjectContainer
        {
            get { return Composer.ObjectContainer; }
        }

        ///// <summary>
        ///// 组件的服务容器。
        ///// </summary>
        //public static IServiceContainer ServiceContainer
        //{
        //    get { return Composer.ServiceContainer; }
        //}

        #endregion
    }

    public enum PluginType
    {
        Domain,
        UI
    }
}