/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141202
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141202 11:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using Rafy.Domain;

namespace Rafy.Web.Http
{
    internal class EntityAwareBodyModelValidator : IBodyModelValidator// DefaultBodyModelValidator
    {
        private DefaultBodyModelValidator _inner = new DefaultBodyModelValidator();

        public bool Validate(object model, Type type, ModelMetadataProvider metadataProvider, HttpActionContext actionContext, string keyPrefix)
        {
            if (model is IDomainComponent)
            {
                return true;
            }

            return _inner.Validate(model, type, metadataProvider, actionContext, keyPrefix);
        }
    }
}