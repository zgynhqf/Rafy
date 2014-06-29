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
    }
}
