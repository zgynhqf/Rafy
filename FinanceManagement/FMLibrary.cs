using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library.ORM.DbMigration;
using OEA.MetaModel.View;
using OEA.MetaModel;
using DbMigration;
using OEA.Module.WPF;
using OEA.Library;
using OEA.RBAC;
using FM.UI;

namespace FM
{
    class FMLibrary : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IApp app)
        {
            InitModules(app);

            InitClient(app);
        }

        private static void InitModules(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                var moduleJXC = CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "个人财务管理",
                    Children =
                    {
                        new ModuleMeta{ Label = "帐务录入", EntityType = typeof(FinanceLog), TemplateType=typeof(FinanceLogInputModule)},
                        new ModuleMeta{ Label = "相关人", EntityType = typeof(Person)},
                        new ModuleMeta{ Label = "标签", EntityType = typeof(Tag)},
                        new ModuleMeta{ Label = "帐务统计（未完成）", EntityType = typeof(FinanceLog)},
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