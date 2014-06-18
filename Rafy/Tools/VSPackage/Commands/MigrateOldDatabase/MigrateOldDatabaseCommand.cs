/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130422 16:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EnvDTE;
using Rafy.Data;

namespace Rafy.VSPackage.Commands.MigrateOldDatabase
{
    /// <summary>
    /// 从旧数据库生成实体代码的命令。
    /// </summary>
    class MigrateOldDatabaseCommand : SelectedProjectsCommand
    {
        public MigrateOldDatabaseCommand()
        {
            this.CommandID = new CommandID(GuidList.guidVSPackageCmdSet, PkgCmdIDList.cmdidMigrateOldDatabaseCommand);
        }

        protected override void ExecuteOnProject(IList<Project> projects)
        {
            var win = new MigerateDatabaseWizardWindow();
            win.txtConnectionString.Text = @"server=.\SQLExpress;database=XXX;uid=XXX;pwd=XXX";
            win.txtDomainName.Text = projects[0].Properties.Item("RootNamespace").Value.ToString();
            var res = win.ShowDialog();
            if (res == true)
            {
                var connectionString = win.txtConnectionString.Text;
                var domainName = win.txtDomainName.Text;

                //构造实体代码生成器。
                var generator = new EntityGenerator();
                generator.DomainName = domainName;
                generator.DbSetting = DbSetting.SetSetting(
                    domainName, connectionString,
                    DbSetting.Provider_SqlClient//目前只支持 SqlServer。
                    );

                //默认生成在 Entities 文件夹内。
                var items = projects[0].ProjectItems;
                var item = items.FindByName("Entities");
                if (item == null)
                {
                    item = items.AddFolder("Entities");
                }
                generator.Directory = item;

                //开始生成代码。
                try
                {
                    var error = generator.Generate();
                    var msg = string.Format("生成完毕，本次一共生成了 {0} 张表。", generator.SuccessCount);
                    if (!string.IsNullOrEmpty(error))
                    {
                        msg += Environment.NewLine + "无法移植的表：" + Environment.NewLine + error;
                    }
                    MessageBox.Show(msg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}