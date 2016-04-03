/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140625
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140625 16:34
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
    /// 服务容器
    /// </summary>
    public interface IServiceContainer : IServiceProvider
    {
        /// <summary>
        /// 如果某个服务有多个实例，则可以使用此方法来获取所有的实例。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        IEnumerable<object> GetAllInstances(Type serviceType);

        /// <summary>
        /// 如果某个服务有多个实例，则可以通过一个键去获取对应的服务实例。
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetService(Type serviceType, string key);
    }
}