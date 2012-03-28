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
using OEA.Web.Json;

namespace OEA.Web
{
    /// <summary>
    /// OEA 实体定义脚本门户
    /// </summary>
    public class OEAEntityScriptHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            return CacheCompress(context, "OEA_Combined_Entity_JS_Script", this.CombineClientEntities);
        }

        protected string CombineClientEntities()
        {
            var buffer = new StringBuilder();

            GenerateClientEntities(buffer);

            return buffer.ToString();
        }

        private static void GenerateClientEntities(StringBuilder buffer)
        {
            var emg = new EntityModelGenerator();

            foreach (var kv in ClientEntities.GetEntities())
            {
                emg.EntityMeta = kv.Value;

                var model = emg.Generate();

                var jsonConfig = model.ToJsonString();

                var js = string.Format(@"Oea.defineEntity('{0}', {1});", kv.Key, jsonConfig);

                buffer.AppendLine(js);
            }
        }
    }
}