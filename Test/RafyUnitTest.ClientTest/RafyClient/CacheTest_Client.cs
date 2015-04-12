/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130319 15:54
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130319 15:54
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

namespace RafyUnitTest.ClientTest
{
    /// <summary>
    /// 这个测试虽然是测试客户端的缓存，但是由于没有真正的服务端，所以同样需要直接连接数据库。
    /// </summary>
    [TestClass]
    public class CacheTest_Client
    {
        [ClassInitialize]
        public static void CacheTest_Client_ClassInitialize(TestContext context)
        {
            ClientTestHelper.ClassInitialize(context);

            Logger.DbAccessed += Logger_DbAccessed;
        }

        [ClassCleanup]
        public static void CacheTest_Client_ClassCleanup()
        {
            Logger.DbAccessed -= Logger_DbAccessed;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            RF.Concrete<PBSTypeRepository>().ClientCache.ClearOnClient();
            RF.Concrete<PBSRepository>().ClientCache.ClearOnClient();
            VersionSyncMgr.Repository.Clear();
        }

        #region 统计非缓存的数据查询。

        private static int DbAccessedCount;

        private static void Logger_DbAccessed(object sender, Logger.DbAccessedEventArgs e)
        {
            if (e.ConnectionSchema.Database == UnitTestEntityRepositoryDataProvider.DbSettingName)
            {
                DbAccessedCount++;
            }
        }

        #endregion

        [TestMethod]
        public void CT_CTC_CacheAll()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
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

                var pbsRepo = RF.Concrete<PBSRepository>();

                var list1 = pbsRepo.CacheAll();
                Assert.IsTrue(list1.Count == 5);

                DbAccessedCount = 0;
                var list2 = pbsRepo.CacheAll();
                Assert.IsTrue(list2.Count == 5);
                Assert.IsTrue(DbAccessedCount == 0, "GetAll 内存缓存应该命中，不会发生数据层访问。");
            }
        }

        [TestMethod]
        public void CT_CTC_CacheByParentId()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
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

                DbAccessedCount = 0;
                var type2 = repo.CacheById(id) as PBSType;
                var list2 = type2.PBSList;
                Assert.IsTrue(DbAccessedCount == 0, "GetByParentId 内存缓存应该命中，不会发生数据层访问。");

                Assert.IsTrue(list2.Count == 5);
            }
        }

        [TestMethod]
        public void CT_CTC_CacheById()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                var pbsType = new PBSType
                {
                    Name = "PBSType1"
                };
                repo.Save(pbsType);
                var id = pbsType.Id;

                var type1 = repo.CacheById(id) as PBSType;

                DbAccessedCount = 0;
                var type2 = repo.CacheById(id) as PBSType;
                Assert.IsTrue(DbAccessedCount == 0, "GetById 内存缓存应该命中，不会发生数据层访问。");

                Assert.IsTrue(type1 != type2, "虽然是从缓存中获取，但是只缓存数据，所以对象不应该是同一个。");
            }
        }

        //[TestMethod]
        //public void CTS_GetByParentId()
        //{
        //    var repo = RF.Concrete<PBSTypeRepository>();
        //    using (RF.TransactionScope(repo))
        //    {
        //        var pbsType = new PBSType
        //        {
        //            PBSList =
        //            {
        //                new PBS { Name = "PBS1" },
        //                new PBS { Name = "PBS2" },
        //                new PBS { Name = "PBS3" },
        //                new PBS { Name = "PBS4" },
        //                new PBS { Name = "PBS5" },
        //                new PBS { Name = "PBS6" },
        //                new PBS { Name = "PBS7" },
        //            }
        //        };
        //        repo.Save(pbsType);
        //        var testTypeId = pbsType.Id;

        //        var type = repo.GetById(testTypeId) as PBSType;

        //        var pbss = type.PBSList;

        //        var type2 = repo.GetById(testTypeId) as PBSType;
        //        var pbss2 = type2.PBSList;

        //        var type3 = repo.GetById(testTypeId).CastTo<PBSType>();
        //        var pbss3 = type3.PBSs;
        //        (pbss3[0] as PBS).Description += " 1";
        //        type3.Save();

        //        var type4 = repo.GetById(testTypeId).CastTo<PBSType>();
        //        var pbss4 = type4.PBSs;
        //    }
        //}

        //[TestMethod]
        //public void CacheTest_GetAll_GetById()
        //{
        //    var pbsRepository = RF.Create<PBS>();
        //    var pbss = pbsRepository.GetAll();
        //    PBS pbs = null;
        //    if (pbss.Count > 0)
        //    {
        //        pbs = pbsRepository.GetById(pbss[0].Id) as PBS;
        //        var pbs2 = pbsRepository.GetById(pbss[0].Id);

        //        Assert.IsTrue(pbs == pbs2);
        //    }


        //    pbss = ELM.GetAll<PBSs>();
        //    var pbss2 = ELM.GetAll<PBSs>();
        //    pbs = EM<PBS>.GetById(pbss[0].Id);
        //}

        //[TestMethod]
        //public void CacheTest_CacheProvider()
        //{
        //    var pbsTypes = ELM.GetAll<PBSTypeList>();

        //    var provider = new SQLCompactProvider(PathHelper.ToAbsolute("Cache.sdf"));

        //    provider.Clear();

        //    provider.Add("key", pbsTypes, new Policy()
        //    {
        //        Checker = new VersionChecker(typeof(PBS))
        //    }, "region");

        //    var pbsTypes2 = provider.GetCacheItem("key", "region");
        //}

        //[TestMethod]
        //public void CacheTest_AggregateRoot()
        //{
        //    var pbsTypes = ELM.GetAll<PBSTypeList>();

        //    if (pbsTypes.Count <= 0) return;

        //    var root1 = EM<PBSType>.GetById(pbsTypes[0].Id);
        //    var root2 = EM<PBSType>.GetById(pbsTypes[0].Id);

        //    Assert.AreSame(root1, root2);
        //}
    }
}
