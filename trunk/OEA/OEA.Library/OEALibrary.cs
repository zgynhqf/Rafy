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
using OEA.ORM.DbMigration;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using Common;
using OEA.ManagedProperty;

namespace OEA.Library
{
    class OEALibrary : LibraryPlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel._System; }
        }

        public override void Initialize(IApp app)
        {
            RepositoryFactoryHost.Factory = RepositoryFactory.Instance;
            PropertyDescriptorFactory.Current = new OEAPropertyDescriptorFactory();

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
                //如果不是单机版，只是在服务端，则在这里升级数据库。
                //否则，如果是单机版，就在 OEA.RBAC.WPF 中升级，以便弹出进度条。
                if (OEAEnvironment.Location != OEALocation.LocalVersion)
                {
                    //根据两种不同的配置名称来判断是否打开 Drop 功能
                    bool runDataLossOperation = false;
                    var configNames = ConfigurationHelper.GetAppSettingOrDefault("OEA_AutoMigrate_Databases_WithoutDropping");
                    if (string.IsNullOrEmpty(configNames))
                    {
                        configNames = ConfigurationHelper.GetAppSettingOrDefault("OEA_AutoMigrate_Databases");
                        if (!string.IsNullOrEmpty(configNames)) { runDataLossOperation = true; }
                    }

                    if (!string.IsNullOrEmpty(configNames))
                    {
                        var configArray = configNames.Replace(ConnectionStringNames.DbMigrationHistory, string.Empty)
                           .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        using (var c = new OEADbMigrationContext(ConnectionStringNames.DbMigrationHistory))
                        {
                            //对于迁移日志库的构造，无法记录它本身的迁移日志
                            c.HistoryRepository = null;

                            c.AutoMigrate();
                        }

                        foreach (var config in configArray)
                        {
                            using (var c = new OEADbMigrationContext(config))
                            {
                                c.RunDataLossOperation = runDataLossOperation;
                                c.AutoMigrate();
                            }
                        }
                    }
                }
            };
        }
    }
}