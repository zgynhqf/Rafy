/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141121
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141121 12:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.Web.Http
{
    public class ODataQueryCriteriaBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            bindingContext.Model = Desrialize(bindingContext.ValueProvider);
            return true;
        }

        protected virtual ODataQueryCriteria Desrialize(IValueProvider values)
        {
            var criteria = new ODataQueryCriteria();

            var orderBy = values.GetValue("$orderby");
            if (orderBy != null)
            {
                criteria.OrderBy = orderBy.AttemptedValue;
            }

            var filter = values.GetValue("$filter");
            if (filter != null)
            {
                criteria.Filter = filter.AttemptedValue;
            }

            ParsePagingInfo(values, criteria);

            var expand = values.GetValue("$expand");
            if (expand != null)
            {
                criteria.Expand = expand.AttemptedValue;
            }

            var markTreeFullLoaded = values.GetValue("$markTreeFullLoaded");
            if (markTreeFullLoaded != null)
            {
                criteria.MarkTreeFullLoaded = true;
            }

            return criteria;
        }

        private static void ParsePagingInfo(IValueProvider values, ODataQueryCriteria criteria)
        {
            var pn = values.GetValue("$pageNumber");
            var tc = values.GetValue("$inlinecount");
            if (pn != null || tc != null)
            {
                var pageNumber = pn != null ? (long)pn.ConvertTo(typeof(long)) : 1;
                var needCount = tc != null && !string.IsNullOrWhiteSpace(tc.AttemptedValue);

                var ps = values.GetValue("$pageSize");
                int pageSize = ps != null ? (int)ps.ConvertTo(typeof(int)) : 10;

                var pagingInfo = new PagingInfo(pageNumber, pageSize, needCount);
                criteria.PagingInfo = pagingInfo;
            }
        }
    }
}