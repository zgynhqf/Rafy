/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100326
 * 说明：当前工程所对应的模块类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100326
 * 
*******************************************************/

using System;
using System.Data.SqlClient;
using Common;
using OEA.WPF.Command;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.WPF;
using OEA.ORM.DbMigration;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 当前工程所对应的模块类。
    /// </summary>
    internal class WPFModule : ModulePlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel._System; }
        }

        public override void Initialize(IClientApp app)
        {
            //初始化命令列表
            WPFCommandNames.CustomizeUI = typeof(CustomizeUI);
            WPFCommandNames.FireQuery = typeof(QueryObjectCommand);
            WPFCommandNames.RefreshDataSourceInRDLC = typeof(RefreshDataSourceInRDLC);
            WPFCommandNames.ShowReportData = typeof(ShowReportData);
            WPFCommandNames.PopupAdd = typeof(PopupAddCommand);
            WPFCommandNames.Add = typeof(AddCommand);
            WPFCommandNames.SaveBill = typeof(SaveBillCommand);
            WPFCommandNames.SaveList = typeof(SaveListCommand);
            WPFCommandNames.Cancel = typeof(CancelCommand);
            WPFCommandNames.Refresh = typeof(RefreshCommand);
            WPFCommandNames.Delete = typeof(DeleteListObjectCommand);
            WPFCommandNames.Edit = typeof(EditDetailCommand);
            WPFCommandNames.ExportToExcel = typeof(ExportToExcelCommand);

            WPFCommandNames.MoveUp = typeof(MoveUpCommand);
            WPFCommandNames.MoveDown = typeof(MoveDownCommand);
            WPFCommandNames.LevelUp = typeof(LevelUpCommand);
            WPFCommandNames.LevelDown = typeof(LevelDownCommand);
            WPFCommandNames.InsertBefore = typeof(InsertBeforeCommand);
            WPFCommandNames.InsertChild = typeof(AddChildCommand);
            WPFCommandNames.ExpandAll = typeof(ExpandAllCommand);
            WPFCommandNames.ExpandOne = typeof(ExpandToLevelOneCommand);
            WPFCommandNames.ExpandTwo = typeof(ExpandToLevelTwoCommand);
            WPFCommandNames.ExpandThree = typeof(ExpandToLevelThreeCommand);
            WPFCommandNames.ExpandFour = typeof(ExpandToLevelFourCommand);

            WPFCommandNames.InitCommonCommands();

            app.Composed += (s, e) => App.Current.InitMainWindow();
            app.LoginSuccessed += (s, e) => App.Current.InitUIModuleList();

            //只有单机版会执行这里的代码
            app.DbMigratingOperations += (o, e) => { MigrateDbInProgress(); };
        }

        private static void MigrateDbInProgress()
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

                MigrationWithProgressBar.Do(ConnectionStringNames.DbMigrationHistory, c =>
                {
                    //对于迁移日志库的构造，无法记录它本身的迁移日志
                    c.HistoryRepository = null;

                    c.AutoMigrate();
                });

                foreach (var config in configArray)
                {
                    //using (var c = new OEADbMigrationContext(config))
                    //{
                    //    c.RollbackAll();
                    //    c.ResetHistory();
                    //    c.ResetDbVersion();
                    //    c.AutoMigrate();
                    //}
                    MigrationWithProgressBar.Do(config, c =>
                    {
                        c.RunDataLossOperation = runDataLossOperation;

                        c.AutoMigrate();
                    });
                }
            }
        }
    }
}