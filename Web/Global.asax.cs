using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using OEA.MetaModel.View;

namespace OEA.Web.Site
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);

            RegisterRoutes(RouteTable.Routes);
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.{type}");

            routes.MapRoute(
                "index", "",
                new { controller = "Default", action = "Index" }
            );
            routes.MapRoute(
                "Default", // Route name
                "{action}", // URL with parameters
                new { controller = "Default" } // Parameter defaults
            );
            routes.MapRoute(
                "Common", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { id = UrlParameter.Optional } // Parameter defaults
            );

            //不起作用
            //routes.IgnoreRoute("{resource}*.ashx");
            //routes.MapRoute(
            //    "EntityModule", "EntityModule/{type}",
            //    new { controller = "Default", action = "EntityModule", type = string.Empty }
            //);

            //routes.MapRoute(
            //    "Default", // Route name
            //    "{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Default", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            //);
        }
    }
}