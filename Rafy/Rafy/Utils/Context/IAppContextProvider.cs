/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141222
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141222 18:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Rafy
{
    /// <summary>
    /// AppContext 的算法提供器。
    /// </summary>
    public interface IAppContextProvider
    {
        /// <summary>
        /// 可获取或设置当前的身份。
        /// </summary>
        IPrincipal CurrentPrincipal { get; set; }

        /// <summary>
        /// 获取或设置当前的上下文数据容器。
        /// </summary>
        IDictionary<string, object> DataContainer { get; set; }
    }
}
