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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Web;
using Rafy.Reflection;

namespace Rafy.Web
{
    public static class CommonWebExtension
    {
        public static T GetOrCreate<T>(this Cache cache, string key, Func<T> producer)
            where T : class
        {
            var result = cache[key] as T;

            if (result == null)
            {
                cache[key] = producer();
            }

            return result;
        }

        public static T GetQueryStringOrDefault<T>(this HttpRequest request, string key, T defaultValue)
        {
            T result = defaultValue;

            var strResult = request.QueryString[key];
            if (!string.IsNullOrWhiteSpace(strResult))
            {
                try
                {
                    result = TypeHelper.CoerceValue<T>(strResult);
                }
                catch { }
            }

            return result;
        }

        public static T GetQueryStringOrDefault<T>(this HttpRequestBase request, string key, T defaultValue)
        {
            T result = defaultValue;

            var strResult = request.QueryString[key];
            if (!string.IsNullOrWhiteSpace(strResult))
            {
                try
                {
                    result = TypeHelper.CoerceValue<T>(strResult);
                }
                catch { }
            }

            return result;
        }
    }
}
