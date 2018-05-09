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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Accounts;
using Rafy.Accounts.Controllers;
using Rafy.DataArchiver;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Stamp;
using Rafy.ManagedProperty;
using Rafy.UnitTest.Repository;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class DataArchiverTest
    {
        public static string DbSettingName = UnitTestEntityRepositoryDataProvider.DbSettingName,
            BackUpDbSettingName = UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate;

        [ClassInitialize]
        public static void DAT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void DAT_Cut()
        {
            using (RF.TransactionScope(BackUpDbSettingName))
            using (RF.TransactionScope(DbSettingName))
            {
                var rawList = new BookCategoryList
                {
                    new BookCategory
                    {
                        Name = "bc1"
                    }
                };

                var repo = RF.ResolveInstance<BookCategoryRepository>();
                repo.Save(rawList);
                Assert.AreEqual(1, repo.CountAll(), "新增数目为 1");

                rawList = repo.GetAll();//由于数据库中存储的值可能与内存中的值有一定的差异，所以这里需要把这些数据重新读取出来再进行对比。

                Archive(new List<ArchiveItem>
                {
                    new ArchiveItem
                    {
                        AggregationRoot = typeof(BookCategory),
                        ArchiveType = ArchiveType.Cut
                    },
                });

                Assert.AreEqual(0, repo.CountAll(), "执行数据归档后数目为 0");
                using (RdbDataProvider.RedirectDbSetting(DbSettingName, BackUpDbSettingName))
                {
                    Assert.AreEqual(1, repo.CountAll(), "数据归档数据库数目为 1");
                }
                AssertAllDataMigrated(rawList, repo);
            }
        }

        [TestMethod]
        public void DAT_Cut_Aggt_Phantom()
        {
            using (RF.TransactionScope(BackUpDbSettingName))
            using (RF.TransactionScope(DbSettingName))
            {
                var rawList = new InvoiceList
                {
                    new Invoice()
                    {
                        Code = "0101",
                        InvoiceItemList =
                        {
                            new InvoiceItem
                            {
                                Amount = 111D
                            },
                            new InvoiceItem
                            {
                                Amount = 222D
                            },
                        }
                    }
                };

                var repo = RF.ResolveInstance<InvoiceRepository>();
                repo.Save(rawList);

                rawList = repo.GetAll();//由于数据库中存储的值可能与内存中的值有一定的差异，所以这里需要把这些数据重新读取出来再进行对比。

                Archive(new List<ArchiveItem>
                {
                    new ArchiveItem
                    {
                        AggregationRoot = typeof(Invoice),
                        ArchiveType = ArchiveType.Cut
                    },
                });

                AssertAllDataMigrated(rawList, repo);
            }
        }

        [TestMethod]
        public void DAT_Cut_Aggt_ManyData()
        {
            using (RF.TransactionScope(BackUpDbSettingName))
            using (RF.TransactionScope(DbSettingName))
            {
                var rawList = new InvoiceList
                {
                    new Invoice()
                    {
                        Code = "0101",
                        InvoiceItemList =
                        {
                            new InvoiceItem
                            {
                                Amount = 111D
                            },
                            new InvoiceItem
                            {
                                Amount = 222D
                            },
                        }
                    },
                    new Invoice()
                    {
                        Code = "0102",
                        InvoiceItemList =
                        {
                            new InvoiceItem
                            {
                                Amount = 333D
                            },
                            new InvoiceItem
                            {
                                Amount = 444D
                            },
                        }
                    },
                    new Invoice
                    {
                        Code = "0103",
                        InvoiceItemList =
                        {
                            new InvoiceItem
                            {
                                Amount = 555D
                            },
                        }
                    },
                    new Invoice
                    {
                        Code = "0103"
                    }
                };

                var repo = RF.ResolveInstance<InvoiceRepository>();
                repo.Save(rawList);
                rawList = repo.GetAll();//由于数据库中存储的值可能与内存中的值有一定的差异，所以这里需要把这些数据重新读取出来再进行对比。

                Archive(new List<ArchiveItem>
                {
                    new ArchiveItem
                    {
                        AggregationRoot = typeof(Invoice),
                        ArchiveType = ArchiveType.Cut
                    },
                });

                AssertAllDataMigrated(rawList, repo);
            }
        }

        [TestMethod]
        public void DAT_Cut_Aggt_TreeEntity()
        {
            //树形实体带聚合子
            var repo = RF.ResolveInstance<FolderRepository>();
            using (RF.TransactionScope(BackUpDbSettingName))
            using (RF.TransactionScope(DbSettingName))
            {
                var rawList = new FolderList
                {
                    new Folder
                    {
                        Name = "001.",
                        FileList =
                        {
                            new File
                            {
                                Name = "f1"
                            },
                            new File
                            {
                                Name = "f2"
                            },
                        },
                        TreeChildren =
                        {
                            new Folder
                            {
                                TreeChildren =
                                {
                                    new Folder()
                                }
                            }
                        }
                    }
                };
                repo.Save(rawList);
                rawList = repo.GetAll();//由于数据库中存储的值可能与内存中的值有一定的差异，所以这里需要把这些数据重新读取出来再进行对比。

                Archive(new List<ArchiveItem>
                {
                    new ArchiveItem
                    {
                        AggregationRoot = typeof(Folder),
                        ArchiveType = ArchiveType.Cut
                    },
                });

                AssertAllDataMigrated(rawList, repo);
            }
        }

        /// <summary>
        /// 多个实体
        /// </summary>
        [TestMethod]
        public void DAT_Cut_Aggt_ManyTypes()
        {
            using (RF.TransactionScope(BackUpDbSettingName))
            using (RF.TransactionScope(DbSettingName))
            {
                var rawFolders = new FolderList
                {
                    new Folder
                    {
                        Name = "001.",
                        FileList =
                        {
                            new File
                            {
                                Name = "f1"
                            },
                            new File
                            {
                                Name = "f2"
                            },
                        },
                        TreeChildren =
                        {
                            new Folder
                            {
                                TreeChildren =
                                {
                                    new Folder()
                                }
                            }
                        }
                    }
                };
                var folderRepo = RF.ResolveInstance<FolderRepository>();
                folderRepo.Save(rawFolders);
                rawFolders = folderRepo.GetAll();//由于数据库中存储的值可能与内存中的值有一定的差异，所以这里需要把这些数据重新读取出来再进行对比。

                var rawInvoices = new InvoiceList
                {
                    new Invoice()
                    {
                        Code = "0101",
                        InvoiceItemList =
                        {
                            new InvoiceItem
                            {
                                Amount = 111D
                            },
                            new InvoiceItem
                            {
                                Amount = 222D
                            },
                        }
                    },
                    new Invoice()
                    {
                        Code = "0102",
                        InvoiceItemList =
                        {
                            new InvoiceItem
                            {
                                Amount = 333D
                            }
                        }
                    }
                };
                var invRepo = RF.ResolveInstance<InvoiceRepository>();
                invRepo.Save(rawInvoices);
                rawInvoices = invRepo.GetAll();//由于数据库中存储的值可能与内存中的值有一定的差异，所以这里需要把这些数据重新读取出来再进行对比。

                Archive(new List<ArchiveItem>
                {
                    new ArchiveItem
                    {
                        AggregationRoot = typeof(Invoice),
                        ArchiveType = ArchiveType.Cut
                    },
                    new ArchiveItem
                    {
                        AggregationRoot = typeof(Folder),
                        ArchiveType = ArchiveType.Cut
                    }
                });

                AssertAllDataMigrated(rawFolders, folderRepo);
                AssertAllDataMigrated(rawInvoices, invRepo);
            }
        }

        //[TestMethod]
        //public void DAT_Copy_Insert()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        //[TestMethod]
        //public void DAT_Copy_Insert_Aggt()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        //[TestMethod]
        //public void DAT_Copy_Update()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        //[TestMethod]
        //public void DAT_Copy_Update_Aggt()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        /// <summary>
        /// 执行数据接口。
        /// </summary>
        /// <param name="items"></param>
        private void Archive(List<ArchiveItem> items)
        {
            var context = new AggregationArchiveContext
            {
                BatchSize = 2,
                DateOfArchiving = DateTime.Now.AddMinutes(1),
                ItemsToArchive = items
            };
            this.Archive(context);
        }

        private void Archive(AggregationArchiveContext context)
        {
            context.OrignalDataDbSettingName = DbSettingName;
            context.BackUpDbSettingName = BackUpDbSettingName;

            var archiver = this.CreateAggregationArchiver();
            archiver.Archive(context);
        }

        protected virtual AggregationArchiver CreateAggregationArchiver()
        {
            return new AggregationArchiver();
        }

        private static void AssertAllDataMigrated(EntityList rawList, IRepository repository)
        {
            Assert.AreEqual(0, repository.CountAll(), "执行数据归档后聚合根的数目为 0");

            EntityList migratedList = null;
            using (RdbDataProvider.RedirectDbSetting(DbSettingName, BackUpDbSettingName))
            {
                migratedList = repository.GetAll();
            }

            AssertAggtEqual(rawList, migratedList, repository);
        }

        private static void AssertAggtEqual(IList<Entity> rawList, IList<Entity> migratedList, IRepository repository)
        {
            Assert.AreEqual(rawList.Count, migratedList.Count, "迁移前后，实体列表的个数应该相同。" + repository.EntityType.FullName);

            var properties = repository.EntityMeta.ManagedProperties.GetNonReadOnlyCompiledProperties();
            for (int j = 0, c2 = rawList.Count; j < c2; j++)
            {
                var raw = rawList[j];
                var migrated = migratedList[j];

                //对比所有的属性。
                for (int i = 0, c = properties.Count; i < c; i++)
                {
                    var property = properties[i];
                    var valueRaw = raw.GetProperty(property);
                    var valueMigrated = migrated.GetProperty(property);
                    Assert.AreEqual(valueRaw, valueMigrated, $"迁移前后，实体 { raw.Id } 的属性 { property.Name } 的值应该相同。");
                }

                //对比所有的聚合子属性。
                var childProperties = repository.GetChildProperties();
                for (int i = 0, c = childProperties.Count; i < c; i++)
                {
                    if (childProperties[i] is IListProperty childProperty)
                    {
                        var childrenRaw = raw.GetLazyList(childProperty);
                        var childrenMigrated = raw.GetLazyList(childProperty);
                        AssertAggtEqual(childrenRaw, childrenMigrated, childrenRaw.GetRepository());
                    }
                }

                if (repository.SupportTree)
                {
                    AssertAggtEqual(raw.TreeChildren, migrated.TreeChildren, repository);
                }
            }
        }
    }
}
