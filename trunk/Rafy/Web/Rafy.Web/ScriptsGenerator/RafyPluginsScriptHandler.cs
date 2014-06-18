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
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Web.Json;

namespace Rafy.Web
{
    /// <summary>
    /// Rafy 所有命令的生成门户
    /// </summary>
    public class RafyPluginsScriptHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            return CacheCompress(context, "Rafy_Combined_Plugins_JS_Script", this.CombinePluginsCodes);
        }

        protected string CombinePluginsCodes()
        {
            var js = new StringBuilder();
            //按照 721 顺序加入所有 Library 中的 js Resource。
            foreach (var plugin in RafyEnvironment.GetAllPlugins())
            {
                this.WritePluginJs(plugin, js);
            }

            //在最后加上所有模块的定义。
            this.WriteModules(js);

            return js.ToString();
        }

        private void WritePluginJs(PluginAssembly plugin, StringBuilder js)
        {
            var jsBlocks = new List<JsBlock>();

            //除了放在 commands 文件夹下的 js，都加载进来。
            var assembly = plugin.Assembly;
            var resources = assembly.GetManifestResourceNames()
                .Where(r => !r.ToLower().Contains("commands.") && r.ToLower().Contains(".js"))
                .ToArray();

            foreach (var resource in resources)
            {
                var stream = assembly.GetManifestResourceStream(resource);
                using (var sr = new StreamReader(stream))
                {
                    var jsContent = sr.ReadToEnd();
                    WebCommandJsHelper.LoadCommandJsBlocks(jsContent, jsBlocks);
                }
            }

            JsBlock.SortByHierachy(jsBlocks);

            foreach (var jsBlock in jsBlocks) { js.AppendLine(jsBlock.JavascriptCode); }
        }

        #region WriteModules

        private void WriteModules(StringBuilder js)
        {
            var result = new ModuleJson();

            var roots = CommonModel.Modules.Roots;
            foreach (WebModuleMeta root in roots)
            {
                var rootJson = ToJson(root);
                result.children.Add(rootJson);
            }

            js.AppendFormat(@"
Rafy.App.getModules()._setRoot({0});
", result.ToJsonString());
        }

        private static ModuleJson ToJson(WebModuleMeta module)
        {
            var mJson = new ModuleJson
            {
                keyLabel = module.KeyLabel
            };

            if (module.HasUI)
            {
                if (module.IsCustomUI)
                {
                    mJson.url = module.Url;
                }
                else
                {
                    mJson.model = ClientEntities.GetClientName(module.EntityType);
                    mJson.clientRuntime = module.ClientRuntime;
                    mJson.viewName = module.AggtBlocksName;
                }
            }

            foreach (WebModuleMeta child in module.Children)
            {
                var childJson = ToJson(child);
                mJson.children.Add(childJson);
            }

            return mJson;
        }

        private class ModuleJson : JsonModel
        {
            public string keyLabel, clientRuntime, model, url, viewName;
            public List<ModuleJson> children = new List<ModuleJson>();

            protected override void ToJson(LiteJsonWriter json)
            {
                json.WritePropertyIf("keyLabel", keyLabel);
                json.WritePropertyIf("url", url);
                json.WritePropertyIf("model", model);
                json.WritePropertyIf("viewName", viewName);
                json.WritePropertyIf("clientRuntime", clientRuntime);
                if (children.Count > 0) { json.WritePropertyIf("children", children); }
            }
        }

        #endregion
    }
}