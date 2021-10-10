using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using Rafy.Domain.ORM.DbMigration;
using Rafy.Reflection;
using Rafy.Utils;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class UtilsTest
    {
        [ClassInitialize]
        public static void UtilsTest_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region DBA

        [TestMethod]
        public void UtilsTest_DBA_QueryValue_DateTime()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            var dp = RdbDataProvider.Get(repo);
            if (dp.DbSetting.ProviderName == DbSetting.Provider_SqlClient)
            {
                using (var dba = dp.CreateDbAccesser())
                {
                    var time = dba.QueryValue(@"SELECT GETDATE()");
                    Assert.IsInstanceOfType(time, typeof(DateTime));
                }
            }
        }

        /// <summary>
        /// 能使用 DbAccesserParameter 才能保证索引可以正常起作用。
        /// </summary>
        [TestMethod]
        public void UtilsTest_DBA_DbAccesserParameter()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (var dba = DbAccesserFactory.Create(repo))
            {
                using (var reader = dba.QueryDataReader(@"SELECT * FROM BOOK WHERE UpdatedTime < {0}",
                    new DbAccesserParameter(DateTime.Now, DbType.DateTime)
                    ))
                {
                    //do nothing;
                }
            }
        }

        /// <summary>
        /// 能使用 DbAccesserParameter 才能保证索引可以正常起作用。
        /// </summary>
        [TestMethod]
        public void UtilsTest_DBA_FormattedSql_DbAccesserParameter()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (var dba = DbAccesserFactory.Create(repo))
            {
                FormattedSql sql = @"SELECT * FROM BOOK WHERE UpdatedTime < {0}";
                sql.Parameters.Add(new DbAccesserParameter(DateTime.Now, DbType.DateTime));

                using (var reader = dba.QueryDataReader(sql, sql.Parameters.ToArray()))
                {
                    //do nothing;
                }
            }
        }

        #endregion

        #region Logger

        [TestMethod]
        public void UtilsTest_Logger_DbAccessed()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                int count = 0;
                EventHandler<Logger.DbAccessedEventArgs> handler = (o, e) =>
                {
                    if (e.ConnectionSchema == RdbDataProvider.Get(repo).DbSetting) count++;
                };
                Logger.DbAccessed += handler;

                repo.Save(new TestUser());

                Logger.DbAccessed -= handler;

                var p = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName).ProviderName;
                if (p == DbSetting.Provider_SqlClient || p == DbSetting.Provider_MySql || p == DbSetting.Provider_SQLite)
                {
                    Assert.IsTrue(count == 1);//sqlServer、MySql= 1
                }
                else
                {
                    Assert.IsTrue(count == 2);//sqlce oracle=2
                }
            }
        }

        [TestMethod]
        public void UtilsTest_Logger_ThreadDbAccessed()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                int count = 0;
                EventHandler<Logger.DbAccessedEventArgs> handler = (o, e) =>
                {
                    if (e.ConnectionSchema == RdbDataProvider.Get(repo).DbSetting) count++;
                };
                Logger.ThreadDbAccessed += handler;

                repo.Save(new TestUser());

                Logger.ThreadDbAccessed -= handler;

                var p = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName).ProviderName;
                if (p == DbSetting.Provider_SqlClient || p == DbSetting.Provider_MySql || p == DbSetting.Provider_SQLite)
                {
                    Assert.IsTrue(count == 1);//sqlServer、MySql= 1
                }
                else
                {
                    Assert.IsTrue(count == 2);//sqlce oracle=2
                }
            }
        }

        [TestMethod]
        public void UtilsTest_Logger_DbAccessedCount()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var c1 = Logger.DbAccessedCount;
                repo.Save(new TestUser());

                var p = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName).ProviderName;
                if (p == DbSetting.Provider_SqlClient || p == DbSetting.Provider_MySql || p == DbSetting.Provider_SQLite)
                {
                    Assert.IsTrue(Logger.DbAccessedCount == c1 + 1);
                }
                else
                {
                    Assert.IsTrue(Logger.DbAccessedCount == c1 + 2);
                }
            }
        }

        [TestMethod]
        public void UtilsTest_Logger_ThreadDbAccessedCount()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var c1 = Logger.ThreadDbAccessedCount;
                repo.Save(new TestUser());

                var p = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName).ProviderName;
                if (p == DbSetting.Provider_SqlClient || p == DbSetting.Provider_MySql || p == DbSetting.Provider_SQLite)
                {
                    Assert.IsTrue(Logger.ThreadDbAccessedCount == c1 + 1);
                }
                else
                {
                    Assert.IsTrue(Logger.ThreadDbAccessedCount == c1 + 2);
                }
            }
        }

        #endregion

        #region TrasactionScope

        [TestMethod]
        public void UtilsTest_TrasactionScope_RollBack()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            Assert.IsTrue(repo.CountAll() == 0);

            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser());
                Assert.IsTrue(repo.CountAll() == 1);
            }

            Assert.IsTrue(repo.CountAll() == 0);
        }

        [TestMethod]
        public void UtilsTest_TrasactionScope_Complete()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            Assert.IsTrue(repo.CountAll() == 0);

            using (var tran = RF.TransactionScope(repo))
            {
                repo.Save(new TestUser());
                Assert.IsTrue(repo.CountAll() == 1);

                tran.Complete();
            }

            Assert.IsTrue(repo.CountAll() == 1);

            DeleteUsers();
        }

        /// <summary>
        /// 在内部回滚
        /// </summary>
        [TestMethod]
        public void UtilsTest_TrasactionScope_Inner_RollBack()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            Assert.IsTrue(repo.CountAll() == 0);

            using (var tranWhole = RF.TransactionScope(repo))
            {
                repo.Save(new TestUser());
                Assert.IsTrue(repo.CountAll() == 1);

                using (var tranSub = RF.TransactionScope(repo))
                {
                    repo.Save(new TestUser());
                    Assert.IsTrue(repo.CountAll() == 2);
                    //内部不提交
                }

                Assert.IsTrue(repo.CountAll() == 2);

                tranWhole.Complete();
            }

            Assert.IsTrue(repo.CountAll() == 0, "内部事务未提交，整个事务不应该提交。");
        }

        /// <summary>
        /// 内部提交，外部回滚
        /// </summary>
        [TestMethod]
        public void UtilsTest_TrasactionScope_Outer_RollBack()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            Assert.IsTrue(repo.CountAll() == 0);

            using (var tranWhole = RF.TransactionScope(repo))
            {
                repo.Save(new TestUser());
                Assert.IsTrue(repo.CountAll() == 1);

                using (var tranSub = RF.TransactionScope(repo))
                {
                    repo.Save(new TestUser());
                    Assert.IsTrue(repo.CountAll() == 2);

                    tranWhole.Complete();
                }

                Assert.IsTrue(repo.CountAll() == 2);
                //外部不提交
            }

            Assert.IsTrue(repo.CountAll() == 0, "外部事务未提交，整个事务不应该提交。");
        }

        /// <summary>
        /// 两个数据库的事务互不干扰
        /// </summary>
        [TestMethod]
        public void UtilsTest_TrasactionScope_MultiDatabases()
        {
            var repoUser = RF.ResolveInstance<TestUserRepository>();
            Assert.IsTrue(repoUser.CountAll() == 0);
            var repoCustomer = RF.ResolveInstance<CustomerRepository>();
            Assert.IsTrue(repoCustomer.CountAll() == 0);

            using (var tranWhole = RF.TransactionScope(repoUser))
            {
                repoUser.Save(new TestUser());
                Assert.IsTrue(repoUser.CountAll() == 1);

                //另一数据的事务。
                using (var tranSub = RF.TransactionScope(repoCustomer))
                {
                    repoUser.Save(new Customer());
                    Assert.IsTrue(repoCustomer.CountAll() == 1);
                    //内部不提交
                }

                tranWhole.Complete();
            }

            Assert.IsTrue(repoCustomer.CountAll() == 0, "两个数据库的事务互不干扰，Customer 对应的数据库事务已经回滚。");
            Assert.IsTrue(repoUser.CountAll() == 1, "两个数据库的事务互不干扰，TestUser 对应的数据库事务提交成功。");

            DeleteUsers();
        }

        ///// <summary>
        ///// 想要两个不同数据库的事务合并，可以在最外层使用 TransactionScope 类型。
        ///// </summary>
        //[TestMethod]
        //public void ___UtilsTest_TrasactionScope_MultiDatabases_OuterTransactionScope()
        //{
        //}

        #endregion

        #region LiteDataTable

        /// <summary>
        /// 通过 IDbAccesser 查询表格。
        /// </summary>
        [TestMethod]
        public void UtilsTest_LiteDataTable_Query()
        {
            var repoUser = RF.ResolveInstance<TestUserRepository>();
            using (var tranWhole = RF.TransactionScope(repoUser))
            {
                repoUser.Save(new TestUser() { Age = 1 });
                repoUser.Save(new TestUser() { Age = 1 });
                repoUser.Save(new TestUser() { Age = 1 });

                using (var dba = DbAccesserFactory.Create(repoUser))
                {
                    var table = dba.QueryLiteDataTable("Select * from Users where id > {0}", 0);
                    var columns = table.Columns;
                    Assert.IsTrue(columns.Find("UserName") != null);
                    Assert.IsTrue(columns.Find("Age") != null);

                    var rows = table.Rows;
                    Assert.IsTrue(rows.Count == 3);
                    Assert.IsTrue(rows[0].Values.Length == columns.Count);
                    Assert.IsTrue(rows[0].GetInt32("Age") == 1);
                }
            }
        }

        /// <summary>
        /// 序列化及反序列化
        /// </summary>
        [TestMethod]
        public void UtilsTest_LiteDataTable_Serialization_WCF()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("UserName", typeof(string)));
            table.Columns.Add(new LiteDataColumn("Age", typeof(string)));

            var row = table.NewRow();
            row["UserName"] = "HuQingfang";
            row["Age"] = 26;
            table.Rows.Add(row);

            var row2 = table.NewRow();
            row2["UserName"] = "XuDandan";
            row2["Age"] = 25;
            table.Rows.Add(row2);

            //序列化。
            var serializer = new DataContractSerializer(typeof(LiteDataTable));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, table);

            //读取 xml
            byte[] bytes = stream.ToArray();
            string xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.IsTrue(xml.Contains("LiteDataTable"));
            Assert.IsTrue(xml.Contains("HuQingfang"));

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var table2 = (LiteDataTable)serializer.ReadObject(stream);

            Assert.IsTrue(table2.Rows.Count == 2);
            Assert.IsTrue(table2[0]["UserName"].ToString() == "HuQingfang", "反序列化后，可以通过列名来获取列");
            Assert.IsTrue(table2[0].GetString("UserName") == "HuQingfang");
            Assert.IsTrue(table2[0].GetInt32("Age") == 26);
            Assert.IsTrue(table2[1].GetString("UserName") == "XuDandan");
            Assert.IsTrue(table2[1].GetInt32("Age") == 25);
        }

        [TestMethod]
        public void UtilsTest_LiteDataTable_Serialization_Binary()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("UserName", typeof(string)));
            table.Columns.Add(new LiteDataColumn("Age", typeof(string)));

            var row = table.NewRow();
            row["UserName"] = "HuQingfang";
            row["Age"] = 26;
            table.Rows.Add(row);

            var row2 = table.NewRow();
            row2["UserName"] = "XuDandan";
            row2["Age"] = 25;
            table.Rows.Add(row2);

            var table2 = ObjectCloner.Clone(table);

            Assert.IsTrue(table2.Rows.Count == 2);
            Assert.IsTrue(table2[0]["UserName"].ToString() == "HuQingfang", "反序列化后，可以通过列名来获取列");
            Assert.IsTrue(table2[0].GetString("UserName") == "HuQingfang");
            Assert.IsTrue(table2[0].GetInt32("Age") == 26);
            Assert.IsTrue(table2[1].GetString("UserName") == "XuDandan");
            Assert.IsTrue(table2[1].GetInt32("Age") == 25);
        }

        #endregion

        #region EnumViewModel

        [TestMethod]
        public void UtilsTest_EnumViewModel_Parse_Name()
        {
            var value = EnumViewModel.Parse("A", typeof(FavorateTypeWithLabel));
            Assert.AreEqual(FavorateTypeWithLabel.A, value);
        }

        [TestMethod]
        public void UtilsTest_EnumViewModel_Parse_Label()
        {
            var value = EnumViewModel.Parse("第一个", typeof(FavorateTypeWithLabel));
            Assert.AreEqual(FavorateTypeWithLabel.A, value);
        }

        [TestMethod]
        public void UtilsTest_EnumViewModel_Parse_Value()
        {
            var value = EnumViewModel.Parse("1", typeof(FavorateTypeWithLabel));
            Assert.AreEqual(FavorateTypeWithLabel.B, value);
        }

        [TestMethod]
        public void UtilsTest_EnumViewModel_Parse_Null()
        {
            var value = EnumViewModel.Parse(null, typeof(FavorateTypeWithLabel));
            Assert.IsNull(value);

            value = EnumViewModel.Parse(string.Empty, typeof(FavorateTypeWithLabel));
            Assert.IsNull(value);
        }

        #endregion

        #region Reflection

        private int TestArguments(int a, int? b)
        {
            return 1;
        }
        private int TestArguments(int a, string c)
        {
            return 2;
        }
        private int TestArguments(int a, PagingInfo d)
        {
            return 3;
        }
        private int TestArguments(int? a, int? b)
        {
            return 5;
        }
        private int TestArguments(int a, int? b, string c, PagingInfo d)
        {
            return 4;
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch1()
        {
            try
            {
                MethodCaller.CallMethod(this, "TestArguments", null, "");
                Assert.IsFalse(true, "应该无法找到对应的方法。");
            }
            catch (InvalidProgramException) { }
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch2()
        {
            try
            {
                var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, null);
                Assert.IsFalse(true, "应该找到过多的方法。");
            }
            catch (InvalidProgramException) { }
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch3()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, new NullParameter { ParameterType = typeof(string) });
            Assert.IsTrue(res == 2);
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch4()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, PagingInfo.Empty);
            Assert.IsTrue(res == 3);
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch5()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, "SDF");
            Assert.IsTrue(res == 2);
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch6()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, 1);
            Assert.IsTrue(res == 1);
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch7()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", null, 1);
            Assert.IsTrue(res == 5);
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch8()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, 1, "", null);
            Assert.IsTrue(res == 4);
        }

        [TestMethod]
        public void UtilsTest_Reflection_MethodCaller_ArgumentMatch9()
        {
            var res = (int)MethodCaller.CallMethod(this, "TestArguments", 1, null, "", null);
            Assert.IsTrue(res == 4);
        }

        #endregion

        #region Extendable

        private class ExtendableObject : Extendable { }

        [TestMethod]
        public void UtilsTest_Extendable()
        {
            var ext = new ExtendableObject();
            ext.SetExtendedProperty("DynamicProperty", "DDDD");

            Assert.AreEqual("DDDD", ext["DynamicProperty"]);
        }

        [TestMethod]
        public void UtilsTest_Extendable_Reflection()
        {
            var ext = new ExtendableObject();
            ext.SetExtendedProperty("DynamicProperty", "Value1");
            ext.SetExtendedProperty("DynamicProperty2", "Value2");

            var dpList = ext.GetExtendedProperties();
            Assert.AreEqual(2, dpList.Count);
            Assert.IsTrue(dpList.ContainsKey("DynamicProperty"));
            Assert.IsTrue(dpList.ContainsKey("DynamicProperty2"));
            Assert.AreEqual("Value1", dpList["DynamicProperty"]);
            Assert.AreEqual("Value2", dpList["DynamicProperty2"]);
        }

        [TestMethod]
        public void UtilsTest_Extendable_SetNull()
        {
            var ext = new ExtendableObject();
            ext.SetExtendedProperty("DynamicProperty", "Value1");

            Assert.AreEqual(1, ext.ExtendedPropertiesCount);

            ext.SetExtendedProperty("DynamicProperty", null);
            Assert.AreEqual(0, ext.ExtendedPropertiesCount, "设置为 null 后，需要清空数据。");
        }

        [TestMethod]
        public void UtilsTest_Extendable_GetOrDefault()
        {
            var ext = new ExtendableObject();
            var value = ext.GetPropertyOrDefault("DN", "HAHA");
            Assert.AreEqual("HAHA", value);
            ext.SetExtendedProperty("DN", "Value2");
            value = ext.GetPropertyOrDefault("DN", "HAHA");
            Assert.AreEqual("Value2", value);
        }

        #endregion

        private static void DeleteUsers()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();

            var users = repo.GetAll();
            users.Clear();
            repo.Save(users);

            Assert.IsTrue(repo.CountAll() == 0);
        }
    }
}