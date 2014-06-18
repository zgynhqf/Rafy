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
//using Rafy;
//using Rafy.Library;
//using Rafy.Library._Test;
//using Rafy.Reflection;
//using Rafy.Domain.ORM.DbMigration;

//namespace RafyUnitTest
//{
//    [TestClass]
//    public class SundryTest
//    {
//        [ClassInitialize]
//        public static void SundryTest_ClassInitialize(TestContext context)
//        {
//            RafyEnvironment.Location = RafyLocation.LocalVersion;

//            ServerTestHelper.ClassInitialize(context);
//        }

//        [TestMethod]
//        public void SundryTest_SplitLocalVersionAsClientAndServer()
//        {
//            Assert.AreEqual(RafyEnvironment.Location, RafyLocation.LocalVersion);

//            Assert.IsTrue(RafyEnvironment.IsOnClient());
//            Assert.IsTrue(!RafyEnvironment.IsOnServer());
//            Assert.AreEqual(RafyEnvironment.ThreadPortalCount, 0);

//            var svc = new SundryTest_SplitLocalVersionAsClientAndServerService();
//            svc.Invoke();
//        }

//        [Serializable]
//        private class SundryTest_SplitLocalVersionAsClientAndServerService : Service
//        {
//            protected override void Execute()
//            {
//                Assert.AreEqual(RafyEnvironment.Location, RafyLocation.LocalVersion);

//                Assert.IsTrue(!RafyEnvironment.IsOnClient());
//                Assert.IsTrue(RafyEnvironment.IsOnServer());
//                Assert.AreEqual(RafyEnvironment.ThreadPortalCount, 1);
//            }
//        }
//    }
//}