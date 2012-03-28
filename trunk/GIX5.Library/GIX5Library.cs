using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library.ORM.DbMigration;
using OEA.MetaModel;

namespace GIX5.Library
{
    class GIX5Library : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "指标分析",
                    Children =
                    {
                        new ModuleMeta{ Label = "项目分类", EntityType = typeof(ProjectCategory)},
                        new ModuleMeta{ Label = "人员", EntityType = typeof(User)},
                        new ModuleMeta{ Label = "项目", CustomUI = "Indicator/Project"}
                    }
                });
            };

            app.DbMigratingOperations += (o, e) =>
            {
                using (var c = new OEADbMigrationContext(GEntity.ConnectionString))
                {
                    //c.DeleteDatabase();
                    //c.ResetHistories();
                    c.AutoMigrate();
                    //c.RollbackAll();
                    //c.JumpToHistory(DateTime.Parse("2012-01-07 21:27:00.000"));
                    //c.RollbackToHistory(DateTime.Parse("2012-01-07 21:27:00.000"), RollbackAction.DeleteHistory);
                };
            };
        }
    }
}