/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Web;
using OEA.Web.ClientMetaModel;

namespace OEA.Web
{
    /// <summary>
    /// 服务调用门户
    /// </summary>
    public class ServiceInvokerHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            var svc = context.Request.GetQueryStringOrDefault("svc", string.Empty);
            if (!string.IsNullOrWhiteSpace(svc))
            {
                var input = context.Request.Form["svcParams"];
                var res = JsonServiceRepository.Invoke(svc, input);
                return res;
            }

            return new ClientResult
            {
                success = true
            }.ToJsonString();
        }
    }
}