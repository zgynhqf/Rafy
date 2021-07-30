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

        #region DT_POCO

        private class CustomerDTO
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal DecimalProperty1 { get; set; }
            public DateTime CreatedTime { get; set; }
            public bool IsPhantom { get; set; }
            public int TreeIndex { get; set; }
            public int? TreePId { get; set; }
            public string NoValue { get; set; }
        }

        /// <summary>
        /// 自己组装的 liteDataTable 能转换成 POCO。
        /// </summary>
        [TestMethod]
        public void DT_POCO_LiteDataTable()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("Id", typeof(int)));
            table.Columns.Add(new LiteDataColumn("Name", typeof(string)));
            table.Columns.Add(new LiteDataColumn("DecimalProperty1", typeof(decimal)));
            table.Columns.Add(new LiteDataColumn("CreatedTime", typeof(DateTime)));
            table.Columns.Add(new LiteDataColumn("IsPhantom", typeof(bool)));
            table.Columns.Add(new LiteDataColumn("TreeIndex", typeof(string)));
            table.Columns.Add(new LiteDataColumn("TreePId", typeof(int)));

            var row = table.NewRow();
            row["Id"] = 1;
            row["Name"] = "Rafy";
            row["DecimalProperty1"] = (decimal)1;
            row["CreatedTime"] = DateTime.Now;
            row["IsPhantom"] = false;
            row["TreePId"] = null;
            table.Rows.Add(row);

            var customerList = table.ToPOCO<CustomerDTO>();
            Assert.AreEqual(1, customerList.Count, "通过自己组装的 liteDataTable 应该能转换成一条 customer 的数据");
            Assert.AreEqual(1, customerList[0].Id);
            Assert.AreEqual("Rafy", customerList[0].Name);
            Assert.AreEqual((decimal)1, customerList[0].DecimalProperty1);
            Assert.IsFalse(customerList[0].IsPhantom);
            Assert.IsNull(customerList[0].TreePId);
        }

        /// <summary>
        /// 自己组装的 DataTable 能转换成 POCO。
        /// </summary>
        [TestMethod]
        public void DT_POCO_DataTable()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("Id", typeof(int)));
            table.Columns.Add(new LiteDataColumn("Name", typeof(string)));
            table.Columns.Add(new LiteDataColumn("DecimalProperty1", typeof(decimal)));
            table.Columns.Add(new LiteDataColumn("CreatedTime", typeof(DateTime)));
            table.Columns.Add(new LiteDataColumn("IsPhantom", typeof(bool)));
            table.Columns.Add(new LiteDataColumn("TreeIndex", typeof(string)));
            table.Columns.Add(new LiteDataColumn("TreePId", typeof(int)));

            var row = table.NewRow();
            row["Id"] = 1;
            row["Name"] = "Rafy";
            row["DecimalProperty1"] = (decimal)1;
            row["CreatedTime"] = DateTime.Now;
            row["IsPhantom"] = false;
            row["TreePId"] = null;
            table.Rows.Add(row);

            var customerList = table.ToPOCO<CustomerDTO>();
            Assert.AreEqual(1, customerList.Count, "通过自己组装的 liteDataTable 应该能转换成一条 customer 的数据");
            Assert.AreEqual(1, customerList[0].Id);
            Assert.AreEqual("Rafy", customerList[0].Name);
            Assert.AreEqual((decimal)1, customerList[0].DecimalProperty1);
            Assert.IsFalse(customerList[0].IsPhantom);
            Assert.IsNull(customerList[0].TreePId);
        }

        #endregion

        #region LiteDataTable

        /// <summary>
        /// 自己组装的 liteDataTable 能转换成 entitylist。
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_ConvertToEntity()
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
            table.Columns.Add(new LiteDataColumn("TreeIndex", typeof(string)));
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

            var customerList = table.ToEntityList<CustomerList>();
            Assert.AreEqual(1, customerList.Count, "通过自己组装的 liteDataTable 应该能转换成一条 customer 的数据");
            Assert.AreEqual(customerList[0].Name, "HuKang", "通过自己组装的 liteDataTable 转换成的 customer 的数据应该保持一致");
        }

        /// <summary>
        /// liteDateTable 没有扩展属性也能转 entitylist 类型。
        /// </summary>
        public void DT_LiteDataTable_ConvertToEntity_NoInheritProperty()
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
            table.Rows.Add(row);

            var customerList = table.ToEntityList<CustomerList>();
            Assert.AreEqual(1, customerList.Count, "通过自己组装的且没有扩展属性的 liteDataTable 应该能转换成一条 customer 的数据");
        }

        /// <summary>
        /// 当实体的属性与数据库的字段不一致时，columnMapToProperty 参数传入 false，也能正常转换。
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_ConvertToEntity_PropertyDiffColumnName()
        {
            //这个位置 testUser 的 name 属性映射到数据库对应的是 userName。
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = new TestUser();
                user.Name = "hk";
                user.LoginName = "wy";
                repo.Save(user);

                var table = repo.GetAllInTable();
                var userList = table.ToEntityList<TestUserList>(false);
                var hkUser = userList.Concrete().FirstOrDefault(u => u.Id == user.Id);
                Assert.IsNotNull(hkUser);
                Assert.AreEqual("hk", hkUser.Name, "当实体的属性与数据库的字段不一致时，转换出来的值应该与原来保持一致。");
            }
        }

        /// <summary>
        /// 当实体的属性与数据库的字段一致时，columnMapToProperty 参数传入 true，也能正常转换。
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_ConvertToEntity_PropertySameColumnName()
        {
            var repo = RF.ResolveInstance<BlogUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var blogUser = new BlogUser();
                blogUser.UserName = "hk2";
                repo.Save(blogUser);

                var table = repo.GetAllInTable();
                var userList = table.ToEntityList<BlogUserList>(true);
                Assert.AreEqual("hk2", userList[0].UserName, "当实体的属性与数据库的字段不一致时，转换出来的值应该与原来保持一致。");
            }
        }

        /// <summary>
        /// 通过数据库查询的 liteDataTable（不带扩展属性） 能转 entitylist。
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_QueryFromDbAndConvertToEntity()
        {
            CustomerRepository repo = RF.ResolveInstance<CustomerRepository>();
            using (RF.TransactionScope(repo))
            {
                var customer = new Customer();
                customer.DecimalProperty1 = 1;
                customer.DecimalProperty2 = 2;
                customer.DecimalProperty3 = 3;
                customer.Name = "hk";
                repo.Save(customer);

                var table = repo.GetAllInTable();
                var customerList = table.ToEntityList<CustomerList>(false);
                Assert.AreEqual(1, customerList.Count, "通过数据库查询出来的 liteDataTable 应该能转换成一条 customer 的数据");
            }
        }

        /// <summary>
        /// 扩展属性映射到数据库，那么 liteDateTable 转换成 entitylist 值应该保持一致。
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_QueryFromDbAndConvertToEntity_HasInheritProperty()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var invoice = new Invoice();
                invoice.Code = "123";
                invoice.SetIsPhantom(true);
                repo.Save(invoice);

                var table = repo.GetAllInTable();
                var invoiceList = table.ToEntityList<InvoiceList>(false);
                Assert.AreEqual(true, invoiceList[0].GetIsPhantom(), "继承属性映射到数据库，那么 liteDateTable 转换成 entitylist 值应该保持一致");
            }
        }

        /// <summary>
        /// 映射数据库的数据类型和实体属性数据类型不一致时候，转换能成功。
        /// （这里是当数据库是 oracle 的时候 isphantom 在数据库中会被映射成整形）
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_QueryFromDbAndConvertToEntity_DifferentDbType()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var invoice = new Invoice();
                invoice.SetIsPhantom(true);
                repo.Save(invoice);

                var table = repo.GetAllInTable();
                var invoiceList = table.ToEntityList<InvoiceList>(false);
                Assert.AreEqual(true, invoiceList[0].GetIsPhantom(), "映射数据库的数据类型和实体属性数据类型不一致时候，转换能成功");
            }
        }

        /// <summary>
        /// 当实体的属性与数据库的字段一致时，columnMapToProperty 参数传入 true，也能正常转换。
        /// </summary>
        [TestMethod]
        public void DT_LiteDataTable_CanConvertToEntity_PropertySameColumnName()
        {
            var repo = RF.ResolveInstance<BlogUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var blogUser = new BlogUser();
                blogUser.UserName = "hk2";
                repo.Save(blogUser);

                var table = repo.GetAllInTable();
                var userList = table.ToEntityList<BlogUserList>(true);
                Assert.AreEqual("hk2", userList[0].UserName, "当实体的属性与数据库的字段不一致时，转换出来的值应该与原来保持一致。");
            }
        }

        #endregion

    }
}