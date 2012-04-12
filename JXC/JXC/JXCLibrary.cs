using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library.ORM.DbMigration;
using OEA.MetaModel.View;
using OEA.MetaModel;

namespace JXC
{
    class JACLibrary : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                var moduleBookImport = CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "进销存系统示例",
                    Children =
                    {
                        new ModuleMeta
                        {
                            Label = "基础数据",
                            Children =
                            {
                                new ModuleMeta{ Label = "计量单位", EntityType = typeof(Unit)},
                                new ModuleMeta{ Label = "商品类别", EntityType = typeof(ProductCategory)},
                                new ModuleMeta{ Label = "商品管理", EntityType = typeof(Product)},
                                new ModuleMeta{ Label = "仓库管理", EntityType = typeof(Storage)},
                                new ModuleMeta{ Label = "客户类别", EntityType = typeof(ClientCategory)},
                                new ModuleMeta{ Label = "客户管理", EntityType = typeof(ClientInfo)},
                            }
                        },
                        new ModuleMeta
                        {
                            Label = "采购管理"
                        },
                        new ModuleMeta
                        {
                            Label = "库存管理"
                        },
                        new ModuleMeta
                        {
                            Label = "销售管理"
                        }
                    }
                });
            };

            app.DbMigratingOperations += (o, e) =>
            {
                using (var c = new OEADbMigrationContext(JXCEntity.ConnectionString))
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
        }
    }
}