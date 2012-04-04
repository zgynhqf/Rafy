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
using System.Web;
using System.Text;
using System.IO;
using System.Reflection;
using OEA.MetaModel;
using OEA.Web.Json;
using System.Collections.Generic;
using System.Linq;
using Common;

namespace OEA.Web
{
    /// <summary>
    /// OEA 脚本门户
    /// </summary>
    public class OEAScriptHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            return CacheCompress(context, "OEA_Combined_JS_Script", this.CombineJavascript);
        }

        protected string CombineJavascript()
        {
            var buffer = new StringBuilder();

            this.CombineClientOEA(buffer,
                "Oea.js", "Window.js",
                "meta/MetaService.js",
                "data/Entity.js", "data/EntityRepository.js", "data/ListChangeSet.js",
                "cmd/Command.js",
                "view/View.js", "view/DetailView.js", "view/ListView.js", "view/RelationView.js", "view/QueryView.js",
                "autoUI/Layout.js", "autoUI/Regions.js",
                "autoUI/layouts/Common.js", "autoUI/layouts/RightChildren.js",
                "autoUI/ViewFactory.js", "autoUI/ControlResult.js", "autoUI/AggtUIGenerator.js",
                "control/ComboList.js",
                "AutoUI.js",
                "svc/ServiceInvoker.js",
                "App.js"
                );

            return buffer.ToString();
        }

        private void CombineClientOEA(StringBuilder buffer, params string[] jsFiles)
        {
            var assembly = this.GetType().Assembly;

            Func<Assembly, string, Stream> getStream = this.GetStream;
            if (HttpContext.Current.IsDebuggingEnabled) { getStream = this.GetStream_Debugging; }

            foreach (var resource in jsFiles)
            {
                var stream = getStream(assembly, resource);
                using (var sr = new StreamReader(stream))
                {
                    var js = sr.ReadToEnd();
                    buffer.AppendLine(js);
                }
            }
        }

        private Stream GetStream(Assembly assembly, string jsFileName)
        {
            var fileName = "OEA.Web.Scripts." + jsFileName.Replace("/", ".");
            return assembly.GetManifestResourceStream(fileName);
        }

        /// <summary>
        /// 这个类主要是方便 OEA JS 的开发人员使用：
        /// 在修改 OEA JS 后，不需要编译这个项目，此 Handler 会直接访问硬盘。
        /// </summary>
        private Stream GetStream_Debugging(Assembly assembly, string jsFileName)
        {
            var dir = ConfigurationHelper.GetAppSettingOrDefault("ForDeveloper_OEARootDir");
            if (dir != string.Empty)
            {
                var file = Path.Combine(dir, "OEA/Web/OEA.Web/Scripts/", jsFileName);
                return File.Open(file, FileMode.Open);
            }

            return this.GetStream(assembly, jsFileName);
        }
    }
}