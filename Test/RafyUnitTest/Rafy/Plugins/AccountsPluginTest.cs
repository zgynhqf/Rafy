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
    public class AccountsPluginTest : Rafy.UnitTest.AccountsPluginTest
    {
        [ClassInitialize]
        public static void APT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);

            APT_ClassInitialize_Base();
        }

        [TestMethod]
        public override void APT_Register_Success()
        {
            base.APT_Register_Success();
        }

        [TestMethod]
        public override void APT_Register_Validate_Password()
        {
            base.APT_Register_Validate_Password();
        }

        [TestMethod]
        public override void APT_Register_Identity_UserName()
        {
            base.APT_Register_Identity_UserName();
        }

        [TestMethod]
        public override void APT_Register_Identity_Email()
        {
            base.APT_Register_Identity_Email();
        }

        [TestMethod]
        public override void APT_Register_Identity_EmailAndUserName()
        {
            base.APT_Register_Identity_EmailAndUserName();
        }

        [TestMethod]
        public override void APT_Login_Success_Email()
        {
            base.APT_Login_Success_Email();
        }

        [TestMethod]
        public override void APT_Login_Success()
        {
            base.APT_Login_Success();
        }

        [TestMethod]
        public override void APT_Login_Failed()
        {
            base.APT_Login_Failed();
        }

        [TestMethod]
        public override void APT_Login_Failed_MaxLoginTimes()
        {
            base.APT_Login_Failed_MaxLoginTimes();
        }

        [TestMethod]
        public override void APT_Login_Failed_UserDisabled()
        {
            base.APT_Login_Failed_UserDisabled();
        }
    }
}
