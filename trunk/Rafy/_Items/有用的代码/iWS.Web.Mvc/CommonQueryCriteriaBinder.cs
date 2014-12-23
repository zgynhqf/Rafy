/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141212
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141212 11:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rafy;
using Rafy.Domain;

namespace iWS.Web.Mvc
{
    public class CommonQueryCriteriaBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(CommonQueryCriteria))
            {
                return DesrializeCommonQueryCriteria(bindingContext.ValueProvider);
            }

            throw new NotSupportedException();
        }

        #region CommonQueryCriteria

        private CommonQueryCriteria DesrializeCommonQueryCriteria(IValueProvider values)
        {
            var criteria = new CommonQueryCriteria();

            if (values.ContainsPrefix("$orderby"))
            {
                var value = values.GetValue("$orderby");
                criteria.OrderBy = value.AttemptedValue;
            }

            if (values.ContainsPrefix("$pageNumber"))
            {
                var pageNumber = (int)values.GetValue("$pageNumber").ConvertTo(typeof(int));
                int pageSize = 10;
                if (values.ContainsPrefix("$pageSize"))
                {
                    pageSize = (int)values.GetValue("$pageSize").ConvertTo(typeof(int));
                }

                var pagingInfo = new PagingInfo(pageNumber, pageSize);
                criteria.PagingInfo = pagingInfo;
            }

            //filter
            //var jFilter = json.Property("$filter");
            //if (jFilter != null)
            //{
            //    var filter = jFilter.Value.Value<string>();
            //    ParseFilter(criteria, filter);
            //}

            return criteria;
        }

        private void ParseFilter(CommonQueryCriteria criteria, string filter)
        {
            //to do
        }

        #endregion
    }
}