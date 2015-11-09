/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150522
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150522 11:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;

namespace Rafy.Web.Http
{
    /// <summary>
    /// 这个类型扩展了类型，
    /// 使得传入 EntityAwareJsonMediaTypeFormatter 中的 HttpContent 不但有来自 Body 的数据，
    /// 也有来自路由的一些值（目前就是 Id）。
    /// 
    /// 实现的功能：PUT 更新数据时，Id 应该从 Url 中提取，而不是在 Json Body 中。
    /// </summary>
    class IdFromRouteFormatterParameterBinding : FormatterParameterBinding
    {
        public IdFromRouteFormatterParameterBinding(HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters, IBodyModelValidator bodyModelValidator)
            : base(descriptor, formatters, bodyModelValidator)
        { }

        public override Task<object> ReadContentAsync(HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            /*********************** 代码块解释 *********************************
             * 以下代码大部分拷贝自 WebApi 中基类的源码。
             * 中间要完成的工作主要是：当路由中提供了 id，那么应该把这个值存储下来，并替换 HttpContent。
             * 注意：
             * 新建了一个类型 IdFromRouteHttpContent 用于把 Id 的值存储下来。
            **********************************************************************/

            HttpContent content = request.Content;

            if (content != null)
            {
                Task<object> result;
                try
                {
                    /*********************** 代码块解释 *********************************
                    //modified by huqf------------start;*/
                    //如果路由中提供了 id，那么应该把这个值存储下来。
                    var route = this.Descriptor.Configuration.Routes.GetRouteData(request);
                    object id = null;
                    if (route.Values.TryGetValue("id", out id) && id != null && id != RouteParameter.Optional)
                    {
                        var idContent = new IdFromRouteHttpContent(content, id.ToString());
                        result = idContent.ReadAsAsync(type, formatters, formatterLogger, cancellationToken);
                    }
                    else
                    {
                        result = content.ReadAsAsync(type, formatters, formatterLogger, cancellationToken);
                    }
                    /*modified by huqf------------end.
                     * **********************************************************************/
                }
                catch (UnsupportedMediaTypeException ex)
                {
                    string format = (content.Headers.ContentType == null) ? "SRResources.UnsupportedMediaTypeNoContentType" : "SRResources.UnsupportedMediaType";
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.UnsupportedMediaType, string.Format(format, new object[]
                    {
                        ex.MediaType.MediaType
                    }), ex));
                }
                return result;
            }
            object defaultValueForType = MediaTypeFormatter.GetDefaultValueForType(type);
            if (defaultValueForType == null)
            {
                return Task.FromResult<object>(null);
            }
            return Task.FromResult(defaultValueForType);
        }
    }
}