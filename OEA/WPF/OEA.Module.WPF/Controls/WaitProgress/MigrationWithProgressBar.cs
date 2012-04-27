/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120427
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120427
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using OEA.Library.ORM.DbMigration;
using System.Windows;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 升级数据库，并显示进度条。
    /// </summary>
    public static class MigrationWithProgressBar
    {
        public static void Do(string dbSetting)
        {
            Do(dbSetting, c => c.AutoMigrate());
        }

        public static void Do(string dbSetting, Action<OEADbMigrationContext> action)
        {
            using (var c = new OEADbMigrationContext(dbSetting))
            {
                if (!c.IsEnabled() || !OEAEnvironment.Location.IsOnClient())
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
                    win.txtTitle.Text = string.Format("正在生成 {0} 数据库，请稍等……", dbSetting);

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

                    if (exception != null) { throw exception; }
                }
            }
        }
    }
}
