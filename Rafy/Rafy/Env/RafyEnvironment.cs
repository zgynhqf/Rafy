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
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Windows;
using Rafy;
using Rafy.ComponentModel;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Utils;

namespace Rafy
{
    /// <summary>
    /// Rafy 的上下文环境
    /// </summary>
    public static partial class RafyEnvironment
    {
        #region Provider

        private static EnvironmentProvider _provider = new EnvironmentProvider();

        /// <summary>
        /// 获取应用程序环境的值提供器。
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public static EnvironmentProvider Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        #endregion

        /// <summary>
        /// Rafy 配置信息。
        /// </summary>
        public static readonly RafyConfiguration Configuration = new RafyConfiguration();

        #region 身份

        /// <summary>
        /// 返回当前上下文中的当前用户。
        /// 
        /// 本属性不会为 null，请使用 IsAuthenticated 属性来判断是否已经登录。
        /// 
        /// 如果想使用实体的 Id 属性，可尝试将此属性转换为 <see cref="IRafyIdentity"/> 接口。
        /// </summary>
        public static IIdentity Identity
        {
            get
            {
                var user = Principal.Identity;
                if (user != null) return user;

                return new AnonymousIdentity();
            }
        }

        /// <summary>
        /// 返回当前上下文中的当前身份。
        /// </summary>
        public static IPrincipal Principal
        {
            get
            {
                var current = AppContext.CurrentPrincipal;
                if (current == null)
                {
                    current = new AnonymousPrincipal();
                    AppContext.CurrentPrincipal = current;
                }
                return current;
            }
            set
            {
                AppContext.CurrentPrincipal = value;
            }
        }

        #endregion

        #region Language

        /// <summary>
        /// 把程序中编写的字符串翻译为当前语言。
        /// 
        /// 直接扩展在字符串上的翻译方法，方便使用
        /// </summary>
        /// <param name="embadedValue"></param>
        /// <returns></returns>
        public static string Translate(this string embadedValue)
        {
            var translator = _provider.Translator;
            return translator != null ? translator.Translate(embadedValue) : embadedValue;
        }

        /// <summary>
        /// 把当前语言翻译为程序中编写的字符串。
        /// 
        /// 直接扩展在字符串上的翻译方法，方便使用
        /// </summary>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        public static string TranslateReverse(this string translatedValue)
        {
            var translator = _provider.Translator;
            return translator != null ? translator.TranslateReverse(translatedValue) : translatedValue;
        }

        #endregion

        #region IApp AppCore

        private static IApp _appCore;

        /// <summary>
        /// 当前的应用程序运行时。
        /// </summary>
        public static IApp App
        {
            get { return _appCore; }
        }

        internal static void SetApp(IApp appCore)
        {
            _appCore = appCore;
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
        public static string MapAbsolutePath(string appRootRelative)
        {
            return Path.Combine(Provider.RootDirectory, appRootRelative);
        }

        /// <summary>
        /// 把绝对路径转换为相对路径。
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static string MapRelativePath(string absolutePath)
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

        [ThreadStatic]
        private static int _threadPortalCount;

        /// <summary>
        /// 应用程序默认的数据门户模式。
        /// </summary>
        public static DataPortalMode DataPortalMode { get; set; } = DataPortalMode.ConnectDirectly;

        /// <summary>
        /// 是否应用程序直接连接数据。
        /// DataPortalMode == DataPortalMode.DirectConnect。
        /// </summary>
        public static bool ConnectDataDirectly
        {
            get { return DataPortalMode == DataPortalMode.ConnectDirectly; }
        }

        /// <summary>
        /// 获取当前线程目前已经进入的数据门户层数。
        /// </summary>
        public static int ThreadPortalCount
        {
            get { return _threadPortalCount; }
            internal set
            {
                //设置此值的位置如下：
                //* Repository.Query
                //* Repository.Save
                //* Service.Invoke
                _threadPortalCount = value;
            }
        }

        ///// <summary>
        ///// 保证当前代码正在运行在服务端，否则抛出异常。
        ///// </summary>
        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        //public static void EnsureOnServer()
        //{
        //    if (!IsOnServer()) throw new InvalidOperationException();
        //}

        ///// <summary>
        ///// 保证当前代码正在运行在客户端，否则抛出异常。
        ///// </summary>
        //[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        //public static void EnsureOnClient()
        //{
        //    if (!IsOnClient()) throw new InvalidOperationException();
        //}

        /// <summary>
        /// 判断是否在服务端。
        /// 
        /// 如果是单机版，则当进入至少一次数据门户后，才能算作服务端，返回true。
        /// </summary>
        /// <returns></returns>
        public static bool IsOnServer()
        {
            return !IsOnClient();
        }

        /// <summary>
        /// 判断是否在客户端
        /// 单机版，如果还没有进入数据门户中，则同样返回 true。
        /// </summary>
        /// <returns></returns>
        public static bool IsOnClient()
        {
            return _appCore.IsOnClient();
        }

        #endregion

        #region NewLocalId

        private static int _maxId = 1000000000;// 本地临时 Id 从 这个值开始（int.MaxValue:2147483647)
        private static object _maxIdLock = new object();

        /// <summary>
        /// 返回一个本地的 Id，该 Id 在当前应用程序中是唯一的，每次调用都会自增一。
        /// </summary>
        /// <returns></returns>
        public static int NewLocalId()
        {
            lock (_maxIdLock) { return _maxId++; }
        }

        #endregion

        /// <summary>
        /// 帮助调试的变量，可随时把即时窗口中的临时对象放在这里进行查看。
        /// </summary>
        public static object DebugHelper;
    }
}