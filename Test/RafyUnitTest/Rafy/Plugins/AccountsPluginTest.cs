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

        /// <summary>
        /// 成功登录。
        /// </summary>
        [TestMethod]
        public void APT_Login_Success()
        {
            var repo = RF.Concrete<UserRepository>();
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
                var res = controller.Login("hqf", "hqf", out user);
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
            var repo = RF.Concrete<UserRepository>();
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
                var res = controller.Login("hqf", "hqf1", out user);
                Assert.IsFalse(res.Success);
                Assert.IsNotNull(user);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
            }
        }

        [TestMethod]
        public void APT_Login_Failed_MaxLoginTimes()
        {
            var repo = RF.Concrete<UserRepository>();
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
                var res = controller.Login("hqf", "hqf1", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
                Assert.AreEqual(1, user.LoginFailedTimes);

                res = controller.Login("hqf", "hqf2", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
                Assert.AreEqual(2, user.LoginFailedTimes);

                res = controller.Login("hqf", "hqf3", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginPasswordError, res.StatusCode);
                Assert.AreEqual(3, user.LoginFailedTimes);

                res = controller.Login("hqf", "hqf4", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginExceedMaxFailedTimes, res.StatusCode, "最多只能输入3次密码错误。");
                Assert.AreEqual(3, user.LoginFailedTimes);

                res = controller.Login("hqf", "hqf", out user);
                Assert.IsFalse(res.Success);
                Assert.AreEqual(ResultCodes.LoginExceedMaxFailedTimes, res.StatusCode, "最多只能输入3次密码错误。");
                Assert.AreEqual(3, user.LoginFailedTimes);
            }
        }
    }
}
