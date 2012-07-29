using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.ORM.DbMigration;
using OEA.MetaModel.View;
using OEA.MetaModel;
using Demo.WPF;

namespace Demo
{
    class DemoLibrary : LibraryPlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public override void Initialize(IApp app)
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
                        new ConditionBlock(typeof(BookQueryCriteria))
                    }
                });
            };

            app.ModuleOperations += (o, e) =>
            {
                var moduleBookImport = CommonModel.Modules.AddRoot(new ModuleMeta
                {
                    Label = "书籍管理系统示例",
                    Children =
                    {
                        new ModuleMeta{ Label = "类别管理", EntityType = typeof(BookCategory)},
                        new ModuleMeta{ Label = "书籍管理", EntityType = typeof(Book)},
                        new ModuleMeta{ Label = "图书管理员", EntityType = typeof(BookAdministrator)},
                        new ModuleMeta{ Label = "书籍查询", EntityType = typeof(Book), WPFTemplateType = typeof(BookQueryModule)}
                    }
                });

                if (OEAEnvironment.IsWeb)
                {
                    moduleBookImport.Children.Add(new ModuleMeta { Label = "163", CustomUI = "http://www.163.com" });

                    var bookQuery = moduleBookImport.Children[3];
                    bookQuery.AggtBlocksName = "书籍查询界面";
                    bookQuery.Children.Add(new ModuleMeta
                    {
                        Label = "书籍查询(Url访问)",
                        CustomUI = "EntityModule?isAggt=1&type=Demo.Book&viewName=书籍查询界面"
                    });
                }
            };
        }
    }
}