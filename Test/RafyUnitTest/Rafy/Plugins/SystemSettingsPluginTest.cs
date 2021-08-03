/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20171104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20171104 14:35
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
using Rafy.SystemSettings;
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
using Rafy.SystemSettings.Controllers;

namespace RafyUnitTest
{
    [TestClass]
    public class SystemSettingsPluginTest
    {
        [ClassInitialize]
        public static void SSPT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);

            RafyEnvironment.LoadPlugin(typeof(SystemSettingsPlugin).Assembly);
        }

        [TestMethod]
        public void SSPT_GetValue()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                try
                {
                    controller.GetValue(key);
                    Assert.IsTrue(false, "不存在，应该抛出异常。");
                }
                catch (InvalidProgramException) { }

                repo.Save(new GlobalSetting
                {
                    Key = key,
                    Value = "1"
                });

                var value = controller.GetValue(key);
                Assert.AreEqual("1", value);

                try
                {
                    Assert.AreEqual(1, value);
                    Assert.IsTrue(false, "两个值的类型不一致，应该抛出异常。");
                }
                catch (Exception) { }
            }
        }

        [TestMethod]
        public void SSPT_GetValue_T()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                try
                {
                    controller.GetValue<int>(key);
                    Assert.IsTrue(false, "不存在，应该抛出异常。");
                }
                catch (InvalidProgramException)
                {
                }

                repo.Save(new GlobalSetting
                {
                    Key = key,
                    Value = "1"
                });

                var value = controller.GetValue<int>(key);
                Assert.AreEqual(1, value);
            }
        }

        [TestMethod]
        public void SSPT_GetValueOrDefault()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                var value = controller.GetValueOrDefault(key, 1);

                Assert.AreEqual(1, value);

                repo.Save(new GlobalSetting
                {
                    Key = key,
                    Value = "2"
                });

                value = controller.GetValueOrDefault(key, 1);
                Assert.AreEqual(2, value);
            }
        }

        [TestMethod]
        public void SSPT_SetValue()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                repo.Save(new GlobalSetting
                {
                    Key = key,
                    Value = "2"
                });

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                controller.SetValue(key, 3);

                var item = repo.GetByKey(key);
                Assert.IsNotNull(item);
                Assert.AreEqual("3", item.Value);
            }
        }

        [TestMethod]
        public void SSPT_SetValue_CreateOnNull()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                var item = repo.GetByKey(key);
                Assert.IsNull(item);

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                controller.SetValue(key, 3);

                item = repo.GetByKey(key);
                Assert.IsNotNull(item);
                Assert.AreEqual("3", item.Value);
            }
        }

        [TestMethod]
        public void SSPT_SetValue_WithDescription()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                repo.Save(new GlobalSetting
                {
                    Key = key,
                    Value = "2",
                    Description = "2"
                });

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                controller.SetValue(key, 3, "3");

                var item = repo.GetByKey(key);
                Assert.IsNotNull(item);
                Assert.AreEqual("3", item.Description);
            }
        }

        [TestMethod]
        public void SSPT_SetValue_WithoutDescription()
        {
            var repo = RF.ResolveInstance<GlobalSettingRepository>();
            using (RF.TransactionScope(repo))
            {
                var key = "SystemSettingsPluginTest_TestKey";

                repo.Save(new GlobalSetting
                {
                    Key = key,
                    Value = "2",
                    Description = "2"
                });

                var controller = DomainControllerFactory.Create<GlobalSettingController>();
                controller.SetValue(key, 3);

                var item = repo.GetByKey(key);
                Assert.IsNotNull(item);
                Assert.AreEqual("2", item.Description);
            }
        }
    }
}
