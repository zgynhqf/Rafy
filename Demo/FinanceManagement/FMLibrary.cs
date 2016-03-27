using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel.View;
using Rafy.MetaModel;
using Rafy.DbMigration;
using Rafy.WPF;
using Rafy.Domain;
using Rafy.RBAC.Old;
using FM.UI;
using Rafy.ComponentModel;

namespace FM
{
    class FMLibrary : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            InitModules(app);

            InitClient(app);
        }

        private static void InitModules(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                var moduleJXC = CommonModel.Modules.AddRoot(new WPFModuleMeta
                {
                    Label = "个人财务管理",
                    Children =
                    {
                        new WPFModuleMeta{ Label = "帐务录入", EntityType = typeof(FinanceLog), BlocksTemplate=typeof(FinanceLogInputModule)},
                        new WPFModuleMeta{ Label = "相关人", EntityType = typeof(Person)},
                        new WPFModuleMeta{ Label = "标签", EntityType = typeof(Tag)},
                        new WPFModuleMeta{ Label = "帐务统计（未完成）", EntityType = typeof(FinanceLog)},
                    }
                });
            };
        }

        private static void InitClient(IApp app)
        {
            //var clientApp = app as IClientApp;
            //if (clientApp != null)
            //{
            //    clientApp.MainWindowLoaded += (o, e) =>
            //    {
            //        App.Current.OpenModuleOrAlert("帐务录入");
            //    };
            //}
        }
    }
}