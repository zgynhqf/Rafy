/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140107
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140107 10:22
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.DbMigration;
using Rafy.UnitTest;
using Rafy.Utils;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class DomainTest
    {
        [ClassInitialize]
        public static void DT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void DT_Service_InterfaceContract()
        {
            var service = ServiceFactory.Create<ITestAddService>();
            service.A = 10;
            service.B = 20;
            service.Invoke();
            Assert.IsTrue(service.Result == 30);
        }

        /// <summary>
        /// 本方法用于测试：在接口的属性上标记的输入与输出，不需要再在服务上进行标记。
        /// </summary>
        [TestMethod]
        public void DT_Service_InterfaceContract_MarkPropertyOnInterface()
        {
            var service = new TestAddService();
            service.A = 10;
            service.B = 20;
            service.Invoke();
            Assert.IsTrue(service.Result == 30);
        }

        [TestMethod]
        public void DT_Service_Override()
        {
            var service = ServiceFactory.Create<AddBookService>();
            service.Invoke();
            Assert.IsTrue(service.Result == 3);
        }

        [TestMethod]
        public void DT_Service_Override_VersionSpecific()
        {
            var service = ServiceFactory.Create(typeof(AddBookService), new Version("1.0.0.2")) as AddBookService;
            service.Invoke();
            Assert.IsTrue(service.Result == 2);
        }

        [TestMethod]
        public void DT_ExtLiteDataTable_CanConvert()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("Id", typeof(int)));
            table.Columns.Add(new LiteDataColumn("Name", typeof(string)));
            table.Columns.Add(new LiteDataColumn("DecimalProperty1", typeof(decimal)));
            table.Columns.Add(new LiteDataColumn("DecimalProperty2", typeof(decimal)));
            table.Columns.Add(new LiteDataColumn("DecimalProperty3", typeof(decimal)));
            table.Columns.Add(new LiteDataColumn("CreatedTime", typeof(DateTime)));
            table.Columns.Add(new LiteDataColumn("CreatedUser", typeof(string)));
            table.Columns.Add(new LiteDataColumn("UpdatedTime", typeof(string)));
            table.Columns.Add(new LiteDataColumn("UpdatedUser", typeof(DateTime)));
            table.Columns.Add(new LiteDataColumn("IsPhantom", typeof(bool)));
            table.Columns.Add(new LiteDataColumn("TreeIndex", typeof(int)));
            table.Columns.Add(new LiteDataColumn("TreePId", typeof(int)));

            var row = table.NewRow();
            row["Id"] = 1;
            row["Name"] = "HuKang";
            row["DecimalProperty1"] = 1;
            row["DecimalProperty2"] = 2;
            row["DecimalProperty3"] = 3;
            row["CreatedTime"] = DateTime.Now;
            row["UpdatedTime"] = DateTime.Now;
            row["CreatedUser"] = "hk";
            row["UpdatedUser"] = "hk";
            row["IsPhantom"] = false;
            row["TreeIndex"] = null;
            row["TreePId"] = null;
            table.Rows.Add(row);

            var customer = table.ToEntityList<CustomerList>();
            Assert.AreEqual(1, customer.Count, "liteDataTable 应该能转换成一条 customer 的数据");
        }

    }
}