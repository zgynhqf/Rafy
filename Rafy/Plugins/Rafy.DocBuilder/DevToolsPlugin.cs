/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120606 11:49
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120606 11:49
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Rafy;
using Rafy.DevTools.CodeGenerator;
using Rafy.DevTools.DbManagement;
using Rafy.DevTools.Document;
using Rafy.DevTools.SysInfo;
using Rafy.DevTools.Theme;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.WPF;
using Rafy.MultiLanguages;
using Rafy.MultiLanguages.WPF;
using Rafy.Domain.ORM.DbMigration.Presistence;
using Rafy.DevTools.Modeling;
using Rafy.ComponentModel;

namespace Rafy.DevTools
{
    class DevToolsPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            //必须是在调试期才起作用。
            if (!RafyEnvironment.IsDebuggingEnabled) return;

            app.ModuleOperations += AddDevModules;

            //如果当前是在客户端，则需要尝试在启动时生成数据库。
            if (RafyEnvironment.Location.IsWPFUI)
            {
                (app as IClientApp).MainProcessStarting += OnMainProcessStarting;
            }
        }

        private static void AddDevModules(object sender, EventArgs e)
        {
            if (!RafyEnvironment.Location.IsWebUI)
            {
                CommonModel.Modules.AddRoot(new WPFModuleMeta
                {
                    Label = "开发工具",
                    Children =
                    {
                        //new ModuleMeta{ Label = "生成属性文档", CustomUI = typeof(PropertyDocBuilderUI)},
                        new WPFModuleMeta{ Label = "实体模型", CustomUI = typeof(ModelViewer)},
                        new WPFModuleMeta{ Label = "生成查询实体", CustomUI = typeof(QueryEntityGenerator)},

                        new WPFModuleMeta{ Label = "系统状态", EntityType=typeof(FrameworkInfoItem)},
                        new WPFModuleMeta{ Label = "数据库管理", EntityType=typeof(DbMigrationHistory), BlocksTemplate= typeof(DbManagementModule)},

                        new WPFModuleMeta{ Label = "开发语言", EntityType=typeof(DevLanguageItem)},
                        new WPFModuleMeta{ Label = "多国语言", EntityType=typeof(Language), BlocksTemplate= typeof(LanguageModule)},

                        new WPFModuleMeta{ Label = "设计皮肤", CustomUI = typeof(ThemeDesigner)},
                    }
                });
            }
        }

        private static void OnMainProcessStarting(object sender, EventArgs e)
        {
            //配置了以下配置项时，才在启动时升级数据库。
            if (ConfigurationHelper.GetAppSettingOrDefault("Rafy.DevTools.GenerateDbOnStartup", false))
            {
                ClientMigrationHelper.MigrateOnClient();
            }
        }
    }
}