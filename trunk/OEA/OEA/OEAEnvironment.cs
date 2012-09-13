/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110218
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100218
 * 添加Location属性。 胡庆访 20100308
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
using System.Runtime;
using Common;
using Microsoft.Practices.Unity;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.Utils;

namespace OEA
{
    /// <summary>
    /// OEA 的上下文环境
    /// </summary>
    public static partial class OEAEnvironment
    {
        #region Provider

        private static EnvironmentProvider _provider;
        public static EnvironmentProvider Provider
        {
            get
            {
                if (_provider == null)
                {
                    _provider = new EnvironmentProvider();
                }
                return _provider;
            }
            set
            {
                _provider = value;
            }
        }

        #endregion

        /// <summary>
        /// 依赖注入容器
        /// </summary>
        public static readonly IUnityContainer UnityContainer = new UnityContainer();

        #region IApp AppCore

        private static IApp _appCore;

        public static IApp AppCore
        {
            get { return _appCore; }
        }

        internal static void InitApp(IApp appCore)
        {
            if (_appCore != null) throw new InvalidOperationException();

            _appCore = appCore;
        }

        #endregion

        #region InitCustomizationPath

        private static PathProvider _pathProvider;

        /// <summary>
        /// 查找文件路径的查找算法提供器。
        /// </summary>
        public static PathProvider CustomerProvider
        {
            get { return _pathProvider; }
        }

        /// <summary>
        /// 提供一个先初始化路径的方法，这个方法可以单独先被调用。
        /// 这样，就可以通过路径找到需要的程序集，其它的初始化才能正常进行。
        /// </summary>
        public static void InitCustomizationPath()
        {
            if (_pathProvider == null)
            {
                _pathProvider = new PathProvider();

                //分支版本名。
                //同时，这个也是客户化文件夹的名字。
                //分支版本定义，需要重写这个属性。
                string customerDir = ConfigurationHelper.GetAppSettingOrDefault("CustomerDir");
                if (!string.IsNullOrWhiteSpace(customerDir))
                {
                    _pathProvider.AddBranch(customerDir);
                }
            }
        }

        /// <summary>
        /// 获取所有此版本中需要加载的实体类Dll集合。
        /// </summary>
        /// <returns></returns>
        public static string[] GetEntityDlls(bool toAbsolute = true)
        {
            return _pathProvider.MapAllPathes("Library", toAbsolute);
        }

        /// <summary>
        /// 获取所有此版本中需要加载的模块Dll集合。
        /// </summary>
        /// <returns></returns>
        public static string[] GetModuleDlls()
        {
            return _pathProvider.MapAllPathes("Module", false);
        }

        #endregion

        #region Path Mapping

        /// <summary>
        /// 使用一个相对的路径来计算绝对路径
        /// </summary>
        /// <param name="appRootRelative"></param>
        /// <returns></returns>
        public static string MapDllPath(string appRootRelative)
        {
            return Path.Combine(Provider.DllRootDirectory, appRootRelative);
        }

        /// <summary>
        /// 相对路径转换为绝对路径。
        /// </summary>
        /// <param name="appRootRelative"></param>
        /// <returns></returns>
        public static string ToAbsolute(string appRootRelative)
        {
            return Path.Combine(Provider.RootDirectory, appRootRelative);
        }

        /// <summary>
        /// 把绝对路径转换为相对路径。
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static string ToRelative(string absolutePath)
        {
            return absolutePath.Replace(Provider.RootDirectory, string.Empty);
        }

        #endregion

        /// <summary>
        /// 当前是否正处于调试状态。
        /// </summary>
        public static bool IsDebuggingEnabled
        {
            get { return Provider.IsDebuggingEnabled; }
        }

        #region Location

        private static OEALocation? _Location;

        [ThreadStatic]
        private static int _threadPortalCount;

        /// <summary>
        /// 当前应用程序执行环境的位置。
        /// </summary>
        public static OEALocation Location
        {
            get
            {
                return _Location.GetValueOrDefault(OEALocation.WPFServer);
                //if (ApplicationContext.ExecutionLocation == ApplicationContext.ExecutionLocations.Server)
                //{
                //    return OEALocation.Server;
                //}

                //if (ApplicationContext.DataPortalProxy.EqualsIgnorecase("local"))
                //{
                //    return OEALocation.LocalVersion;
                //}

                //return OEALocation.Client;
            }
            set
            {
                if (_Location.HasValue) throw new InvalidOperationException("Location 只能被设置一次。");

                _Location = value;
            }
        }

        /// <summary>
        /// 获取当前线程目前已经进入的数据门户层数。
        /// 
        /// Set 方法为 OEA 框架内部调用，外部请不要设置，否则会引起未知的异常。
        /// </summary>
        public static int ThreadPortalCount
        {
            get { return _threadPortalCount; }
            set { _threadPortalCount = value; }
        }

        /// <summary>
        /// 保证当前代码正在运行在服务端，否则抛出异常。
        /// </summary>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void EnsureOnServer()
        {
            if (!IsOnServer()) throw new InvalidOperationException();
        }

        /// <summary>
        /// 保证当前代码正在运行在客户端，否则抛出异常。
        /// </summary>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void EnsureOnClient()
        {
            if (!IsOnClient()) throw new InvalidOperationException();
        }

        /// <summary>
        /// 判断是否在服务端。
        /// 
        /// 如果是单机版，则当进入至少一次数据门户后，才能算作服务端，返回true。
        /// </summary>
        /// <returns></returns>
        public static bool IsOnServer()
        {
            //当在服务端、或者是单机版模拟服务端时，默认值为直接在服务端运行。
            var l = Location;
            return l == OEALocation.WebServer || l == OEALocation.WPFServer ||
                (l == OEALocation.LocalVersion && _threadPortalCount > 0);
        }

        /// <summary>
        /// 判断是否在客户端
        /// 
        /// 单机版，同样返回true。
        /// </summary>
        /// <returns></returns>
        public static bool IsOnClient()
        {
            var l = Location;
            return l == OEALocation.Client || l == OEALocation.LocalVersion;
        }

        #endregion

        #region Web or WPF

        /// <summary>
        /// Web or WPF
        /// </summary>
        public static bool IsWeb
        {
            get { return Location == OEALocation.WebServer; }
        }

        /// <summary>
        /// WPF or Web
        /// </summary>
        public static bool IsWPF
        {
            get { return Location != OEALocation.WebServer; }
        }

        /// <summary>
        /// 是否需要 Web 命令。
        /// </summary>
        public static bool NeedWebCommands
        {
            get
            {
                return Location == OEALocation.WebServer;
            }
        }

        /// <summary>
        /// 是否需要添加 WPF 命令。
        /// </summary>
        public static bool NeedWPFCommands
        {
            get
            {
                var l = Location;
                return l == OEALocation.Client && l == OEALocation.LocalVersion;
            }
        }

        #endregion

        #region NewLocalId

        private static int _maxId = 100000000;// 本地临时 Id 从 这个值开始
        private static object _maxIdLock = new object();

        public static int NewLocalId()
        {
            lock (_maxIdLock) { return _maxId++; }
        }

        #endregion
    }
}