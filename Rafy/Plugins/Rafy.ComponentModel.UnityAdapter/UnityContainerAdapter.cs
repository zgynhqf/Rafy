/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140704
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140704 23:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Rafy.ComponentModel.UnityAdapter
{
    class UnityContainerAdapter : IObjectContainer
    {
        private UnityContainer _container;

        public UnityContainerAdapter(UnityContainer container)
        {
            _container = container;
        }

        public UnityContainer Container
        {
            get { return _container; }
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return _container.ResolveAll(type);
        }

        public object Resolve(Type type, string key = null)
        {
            return _container.Resolve(type, key);
        }

        public void RegisterInstance(Type type, object instance, string key = null)
        {
            _container.RegisterInstance(type, key, instance);
        }

        private static readonly InjectionMember[] EmptyInjection = new InjectionMember[0];

        public void RegisterInstance(Type type, Type instanceType, string key = null)
        {
            _container.RegisterType(type, instanceType, key, new ContainerControlledLifetimeManager(), EmptyInjection);
        }

        public void RegisterType(Type from, Type to, string key = null)
        {
            _container.RegisterType(from, to, key);
        }
    }
}