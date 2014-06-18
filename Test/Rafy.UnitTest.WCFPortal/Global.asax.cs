using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Rafy.Domain;
using UT;

namespace Rafy.UnitTest.WCFPortal
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RafyEnvironment.Provider.IsDebuggingEnabled = HttpContext.Current.IsDebuggingEnabled;
            RafyEnvironment.Provider.DllRootDirectory = Path.Combine(RafyEnvironment.Provider.RootDirectory, "Bin");

            PluginTable.DomainLibraries.AddPlugin<UnitTestPlugin>();

            var app = new DomainApp();
            app.Startup();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}