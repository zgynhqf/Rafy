/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150318
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150318 16:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Validation;
using Rafy.Domain;

namespace Rafy.Web.Http
{
    /// <summary>
    /// WebApi 注册器。
    /// </summary>
    public class HttpRegister
    {
        public HttpRegister()
        {
            this.Formatter = new EntityAwareJsonMediaTypeFormatter();
        }

        /// <summary>
        /// 负责完成实体的序列化、反序列化。
        /// 同时使用 JSON.NET 完成其它对象的序列化、反序列化。
        /// </summary>
        public EntityAwareJsonMediaTypeFormatter Formatter { get; set; }

        /// <summary>
        /// 在 HttpConfiguration 中注册一些必要的绑定器。
        /// </summary>
        /// <param name="config"></param>
        public void Register(HttpConfiguration config)
        {
            config.Formatters.Insert(0, this.Formatter);
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.Services.Replace(typeof(IBodyModelValidator), new EntityAwareBodyModelValidator());
            config.BindParameter(typeof(ODataQueryCriteria), new ODataQueryCriteriaBinder());

            #region PUT Id 参数绑定

            //暂时不需要使用 REST PUT
            ////PUT 时，Id 需要从路由中获取，这里需要提供一个额外的参数绑定实现。
            //config.ParameterBindingRules.Add(parameter =>
            //{
            //    Type parameterType = parameter.ParameterType;
            //    if (typeof(Entity).IsAssignableFrom(parameterType))
            //    {
            //        var formatters = parameter.Configuration.Formatters;
            //        var bodyModelValidator = parameter.Configuration.Services.GetBodyModelValidator();
            //        return new IdFromRouteFormatterParameterBinding(parameter, formatters, bodyModelValidator);
            //    }
            //    return null;
            //});
            ////上述方案，也可以替换以下服务实现。
            ////config.Services.Replace(typeof(IActionValueBinder), new RafyActionValueBinder()); 

            #endregion
        }
    }
}