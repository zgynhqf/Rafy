/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160413 16:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.SerialNumber;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.BatchSubmit;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.Serialization;
using Rafy.Domain.Serialization.Json;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.Reflection;
using Rafy.UnitTest.IDataProvider;
using Rafy.UnitTest.Repository;
using Rafy.Utils;
using UT;
using Rafy.Accounts;
using Rafy.Accounts.Controllers;
using System.Threading;
using System.Threading.Tasks;
using System.Data;

namespace RafyUnitTest
{
    [TestClass]
    public class AccountsPluginTest
    {
        [ClassInitialize]
        public static void APT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        //[TestMethod]
        //public void APT_Transaction_IsolationLevel()
        //{
        //    var repo = RF.ResolveInstance<UserRepository>();
        //    //using (RF.TransactionScope(repo))
        //    //{
        //    //    var controller = DomainControllerFactory.Create<AccountController>();
        //    //    var res = controller.Register(new User
        //    //    {
        //    //        UserName = "hqf",
        //    //        RealName = "hqf",
        //    //        Password = controller.EncodePassword("hqf")
        //    //    });
        //    //}

        //    var db = RdbDataProvider.Get(repo).DbSetting;

        //    TaskFactory factory = new TaskFactory();
        //    //EventWaitHandle event1 = new AutoResetEvent(false);
        //    //EventWaitHandle event2 = new AutoResetEvent(false);
        //    bool let1Start = true, let1Save= true, let1Complete= false;
        //    bool let2Start = true, let2Query= false, let2Save= false, let2Complete= false;
        //    IsolationLevel level = IsolationLevel.ReadUncommitted;
        //    String rn1, rn2;
        //    var task1 = factory.StartNew(() =>
        //    {
        //        rn1 = Thread.CurrentThread.ManagedThreadId.ToString();

        //        using(var tran = RF.TransactionScope(db, level))
        //        {
        //            while(!let1Start)
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            var user = repo.GetByUserName("hqf");
        //            user.RealName = rn1;
        //            while(!let1Save)
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            repo.Save(user);

        //            while(!let1Complete)
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            tran.Complete();
        //        }
        //    });
        //    var task2 = factory.StartNew(() =>
        //    {
        //        rn2 = Thread.CurrentThread.ManagedThreadId.ToString();
        //        while(!let2Start)
        //        {
        //            Thread.Sleep(1000);
        //        }
        //        using(var tran = RF.TransactionScope(db, level))
        //        {
        //            while(!let2Query)
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            var user = repo.GetByUserName("hqf");
        //            user.RealName = rn2;
        //            while(!let2Save)
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            repo.Save(user);
        //            while(!let2Complete)
        //            {
        //                Thread.Sleep(1000);
        //            }
        //            tran.Complete();
        //        }
        //    });
        //    task1.Wait();
        //    task2.Wait();
        //}

        [TestMethod]
        public void APT_Register_Success()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();

                var res = controller.Register(new User
                {
                    UserName = "hqf",
                    RealName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });

