/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151207
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151207 16:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Stamp;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class StampTest
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void StampT_PropertyRegistered()
        {
            var inv = new Invoice();
            Assert.IsNotNull(inv.GetCreatedTime(), "属性可以正常的获取到，说明已经注册在实体上了。");
            Assert.IsNotNull(inv.GetUpdatedTime(), "属性可以正常的获取到，说明已经注册在实体上了。");
            Assert.AreEqual(inv.GetCreatedUser(), null, "属性可以正常的获取到，说明已经注册在实体上了。");
            Assert.AreEqual(inv.GetUpdatedUser(), null, "属性可以正常的获取到，说明已经注册在实体上了。");
        }

        [TestMethod]
        public void StampT_Insert_CreatedTime()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var inv = new Invoice();

                repo.Save(inv);
                Assert.IsTrue(inv.GetCreatedTime().Date == DateTime.Today, "创建时间 属性已经正确的设置。");

                inv = repo.GetById(inv.Id);
                Assert.IsTrue(inv.GetCreatedTime().Date == DateTime.Today, "创建时间 属性已经正确的设置。");
            }
        }

        [TestMethod]
        public void StampT_Insert_CreatedUser()
        {
            RafyEnvironment.Principal = null;

            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var inv = new Invoice();

                repo.Save(inv);
                Assert.AreEqual(inv.GetCreatedUser(), null, "还没有登录时，创建人属性应该是空的。");

                inv = repo.GetById(inv.Id);
                Assert.AreEqual(inv.GetCreatedUser(), null, "还没有登录时，创建人属性应该是空的。");
            }
        }

        [TestMethod]
        public void StampT_Insert_CreatedUser_Logined()
        {
            var oldPrincipal = RafyEnvironment.Principal;

            try
            {
                var userName = "test user";
                RafyEnvironment.Principal = new GenericPrincipal(new GenericIdentity(userName), null);

                var repo = RF.ResolveInstance<InvoiceRepository>();
                using (RF.TransactionScope(repo))
                {
                    var inv = new Invoice();

                    repo.Save(inv);
                    Assert.AreEqual(inv.GetCreatedUser(), userName, "登录后，创建人属性应该正确的设置上。");

                    inv = repo.GetById(inv.Id);
                    Assert.AreEqual(inv.GetCreatedUser(), userName, "登录后，创建人属性应该正确的设置上。");
                }
            }
            finally
            {
                RafyEnvironment.Principal = oldPrincipal;
            }
        }

        [TestMethod]
        public void StampT_Update_UpdatedTime()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var inv = new Invoice();

                repo.Save(inv);
                Assert.IsTrue(inv.GetUpdatedTime().Date == DateTime.Today, "刚创建的对象，需要正确设置‘最后更新时间’属性。");

                inv = repo.GetById(inv.Id);
                inv.Code = "Code Modified";
                repo.Save(inv);

                Assert.IsTrue(inv.GetUpdatedTime().Date == DateTime.Today, "实体更新并保存后，需要正确设置‘最后更新时间’属性。");
                inv = repo.GetById(inv.Id);
                Assert.IsTrue(inv.GetUpdatedTime().Date == DateTime.Today, "实体更新并保存后，需要正确设置‘最后更新时间’属性。");
            }
        }

        [TestMethod]
        public void StampT_Update_UpdatedUser()
        {
            RafyEnvironment.Principal = null;

            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var inv = new Invoice();
                repo.Save(inv);

                inv = repo.GetById(inv.Id);
                inv.Code = "Code Modified";
                repo.Save(inv);

                Assert.AreEqual(inv.GetUpdatedUser(), null, "还没有登录时，最后更新人属性应该是空的。");
                inv = repo.GetById(inv.Id);
                Assert.AreEqual(inv.GetUpdatedUser(), null, "还没有登录时，最后更新人属性应该是空的。");
            }
        }

        [TestMethod]
        public void StampT_Update_UpdatedUser_Logined()
        {
            var oldPrincipal = RafyEnvironment.Principal;

            try
            {
                var userName = "test user";
                RafyEnvironment.Principal = new GenericPrincipal(new GenericIdentity(userName), null);

                var repo = RF.ResolveInstance<InvoiceRepository>();
                using (RF.TransactionScope(repo))
                {
                    var inv = new Invoice();

                    repo.Save(inv);
                    Assert.AreEqual(inv.GetUpdatedUser(), userName, "登录后，创建人属性应该正确的设置上。");

                    inv = repo.GetById(inv.Id);
                    inv.Code = "Code Modified";
                    repo.Save(inv);

                    Assert.AreEqual(inv.GetUpdatedUser(), userName, "登录后，创建人属性应该正确的设置上。");
                    inv = repo.GetById(inv.Id);
                    Assert.AreEqual(inv.GetUpdatedUser(), userName, "登录后，创建人属性应该正确的设置上。");
                }
            }
            finally
            {
                RafyEnvironment.Principal = oldPrincipal;
            }
        }

        [TestMethod]
        public void StampT_Disable()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var inv = new Invoice();

                using (StampContext.DisableAutoSetStamps())
                {
                    repo.Save(inv);
                    Assert.AreEqual(DateTime.Parse("2000-01-01 00:00:00"), inv.GetCreatedTime().Date, "被禁用，创建时间 属性已经正确的设置。");

                    inv = repo.GetById(inv.Id);
                    Assert.AreEqual(DateTime.Parse("2000-01-01 00:00:00"), inv.GetCreatedTime().Date, "被禁用，创建时间 属性已经正确的设置。");
                }
            }
        }
    }
}
