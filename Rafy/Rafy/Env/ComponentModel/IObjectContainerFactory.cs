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
    /// IOC 容器工厂。
    /// </summary>
    public interface IObjectContainerFactory
    {
        /// <summary>
        /// 创建一个独立的的 IOC 容器
        /// </summary>
        /// <returns></returns>
        IObjectContainer CreateContainer();
    }
}
