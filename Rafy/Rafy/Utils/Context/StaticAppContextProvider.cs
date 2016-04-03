/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141222
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141222 18:23
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Rafy
{
    /// <summary>
    /// 服务器端上下文提供器。
    /// 默认实现：静态字段。
    /// </summary>
    public class StaticAppContextProvider : IAppContextProvider
    {
        public virtual IPrincipal CurrentPrincipal { get; set; }

        public virtual IDictionary<string, object> DataContainer { get; set; }
    }
}
