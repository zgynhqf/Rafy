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
using Rafy.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                var allAssemblies = RafyEnvironment.AllPlugins.Select(p => p.Assembly);

                ManagedPropertyRepository.Instance.IntializeExtension(allAssemblies);
            }
        }

        #endregion

        #region 获取所有 Plugins

        private static PluginCollection _domainPlugins;
        private static PluginCollection _uiPlugins;
        private static PluginCollection _allPlugins;

        /// <summary>
        /// 当前程序所有可运行的领域实体插件。
        /// </summary>
        public static PluginCollection DomainPlugins
        {
            get { return _domainPlugins; }
        }

        /// <summary>
        /// 当前程序所有可运行的界面插件程序集。
        /// </summary>
        public static PluginCollection UIPlugins
        {
            get { return _uiPlugins; }
        }

        /// <summary>
        /// 获取当前环境被初始化的所有插件。
        /// </summary>
        public static PluginCollection AllPlugins
        {
            get { return _allPlugins; }
        }

        internal static void Reset()
        {
            _domainPlugins = new PluginCollection();
            _uiPlugins = new PluginCollection();
            _allPlugins = null;

            ResetLocation();
        }

        /// <summary>
        /// 启动所有的插件
        /// </summary>
        internal static void InitPlugins()
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

        internal static void LockPlugins()
        {
            //所有插件（其中，DomainPlugins 在列表的前面，UIPlugins 在列表的后面。）
            _allPlugins = new PluginCollection();

            //domain plugins.
            var configPlugins = Configuration.Section.DomainPlugins.GetChildren().OfType<PluginElement>().Select(e => e.Plugin).ToArray();
            if (configPlugins.Length > 0)
            {
                InitPluginsByConfig(_domainPlugins, configPlugins);
            }
            _domainPlugins.Insert(0, new Rafy.Domain.RafyDomainPlugin());
            _domainPlugins.Lock();

            foreach (var item in _domainPlugins) { _allPlugins.Add(item); }

            //ui plugins.
            if (_location.IsUI)
            {
                configPlugins = Configuration.Section.UIPlugins.GetChildren().OfType<PluginElement>().Select(e => e.Plugin).ToArray();
                if (configPlugins.Length > 0)
                {
                    InitPluginsByConfig(_uiPlugins, configPlugins);
                }
                if (_location.IsWPFUI)
                {
                    _uiPlugins.Insert(0, LoadRafyPlugin("Rafy.WPF"));
                }
                _uiPlugins.Lock();

                foreach (var item in _uiPlugins) { _allPlugins.Add(item); }
            }

            _allPlugins.Lock();
        }

        private static IPlugin LoadRafyPlugin(string name)
        {
            var aName = typeof(RafyEnvironment).Assembly.GetName();
            aName.Name = name;
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

        private static void InitPluginsByConfig(PluginCollection pluginList, string[] sortedPlugins)
        {
            if (sortedPlugins.Length == 0) return;

            //如果提供了配置信息，则完成按照配置中的插件列表来初始化，所以先清空该列表。
            pluginList.Clear();

            for (int i = 0, c = sortedPlugins.Length; i < c; i++)
            {
                var name = sortedPlugins[i];

                IPlugin plugin = null;

                //可以只填写程序集名称，也可以写出插件类型的全名称。
                if (!name.Contains(','))
                {
                    #region 按照程序集名称来加载插件

                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.Load(sortedPlugins[i]);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidProgramException(string.Format("无法加载配置文件中指定的插件：{0}。", sortedPlugins[i]), ex);
                    }
                    plugin = CreatePluginFromAssembly(assembly);

                    #endregion
                }
                else
                {
                    #region 按照插件类型名称来加载插件

                    Type pluginType = null;
                    try
                    {
                        pluginType = Type.GetType(sortedPlugins[i]);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidProgramException(string.Format("无法加载配置文件中指定的插件类型：{0}。", sortedPlugins[i]), ex);
                    }
                    if (pluginType == null) { throw new InvalidProgramException(string.Format("无法加载配置文件中指定的插件类型：{0}。", sortedPlugins[i])); }

                    plugin = Activator.CreateInstance(pluginType, true) as IPlugin;

                    #endregion
                }

                pluginList.Add(plugin);
            }
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
                for (int index = 0, c = AllPlugins.Count; index < c; index++)
                {
                    var plugin = AllPlugins[index];
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
                    var orderedList = configList.OrderBy(o => o.PluginIndex).ThenBy(o => o.InheritanceCount);
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
                    for (int index = 0, c = RafyEnvironment.AllPlugins.Count; index < c; index++)
                    {
                        var plugin = RafyEnvironment.AllPlugins[index];
                        foreach (var type in plugin.Assembly.GetTypes())
                        {
                            if (!type.IsGenericTypeDefinition && !type.IsAbstract && entityType.IsAssignableFrom(type))
                            {
                                var config = Activator.CreateInstance(type) as TViewConfig;
                                config.PluginIndex = index;
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

                        List<TViewConfig> configList = null;
                        if (_extendConfigurations.TryGetValue(key, out configList))
                        {
                            var orderedList = configList.OrderBy(o => o.PluginIndex).ThenBy(o => o.InheritanceCount);
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

        #region IOC & SOA

        /// <summary>
        /// 组件的 IOC 容器。
        /// </summary>
        public static IObjectContainer ObjectContainer
        {
            get { return Composer.ObjectContainer; }
        }

        //public static IServiceCollection ServiceCollection
        //{
        //    get
        //    {
        //        throw new NotImplementedException();//huqf
        //    }
        //}

        ///// <summary>
        ///// 组件的服务容器。
        ///// </summary>
        //public static IServiceContainer ServiceContainer
        //{
        //    get { return Composer.ServiceContainer; }
        //}

        #endregion
    }
}