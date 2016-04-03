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

        public void RegisterInstance(Type type, Type instanceType, string key = null)
        {
            this.RegisterInstance(type, instanceType as object, key);
        }

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

            return ResolveInstance(type, key);
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            var list = new List<object>();

            //以下代码为了与 UnityContainer 保持兼容，当注册了有名字的项时，无名字的项需要被忽略。

            #region _instances

            try
            {
                _lock.EnterReadLock();

                Dictionary<string, object> instances = null;
                if (_instances.TryGetValue(type, out instances))
                {
                    if (instances.Count > 1)
                    {
                        foreach (var kv in instances)
                        {
                            if (!string.IsNullOrEmpty(kv.Key))
                            {
                                list.Add(kv.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (var kv in instances)
                        {
                            list.Add(kv.Value);
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            #endregion

            var instanceResultCount = list.Count;

            #region _types

            try
            {
                _lock.EnterReadLock();

                Dictionary<string, Type> types = null;
                if (_types.TryGetValue(type, out types))
                {
                    if (types.Count > 1)
                    {
                        foreach (var kv in types)
                        {
                            if (!string.IsNullOrEmpty(kv.Key))
                            {
                                list.Add(kv.Value);
                            }
                        }
                    }
                    else
                    {
                        foreach (var kv in types)
                        {
                            list.Add(kv.Value);
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            #endregion

            //如果使用 RegisterInstance 方法时使用的是 Type 类型，则需要创建新的实例，并替换 Dic 中的值。
            bool hasInstanceType = false;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var itemType = list[i] as Type;
                if (itemType != null)
                {
                    if (i < instanceResultCount)
                    {
                        hasInstanceType = true;
                    }
                    list[i] = this.CreateInstance(itemType);
                }
            }
            if (hasInstanceType)
            {
                try
                {
                    _lock.EnterWriteLock();

                    var indexInList = 0;
                    var instances = _instances[type];
                    if (instances.Count > 1)
                    {
                        foreach (var kv in instances)
                        {
                            if (!string.IsNullOrEmpty(kv.Key))
                            {
                                indexInList++;
                                if (kv.Value is Type)
                                {
                                    var item = list[indexInList];
                                    instances[kv.Key] = item;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var kv in instances)
                        {
                            indexInList++;
                            if (kv.Value is Type)
                            {
                                var item = list[indexInList];
                                instances[kv.Key] = item;
                            }
                        }
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            return list;
        }

        private object ResolveInstance(Type type, string key)
        {
            object instanceResult = null;
            try
            {
                _lock.EnterReadLock();

                //先尝试从 _instances 集合中获取
                Dictionary<string, object> instances = null;
                if (_instances.TryGetValue(type, out instances))
                {
                    instances.TryGetValue(key, out instanceResult);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
            if (instanceResult != null)
            {
                if (instanceResult is Type)
                {
                    instanceResult = this.CreateInstance(instanceResult as Type);
                    this.RegisterInstance(type, instanceResult, key);
                }
                return instanceResult;
            }

            Type typeResult = null;
            try
            {
                _lock.EnterReadLock();

                if (instanceResult == null)
                {
                    Dictionary<string, Type> types = null;
                    if (_types.TryGetValue(type, out types))
                    {
                        types.TryGetValue(key, out typeResult);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
            if (typeResult != null)
            {
                return this.CreateInstance(typeResult);
            }

            return this.CreateInstance(type);
        }

        private object CreateInstance(Type type)
        {
            var construtors = type.GetConstructors();
            if (construtors.Length == 0)
            {
                throw new InvalidProgramException(string.Format("{0} 类型没有构造函数，无法构造它的实例。", type));
            }

            var constructor = construtors[0];
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