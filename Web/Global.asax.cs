using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel.View;

namespace Rafy.Web.Site
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

        public override void Init()
        {
            base.Init();

            new WebAppStarter(this).Start(new DemoWebApp());
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

        private class DemoWebApp : WebApp
        {
            protected override void OnRuntimeStarting()
            {
                base.OnRuntimeStarting();

                var svc = ServiceFactory.Create<MigrateService>();
                svc.Options = new MigratingOptions
                {
                    //ReserveHistory = true,//ReserveHistory 表示是否需要保存所有数据库升级的历史记录
                    RunDataLossOperation = DataLossOperation.All,//要支持数据库表、字段的删除操作，取消本行注释。
                    Databases = new string[] {
                        DbSettingNames.RafyPlugins,
                        "JXC"
                    }
                };
                svc.Invoke();
            }
        }
    }
}