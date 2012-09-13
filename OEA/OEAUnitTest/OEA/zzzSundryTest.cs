///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120828 11:44
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120828 11:44
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using OEA;
//using OEA.Library;
//using OEA.Library._Test;
//using OEA.Reflection;
//using OEA.ORM.DbMigration;

//namespace OEAUnitTest
//{
//    [TestClass]
//    public class SundryTest : TestBase
//    {
//        [ClassInitialize]
//        public static void SundryTest_ClassInitialize(TestContext context)
//        {
//            OEAEnvironment.Location = OEALocation.LocalVersion;

//            ClassInitialize(context, true);
//        }

//        [TestMethod]
//        public void SundryTest_SplitLocalVersionAsClientAndServer()
//        {
//            Assert.AreEqual(OEAEnvironment.Location, OEALocation.LocalVersion);

//            Assert.IsTrue(OEAEnvironment.IsOnClient());
//            Assert.IsTrue(!OEAEnvironment.IsOnServer());
//            Assert.AreEqual(OEAEnvironment.ThreadPortalCount, 0);

//            var svc = new SundryTest_SplitLocalVersionAsClientAndServerService();
//            svc.Invoke();
//        }

//        [Serializable]
//        private class SundryTest_SplitLocalVersionAsClientAndServerService : Service
//        {
//            protected override void Execute()
//            {
//                Assert.AreEqual(OEAEnvironment.Location, OEALocation.LocalVersion);

//                Assert.IsTrue(!OEAEnvironment.IsOnClient());
//                Assert.IsTrue(OEAEnvironment.IsOnServer());
//                Assert.AreEqual(OEAEnvironment.ThreadPortalCount, 1);
//            }
//        }
//    }
//}