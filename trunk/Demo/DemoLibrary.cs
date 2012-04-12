using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library.ORM.DbMigration;
using OEA.MetaModel.View;
using OEA.MetaModel;

namespace Demo
{
    class DemoLibrary : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IApp app)
        {
            app.AllPluginsMetaIntialized += (o, e) =>
            {
                //定义书籍查询界面的结构
                UIModel.AggtBlocks.DefineBlocks("书籍查询界面", m => new AggtBlocks
                {
                    MainBlock = new Block(typeof(Book))
                    {
                        ExtendView = "书籍查询界面_Book"
                    },
                    Surrounders = 
                    {
                        new SurrounderBlock(typeof(BookQueryCriteria))
                        {
                            BlockType = BlockType.Detail,
                            SurrounderType = SurrounderType.Condition,
                        }
                    }
                });
            };

            app.ModuleOperations += (o, e) =>
            {
                var moduleBookImport = CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "书籍录入",
                    Children =
                    {
                        new ModuleMeta{ Label = "类别管理", EntityType = typeof(BookCategory)},
                        new ModuleMeta{ Label = "书籍管理", EntityType = typeof(Book)},
                        new ModuleMeta{ Label = "图书管理员", EntityType = typeof(BookAdministrator)},
                    }
                });

                var moduleQuery = CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "书籍查询模块",
                    Children =
                    {
                        new ModuleMeta
                        {
                            Label = "书籍查询", EntityType = typeof(Book), AggtBlocksName= "书籍查询界面",
                        }
                    }
                });

                if (OEAEnvironment.IsWeb)
                {
                    moduleBookImport.Children.Add(new ModuleMeta { Label = "163", CustomUI = "http://www.163.com" });

                    moduleQuery.Children[0].Children.Add(new ModuleMeta
                    {
                        Label = "书籍查询(Url访问)",
                        CustomUI = "EntityModule?isAggt=1&type=Demo.Book&viewName=书籍查询界面"
                    });
                }
            };

            app.DbMigratingOperations += (o, e) =>
            {
                using (var c = new OEADbMigrationContext(DemoEntity.ConnectionString))
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