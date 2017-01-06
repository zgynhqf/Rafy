/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151024 12:32
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Domain;
using Rafy.Domain.EntityPhantom;
using UT;

namespace RafyUnitTest
{
    /// <summary>
    /// 幽灵实体功能测试。
    /// </summary>
    [TestClass]
    public class EntityPhantomTest
    {
        [ClassInitialize]
        public static void EPT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void EPT_IsPhantom()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var inv = new Invoice();
                Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(inv), false, "所有实体的幽灵标识为 false。");

                repo.Save(inv);
                Assert.AreEqual(repo.CountAll(), 1);
            }
        }

        [TestMethod]
        public void EPT_SimpleDelete()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var item = new Invoice();
                repo.Save(item);
                Assert.IsTrue(repo.CountAll() == 1);

                item.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(item);

                Assert.AreEqual(repo.CountAll(), 0, "开启了幽灵功能的实体，也需要能被正常的删除和查询。");
            }
        }

        [TestMethod]
        public void EPT_Clear()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var InvoiceList = new InvoiceList
                {
                    new Invoice(),
                    new Invoice(),
                    new Invoice()
                };
                repo.Save(InvoiceList);
                Assert.AreEqual(repo.CountAll(), 3);

                InvoiceList.Clear();
                repo.Save(InvoiceList);
                Assert.AreEqual(repo.CountAll(), 0);
            }
        }

        [TestMethod]
        public void EPT_Query()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var Invoice = new Invoice();
                repo.Save(Invoice);
                Assert.AreEqual(repo.CountAll(), 1);

                Invoice.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(Invoice);

                Assert.AreEqual(repo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                var all = repo.GetAll();
                Assert.AreEqual(all.Count, 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");

                using (PhantomContext.DontFilterPhantoms())
                {
                    Assert.AreEqual(repo.CountAll(), 1, "幽灵状态的实体，可以使用特定 API 查出。");
                    var all2 = repo.GetAll();
                    Assert.AreEqual(all2.Count, 1, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(all2[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                }
            }
        }

        [TestMethod]
        public void EPT_Query_Join()
        {
            /*********************** sql语句 *********************************
              SELECT [Invoice].*
                FROM [Invoice]
                    INNER JOIN [InvoiceItem] ON [Invoice].[Id] = [InvoiceItem].[InvoiceId]
                WHERE [Invoice].[IsPhantom] = 0 AND [InvoiceItem].[IsPhantom] = 0
                ORDER BY [Invoice].[Id] ASC;
             **********************************************************************/

            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var item1 = new InvoiceItem();
                var invoice = new Invoice()
                {
                    InvoiceItemList =
                    {
                        new InvoiceItem(),
                        item1
                    }
                };

                repo.Save(invoice);
                Assert.AreEqual(repo.GetInvoice().Count, 2);

                item1.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(invoice);
                Assert.AreEqual(repo.GetInvoice().Count, 1, "删除后，Join的表添加过滤条件，数量为1");
            }
        }

        [TestMethod]
        public void EPT_Query_Exists()
        {
            /*********************** sql语句 *********************************
                SELECT [Invoice].*
                FROM [Invoice]
                WHERE EXISTS (
                    SELECT *
                    FROM [InvoiceItem]
                    WHERE [InvoiceItem].[InvoiceId] = [Invoice].[Id] AND [InvoiceItem].[IsPhantom] = 0
                ) AND [Invoice].[IsPhantom] = 0
                ORDER BY [Invoice].[Id] ASC;
           **********************************************************************/

            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var item1 = new InvoiceItem();
                var invoice = new Invoice()
                {
                    InvoiceItemList =
                    {
                        item1
                    }
                };

                repo.Save(invoice);
                Assert.AreEqual(repo.GetInvoiceByHasItem().Count, 1);

                item1.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(invoice);
                Assert.AreEqual(repo.GetInvoiceByHasItem().Count, 0, "删除后，Exists子查询添加过滤条件，数量为0");
            }
        }

        [TestMethod]
        public void EPT_Clear_Query()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var InvoiceList = new InvoiceList
                {
                    new Invoice(),
                    new Invoice(),
                    new Invoice()
                };
                repo.Save(InvoiceList);
                Assert.AreEqual(repo.CountAll(), 3);

                InvoiceList.Clear();
                repo.Save(InvoiceList);
                Assert.AreEqual(repo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                var all = repo.GetAll();
                Assert.AreEqual(all.Count, 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");

                using (PhantomContext.DontFilterPhantoms())
                {
                    Assert.AreEqual(repo.CountAll(), 3, "幽灵状态的实体，可以使用特定 API 查出。");
                    var all2 = repo.GetAll();
                    Assert.AreEqual(all2.Count, 3, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(all2[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(all2[1]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(all2[2]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                }
            }
        }

        [TestMethod]
        public void EPT_Aggt()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            var itemRepo = RF.ResolveInstance<InvoiceItemRepository>();
            using (RF.TransactionScope(repo))
            {
                var item = new Invoice
                {
                    InvoiceItemList =
                    {
                        new InvoiceItem(),
                        new InvoiceItem(),
                    }
                };

                repo.Save(item);

                Assert.AreEqual(1, repo.CountAll());
                Assert.AreEqual(2, itemRepo.CountAll());

                item.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(item);

                Assert.AreEqual(repo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                Assert.AreEqual(itemRepo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");

                using (PhantomContext.DontFilterPhantoms())
                {
                    Assert.AreEqual(repo.CountAll(), 1, "幽灵状态的实体，可以使用特定 API 查出。");
                    var roots = repo.GetAll();
                    Assert.AreEqual(roots.Count, 1, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(roots[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");

                    Assert.AreEqual(itemRepo.CountAll(), 2, "幽灵状态的实体，可以使用特定 API 查出。");
                    var items = itemRepo.GetAll();
                    Assert.AreEqual(items.Count, 2, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(items[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                }
            }
        }

        [TestMethod]
        public void EPT_BatchImport()
        {
            int size = EntityTest.BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new InvoiceList();
                for (int i = 0; i < size; i++)
                {
                    var item = new Invoice();
                    list.Add(item);
                }
                repo.CreateImporter().Save(list);

                list.Clear();
                repo.CreateImporter().Save(list);

                Assert.AreEqual(repo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");

                using (PhantomContext.DontFilterPhantoms())
                {
                    Assert.AreEqual(repo.CountAll(), size, "幽灵状态的实体，可以使用特定 API 查出。");
                    var all2 = repo.GetAll();
                    Assert.AreEqual(all2.Count, size, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(all2[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                }
            }
        }

        [TestMethod]
        public void EPT_BatchImport_Aggt()
        {
            int size = EntityTest.BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<InvoiceRepository>();
            var itemRepo = RF.ResolveInstance<InvoiceItemRepository>();
            using (RF.TransactionScope(repo))
            {
                var invoices = new InvoiceList();
                for (int i = 0; i < size; i++)
                {
                    var Invoice = new Invoice
                    {
                        InvoiceItemList =
                        {
                            new InvoiceItem(),
                            new InvoiceItem(),
                        }
                    };
                    invoices.Add(Invoice);
                }

                var importer = repo.CreateImporter();
                importer.Save(invoices);

                Assert.AreEqual(size, repo.CountAll());
                Assert.AreEqual(size * 2, itemRepo.CountAll());

                invoices.Clear();
                importer.Save(invoices);

                Assert.AreEqual(repo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                Assert.AreEqual(itemRepo.CountAll(), 0, "幽灵状态的实体，应该无法通过正常的 API 查出。");

                using (PhantomContext.DontFilterPhantoms())
                {
                    Assert.AreEqual(repo.CountAll(), size, "幽灵状态的实体，可以使用特定 API 查出。");
                    var roots = repo.GetAll();
                    Assert.AreEqual(roots.Count, size, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(roots[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");

                    Assert.AreEqual(itemRepo.CountAll(), size * 2, "幽灵状态的实体，可以使用特定 API 查出。");
                    var items = itemRepo.GetAll();
                    Assert.AreEqual(items.Count, size * 2, "幽灵状态的实体，应该无法通过正常的 API 查出。");
                    Assert.AreEqual(EntityPhantomExtension.GetIsPhantom(items[0]), true, "幽灵状态的实体，IsPhantom 值为 true。");
                }
            }
        }
    }
}
