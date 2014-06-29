/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120310
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120310
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rafy;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 这个类为 ClientApp、ServerApp、WebApp 等类提供了一致的基类。
    /// </summary>
    public abstract class AppImplementationBase : IServerApp, IClientApp
    {
        /// <summary>
        /// 子类在合适的时间调用此方法来启动整个 Rafy 应用程序。
        /// 
        /// 注意，为了支持重新启动，这个类中的所有方法都应该可以运行多次。
        /// 
        /// 但是，第二次及之后的重启，不应该使用同一个 AppImplementationBase 的实例，否则可能会造成插件重复监听同一事件。
        /// </summary>
        protected void StartupApplication()
        {
            this.PrepareToStartup();

            this.InitEnvironment();

            //注册所有扩展属性
            //由于插件的 Intialize 方法中应用层代码，有可能主动使用实体类而造成实体类的静态构造函数被执行，
            //所以必须在 Intialize 方法执行前，即任何实体被使用前，初始化所有的扩展属性。
            RafyEnvironment.InitExtensionProperties();

            //调用所有插件的 Initialize 方法。
            this.InitAllPlugins();
            this.OnAllPluginsIntialized();

            //初始化编译期元数据
            this.CompileMeta();
            this.OnMetaCompiled();

            //定义模块列表
            this.RaiseModuleOpertions();
            this.OnModuleOpertionsCompleted();

            //冻结模块的元数据
            CommonModel.Modules.Freeze();
            this.OnAppMetaCompleted();

            //组合所有模块的 IOC、事件、
            this.Compose();
            this.RaiseComposeOperations();
            this.OnComposed();

            //开始运行时行为。此行代码后的所有代码都可以看作运行时行为。
            this.OnRuntimeStarting();

            //设置多国语言
            this.SetupLanguage();

            //启动主过程
            this.OnMainProcessStarting();
            this.StartMainProcess();

            //整个初始化完毕。
            this.OnStartupCompleted();
        }

        /// <summary>
        /// 注意，以下代码应该保证可以多次运行。
        /// 这是因为如果一旦出现异常，程序可以尝试再次使用此方法以重新启动。
        /// </summary>
        protected virtual void PrepareToStartup()
        {
            RafyEnvironment.Reset();
            CommonModel.Reset();
            UIModel.Reset();
            WPFCommandNames.Clear();
        }

        protected virtual void InitEnvironment()
        {
            //如果配置了文化，则修改 UI 文化。否则使用系统默认的文化。
            var cultureName = RafyEnvironment.Configuration.Section.CurrentCulture;
            if (!string.IsNullOrWhiteSpace(cultureName))
            {
                try
                {
                    var culture = CultureInfo.GetCultureInfo(cultureName);
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
                catch (CultureNotFoundException) { }
            }

            ServerContext.SetCurrent(new WebThreadContextProvider());

            RafyEnvironment.InitCustomizationPath();

            RafyEnvironment.SetApp(this);
        }

        /// <summary>
        /// 初始化所有Plugins
        /// </summary>
        protected void InitAllPlugins()
        {
            //先初始化实体插件
            RafyEnvironment.StartupDomainPlugins();

            //初始化界面插件。
            RafyEnvironment.StartupUIPlugins();

            //switch (RafyEnvironment.Location)
            //{
            //    case RafyLocation.WPFClient:
            //    case RafyLocation.LocalVersion:
            //        //初始化界面插件。
            //        RafyEnvironment.StartupUIPlugins();
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
        /// 设置当前语言
        /// 
        /// 需要在所有 Translator 依赖注入完成后调用。
        /// </summary>
        private void SetupLanguage()
        {
            //当前线程的文化名，就是 Rafy 多国语言的标识。
            var culture = Thread.CurrentThread.CurrentUICulture.Name;
            if (!Translator.IsDevCulture(culture))
            {
                var translator = RafyEnvironment.Provider.Translator;
                //目前，在服务端进行翻译时，只支持一种语言。所以尽量在客户端进行翻译。
                translator.CurrentCulture = culture;
                translator.Enabled = true;
            }
        }

        /// <summary>
        /// 初始化必须在初始化期定义的各种元数据。
        /// </summary>
        protected virtual void CompileMeta() { }

        /// <summary>
        /// 组合所有的组件。
        /// </summary>
        protected virtual void Compose()
        {
            ComposeObjectContainer();
        }

        #region ComposeObjectContainer

        /// <summary>
        /// 组合所有组件中标记了 ContainerItemAttribute 的类型到 IOC 容器中。
        /// </summary>
        protected virtual void ComposeObjectContainer()
        {
            //处理 ContainerItemAttribute
            var iocContainer = Composer.ObjectContainer;
            var instancesRegistry = new List<InstanceContainerItem>(100);
            foreach (var plugin in RafyEnvironment.GetAllPlugins())
            {
                var types = plugin.Assembly.GetTypes();
                for (int i = 0, c = types.Length; i < c; i++)
                {
                    var type = types[i];
                    if (!type.IsAbstract && !type.IsGenericTypeDefinition)
                    {
                        var attriList = type.GetCustomAttributes(typeof(ContainerItemAttribute), false);
                        if (attriList.Length > 0)
                        {
                            foreach (ContainerItemAttribute attri in attriList)
                            {
                                if (attri.RegisterWay == RegisterWay.Type)
                                {
                                    iocContainer.RegisterType(attri.ProvideFor, type, attri.Key);
                                }
                                else
                                {
                                    instancesRegistry.Add(new InstanceContainerItem
                                    {
                                        RegisterType = type,
                                        Attribute = attri
                                    });
                                }
                            }
                        }
                    }
                }
            }
            //实例的注册放到最后，而且按照优先级进行排序后，再注册。
            instancesRegistry = instancesRegistry.OrderBy(i => i.Attribute.Priority).ToList();
            foreach (var item in instancesRegistry)
            {
                var instance = iocContainer.Resolve(item.RegisterType);
                iocContainer.RegisterInstance(item.Attribute.ProvideFor, instance, item.Attribute.Key);
            }
        }

        private class InstanceContainerItem
        {
            public Type RegisterType;
            public ContainerItemAttribute Attribute;
        }

        #endregion

        /// <summary>
        /// 子类重写此方法实现启动主逻辑。
        /// </summary>
        protected virtual void StartMainProcess() { }

        #region IClientApp IServerApp 的方法

        public virtual void ShowMessage(string message, string title) { }

        public virtual void Shutdown() { }

        #endregion

        #region IClientApp IServerApp 的事件

        public event EventHandler AllPluginsIntialized;

        protected virtual void OnAllPluginsIntialized()
        {
            var handler = this.AllPluginsIntialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler MetaCompiled;

        protected virtual void OnMetaCompiled()
        {
            var handler = this.MetaCompiled;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler CommandMetaIntialized;

        protected virtual void OnCommandMetasIntialized()
        {
            var handler = this.CommandMetaIntialized;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ModuleOperations;

        protected virtual void RaiseModuleOpertions()
        {
            var handler = this.ModuleOperations;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ModuleOperationsCompleted;

        protected virtual void OnModuleOpertionsCompleted()
        {
            var handler = this.ModuleOperationsCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler AppMetaCompleted;

        protected virtual void OnAppMetaCompleted()
        {
            var handler = this.AppMetaCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler ComposeOperations;

        protected virtual void RaiseComposeOperations()
        {
            var handler = this.ComposeOperations;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler Composed;

        protected virtual void OnComposed()
        {
            var handler = this.Composed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler RuntimeStarting;

        protected virtual void OnRuntimeStarting()
        {
            var handler = this.RuntimeStarting;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler MainProcessStarting;

        protected virtual void OnMainProcessStarting()
        {
            var handler = this.MainProcessStarting;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler Exit;

        protected virtual void OnExit()
        {
            var handler = this.Exit;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LoginSuccessed;

        protected virtual void OnLoginSuccessed()
        {
            var handler = this.LoginSuccessed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler LoginFailed;

        protected virtual void OnLoginFailed()
        {
            var handler = this.LoginFailed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler StartupCompleted;

        protected virtual void OnStartupCompleted()
        {
            var handler = this.StartupCompleted;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }
}