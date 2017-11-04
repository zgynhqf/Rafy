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
    }
}
