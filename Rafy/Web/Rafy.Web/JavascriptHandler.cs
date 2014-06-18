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
using System.Web;
using System.Text.RegularExpressions;

namespace Rafy.Web
{
    /// <summary>
    /// 响应为 js 的门户基类
    /// </summary>
    public abstract class JavascriptHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            var scripts = this.ResponseScript(context);

            context.Response.Write(scripts);
            context.Response.ContentType = "application/x-javascript";
        }

        protected abstract string ResponseScript(HttpContext context);

        /// <summary>
        /// 缓存 + 压缩
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="producer"></param>
        /// <returns></returns>
        protected static string CacheCompress(HttpContext context, string key, Func<string> producer)
        {
            if (RafyEnvironment.IsDebuggingEnabled) { return producer(); }

            //缓存 + 压缩
            return context.Cache.GetOrCreate(key, () =>
            {
                var result = producer();

                result = Compress(result);

                return result;
            });
        }

        protected static string Compress(string result)
        {
            if (RafyEnvironment.IsDebuggingEnabled) { return result; }

            result = Regex.Replace(result, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            //以非协议（不是冒号起头）的双斜杠都算是注释
            result = Regex.Replace(result, @"(?<nonColon>[^:])//.+$", "${nonColon}", RegexOptions.Multiline);
            result = Regex.Replace(result, @"\s+", " ");

            //把操作符旁边的空格去除。
            result = Regex.Replace(result, @"\s*(?<operator>[\{\}\(\);:,=\<\>\+\|\!])\s*", "${operator}");

            return result;
        }
    }
}