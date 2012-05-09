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
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Practices.Unity;
using Common;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.Utils;
using System.Collections.ObjectModel;

namespace OEA
{
    /// <summary>
    /// OEA 的上下文环境
    /// </summary>
    public static partial class OEAEnvironment
    {
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

        /// <summary>
        /// 依赖注入容器
        /// </summary>
        public static readonly IUnityContainer UnityContainer = new UnityContainer();

        private static IApp _appCore;

        public static IApp AppCore
        {
            get { return _appCore; }
        }

        public static void InitApp(IApp appCore)
        {
            if (_appCore != null) throw new InvalidOperationException();

            _appCore = appCore;
        }

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

        /// <summary>
        /// 使用一个相对的路径来计算绝对路径
        /// </summary>
        /// <param name="appRootRelative"></param>
        /// <returns></returns>
        public static string MapDllPath(string appRootRelative)
        {
            return Path.Combine(Provider.DllRootDirectory, appRootRelative);
        }

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

        public static bool IsDebuggingEnabled
        {
            get { return Provider.IsDebuggingEnabled; }
        }

        private static OEALocation? _Location;

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
                if (_Location.HasValue) throw new InvalidOperationException();

                _Location = value;
            }
        }

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

        public static void EnsureOnServer()
        {
            if (!OEAEnvironment.Location.IsOnServer()) throw new InvalidOperationException();
        }

        public static void EnsureOnClient()
        {
            if (!OEAEnvironment.Location.IsOnClient()) throw new InvalidOperationException();
        }

        private static int _maxId = 100000000;// 本地临时 Id 从 这个值开始
        private static object _maxIdLock = new object();

        public static int NewLocalId()
        {
            lock (_maxIdLock) { return _maxId++; }
        }
    }
}