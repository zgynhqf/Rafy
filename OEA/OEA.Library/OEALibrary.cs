/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110309
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100309
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration;
using DbMigration.SqlServer;
using hxy.Common.Data;
using hxy.Common.Data.Providers;
using OEA.Library.Caching;
using OEA.Library.ORM.DbMigration;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace OEA.Library
{
    class OEALibrary : ILibrary
    {
        ReuseLevel IPlugin.ReuseLevel { get { return ReuseLevel._System; } }

        void ILibrary.Initialize(IApp app)
        {
            RepositoryFactoryHost.Factory = RepositoryFactory.Instance;

            app.AllPluginsMetaIntialized += (o, e) =>
            {
                UIModel.AggtBlocks.DefineBlocks("ViewConfigurationModel模块界面", m =>
                {
                    var blocks = new AggtBlocks
                    {
                        MainBlock = new Block(typeof(ViewConfigurationModel))
                        {
                            BlockType = BlockType.Detail
                        },
                        Children = 
                        {
                            new ChildBlock("属性", ViewConfigurationModel.ViewConfigurationPropertyListProperty),
                            new ChildBlock("命令", ViewConfigurationModel.ViewConfigurationCommandListProperty)
                        }
                    };

                    if (OEAEnvironment.IsWeb)
                    {
                        blocks.Layout.Class = "Oea.autoUI.layouts.RightChildren";
                    }
                    else
                    {
                        blocks.Layout.IsLayoutChildrenHorizonal = true;
                    }

                    return blocks;
                });
            };

            app.AppModelCompleted += (o, e) =>
            {
                foreach (var kv in CommonModel.Entities.EnumerateAllOverriding())
                {
                    RepositoryFactory.Instance.OverrideRepository(kv.Key, kv.Value);
                }
            };

            app.DbMigratingOperations += (o, e) =>
            {
                using (var c = new OEADbMigrationContext(ConnectionStringNames.DbMigrationHistory))
                {
                    //对于迁移日志库的构造，无法记录它本身的迁移日志
                    c.HistoryRepository = null;

                    c.AutoMigrate();
                }
            };
        }
    }
}