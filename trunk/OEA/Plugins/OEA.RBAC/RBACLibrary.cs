/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.MetaModel;
using OEA.Library.ORM.DbMigration;
using OEA.Library.Caching;
using OEA.Library;
using OEA.Library.Audit;
using OEA.Web;
using OEA.MetaModel.View;
using OEA.RBAC.Security;
using OEA.MetaModel.Audit;

namespace OEA.RBAC
{
    /// <summary>
    /// 比较简单的通用权限系统。
    /// </summary>
    internal class RBACLibrary : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel._System; }
        }

        public void Initialize(IApp app)
        {
            //设置权限提供程序为本模块中实体类
            PermissionMgr.Provider = new OEAPermissionMgr();

            //依赖注入
            if (OEAEnvironment.Location.IsOnServer())
            {
                AuditLogService.SetProvider(new ServerAuditLogProvider());
            }
            else
            {
                AuditLogService.SetProvider(new ClientAuditLogProvider());
            }

            app.ModuleOperations += (o, e) =>
            {
                var module = CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "权限管理",
                    Children =
                    {
                        new ModuleMeta{ Label = "人员管理", EntityType = typeof(User)},
                        new ModuleMeta{ Label = "岗位管理", EntityType = typeof(Position)},
                        new ModuleMeta{ Label = "部门管理", EntityType = typeof(Org)},
                        new ModuleMeta{ Label = "日志管理", EntityType = typeof(AuditItem)}
                    }
                });

                if (OEAEnvironment.IsWeb)
                {
                    module.Children.Add(new ModuleMeta
                    {
                        Label = "部门管理(iframe)",
                        CustomUI = "EntityModule?isAggt=1&type=" + ClientEntities.GetClientName(typeof(Org))
                    });
                }
            };

            app.DbMigratingOperations += (o, e) =>
            {
                using (var c = new OEADbMigrationContext(ConnectionStringNames.OEA))
                {
                    c.AutoMigrate();

                    //其它一些可用的API
                    //c.ClassMetaReader.IgnoreTables.Add("ReportObjectMetaData");
                    //c.RollbackToHistory(DateTime.Parse("2008-12-31 23:59:58.700"), RollbackAction.DeleteHistory);
                    //c.DeleteDatabase();
                    //c.ResetHistories();
                    //c.RollbackAll();
                    //c.JumpToHistory(DateTime.Parse("2012-01-07 21:27:00.000"));
                };
            };

            app.StartupCompleted += (o, e) => { CacheDefinition.Instance.Enable<Position>(); };

            if (OEAEnvironment.Location.IsOnClient())
            {
                app.Exit += (o, e) =>
                {
                    if (PermissionMgr.Provider.CurrentUser.IsAuthenticated)
                    {
                        UserLoginLogService.NotifyLogout();
                    }
                };
            }
        }
    }
}