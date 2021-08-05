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
using Demo.WPF;
using Rafy.ComponentModel;

namespace Demo
{
    class DemoLibrary : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            app.MetaCreating += (o, e) =>
            {
                //定义书籍查询界面的结构
                UIModel.AggtBlocks.DefineBlocks("书籍查询界面", m => new AggtBlocks
                {
                    MainBlock = new Block(typeof(Book))
                    {
                        //ExtendView = "书籍查询界面_Book"
                    },
                    Surrounders = 
                    {
                        new ConditionBlock(typeof(BookQueryCriteria))
                    }
                });

                if (RafyEnvironment.Location.IsWebUI)
                {
                    var moduleBookImport = CommonModel.Modules.AddRoot(new WebModuleMeta
                    {
                        Label = "书籍管理系统示例",
                        Children =
                        {
                            new WebModuleMeta{ Label = "类别管理", EntityType = typeof(BookCategory)},
                            new WebModuleMeta{ Label = "书籍管理", EntityType = typeof(Book)},
                            new WebModuleMeta{ Label = "图书管理员", EntityType = typeof(BookAdministrator)},
                            new WebModuleMeta{ Label = "书籍查询", EntityType = typeof(Book), BlocksTemplate = typeof(BookQueryModule)}
                        }
                    });

                    moduleBookImport.Children.Add(new WebModuleMeta { Label = "163", Url = "http://www.163.com" });

                    var bookQuery = moduleBookImport.Children[3];
                    bookQuery.AggtBlocksName = "书籍查询界面";
                    bookQuery.Children.Add(new WebModuleMeta
                    {
                        Label = "书籍查询(Url访问)",
                        Url = "EntityModule?isAggt=1&type=Demo.Book&viewName=书籍查询界面"
                    });
                }
                else
                {
                    CommonModel.Modules.AddRoot(new WPFModuleMeta
                    {
                        Label = "书籍管理系统示例",
                        Children =
                        {
                            new WPFModuleMeta{ Label = "类别管理", EntityType = typeof(BookCategory)},
                            new WPFModuleMeta{ Label = "书籍管理", EntityType = typeof(Book)},
                            new WPFModuleMeta{ Label = "图书管理员", EntityType = typeof(BookAdministrator)},
                            new WPFModuleMeta{ Label = "书籍查询", EntityType = typeof(Book), BlocksTemplate = typeof(BookQueryModule)}
                        }
                    });
                }
            };
        }
    }
}