/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130904
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130904 11:32
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Rafy.Threading;

namespace Rafy.Domain
{
    /// <summary>
    /// 服务工厂
    /// <remarks>
    /// 本类是线程安全的。
    /// </remarks>
    /// </summary>
    public static class ServiceFactory
    {
        /// <summary>
        /// 创建一个具体的服务。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Create<T>()
            where T : IService
        {
            return (T)Create(typeof(T));
        }

        /// <summary>
        /// 创建一个具体的服务。
        /// </summary>
        /// <param name="contractType">契约类型。</param>
        /// <returns></returns>
        public static IService Create(Type contractType)
        {
            IService res = null;

            var impl = ServiceLocator.FindImpl(contractType);
            if (impl != null)
            {
                res = Activator.CreateInstance(impl.ServiceType) as IService;
                if (res == null)
                {
                    throw new InvalidProgramException(string.Format("{0} 类型必须实现 Rafy.Domain.IService 接口。", impl.ServiceType));
                }
            }
            //else
            //{
            //    if (contractType.IsClass && !contractType.IsAbstract)
            //    {
            //        //如果没有找到契约的实现，而且传入的类型是一个具体的服务类型，则尝试直接创建该类的对象为服务。
            //        res = Activator.CreateInstance(contractType) as IService;
            //    }
            //}

            if (res == null)
            {
                throw new InvalidProgramException(string.Format("没有注册实现 {0} 契约类型的服务，请在相应的服务类型上标记 ContractImplAttribute。", contractType));
            }

            return res;
        }

        /// <summary>
        /// 创建一个指定版本的服务。
        /// </summary>
        /// <param name="contractType">服务类型.</param>
        /// <param name="version">需要的服务的版本号.</param>
        /// <returns></returns>
        public static IService Create(Type contractType, Version version)
        {
            var impl = ServiceLocator.FindImpl(contractType, version);
            if (impl != null)
            {
                return Activator.CreateInstance(impl.ServiceType) as IService;
            }

            return null;
        }
    }
}