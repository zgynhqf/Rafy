/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151024 23:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Rafy.Domain;

namespace Rafy.Web
{
    /// <summary>
    /// 封装启动领域应用程序的逻辑。
    /// </summary>
    public class WebAppStarter
    {
        private static bool _webAppStarted = false;

        private static object _lock = new object();

        private HttpApplication _context;

        private DomainApp _app;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAppStarter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public WebAppStarter(HttpApplication context)
        {
            if (context == null) throw new ArgumentNullException("context");
            _context = context;
        }

        /// <summary>
        /// 把指定的领域应用程序启动。
        /// </summary>
        /// <param name="app">The application.</param>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public void Start(DomainApp app)
        {
            if (app == null) throw new ArgumentNullException("app");
            _app = app;

            //由于应用程序池会定时回收，所以需要在每个请求到来时都检测是否 WebApp 是否已经启动成功。
            _context.BeginRequest += OnBeginRequest;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            if (!_webAppStarted)
            {
                lock (_lock)
                {
                    if (!_webAppStarted)
                    {
                        _app.Startup();

                        _context.Disposed += (oo, ee) => { _app.NotifyExit(); };

                        _webAppStarted = true;
                    }
                }
            }
        }
    }
}