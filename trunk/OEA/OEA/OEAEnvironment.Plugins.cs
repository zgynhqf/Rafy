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
                var library = pluginAssembly.Instance as ILibrary;
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
                var module = pluginAssembly.Instance as IModule;
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
            if (Location.IsOnClient())
            {
                return GetAllLibraries().Union(GetAllModules());
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

        internal static Dictionary<Type, List<EntityConfig>> GetTypeConfigurations()
        {
            if (_typeConfigurations == null)
            {
                var repo = new Dictionary<Type, List<EntityConfig>>(100);
                var entityType = typeof(EntityConfig);
                foreach (var p in OEAEnvironment.GetAllPlugins())
                {
                    foreach (var type in p.Assembly.GetTypes())
                    {
                        if (entityType.IsAssignableFrom(type))
                        {
                            var config = Activator.CreateInstance(type).CastTo<EntityConfig>();
                            config.ReuseLevel = (int)p.Instance.ReuseLevel;
                            config.InheritanceCount = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject)).Count();

                            List<EntityConfig> typeList = null;
                            if (!repo.TryGetValue(config.EntityType, out typeList))
                            {
                                typeList = new List<EntityConfig>(2);
                                repo.Add(config.EntityType, typeList);
                            }

                            typeList.Add(config);
                        }
                    }
                }
                _typeConfigurations = repo;
            }

            return _typeConfigurations;
        }

        internal static IEnumerable<EntityConfig> FindConfigurations(Type entityType)
        {
            var hierachy = TypeHelper.GetHierarchy(entityType, typeof(ManagedPropertyObject)).Reverse();
            foreach (var type in hierachy)
            {
                List<EntityConfig> configList = null;
                if (OEAEnvironment.GetTypeConfigurations().TryGetValue(type, out configList))
                {
                    var orderedList = configList.OrderByDescending(o => o.ReuseLevel).ThenBy(o => o.InheritanceCount);
                    foreach (var config in orderedList) { yield return config; }
                }
            }
        }

        #endregion
    }
}