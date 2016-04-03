/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140625
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140625 16:38
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
    /// IServiceContainer 扩展方法集
    /// </summary>
    public static class CompositionExtension
    {
        /// <summary>
        /// 获取指定类型的实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceProvider sp)
            where T : class
        {
            return sp.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// 获取指定类型指定键名的实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetService<T>(this IServiceContainer sp, string key)
            where T : class
        {
            return sp.GetService(typeof(T), key) as T;
        }

        /// <summary>
        /// 获取指定类型的所有实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sp"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetAllInstances<T>(this IServiceContainer sp)
            where T : class
        {
            return sp.GetAllInstances(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取指定类型的实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static T Resolve<T>(this IObjectContainer container)
            where T : class
        {
            return container.Resolve(typeof(T)) as T;
        }

        /// <summary>
        /// 获取指定类型指定键名的实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Resolve<T>(this IObjectContainer container, string key)
            where T : class
        {
            return container.Resolve(typeof(T), key) as T;
        }

        /// <summary>
        /// 获取指定类型的所有实例。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <returns></returns>
        public static IEnumerable<T> ResolveAll<T>(this IObjectContainer container)
            where T : class
        {
            return container.ResolveAll(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 注册唯一实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container">The container.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="key">The key.</param>
        public static void RegisterInstance<T>(this IObjectContainer container, T instance, string key = null)
        {
            container.RegisterInstance(typeof(T), instance, key);
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <typeparam name="TFrom">The type of the interface.</typeparam>
        /// <typeparam name="TTo">The type of the class.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="key">The key.</param>
        public static void RegisterType<TFrom, TTo>(this IObjectContainer container, string key = null)
            where TTo : TFrom
        {
            container.RegisterType(typeof(TFrom), typeof(TTo), key);
        }
    }
}