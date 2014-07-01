/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121102 16:04
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121102 16:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using MP.WPF;
using Rafy;
using Rafy.MetaModel;
using Rafy.WPF;
using Rafy.WPF.Shell;
using Rafy.Domain.ORM.DbMigration;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.ComponentModel;

namespace MP
{
    public class MonthPlanPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            //重新设置主窗体。
            App.MainWindowType = typeof(EmptyShell);
            //不需要登录界面。
            ClientApp.LoginWindowType = null;
            //不需要任何权限。
            PermissionMgr.Provider = null;

            app.AllPluginsIntialized += (oo, ee) =>
            {
                RafyResources.AddResource(typeof(MonthPlanPlugin), "Resources/MonthPlanResources.xaml");
            };

            app.ModuleOperations += (o, e) =>
            {
                CommonModel.Modules.AddRoot(new WPFModuleMeta { Label = "月度计划", EntityType = typeof(MonthPlan), BlocksTemplate = typeof(MonthPlanModule) });
            };

            app.RuntimeStarting += (o, e) =>
            {
                AutoUpdateDb();
            };

            app.StartupCompleted += (o, e) =>
            {
                App.Current.OpenModuleOrAlert("月度计划");
            };
        }

        /// <summary>
        /// 自动升级数据库。
        /// </summary>
        private static void AutoUpdateDb()
        {
            var svc = ServiceFactory.Create<MigrateService>();
            svc.DataPortalLocation = DataPortalLocation.Local;
            svc.Options = new MigratingOptions
            {
                RunDataLossOperation = Rafy.DbMigration.DataLossOperation.All,
                Databases = new string[] { MPEntityRepository.DbSettingName }
            };
            svc.Invoke();
        }
    }
}