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
using OEA.ORM.DbMigration;
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
    internal class RBACLibrary : LibraryPlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel._System; }
        }

        public override void Initialize(IApp app)
        {
            //设置权限提供程序为本模块中实体类
            PermissionMgr.Provider = new OEAPermissionMgr();

            //依赖注入
            if (OEAEnvironment.IsOnServer())
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
                        new ModuleMeta{ Label = "部门权限管理", EntityType = typeof(Org), WPFTemplateType=typeof(OrgModuleTemplate)},
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

            if (OEAEnvironment.IsOnClient())
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