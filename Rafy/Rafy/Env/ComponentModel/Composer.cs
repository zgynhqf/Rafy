/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140625
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140625 15:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 组件组合器。
    /// 实现组件间组合的通信机制。
    /// </summary>
    public static class Composer
    {
        #region IObjectContainer

        private static IObjectContainer _defaultContainer;

        /// <summary>
        /// 默认的 IOC 容器。
        /// </summary>
        public static IObjectContainer ObjectContainer
        {
            get
            {
                if (_defaultContainer == null)
                {
                    _defaultContainer = ObjectContainerFactory.CreateContainer();
                }
                return _defaultContainer;
            }
        }

        #endregion

        #region ServiceContainer

        private static IServiceContainer _serviceContainer;

        /// <summary>
        /// 组件的服务容器。
        /// </summary>
        public static IServiceContainer ServiceContainer
        {
            get
            {
                if (_serviceContainer == null)
                {
                    _serviceContainer = ObjectContainer.ResolveAll<IServiceContainer>().FirstOrDefault();
                    if (_serviceContainer == null)
                    {
                        //var objContainer = ObjectContainerFactory.CreateContainer();
                        _serviceContainer = new ObjectContainerToServiceConatinerAdapter(ObjectContainer);
                    }
                }
                return _serviceContainer;
            }
        }

        #endregion

        #region EventBus

        private static IEventBus _eventBus;

        /// <summary>
        /// 事件总线
        /// </summary>
        public static IEventBus EventBus
        {
            get
            {
                if (_eventBus == null)
                {
                    _eventBus = ObjectContainer.ResolveAll<IEventBus>().FirstOrDefault();

                    //如果服务容器中没有 IEventBus 实例，则创建一个默认的事件总线。
                    if (_eventBus == null)
                    {
                        _eventBus = new EventBus();
                    }
                }
                return _eventBus;
            }
        }

        #endregion

        #region RegisterByAttribute

        /// <summary>
        /// 组合所有组件中标记了 <see cref="ContainerItemAttribute"/> 的类型到 IOC 容器中。
        /// 
        /// 此方法只能调用一次，
        /// 而且应该重写 <see cref="AppImplementationBase.RaiseComposeOperations"/> 方法中调用。
        /// </summary>
        public static void RegisterAllPluginsByAttribute()
        {
            var assemblies = RafyEnvironment.AllPlugins.Select(p => p.Assembly);
            RegisterByAttribute(assemblies);
        }

        /// <summary>
        /// 注册指定插件中标记了 <see cref="ContainerItemAttribute" /> 的类型到 IOC 容器中。
        /// 此方法应该在 ComposeOperations 周期中执行。
        /// 
        /// <example>
        /// <![CDATA[
        /// 使用方法
        /// app.ComposeOperations += app_ComposeOperations;
        /// void app_ComposeOperations(object sender, EventArgs e)
        /// {
        ///     //直接通过 ContainerItemAttribute 注册整个程序集。
        ///     //Composer.AutoRegisterByContainerItemAttribute(this);
        /// 
        ///     //使用 ObjectContainer 来注册。
        ///     //var container = Composer.ObjectContainer;
        ///     //container.RegisterType<IPlugin, UnityAdapterPlugin>();
        /// 
        ///     //引用 Rafy.ComponentModel.UnityAdapter 插件后，还可以使用 UnityContainer 来注册，并同时注册拦截器。
        ///     //var container = UnityAdapterHelper.GetUnityContainer(Composer.ObjectContainer);
        ///     //container.RegisterType<IPlugin, UnityAdapterPlugin>();
        /// }
        /// ]]>
        /// </example>
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        /// <exception cref="System.ArgumentNullException">plugin</exception>
        public static void RegisterByAttribute(IPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException("plugin");
            RegisterByAttribute(new Assembly[] { plugin.Assembly });
        }

        private static void RegisterByAttribute(IEnumerable<Assembly> assemblies)
        {
            //处理 ContainerItemAttribute
            var iocContainer = ObjectContainer;
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                for (int i = 0, c = types.Length; i < c; i++)
                {
                    var type = types[i];
                    if (!type.IsInterface && !type.IsAbstract && !type.IsGenericTypeDefinition)
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
                                    iocContainer.RegisterInstance(attri.ProvideFor, type, attri.Key);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
