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
using System.Text;
using System.Web;
using Rafy;

namespace Rafy
{
    /// <summary>
    /// 一个模拟 Web 环境的服务器端上下文提供器。
    /// 每次请求使用一个单独的数据上下文
    /// </summary>
    public class WebThreadContextProvider : ServerContextProvider
    {
        protected const string LocalContextName = "Rafy.WebThreadContext";

        protected override IDictionary<string, object> GetValueContainer()
        {
            if (HttpContext.Current == null)
            {
                return base.GetValueContainer();
            }
            else
            {
                return (IDictionary<string, object>)HttpContext.Current.Items[LocalContextName];
            }
        }

        protected override void SetValueContainer(IDictionary<string, object> localContext)
        {
            if (HttpContext.Current == null)
            {
                base.SetValueContainer(localContext);
            }
            else
            {
                HttpContext.Current.Items[LocalContextName] = localContext;
            }
        }
    }
}