/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 14:33
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 14:33
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Rafy.WPF;
using Rafy.Domain.ORM.DbMigration;
using Rafy.DbMigration;
using Rafy.Domain;

namespace Rafy.DevTools.DbManagement
{
    /// <summary>
    /// 客户端数据库升级助手。
    /// </summary>
    public class ClientMigrationHelper
    {
        /// <summary>
        /// 在客户端弹出窗口进行数据库升级。
        /// </summary>
        public static void MigrateOnClient()
        {
            bool success = false;
            var control = new DatabaseMigrationControl();
            App.Windows.ShowDialog(control, win =>
            {
                win.Title = "选择要升级的数据库";
                win.Width = 200;
                win.SizeToContent = SizeToContent.Height;
                win.ShowInTaskbar = true;

                win.ValidateOperations += (o, e) =>
                {
                    var options = control.GetSelectionResult();
                    if (!HasDatabase(options)) { e.Cancel = true; }

                    if (RafyEnvironment.Location.IsWPFUI)
                    {
                        if (RafyEnvironment.Location.ConnectDataDirectly)
                        {
                            MigrateDbInProgress(options);
                        }
                        else
                        {
                            MigrateOnServer(options);
                        }
                        success = true;
                    }
                };
            });
            if (success) { App.MessageBox.Show("数据库升级完成。"); }
        }

        /// <summary>
        /// 是否有需要升级的数据库。
        /// </summary>
        /// <returns></returns>
        private static bool HasDatabase(MigratingOptions options)
        {
            return options.Databases != null && options.Databases.Length > 0;
        }

        /// <summary>
        /// 单机版的数据库生成。
        /// </summary>
        private static void MigrateDbInProgress(MigratingOptions options)
        {
            MigrateInProgressBar(ConnectionStringNames.DbMigrationHistory, c =>
            {
                c.AutoMigrate();
            });

            foreach (var config in options.Databases)
            {
                MigrateInProgressBar(config, c =>
                {
                    c.RunDataLossOperation = options.RunDataLossOperation;
                    c.HistoryRepository = new DbHistoryRepository();

                    c.AutoMigrate();
                });
            }
        }

        /// <summary>
        /// 在进行条中执行指定数据库的升级操作。
        /// </summary>
        /// <param name="dbSetting"></param>
        /// <param name="action"></param>
        private static void MigrateInProgressBar(string dbSetting, Action<RafyDbMigrationContext> action)
        {
            using (var c = new RafyDbMigrationContext(dbSetting))
            {
                c.RunDataLossOperation = DataLossOperation.All;

                if (!RafyDbMigrationContext.IsEnabled())
                {
                    action(c);

                    //c.DeleteDatabase();
                    //c.AutoMigrate();

                    //其它一些可用的API
                    //c.ClassMetaReader.IgnoreTables.Add("ReportObjectMetaData");
                    //c.RollbackToHistory(DateTime.Parse("2008-12-31 23:59:58.700"), RollbackAction.DeleteHistory);
                    //c.DeleteDatabase();
                    //c.ResetHistories();
                    //c.RollbackAll();
                    //c.JumpToHistory(DateTime.Parse("2012-01-07 21:27:00.000"));
                }
                else
                {
                    var win = new WaitDialog();
                    win.Width = 500;
                    win.Opacity = 0;
                    win.ShowInTaskbar = false;
                    win.Text = string.Format("正在生成 {0} 数据库，请稍侯……", dbSetting);

                    Exception exception = null;

                    ThreadPool.QueueUserWorkItem(oo =>
                    {
                        try
                        {
                            bool first = false;
                            c.ItemMigrated += (o, e) =>
                            {
                                if (!first)
                                {
                                    Action setVisible = () => win.Opacity = 1;
                                    win.Dispatcher.Invoke(setVisible);
                                    first = true;
                                }
                                win.ProgressValue = new ProgressValue
                                {
                                    Percent = 100 * e.Index / (double)e.TotalCount
                                };
                            };

                            action(c);
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }

                        Action ac = () => win.DialogResult = true;
                        win.Dispatcher.BeginInvoke(ac);
                    });

                    win.ShowDialog();

                    if (exception != null) { throw new Rafy.DbMigration.DbMigrationException("数据库升级时出错，请查看 InnerException。", exception); }
                }
            }
        }

        /// <summary>
        /// 调用服务在服务端升级数据库。
        /// </summary>
        /// <param name="options"></param>
        private static void MigrateOnServer(MigratingOptions options)
        {
            var win = new WaitDialog();
            win.Width = 500;
            win.IsIndeterminate = true;
            win.ShowInTaskbar = false;
            win.Text = "正在服务端执行数据库升级操作，请稍等……";

            Exception exception = null;

            ThreadPool.QueueUserWorkItem(oo =>
            {
                try
                {
                    var svc = ServiceFactory.Create<MigrateService>();
                    svc.Options = options;
                    svc.Invoke();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Action ac = () => win.DialogResult = true;
                win.Dispatcher.BeginInvoke(ac);
            });

            win.ShowDialog();

            if (exception != null) { throw new Rafy.DbMigration.DbMigrationException("数据库升级时出错，请查看 InnerException。", exception); }
        }
    }
}