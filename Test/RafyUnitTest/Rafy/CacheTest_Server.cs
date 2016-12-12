/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel;
using Rafy.Utils.Caching;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class CacheTest_Server
    {
        [ClassInitialize]
        public static void CacheTest_Server_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            CacheInstances.Memory.Clear();
        }

        [TestMethod]
        public void CTS_CacheInstance()
        {
            var cache = CacheInstances.Memory;
            cache.Add("TestKey", "TestContent", Policy.Empty);
            Assert.AreEqual(cache.Get("TestKey"), "TestContent");

            cache = CacheInstances.Disk;
            cache.Add("TestKey", "TestContent", Policy.Empty);
            Assert.AreEqual(cache.Get("TestKey"), "TestContent");

            cache = CacheInstances.MemoryDisk;
            cache.Add("TestKey2", "TestContent", Policy.Empty);
            Assert.AreEqual(cache.Get("TestKey2"), "TestContent");
        }

        [TestMethod]
        public void CTS_CacheAll()
        {
            var repo = RF.ResolveInstance<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new PBSType
                {
                    Name = "PBSType1",
                    PBSList =
                    {
                        new PBS { Name = "PBS1" },
                        new PBS { Name = "PBS2" },
                        new PBS { Name = "PBS3" },
                        new PBS { Name = "PBS4" },
                        new PBS { Name = "PBS5" },
                    }
                });

                var pbsRepo = RF.ResolveInstance<PBSRepository>();

                var list1 = pbsRepo.CacheAll();
                Assert.IsTrue(list1.Count == 5);

                var count = Logger.DbAccessedCount;
                var list2 = pbsRepo.CacheAll();
                Assert.IsTrue(list2.Count == 5);
                Assert.IsTrue(Logger.DbAccessedCount == count, "GetAll 内存缓存应该命中，不会发生数据层访问。");
            }
        }

        [TestMethod]
        public void CTS_CacheByParentId()
        {
            var repo = RF.ResolveInstance<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                var pbsType = new PBSType
                {
                    Name = "PBSType1",
                    PBSList =
                    {
                        new PBS { Name = "PBS1" },
                        new PBS { Name = "PBS2" },
                        new PBS { Name = "PBS3" },
                        new PBS { Name = "PBS4" },
                        new PBS { Name = "PBS5" },
                    }
                };
                repo.Save(pbsType);
                var id = pbsType.Id;

                var type1 = repo.CacheById(id) as PBSType;
                var list1 = type1.PBSList;
                Assert.IsTrue(list1.Count == 5);

                var count = Logger.DbAccessedCount;
                var type2 = repo.CacheById(id) as PBSType;//Cache By Parent Id
                var list2 = type2.PBSList;
                Assert.IsTrue(Logger.DbAccessedCount == count, "CacheByParentId 内存缓存应该命中，不会发生数据层访问。");

                Assert.IsTrue(list2.Count == 5);
            }
        }

        [TestMethod]
        public void CTS_CacheById()
        {
            var repo = RF.ResolveInstance<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                var pbsType = new PBSType
                {
                    Name = "PBSType1"
                };
                repo.Save(pbsType);
                var id = pbsType.Id;

                var type1 = repo.CacheById(id) as PBSType;

                var count = Logger.DbAccessedCount;
                var type2 = repo.CacheById(id) as PBSType;
                Assert.IsTrue(Logger.DbAccessedCount == count, "GetById 内存缓存应该命中，不会发生数据层访问。");

                Assert.IsTrue(type1 != type2, "虽然是从缓存中获取，但是只缓存数据，所以对象不应该是同一个。");
            }
        }

        [TestMethod]
        public void CTS_EntityContext()
        {
            var repo = RF.ResolveInstance<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                var pbsType = new PBSType
                {
                    Name = "PBSType1",
                    PBSList =
                    {
                        new PBS { Name = "PBS1" },
                        new PBS { Name = "PBS2" },
                        new PBS { Name = "PBS3" },
                        new PBS { Name = "PBS4" },
                        new PBS { Name = "PBS5" },
                    }
                };
                repo.Save(pbsType);
                var id = pbsType.Id;

                using (RF.EnterEntityContext())
                {
                    var type1 = repo.CacheById(id) as PBSType;

                    var count = Logger.DbAccessedCount;
                    var type2 = repo.CacheById(id) as PBSType;
                    Assert.IsTrue(Logger.DbAccessedCount == count, "GetById 内存缓存应该命中，不会发生数据层访问。");

                    Assert.IsTrue(type1 == type2, "由于使用了 EntityContext，从缓存中读取的对象，也应该是同一个。");

                    var list1 = type1.PBSList;
                    Assert.IsTrue(list1.Count == 5);

                    count = Logger.DbAccessedCount;
                    var list2 = type2.PBSList;
                    Assert.IsTrue(Logger.DbAccessedCount == count, "GetByParentId 内存缓存应该命中，不会发生数据层访问。");

                    foreach (var pbs1 in list1)
                    {
                        var pbs2 = list2.Find(pbs1.Id);
                        Assert.IsTrue(pbs1 == pbs2, "由于使用了 EntityContext，从缓存中读取的对象，也应该是同一个。");
                    }
                }
            }
        }
    }
}