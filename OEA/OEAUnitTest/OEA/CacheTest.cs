//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using GIX4.Library;
//using OEA;
//using OEA.Library.Caching;
//using OEA.MetaModel;
//using OEA.Library;

//namespace OEAUnitTest
//{
//    /// <summary>
//    /// 注意，由于缓存对上层透明，这里较难使用Assert判断测试结果。只能人工观察数据访问次数。
//    /// </summary>
//    [TestClass]
//    public class CacheTest : TestBase
//    {
//        [ClassInitialize]
//        public static void CacheTest_ClassInitialize(TestContext context)
//        {
//            ClassInitialize(context);

//            CacheScope scope;
//            if (!CacheDefinition.Instance.TryGetScope(typeof(PBSType), out scope))
//            {
//                CacheDefinition.Instance.Enable<PBSType>();
//            }
//        }

//        [TestMethod]
//        public void CacheTest_GetByParentId()
//        {
//            var testTypeId = new Guid("8B792F18-C5D8-40CC-A6D0-4C8D02474D4B");

//            var repo = RF.Create<PBSType>();

//            var type = repo.GetById(testTypeId) as PBSType;
//            if (type == null) return;

//            var pbss = type.PBSs;

//            var type2 = repo.GetById(testTypeId) as PBSType;
//            var pbss2 = type2.PBSs;

//            var type3 = repo.GetById(testTypeId).CastTo<PBSType>();
//            var pbss3 = type3.PBSs;
//            (pbss3[0] as PBS).Description += " 1";
//            type3.Save();

//            var type4 = repo.GetById(testTypeId).CastTo<PBSType>();
//            var pbss4 = type4.PBSs;
//        }

//        [TestMethod]
//        public void CacheTest_GetAll_GetById()
//        {
//            var pbsRepository = RF.Create<PBS>();
//            var pbss = pbsRepository.GetAll();
//            PBS pbs = null;
//            if (pbss.Count > 0)
//            {
//                pbs = pbsRepository.GetById(pbss[0].Id) as PBS;
//                var pbs2 = pbsRepository.GetById(pbss[0].Id);

//                Assert.IsTrue(pbs == pbs2);
//            }


//            pbss = ELM.GetAll<PBSs>();
//            var pbss2 = ELM.GetAll<PBSs>();
//            pbs = EM<PBS>.GetById(pbss[0].Id);
//        }

//        [TestMethod]
//        public void CacheTest_CacheProvider()
//        {
//            var pbsTypes = ELM.GetAll<PBSTypeList>();

//            var provider = new SQLCompactProvider(PathHelper.ToAbsolute("Cache.sdf"));

//            provider.Clear();

//            provider.Add("key", pbsTypes, new Policy()
//            {
//                Checker = new VersionChecker(typeof(PBS))
//            }, "region");

//            var pbsTypes2 = provider.GetCacheItem("key", "region");
//        }

//        [TestMethod]
//        public void CacheTest_AggregateRoot()
//        {
//            var pbsTypes = ELM.GetAll<PBSTypeList>();

//            if (pbsTypes.Count <= 0) return;

//            var root1 = EM<PBSType>.GetById(pbsTypes[0].Id);
//            var root2 = EM<PBSType>.GetById(pbsTypes[0].Id);

//            Assert.AreSame(root1, root2);
//        }
//    }
//}
