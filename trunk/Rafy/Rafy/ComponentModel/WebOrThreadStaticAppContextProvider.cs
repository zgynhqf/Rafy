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
using System.Web;
using Rafy;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 一个模拟 Web 环境的服务器端上下文提供器。
    /// 每次请求使用一个单独的数据上下文
    /// </summary>
    internal class WebOrThreadStaticAppContextProvider : ThreadStaticAppContextProvider
    {
        protected const string LocalContextName = "Rafy.ComponentModel.WebThreadContextProvider";

        public override IPrincipal CurrentPrincipal
        {
            get
            {
                var webContext = HttpContext.Current;
                if (webContext != null)
                {
                    return webContext.User;
                }

                return base.CurrentPrincipal;
            }
            set
            {
                var webContext = HttpContext.Current;
                if (webContext != null)
                {
                    webContext.User = value;
                }

                base.CurrentPrincipal = value;
            }
        }

        public override IDictionary<string, object> DataContainer
        {
            get
            {
                var webContext = HttpContext.Current;
                if (webContext != null)
                {
                    return webContext.Items[LocalContextName] as IDictionary<string, object>;
                }

                return base.DataContainer;
            }
            set
            {
                var webContext = HttpContext.Current;
                if (webContext != null)
                {
                    webContext.Items[LocalContextName] = value;
                }

                base.DataContainer = value;
            }
        }
    }
}