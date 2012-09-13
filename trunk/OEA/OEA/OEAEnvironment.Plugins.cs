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
using Microsoft.Practices.Unity;

using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.Utils;
using OEA.Reflection;

namespace OEA
{
    /// <summary>
    /// Library Module Plugins
    /// </summary>
    public partial class OEAEnvironment
    {
        #region 启动 Plugins

        private static bool _initialized;

        /// <summary>
        /// 启动所有的 实体插件
        /// </summary>
        /// <param name="app"></param>
        internal static void StartupEntityPlugins()
        {
            CheckUnInit();

            var libraries = GetAllLibraries();

            foreach (var pluginAssembly in libraries)
            {
                //调用 ILibrary
                var library = pluginAssembly.Instance as LibraryPlugin;
                if (library != null) library.Initialize(_appCore);
            }
        }

        /// <summary>
        /// 启动所有的 模块插件
        /// </summary>
        /// <param name="app"></param>
        public static void StartupModulePlugins()
        {
            CheckUnInit();

            foreach (var pluginAssembly in GetAllModules())
            {
                //调用 IModule
                var module = pluginAssembly.Instance as ModulePlugin;
                if (module != null) module.Initialize(_appCore as IClientApp);
            }
        }

        private static void CheckUnInit()
        {
            if (_initialized) throw new NotSupportedException("OEA框架已经初始化完成！");
        }

        internal static void NotifyIntialized()
        {
            _initialized = true;
        }

        #endregion

        #region 初始化托管属性

        internal static void InitManagedProperties()
        {
            var allAssemblies = OEAEnvironment.GetAllPlugins()
                .Select(p => p.Assembly);

            ManagedPropertyInitializer.Initialize(allAssemblies);
        }

        #endregion

        #region 获取所有 Plugins

        private static IEnumerable<PluginAssembly> _libraries;

        private static IEnumerable<PluginAssembly> _modules;

        /// <summary>
        /// 找到当前程序所有可运行的 Entity Dll
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginAssembly> GetAllLibraries()
        {
            if (_libraries == null)
            {
                var files = EnumerateAllLibFiles();
                _libraries = LoadPlugins(files);
            }
            return _libraries;
        }

        /// <summary>
        /// 找到当前程序所有可运行的 Module Dll
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginAssembly> GetAllModules()
        {
            if (_modules == null)
            {
                var files = EnumerateAllModuleFiles();
                _modules = LoadPlugins(files);
            }
            return _modules;
        }

        public static IEnumerable<PluginAssembly> GetAllPlugins()
        {
            if (IsOnClient())
            {
                return GetAllLibraries().Union(GetAllModules()).OrderBy(a => a.Instance.ReuseLevel);
            }

            return GetAllLibraries();
        }

        private static ReadOnlyCollection<PluginAssembly> LoadPlugins(IEnumerable<string> files)
        {
            return files.Select(file =>
            {
                var assembly = Assembly.LoadFrom(file);
                var pluginType = assembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                IPlugin pluginInstance = PluginAssembly.EmptyPlugin;

                if (pluginType != null)
                {
                    pluginInstance = Activator.CreateInstance(pluginType) as IPlugin;
                    //throw new NotSupportedException("所有插件包中必须有且仅有一个实现 IPlugin 接口的类型！" + Environment.NewLine + "文件路径：" + file);
                }

                return new PluginAssembly(assembly, pluginInstance);
            })
                //这里按照产品 721 进行排序。
            .OrderBy(a => a.Instance.ReuseLevel)
            .ToList()
            .AsReadOnly();
        }

