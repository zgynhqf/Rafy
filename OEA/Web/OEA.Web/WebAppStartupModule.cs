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

namespace OEA.Web
{
    /// <summary>
    /// 此 module 用于启动 OEA
    /// </summary>
    public class WebAppStartupModule : IHttpModule
    {
        private static bool _initialized = false;

        private HttpApplication context;

        public void Init(HttpApplication context)
        {
            this.context = context;
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        private void context_BeginRequest(object sender, EventArgs e)
        {
            context.BeginRequest -= new EventHandler(context_BeginRequest);

            lock (context)
            {
                if (!_initialized)
                {
                    var webApp = new WebApp();
                    webApp.Startup();

                    context.Disposed += (oo, ee) => { webApp.NotifyExit(); };

                    _initialized = true;
                }
            }
        }

        public void Dispose() { }
    }
}