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
using System.Web.Http.Validation;
using Rafy.Domain;

namespace iWS.Web.Http
{
    internal class Starter
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Insert(0, new EntityAwareJsonMediaTypeFormatter());

            config.Services.Replace(typeof(IBodyModelValidator), new EntityAwareBodyModelValidator());
            config.BindParameter(typeof(ODataQueryCriteria), new ODataQueryCriteriaBinder());
        }
    }
}