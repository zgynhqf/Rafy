/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211027
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211027 01:55
 * 
*******************************************************/

using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Rafy.ComponentModel;

namespace Rafy.UI
{
    /// <summary>
    /// Environment of UIApplication
    /// </summary>
    public static class UIEnvironment
    {
        /// <summary>
        /// 当前应用程序是否是一个 WPF UI 应用程序。
        /// </summary>
        public static bool IsWPFUI { get; set; }

        /// <summary>
        /// 当前应用程序是否是一个 Web UI 应用程序。
        /// </summary>
        public static bool IsWebUI { get; set; }

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
                        RafyEnvironment.LoadPlugin(type.Assembly);
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
                        RafyEnvironment.LoadPlugin(type.Assembly);
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
                    lock (this)
                    {
                        if (_extendConfigurations == null)
                        {
                            InitConfigurationsCore();
                        }
                    }
                }
            }

            private void InitConfigurationsCore()
            {
                /*********************** 代码块解释 *********************************
                * 查找所有 EntityConfig 类型，并根据是否为扩展视图的配置类，
                * 分别加入到两个不同的列表中。
                **********************************************************************/

                _configurations = new Dictionary<Type, List<ViewConfig>>(100);
                _extendConfigurations = new Dictionary<ExtendTypeKey, List<ViewConfig>>(100);

                //视图配置可以放在所有插件中。
                RafyEnvironment.HandleAllPlugins(AddByPlugin);
            }

            private void AddByPlugin(IPlugin plugin)
            {
                foreach (var type in plugin.Assembly.GetTypes())
                {
                    if (!type.IsGenericTypeDefinition && !type.IsAbstract && _viewConfigType.IsAssignableFrom(type))
                    {
                        var config = Activator.CreateInstance(type) as ViewConfig;
                        config.PluginIndex = RafyEnvironment.Plugins.IndexOf(plugin);
                        config.InheritanceCount = TypeHelper.GetHierarchy(type, typeof(ManagedPropertyObject)).Count();

                        List<ViewConfig> typeList = null;

                        if (config.ExtendView == null)
                        {
                            if (!_configurations.TryGetValue(config.EntityType, out typeList))
                            {
                                typeList = new List<ViewConfig>(2);
                                _configurations.Add(config.EntityType, typeList);
                            }
                        }
                        else
                        {
                            var key = new ExtendTypeKey { EntityType = config.EntityType, ExtendView = config.ExtendView };
                            if (!_extendConfigurations.TryGetValue(key, out typeList))
                            {
                                typeList = new List<ViewConfig>(2);
                                _extendConfigurations.Add(key, typeList);
                            }
                        }

                        typeList.Add(config);
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

        private static Translator _translator;

        /// <summary>
        /// 当前使用的翻译器
        /// </summary>
        public static Translator Translator
        {
            get
            {
                if (_translator == null)
                {
                    _translator = new EmptyTranslator();
                }
                return _translator;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");

                _translator = value;
            }
        }

        #region InitCustomizationPath

        private static BranchPathProvider _branchProvider = new BranchPathProvider();

        /// <summary>
        /// 查找文件路径的查找算法提供器。
        /// </summary>
        public static BranchPathProvider BranchProvider
        {
            get { return _branchProvider; }
        }

        private const string DomainPluginFolder = "Domain";
        private const string UIPluginFolder = "UI";

        /// <summary>
        /// 获取所有此版本中需要加载的实体类Dll集合。
        /// </summary>
        /// <returns></returns>
        public static string[] GetCustomerEntityDlls(bool toAbsolute = true)
        {
            return _branchProvider.MapAllBranchPathes(DomainPluginFolder, toAbsolute);
        }

        /// <summary>
        /// 获取所有此版本中需要加载的模块Dll集合。
        /// </summary>
        /// <returns></returns>
        internal static string[] GetCustomerModuleDlls()
        {
            return _branchProvider.MapAllBranchPathes(UIPluginFolder, false);
        }

        #endregion
    }
}