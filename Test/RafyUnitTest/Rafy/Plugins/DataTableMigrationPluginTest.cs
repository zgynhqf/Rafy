/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 15:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Accounts;
using Rafy.Accounts.Controllers;
using Rafy.DataTableMigration;
using Rafy.DataTableMigration.Contexts;
using Rafy.DataTableMigration.Services;
using Rafy.Domain;
using Rafy.Domain.ORM;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class DataTableMigrationPluginTest
    {
        [ClassInitialize]
        public static void APT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void DPT_Entity_Migration()
        {
            //聚合实体
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(DataTableMigrationPlugin.BackUpDbSettingName))
            {
                using (RF.TransactionScope(DataTableMigrationPlugin.DbSettingName))
                {
                    RF.Save(new Invoice()
                    {
                        InvoiceItemList =
                        {
                            new InvoiceItem(),
                            new InvoiceItem(),
                        }
                    });
                    Assert.AreEqual(repo.GetAll().Count, 1, "新增 Invoice 数目为1");

                    var context = new DataTableMigrationContext(
                        2,
                        DateTime.Now.AddMinutes(2),
                        new List<Type>()
                        {
                            typeof (Invoice)
                        }
                        );

                    var dataTableMigrationService = new DataTableMigrationService(context);

                    dataTableMigrationService.ExecuteArchivingData();

                    Assert.AreEqual(repo.GetAll().Count, 0, "执行数据归档后 Invoice 数目为 0");

                    using (
                        RdbDataProvider.RedirectDbSetting(DataTableMigrationPlugin.DbSettingName,
                            DataTableMigrationPlugin.BackUpDbSettingName))
                    {
                        Assert.AreEqual(repo.GetAll().Count, 1, "数据归档数据库 Invoice 数目为 1");
                    }
                }
            }
        }

        [TestMethod]
        public void DPT_EntityList_Migration()
        {
            //聚合实体
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(DataTableMigrationPlugin.BackUpDbSettingName))
            {
                using (RF.TransactionScope(DataTableMigrationPlugin.DbSettingName))
                {
                    var invoiceList = new InvoiceList
                    {
                        new Invoice()
                        {
                            InvoiceItemList =
                            {
                                new InvoiceItem(),
                                new InvoiceItem(),
                            }
                        },
                        new Invoice()
                        {
                            InvoiceItemList =
                            {
                                new InvoiceItem(),
                                new InvoiceItem(),
                            }
                        },
                        new Invoice(),
                        new Invoice()
                    };

                    RF.Save(invoiceList);

                    Assert.AreEqual(repo.GetAll().Count, 4, "新增 Invoice 数目为4");

                    var context = new DataTableMigrationContext(
                        2,
                        DateTime.Now.AddMinutes(2),
                        new List<Type>()
                        {
                            typeof (Invoice)
                        }
                        );

                    var dataTableMigrationService = new DataTableMigrationService(context);

                    dataTableMigrationService.ExecuteArchivingData();

                    Assert.AreEqual(repo.GetAll().Count, 0, "执行数据归档后 Invoice 数目为 0");

                    using (RdbDataProvider.RedirectDbSetting(
                        DataTableMigrationPlugin.DbSettingName,
                        DataTableMigrationPlugin.BackUpDbSettingName
                        ))
                    {
                        Assert.AreEqual(repo.GetAll().Count, 4, "数据归档数据库 Invoice 数目为 4");
                    }
                }
            }
        }

        [TestMethod]
        public void DPT_TreeEntity_Migration()
        {
            //树形实体带聚合子

            var repo = RF.ResolveInstance<FolderRepository>();
            using (RF.TransactionScope(DataTableMigrationPlugin.BackUpDbSettingName))
            {
                using (RF.TransactionScope(DataTableMigrationPlugin.DbSettingName))
                {
                    RF.Save(new Folder
                    {
                        Name = "001.",
                        FileList = { new File(), new File() },
                        TreeChildren =
                        {
                            new Folder
                            {
                                TreeChildren = {new Folder()}
                            }
                        }
                    });
                    Assert.AreEqual(repo.GetAll().Count, 1, "树形实体只查根实体 Folder 数目为1");

                    var context = new DataTableMigrationContext(
                        2,
                        DateTime.Now.AddMinutes(2),
                        new List<Type>()
                        {
                            typeof (Folder)
                        }
                        );

                    var dataTableMigrationService = new DataTableMigrationService(context);

                    dataTableMigrationService.ExecuteArchivingData();

                    Assert.AreEqual(repo.GetAll().Count, 0, "执行数据归档后 Folder 数目为 0");

                    using (
                        RdbDataProvider.RedirectDbSetting(DataTableMigrationPlugin.DbSettingName,
                            DataTableMigrationPlugin.BackUpDbSettingName))
                    {
                        Assert.AreEqual(repo.GetAll().Count, 1, "归档数据库 Folder 数目为 3");
                        Assert.AreEqual(repo.GetAll()[0].FileList.Count, 2, "归档数据库聚合子 File 数目为 2");
                    }
                }
            }
        }

        [TestMethod]
        public void DPT_MoreEntity_Migration()
        {
            //多个实体

            var repo = RF.ResolveInstance<FolderRepository>();
            var repoInvoice = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(DataTableMigrationPlugin.BackUpDbSettingName))
            {
                using (RF.TransactionScope(DataTableMigrationPlugin.DbSettingName))
                {
                    RF.Save(new Folder
                    {
                        Name = "001.",
                        FileList = { new File(), new File() },
                        TreeChildren =
                        {
                            new Folder
                            {
                                TreeChildren = {new Folder()}
                            }
                        }
                    });
                    Assert.AreEqual(repo.GetAll().Count, 1, "树形实体只查根实体 Folder 数目为1");

                    RF.Save(new InvoiceList
                    {
                        new Invoice()
                            {
                                InvoiceItemList =
                                {
                                    new InvoiceItem(),
                                    new InvoiceItem(),
                                }
                            },
                        new Invoice()
                    });
                    Assert.AreEqual(repoInvoice.GetAll().Count, 2, "新增 Invoice 数目为2");

                    var context = new DataTableMigrationContext(
                        2,
                        DateTime.Now.AddMinutes(2),
                        new List<Type>()
                        {
                            typeof (Folder),
                            typeof(Invoice)
                        }
                        );

                    var dataTableMigrationService = new DataTableMigrationService(context);

                    dataTableMigrationService.ExecuteArchivingData();

                    Assert.AreEqual(repo.GetAll().Count, 0, "执行数据归档后 Folder 数目为 0");

                    Assert.AreEqual(repoInvoice.GetAll().Count, 0, "执行数据归档后 Invoice 数目为 0");

                    using (
                        RdbDataProvider.RedirectDbSetting(DataTableMigrationPlugin.DbSettingName,
                            DataTableMigrationPlugin.BackUpDbSettingName))
                    {
                        Assert.AreEqual(repo.GetAll().Count, 1, "归档数据库 Folder 数目为 3");
                        Assert.AreEqual(repo.GetAll()[0].FileList.Count, 2, "归档数据库聚合子 File 数目为 2");
                        Assert.AreEqual(repoInvoice.GetAll().Count, 2, "数据归档数据库 Invoice 数目为 2");
                    }
                }
            }
        }
    }
}
