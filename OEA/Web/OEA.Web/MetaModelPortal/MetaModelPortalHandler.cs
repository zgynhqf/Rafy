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

            var typeName = request.QueryString["type"];
            var isAggt = request.GetQueryStringOrDefault("isAggt", 0);

            var converter = new ClientMetaFactory();
            var op = converter.Option;
            op.ignoreCommands = request.GetQueryStringOrDefault("ignoreCommands", 0) == 1;
            op.isDetail = request.GetQueryStringOrDefault("isDetail", 0) == 1;
            op.isLookup = request.GetQueryStringOrDefault("isLookup", 0) == 1;
            op.isReadonly = request.GetQueryStringOrDefault("isReadonly", 0) == 1;
            op.viewName = request.GetQueryStringOrDefault("viewName", "");

            var type = ClientEntities.Find(typeName);

            JsonModel jsonResult = null;
            if (isAggt == 1)
            {
                AggtBlocks aggt = null;

                if (!string.IsNullOrEmpty(op.viewName))
                {
                    aggt = UIModel.AggtBlocks.GetDefinedBlocks(op.viewName);
                }
                else
                {
                    aggt = UIModel.AggtBlocks.GetDefaultBlocks(type.EntityType);
                }

                jsonResult = converter.ConvertToAggtMeta(aggt);
            }
            else
            {
                var evm = UIModel.Views.Create(type.EntityType, op.viewName);

                jsonResult = converter.ConvertToClientMeta(evm);
            }

            var json = LiteJsonWriter.Convert(jsonResult);

            return json;
        }
    }
}