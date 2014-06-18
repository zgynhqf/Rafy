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
using Rafy.MetaModel.View;

namespace Rafy.Web
{
    /// <summary>
    /// Rafy 所有命令的生成门户
    /// </summary>
    public class RafyCommandsScriptHandler : JavascriptHandler
    {
        protected override string ResponseScript(HttpContext context)
        {
            return CacheCompress(context, "Rafy_Combined_Commands_JS_Script", this.CombineClientCommands);
        }

        protected string CombineClientCommands()
        {
            var js = UIModel.WebCommands.CombineAll();
            return js;
        }
    }
}