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
    internal class ObjectContainerToServiceConatinerAdapter : IServiceContainer
    {
        private IObjectContainer _container;

        public ObjectContainerToServiceConatinerAdapter(IObjectContainer container)
        {
            if (container == null) throw new ArgumentNullException("container");
            _container = container;
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.ResolveAll(serviceType);
        }

        public object GetService(Type serviceType, string key)
        {
            return _container.Resolve(serviceType, key);
        }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}
