/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140629
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140629 01:24
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
    /// 从 IObjectContainer 到 IServiceContainer 的适配器。
    /// </summary>
    public class ObjectContainerToServiceConatinerAdapter : IServiceContainer
    {
        private IObjectContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectContainerToServiceConatinerAdapter"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <exception cref="System.ArgumentNullException">container</exception>
        public ObjectContainerToServiceConatinerAdapter(IObjectContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        /// <summary>
        /// 如果某个服务有多个实例，则可以使用此方法来获取所有的实例。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.ResolveAll(serviceType);
        }

        /// <summary>
        /// 如果某个服务有多个实例，则可以通过一个键去获取对应的服务实例。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetService(Type serviceType, string key)
        {
            return _container.Resolve(serviceType, key);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType" />.-or- null if there is no service object of type <paramref name="serviceType" />.
        /// </returns>
        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}
