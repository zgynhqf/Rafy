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
using Rafy.Web.ClientMetaModel;
using Rafy.Web.Json;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.Web
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
            op.ignoreCommands = request.GetQueryStringOrDefault("ignoreCommands", 0) == 1;
            op.isDetail = request.GetQueryStringOrDefault("isDetail", 0) == 1;
            op.isLookup = request.GetQueryStringOrDefault("isLookup", 0) == 1;
            op.isReadonly = request.GetQueryStringOrDefault("isReadonly", 0) == 1;
            op.viewName = request.GetQueryStringOrDefault("viewName", string.Empty);
            var moduleName = request.GetQueryStringOrDefault("module", string.Empty);
            var typeName = request.GetQueryStringOrDefault("type", string.Empty);
            var templateType = request.GetQueryStringOrDefault("templateType", string.Empty);
            var isAggt = request.GetQueryStringOrDefault("isAggt", 0) == 1;

            JsonModel jsonResult = null;

            //如果指定了 module，则直接返回模块的格式。
            if (!string.IsNullOrEmpty(moduleName))
            {
                var module = UIModel.Modules[moduleName];
                var aggt = UIModel.AggtBlocks.GetModuleBlocks(module);
                jsonResult = converter.ConvertToAggtMeta(aggt);
            }
            else
            {
                var type = ClientEntities.Find(typeName);

                //需要聚合块
                if (isAggt)
                {
                    AggtBlocks aggt = null;
                    //通过定义的模板类来返回模块格式
                    if (!string.IsNullOrEmpty(templateType))
                    {
                        var uiTemplateType = Type.GetType(templateType);
                        var template = Activator.CreateInstance(uiTemplateType) as BlocksTemplate;
                        template.EntityType = type.EntityType;
                        aggt = template.GetBlocks();
                    }
                    else
                    {
                        //通过定义的聚合块名称来获取聚合块
                        if (!string.IsNullOrEmpty(op.viewName))
                        {
                            aggt = UIModel.AggtBlocks.GetDefinedBlocks(op.viewName);
                        }
                        else
                        {
                            //通过默认的聚合块名称来获取聚合块
                            aggt = UIModel.AggtBlocks.GetDefaultBlocks(type.EntityType);
                        }
                    }

                    jsonResult = converter.ConvertToAggtMeta(aggt);
                }
                else
                {
                    //获取单块 UI 的元数据
                    var evm = UIModel.Views.Create(type.EntityType, op.viewName) as WebEntityViewMeta;

                    jsonResult = converter.ConvertToClientMeta(evm);
                }
            }

            var json = jsonResult.ToJsonString();

            return json;
        }
    }
}