        private static IEnumerable<string> EnumerateAllLibFiles()
        {
            yield return MapDllPath("OEA.Library.dll");

            //查找Library目录下的所有程序集
            string libraryPath = MapDllPath("Library\\");
            Directory.CreateDirectory(libraryPath);
            foreach (var dll in Directory.GetFiles(libraryPath, "*.dll"))
            {
                yield return dll;
            }

            //加入所有客户化的DLL
            foreach (var dir in GetEntityDlls())
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

        private static IEnumerable<string> EnumerateAllModuleFiles()
        {
            yield return MapDllPath("OEA.Module.WPF.dll");

            //查找 Module 目录下的所有程序集
            string modulePath = MapDllPath("Module\\");
            Directory.CreateDirectory(modulePath);
            foreach (var dll in Directory.GetFiles(modulePath, "*.dll"))
            {
                yield return dll;
            }

            //加入所有客户化的 DLL
            foreach (var dir in GetModuleDlls())
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

        #endregion

        #region RootType

        private static List<Type> _allRootTypes;

        /// <summary>
        /// 遍历系统中存在的所有聚合根类型
        /// </summary>
        /// <returns></returns>
        internal static IList<Type> GetAllRootTypes()
        {
            if (_allRootTypes == null)
            {
                var result = new List<Type>();

                foreach (var plugin in GetAllPlugins())
                {
                    foreach (var type in plugin.Assembly.GetTypes())
                    {
                        if (IsRootType(type)) result.Add(type);
                    }
                }

                _allRootTypes = result;
            }

            return _allRootTypes;
        }

        internal static bool IsRootType(Type entityType)
        {
            return entityType.HasMarked<RootEntityAttribute>() ||
                entityType.HasMarked<QueryEntityAttribute>();
        }

        #endregion

        #region Criteria

        /// <summary>
        /// 获取除了标记了 CriteriaAttribute 的类型
        /// </summary>
        /// <returns></returns>
        internal static IList<Type> GetCriteriaTypes()
        {
            var result = new List<Type>();

            foreach (var plugin in GetAllLibraries())
            {
                foreach (var type in plugin.Assembly.GetTypes())
                {
                    if (!type.IsAbstract && !type.IsGenericType && type.HasMarked<CriteriaAttribute>())
                    {
                        result.Add(type);
                    }
                }
            }

            return result;
        }

        #endregion

        #region EntityConfig

        private static Dictionary<Type, List<EntityConfig>> _typeConfigurations;
        private static Dictionary<ExtendTypeKey, List<EntityConfig>> _typeExtendConfigurations;

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
            if (_typeExtendConfigurations == null)
            {
                /*********************** 代码块解释 *********************************
                 * 查找所有 EntityConfig 类型，并根据是否为扩展视图的配置类，
                 * 分别加入到两个不同的列表中。
                **********************************************************************/

                var defaultRepo = new Dictionary<Type, List<EntityConfig>>(100);
                var extendRepo = new Dictionary<ExtendTypeKey, List<EntityConfig>>(100);
                var entityType = typeof(EntityConfig);
                foreach (var p in OEAEnvironment.GetAllPlugins())
                {
                    foreach (var type in p.Assembly.GetTypes())
                    {
                        if (!type.IsGenericTypeDefinition && entityType.IsAssignableFrom(type))
                        {
                            var config = Activator.CreateInstance(type) as EntityConfig;
                            config.ReuseLevel = (int)p.Instance.ReuseLevel;
                            config.InheritanceCount = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject)).Count();

                            List<EntityConfig> typeList = null;

                            if (config.ExtendView == null)
                            {
                                if (!defaultRepo.TryGetValue(config.EntityType, out typeList))
                                {
                                    typeList = new List<EntityConfig>(2);
                                    defaultRepo.Add(config.EntityType, typeList);
                                }
                            }
                            else
                            {
                                var key = new ExtendTypeKey { EntityType = config.EntityType, ExtendView = config.ExtendView };
                                if (!extendRepo.TryGetValue(key, out typeList))
                                {
                                    typeList = new List<EntityConfig>(2);
                                    extendRepo.Add(key, typeList);
                                }
                            }

                            typeList.Add(config);
                        }
                    }
                }
                _typeConfigurations = defaultRepo;
                _typeExtendConfigurations = extendRepo;
            }
        }

        /// <summary>
        /// 获取某个实体视图的所有配置类实例
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="extendView">如果想获取扩展视图列表，则需要传入指定的扩展视图列表</param>
        /// <returns></returns>
        internal static IEnumerable<EntityConfig> FindConfigurations(Type entityType, string extendView = null)
        {
            InitTypeConfigurations();

            var hierachy = TypeHelper.GetHierarchy(entityType, typeof(ManagedPropertyObject)).Reverse();
            if (extendView == null)
            {
                foreach (var type in hierachy)
                {
                    List<EntityConfig> configList = null;
                    if (_typeConfigurations.TryGetValue(type, out configList))
                    {
                        var orderedList = configList.OrderByDescending(o => o.ReuseLevel).ThenBy(o => o.InheritanceCount);
                        foreach (var config in orderedList) { yield return config; }
                    }
                }
            }
            else
            {
                foreach (var type in hierachy)
                {
                    var key = new ExtendTypeKey { EntityType = type, ExtendView = extendView };

                    List<EntityConfig> configList = null;
                    if (_typeExtendConfigurations.TryGetValue(key, out configList))
                    {
                        var orderedList = configList.OrderByDescending(o => o.ReuseLevel).ThenBy(o => o.InheritanceCount);
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

        #endregion
    }
}