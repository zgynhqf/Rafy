/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140627
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140627 23:37
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    internal class SimpleObjectContainer : IObjectContainer
    {
        private static readonly object[] _emptyArguments = new object[0];
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly Dictionary<Type, Dictionary<string, Type>> _types = new Dictionary<Type, Dictionary<string, Type>>();
        private readonly Dictionary<Type, Dictionary<string, object>> _instances = new Dictionary<Type, Dictionary<string, object>>();

        public void RegisterInstance(Type type, object instance, string key = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (instance == null) throw new ArgumentNullException("instance");
            if (key == null) key = string.Empty;

            try
            {
                _lock.EnterWriteLock();

                Dictionary<string, object> res = null;
                if (!_instances.TryGetValue(type, out res))
                {
                    res = new Dictionary<string, object>();
                    _instances.Add(type, res);
                }

                res[key] = instance;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RegisterType(Type from, Type to, string key = null)
        {
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            if (key == null) { key = string.Empty; }

            try
            {
                _lock.EnterWriteLock();

                Dictionary<string, Type> types = null;

                if (!_types.TryGetValue(from, out types))
                {
                    types = new Dictionary<string, Type>();
                    _types.Add(from, types);
                }

                types[key] = to;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public object Resolve(Type type, string key = null)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (key == null) { key = string.Empty; }

            try
            {
                _lock.EnterReadLock();

                return ResolveInstance(type, key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            try
            {
                _lock.EnterReadLock();

                Dictionary<string, object> instances = null;
                if (_instances.TryGetValue(type, out instances))
                {
                    foreach (var kv in instances)
                    {
                        yield return kv.Value;
                    }
                }

                Dictionary<string, Type> types = null;
                if (_types.TryGetValue(type, out types))
                {
                    foreach (var kv in types)
                    {
                        yield return this.CreateInstance(kv.Value);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private object ResolveInstance(Type type, string key)
        {
            //先尝试从 _instances 集合中获取
            Dictionary<string, object> instances = null;
            if (_instances.TryGetValue(type, out instances))
            {
                object res = null;
                if (instances.TryGetValue(key, out res))
                {
                    return res;
                }
            }

            Dictionary<string, Type> types = null;
            if (_types.TryGetValue(type, out types))
            {
                Type typeResult = null;
                if (types.TryGetValue(key, out typeResult))
                {
                    return this.CreateInstance(typeResult);
                }
            }

            return this.CreateInstance(type);
        }

        private object CreateInstance(Type type)
        {
            var constructor = type.GetConstructors()[0];
            var parameterInfos = constructor.GetParameters();
            if (parameterInfos.Length == 0)
            {
                return constructor.Invoke(_emptyArguments);
            }

            var parameters = new object[parameterInfos.Length];
            foreach (var parameterInfo in parameterInfos)
            {
                parameters[parameterInfo.Position] = this.ResolveInstance(parameterInfo.ParameterType, string.Empty);
            }
            return constructor.Invoke(parameters);
        }

        //private static void ThrowNotRegistered(NamedType key)
        //{
        //    throw new ActivationException(string.Format(
        //        "没有在 IOC 容器中注册名为 {0} 类型为 {1} 的项。",
        //        key.Name, key.Type
        //        ));
        //}

        //private static void ThrowNotActivated(NamedType key)
        //{
        //    throw new ActivationException(string.Format(
        //        "无法找到合适的构造器为类型 {0} 进行构造。",
        //        key.Name, key.Type
        //        ));
        //}
    }
}