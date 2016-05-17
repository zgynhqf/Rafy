/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160318
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160318 10:18
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

namespace RafyUnitTest
{
    [TestClass]
    public class SerialNumberPluginTest
    {
        [ClassInitialize]
        public static void SNPT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        /// <summary>
        /// 测试最简单的使用场景。
        /// </summary>
        [TestMethod]
        public void SNPT_DailySerialNumberInfo()
        {
            using (RF.TransactionScope(SerialNumberPlugin.DbSettingName))
            {
                var name = "testACP";
                var controller = DomainControllerFactory.Create<SerialNumberController>();
                var sni = controller.CreateDailySerialNumberInfo(name);

                var today = DateTime.Today.ToString("yyyyMMdd");

                var next = controller.GenerateNext(name);
                Assert.AreEqual(today + "00000001", next);
                next = controller.GenerateNext(name);
                Assert.AreEqual(today + "00000002", next);
                next = controller.GenerateNext(sni);
                Assert.AreEqual(today + "00000003", next);
            }
        }

        /// <summary>
        /// 不同名字的规则，数据互相隔离，不会干扰。
        /// </summary>
        [TestMethod]
        public void SNPT_Seperation()
        {
            using (RF.TransactionScope(SerialNumberPlugin.DbSettingName))
            {
                var name1 = "testACP-1";
                var name2 = "testACP-2";
                var controller = DomainControllerFactory.Create<SerialNumberController>();
                var aci1 = controller.CreateDailySerialNumberInfo(name1);
                var aci2 = controller.CreateDailySerialNumberInfo(name2);

                var today = DateTime.Today.ToString("yyyyMMdd");

                var next = controller.GenerateNext(name1);
                Assert.AreEqual(today + "00000001", next);
                next = controller.GenerateNext(name2);
                Assert.AreEqual(today + "00000001", next);
                next = controller.GenerateNext(aci2);
                Assert.AreEqual(today + "00000002", next);
                next = controller.GenerateNext(aci2);
                Assert.AreEqual(today + "00000003", next);
                next = controller.GenerateNext(aci1);
                Assert.AreEqual(today + "00000002", next);
            }
        }

        /// <summary>
        /// 可以通过指定的时间来生成流水号。
        /// </summary>
        [TestMethod]
        public void SNPT_SpecificTime()
        {
            using (RF.TransactionScope(SerialNumberPlugin.DbSettingName))
            {
                var name = "testACP";

                var controller = DomainControllerFactory.Create<SerialNumberController>();
                var sni = controller.CreateDailySerialNumberInfo(name);

                var next = controller.GenerateNext(name, DateTime.Parse("2000-01-01"));
                Assert.AreEqual("2000010100000001", next);
                next = controller.GenerateNext(name, DateTime.Parse("2000-01-01"));
                Assert.AreEqual("2000010100000002", next);

                next = controller.GenerateNext(name, DateTime.Parse("2016-05-01"));
                Assert.AreEqual("2016050100000001", next);

                next = controller.GenerateNext(sni, DateTime.Parse("2016-05-02"));
                Assert.AreEqual("2016050200000001", next);
                next = controller.GenerateNext(sni, DateTime.Parse("2016-05-02"));
                Assert.AreEqual("2016050200000002", next);
                next = controller.GenerateNext(sni, DateTime.Parse("2016-05-02"));
                Assert.AreEqual("2016050200000003", next);

                var valueRepo = RF.Concrete<SerialNumberValueRepository>();
                var value = valueRepo.GetByTime(sni, DateTime.Parse("2016-05-02"));
                Assert.AreEqual(3, value.RollValue);
            }
        }
    }
}