                Assert.IsTrue(res.Success);
            }
        }

        [TestMethod]
        public void APT_Register_Validate_Password()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();

                var res = controller.Register(new User
                {
                    UserName = "hqf",
                    Email = "9474649@qq.com",
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterPropertiesInvalid, res.StatusCode);
            }
        }

        [TestMethod]
        public void APT_Register_Identity_UserName()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();

                var res = controller.Register(new User());
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterUserNameInvalid, res.StatusCode);

                res = controller.Register(new User
                {
                    UserName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsTrue(res.Success);

                res = controller.Register(new User
                {
                    UserName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterUserNameDuplicated, res.StatusCode);
            }
        }

        [TestMethod]
        public void APT_Register_Identity_Email()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();
                controller.IdentityMode = UserIdentityMode.Email;

                var res = controller.Register(new User());
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterEmailInvalid, res.StatusCode);

                res = controller.Register(new User
                {
                    Email = "@@@@@",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterEmailInvalid, res.StatusCode);

                res = controller.Register(new User
                {
                    Email = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsTrue(res.Success);

                res = controller.Register(new User
                {
                    Email = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterEmailDuplicated, res.StatusCode);
            }
        }

        [TestMethod]
        public void APT_Register_Identity_EmailAndUserName()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();
                controller.IdentityMode = UserIdentityMode.Email | UserIdentityMode.UserName;

                var res = controller.Register(new User());
                Assert.IsFalse(res.Success);

                res = controller.Register(new User
                {
                    Email = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterUserNameInvalid, res.StatusCode);

                res = controller.Register(new User
                {
                    UserName = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterEmailInvalid, res.StatusCode);

                res = controller.Register(new User
                {
                    UserName = "9474649@qq.com",
                    Email = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsTrue(res.Success);

                res = controller.Register(new User
                {
                    UserName = "XXXXXXXXXXX",
                    Email = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterEmailDuplicated, res.StatusCode);

                res = controller.Register(new User
                {
                    UserName = "9474649@qq.com",
                    Email = "111111111@qq.com",
                    Password = controller.EncodePassword("hqf")
                });
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.RegisterUserNameDuplicated, res.StatusCode);
            }
        }

        [TestMethod]
        public void APT_Login_Success_Email()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();

                controller.Register(new User
                {
                    UserName = "hqf",
                    Email = "9474649@qq.com",
                    Password = controller.EncodePassword("hqf")
                });

                User user = null;
                var res = controller.LoginByEmail("9474649@qq.com", "hqf", out user);
                Assert.IsTrue(res.Success);
                Assert.IsNotNull(user);
                Assert.AreEqual("hqf", user.UserName);
                Assert.AreEqual("9474649@qq.com", user.Email);
                Assert.AreEqual(controller.EncodePassword("hqf"), user.Password);
            }
        }
        [TestMethod]
        public void APT_Login_Success()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();

                controller.Register(new User
                {
                    UserName = "hqf",
                    RealName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });

                User user = null;
                var res = controller.LoginByUserName("hqf", "hqf", out user);
                Assert.IsTrue(res.Success);
                Assert.IsNotNull(user);
                Assert.AreEqual("hqf", user.UserName);
                Assert.AreEqual("hqf", user.RealName);
                Assert.AreEqual(controller.EncodePassword("hqf"), user.Password);
            }
        }

        [TestMethod]
        public void APT_Login_Failed()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();

                repo.Save(new User
                {
                    UserName = "hqf",
                    RealName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });

                User user = null;
                var res = controller.LoginByUserName("hqf", "hqf1", out user);
                Assert.IsFalse(res.Success);
                Assert.IsNotNull(user);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
            }
        }

        [TestMethod]
        public void APT_Login_Failed_MaxLoginTimes()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();
                controller.MaxLoginFailedTimes = 3;

                repo.Save(new User
                {
                    UserName = "hqf",
                    RealName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });

                User user = null;
                var res = controller.LoginByUserName("hqf", "hqf1", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
                Assert.AreEqual(1, user.LoginFailedTimes);

                res = controller.LoginByUserName("hqf", "hqf2", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
                Assert.AreEqual(2, user.LoginFailedTimes);

                res = controller.LoginByUserName("hqf", "hqf3", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
                Assert.AreEqual(3, user.LoginFailedTimes);

                res = controller.LoginByUserName("hqf", "hqf4", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginExceedMaxFailedTimes, res.StatusCode, "最多只能输入3次密码错误。");
                Assert.AreEqual(3, user.LoginFailedTimes);

                res = controller.LoginByUserName("hqf", "hqf", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginExceedMaxFailedTimes, res.StatusCode, "最多只能输入3次密码错误。");
                Assert.AreEqual(3, user.LoginFailedTimes);
            }
        }

        [TestMethod]
        public void APT_Login_Failed_UserDisabled()
        {
            var repo = RF.ResolveInstance<UserRepository>();
            using (RF.TransactionScope(repo))
            {
                var controller = DomainControllerFactory.Create<AccountController>();
                controller.MaxLoginFailedTimes = 3;

                repo.Save(new User
                {
                    UserName = "hqf",
                    RealName = "hqf",
                    Password = controller.EncodePassword("hqf")
                });

                User user = null;
                var res = controller.LoginByUserName("hqf", "hqf", out user);
                Assert.IsTrue(res.Success);

                user.IsDisabled = true;
                repo.Save(user);

                res = controller.LoginByUserName("hqf", "hqf", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginUserDisabled, res.StatusCode, "用户已经被禁用。");
            }
        }
    }
}
