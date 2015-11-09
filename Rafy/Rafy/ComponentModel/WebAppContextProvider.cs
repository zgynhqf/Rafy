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
    /// 基于 HttpContext 实现的上下文提供器。
    /// 每次请求使用一个单独的数据上下文
    /// </summary>
    internal class WebAppContextProvider : IAppContextProvider
    {
        protected const string LocalContextName = "Rafy.ComponentModel.WebThreadContextProvider";

        public IPrincipal CurrentPrincipal
        {
            get { return HttpContext.Current.User; }
            set { HttpContext.Current.User = value; }
        }

        public IDictionary<string, object> DataContainer
        {
            get
            {
                return HttpContext.Current.Items[LocalContextName] as IDictionary<string, object>;
            }
            set
            {
                HttpContext.Current.Items[LocalContextName] = value;
            }
        }
    }
}