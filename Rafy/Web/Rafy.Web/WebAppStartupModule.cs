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

namespace Rafy.Web
{
    /// <summary>
    /// 此 module 用于启动 Rafy
    /// </summary>
    public class WebAppStartupModule : IHttpModule
    {
        private static bool _initialized = false;

        private static object _lock = new object();

        private HttpApplication context;

        public void Init(HttpApplication context)
        {
            this.context = context;
            context.BeginRequest += OnBeginRequest;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                lock (_lock)
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
        }

        public void Dispose() { }
    }
}