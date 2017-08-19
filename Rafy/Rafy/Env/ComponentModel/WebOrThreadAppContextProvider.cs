/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121217 17:50
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121217 17:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using Rafy;
using Rafy.Utils;
using System.Security.Claims;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 基于 HttpContext 实现的上下文提供器。
    /// 每次请求使用一个单独的数据上下文。
    /// 在 WebApi 等一些框架中，会开启异步线程去执行一些序列化的代码，
    /// 这时会找不到 HttpContext.Current 对象，所以只能退而求其次使用线程中的数据。
    /// </summary>
    internal class WebOrThreadAppContextProvider : IAppContextProvider
    {
        protected const string HttpContextName = "Rafy.ComponentModel.WebOrThreadAppContextProvider";

        [ThreadStatic]
        private static IDictionary<string, object> _threadItems;

        public IPrincipal CurrentPrincipal
        {
            get
            {
                var hc = HttpContext.Current;
                if (hc != null)
                {
                    return hc.User;
                }

                return Thread.CurrentPrincipal;
            }
            set
            {
                var hc = HttpContext.Current;
                if (hc != null)
                {
                    hc.User = (ClaimsPrincipal)value;
                }

                Thread.CurrentPrincipal = value;
            }
        }

        public IDictionary<string, object> DataContainer
        {
            get
            {
                var hc = HttpContext.Current;
                if (hc != null)
                {
                    return hc.Items[HttpContextName] as IDictionary<string, object>;
                }

                return _threadItems;
            }
            set
            {
                var hc = HttpContext.Current;
                if (hc != null)
                {
                    hc.Items[HttpContextName] = value;
                    return;
                }

                _threadItems = value;
            }
        }
    }
}