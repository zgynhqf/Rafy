/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Collections.Generic;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 一个通用的工厂基类
    /// 
    /// 此工厂以命名的类型为存储，然后生成具体的实例
    /// </summary>
    /// <typeparam name="T">
    /// 实例类型
    /// </typeparam>
    public abstract class NamedTypeFactory<T>
        where T : class
    {
        private Dictionary<string, Type> _map = new Dictionary<string, Type>();

        /// <summary>
        /// 使用此方法构建所有类的实例
        /// </summary>
        /// <param name="key"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected T CreateInstance(string key, bool suppressEvent = false)
        {
            var type = this.TryFind(key);
            if (type == null) return null;

            var instance = Activator.CreateInstance(type, true) as T;

            if (instance != null && !suppressEvent)
            {
                this.OnInstanceCreated(new InstanceCreatedEventArgs<T>(instance));
            }

            return instance;
        }

        /// <summary>
        /// 在实例被创建时发生。
        /// 
        /// 当不使用 CreateInstance 方法构建某个实例时，必须调用此方法进行事件通知。
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnInstanceCreated(InstanceCreatedEventArgs<T> args)
        {
            var handler = this.InstanceCreated;
            if (handler != null) handler(this, args);
        }

        /// <summary>
        /// 在实例被创建时发生。
        /// </summary>
        public event EventHandler<InstanceCreatedEventArgs<T>> InstanceCreated;

        /// <summary>
        /// 设置一个命名类型
        /// </summary>
        /// <typeparam name="TSub"></typeparam>
        /// <param name="key"></param>
        public void Set<TSub>(string key)
            where TSub : T
        {
            this._map[key] = typeof(TSub);
        }

        /// <summary>
        /// 设置一个命名类型
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, Type value)
        {
            this._map[key] = value;
        }

        /// <summary>
        /// 设置一组命名类型
        /// </summary>
        /// <param name="dic"></param>
        protected void SetDictionary(IDictionary<string, Type> dic)
        {
            foreach (var kv in dic)
            {
                this.Set(kv.Key, kv.Value);
            }
        }

        private Type TryFind(string key)
        {
            Type result = null;
            this._map.TryGetValue(key, out result);
            return result;
        }
    }

    /// <summary>
    /// 实例被创建事件的参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InstanceCreatedEventArgs<T> : EventArgs
        where T : class
    {
        public InstanceCreatedEventArgs(T instance)
        {
            this.Instance = instance;
        }

        /// <summary>
        /// 被创建的实例
        /// </summary>
        public T Instance { get; private set; }
    }
}