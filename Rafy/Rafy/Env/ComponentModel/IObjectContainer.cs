/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140627
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140627 23:14
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
    /// IOC 容器
    /// </summary>
    public interface IObjectContainer
    {
        /// <summary>
        /// 如果某个服务有多个实例，则可以使用此方法来获取所有的实例。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<object> ResolveAll(Type type);

        /// <summary>
        /// 如果某个服务有多个实例，则可以通过一个键去获取对应的服务实例。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(Type type, string key = null);

        /// <summary>
        /// 注册唯一实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        /// <param name="key">如果有必须，则传入实例的键。</param>
        void RegisterInstance(Type type, object instance, string key = null);

        /// <summary>
        /// 注册唯一实例
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="instanceType">唯一实例的类型，该类型的实例会在第一次使用时被创建。</param>
        /// <param name="key">如果有必须，则传入实例的键。</param>
        void RegisterInstance(Type type, Type instanceType, string key = null);

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="key">如果有必须，则传入实例的键。否则传入 null。</param>
        void RegisterType(Type from, Type to, string key = null);
    }
}