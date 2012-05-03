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
using OEA.Web.ClientMetaModel;
using OEA.Web.Json;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Web
{
    /// <summary>
    /// 元数据门户
    /// </summary>
    public class MetaModelPortalHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            var request = context.Request;

            var converter = new ClientMetaFactory();
            var op = converter.Option;
            op.module = request.GetQueryStringOrDefault("module", string.Empty);
            op.ignoreCommands = request.GetQueryStringOrDefault("ignoreCommands", 0) == 1;
            op.isDetail = request.GetQueryStringOrDefault("isDetail", 0) == 1;
            op.isLookup = request.GetQueryStringOrDefault("isLookup", 0) == 1;
            op.isReadonly = request.GetQueryStringOrDefault("isReadonly", 0) == 1;
            op.viewName = request.GetQueryStringOrDefault("viewName", string.Empty);

            JsonModel jsonResult = null;

            //如果指定了 module，则直接返回模块的格式。
            if (!string.IsNullOrEmpty(op.module))
            {
                var module = UIModel.Modules[op.module];
                var aggt = UIModel.AggtBlocks.GetModuleBlocks(module);
                jsonResult = converter.ConvertToAggtMeta(aggt);
            }
            else
            {
                var typeName = request.QueryString["type"];
                var isAggt = request.GetQueryStringOrDefault("isAggt", 0);

                if (isAggt == 1)
                {
                    AggtBlocks aggt = null;
                    if (!string.IsNullOrEmpty(op.viewName))
                    {
                        aggt = UIModel.AggtBlocks.GetDefinedBlocks(op.viewName);
                    }
                    else
                    {
                        var type = ClientEntities.Find(typeName);
                        aggt = UIModel.AggtBlocks.GetDefaultBlocks(type.EntityType);
                    }

                    jsonResult = converter.ConvertToAggtMeta(aggt);
                }
                else
                {
                    var type = ClientEntities.Find(typeName);
                    var evm = UIModel.Views.Create(type.EntityType, op.viewName);

                    jsonResult = converter.ConvertToClientMeta(evm);
                }
            }

            var json = LiteJsonWriter.Convert(jsonResult);

            return json;
        }
    }
}