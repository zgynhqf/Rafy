/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140106
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140106 16:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Rafy;
using Rafy.Reflection;

namespace Rafy.Domain
{
    /// <summary>
    /// 服务实现的定位器
    /// <threadsafety static="true" instance="true"/>
    /// </summary>
    internal static class ServiceLocator
    {
        private static Dictionary<Type, ContractImplList> _allServices = new Dictionary<Type, ContractImplList>(100);

        //由于写和读并不是同一时间段并发，所以不需要锁。
        //private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        //public static TServiceContract Create<TServiceContract>()
        //    where TServiceContract : class,IService
        //{
        //    return Create(typeof(TServiceContract)) as TServiceContract;
        //}

        //public static IService Create(Type contractType)
        //{
        //    var impl = FindImpl(contractType);
        //    if (impl != null)
        //    {
        //        var serviceType = impl.ServiceType;
        //        return Activator.CreateInstance(serviceType) as IService;
        //    }
        //    return null;
        //}

        //public static IService Create(Type contractType, Version version)
        //{
        //    var impl = FindImpl(contractType, version);
        //    if (impl != null)
        //    {
        //        var serviceType = impl.ServiceType;
        //        return Activator.CreateInstance(serviceType) as IService;
        //    }
        //    return null;
        //}

        internal static void TryAddService(Type serviceType)
        {
            //找到在 serviceType 上标记的所有 ContractImplAttribute，并构造对应的 ContractImpl 对象，加入到列表中。
            var attriList = serviceType.GetCustomAttributes(typeof(ContractImplAttribute), false);
            foreach (ContractImplAttribute attri in attriList)
            {
                //try
                //{
                //    _lock.ExitWriteLock();

                var impl = new ContractImpl
                {
                    ServiceType = serviceType,
                    ContractType = attri.ContractType ?? serviceType,
                    Version = new Version(attri.Version),
                };
                if (!impl.ContractType.HasMarked<ContractAttribute>())
                {
                    throw new InvalidProgramException(string.Format(
                        "{0} 类型实现了契约类型 {1} ，需要为这个契约添加 ContractAttribute 标记。", serviceType, impl.ContractType
                        ));
                }

                //找到对应的列表，如果不存在，则添加一个新的列表。
                ContractImplList list = null;
                if (!_allServices.TryGetValue(impl.ContractType, out list))
                {
                    list = new ContractImplList(impl.ContractType);
                    _allServices.Add(impl.ContractType, list);
                }

                list.Add(impl);
                //}
                //finally
                //{
                //    _lock.ExitWriteLock();
                //}
            }
        }

        internal static ContractImpl FindImpl(Type contractType)
        {
            //try
            //{
            //    _lock.EnterReadLock();

            ContractImplList list = null;
            if (_allServices.TryGetValue(contractType, out list))
            {
                return list.Default();
            }
            //}
            //finally
            //{
            //    _lock.ExitReadLock();
            //}

            return null;
        }

        internal static ContractImpl FindImpl(Type contractType, Version version)
        {
            //try
            //{
            //    _lock.EnterReadLock();

            ContractImplList list = null;
            if (_allServices.TryGetValue(contractType, out list))
            {
                var res = list.Find(version);
                if (res != null) { return res; }
            }
            //}
            //finally
            //{
            //    _lock.ExitReadLock();
            //}

            return null;
        }

        #region private class ContractImplList

        private class ContractImplList
        {
            private List<ContractImpl> _list = new List<ContractImpl>();

            /// <summary>
            /// 对应这个契约类型
            /// </summary>
            private Type _contractType;

            public ContractImplList(Type contractType)
            {
                _contractType = contractType;
            }

            public ContractImpl Default()
            {
                if (_list.Count > 0)
                {
                    return _list[_list.Count - 1];
                }
                return null;
            }

            /// <summary>
            /// 添加一个契约实现到服务中。
            /// </summary>
            /// <param name="impl"></param>
            public void Add(ContractImpl impl)
            {
                var exists = this.Find(impl.Version);
                if (exists != null)
                {
                    //以防止重入的方式来添加服务的实现，防止多次启动应用程序而重复添加同一服务。
                    if (exists.ServiceType == impl.ServiceType) { return; }

                    throw new InvalidProgramException(string.Format(
                        "契约 {0} 不能同时注册相同版本号({1})的两个服务实现：{2}、{3}。",
                        _contractType,
                        impl.Version,
                        exists.ServiceType,
                        impl.ServiceType
                        ));
                }

                _list.Add(impl);
                _list.Sort();
            }

            public void Remove(ContractImpl impl)
            {
                _list.Remove(impl);
            }

            /// <summary>
            /// 通过版本号来查找对应的契约实现。
            /// </summary>
            /// <param name="version"></param>
            /// <returns></returns>
            public ContractImpl Find(Version version)
            {
                int min = 0, max = _list.Count - 1;
                while (min <= max)
                {
                    var i = min + (max - min >> 1);//(min + max) / 2
                    var item = _list[i];
                    var cResult = item.Version.CompareTo(version);
                    if (cResult > 0)
                    {
                        max = i - 1;
                    }
                    else if (cResult < 0)
                    {
                        min = i + 1;
                    }
                    else
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        #endregion

        #region private class ContractImpl

        internal class ContractImpl : IComparable<ContractImpl>
        {
            public Type ServiceType;
            public Type ContractType;
            public Version Version;

            public int CompareTo(ContractImpl other)
            {
                return this.Version.CompareTo(other.Version);
            }
        }

        #endregion
    }
}