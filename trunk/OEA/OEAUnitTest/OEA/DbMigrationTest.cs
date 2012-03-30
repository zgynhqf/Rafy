/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110102
 * 
*******************************************************/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using OEA.Library._Test;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using OEA.Library;
using DbMigration;
using DbMigration.Model;
using OEA.Library.ORM.DbMigration;
using DbMigration.SqlServer;
using OEA;
using hxy.Common.Data;
using System.IO;
using System;
using System.Data;
using System.Data.SqlClient;
using DbMigration.Operations;

namespace OEAUnitTest
{
    [TestClass]
    public class DbMigrationTest : TestBase
    {
        [ClassInitialize]
        public static void DbMigrationTest_ClassInitialize(TestContext context)
        {
            ClassInitialize(context, false);

            //运行测试前，这个库升级到最新的内容，同时它的历史记录需要清空
            using (var c = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                //c.DeleteDatabase();

                c.AutoMigrate();

                c.ResetDbVersion();
                c.ResetHistory();
            };
        }

        [TestMethod]
        public void DMT_AutoMigrate()
        {
            using (var c = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                c.AutoMigrate();
            };
        }

        [TestMethod]
        public void DMT_RollbackAll()
        {
            using (var context = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                context.RollbackAll(RollbackAction.DeleteHistory);

                var histories = context.GetHistories();

                Assert.IsTrue(histories.Count == 0);

                Assert.IsTrue(context.HasNoHistory());
            }
        }

        [TestMethod]
        public void DMT_CreateTable()
        {
            this.Test(destination =>
            {
                var tmpTable = new Table("TestingTable", destination);
                tmpTable.AddColumn("Id", DbType.Int32, isPrimaryKey: true);
                tmpTable.AddColumn("Name", DbType.String);
                destination.Tables.Add(tmpTable);
            }, result =>
            {
                var tmpTable2 = result.FindTable("TestingTable");
                AssertIsTrue(tmpTable2 != null);
                AssertIsTrue(tmpTable2.Columns.Count == 2);
            });
        }

        [TestMethod]
        public void DMT_DropTable()
        {
            this.Test(destination =>
            {
                destination.Tables.Remove(destination.FindTable("Task"));
            }, result =>
            {
                AssertIsTrue(result.FindTable("Task") == null);
            });
        }

        [TestMethod]
        public void DMT_CreateColumn()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.String);
                taskTable.AddColumn("TestingColumn2", DbType.String, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                var c2 = taskTable.FindColumn("TestingColumn2");
                AssertIsTrue(c1 != null);
                AssertIsTrue(c2 != null && c2.IsRequired);
            });
        }

        [TestMethod]
        public void DMT_DropColumn()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.Columns.Remove(taskTable.FindColumn("AllTime"));
                taskTable.Columns.Remove(taskTable.FindColumn("OrderNo"));
                taskTable.Columns.Remove(taskTable.FindColumn("PId"));
                taskTable.Columns.Remove(taskTable.FindColumn("TestUserId"));
            }, result =>
            {
                var taskTable2 = result.FindTable("Task");
                AssertIsTrue(taskTable2.FindColumn("AllTime") == null);
                AssertIsTrue(taskTable2.FindColumn("OrderNo") == null);
                AssertIsTrue(taskTable2.FindColumn("PId") == null);
                AssertIsTrue(taskTable2.FindColumn("TestUserId") == null);
            });
        }

        [TestMethod]
        public void DMT_AddFK()
        {
            this.Test(destination =>
            {
                destination.FindTable("Task").AddColumn("TestingColumn3", DbType.Int32,
                    isRequired: true,
                    foreignConstraint: new ForeignConstraint(destination.FindTable("User").FindColumn("Id"))
                    );
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c = taskTable.FindColumn("TestingColumn3");
                AssertIsTrue(c != null);
                var c3FK = c.ForeignConstraint.PKColumn;
                AssertIsTrue(c3FK.Table.Name == "User" && c3FK.Name == "Id");
            });
        }

        [TestMethod]
        public void DMT_RemoveFK()
        {
            this.Test(destination =>
            {
                destination.FindTable("Task").FindColumn("TestUserId").ForeignConstraint = null;
            }, result =>
            {
                AssertIsTrue(result.FindTable("Task").FindColumn("TestUserId").ForeignConstraint == null);
            });
        }

        [TestMethod]
        public void DMT_AddNotNull()
        {
            this.Test(destination =>
            {
                destination.FindTable("User").FindColumn("Level").IsRequired = true;
            }, result =>
            {
                AssertIsTrue(result.FindTable("User").FindColumn("Level").IsRequired == true);
            });
        }

        [TestMethod]
        public void DMT_RemoveNotNull()
        {
            this.Test(destination =>
            {
                destination.FindTable("Task").FindColumn("AllTime").IsRequired = false;
            }, result =>
            {
                AssertIsTrue(result.FindTable("Task").FindColumn("AllTime").IsRequired == false);
            });
        }

        private void Test(Action<Database> action, Action<Database> assertAction)
        {
            using (var context = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                var destination = context.ClassMetaReader.Read();
                action(destination);

                try
                {
                    context.MigrateTo(destination);

                    var result = context.DatabaseMetaReader.Read();
                    assertAction(result);
                }
                finally
                {
                    context.RollbackAll(RollbackAction.DeleteHistory);
                }
            }
        }

        [TestMethod]
        public void DMT_DataLoss_DropTable()
        {
            using (var context = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                context.IgnoreDataLoss = false;

                var destination = context.ClassMetaReader.Read();

                destination.Tables.Remove(destination.FindTable("Task"));

                try
                {
                    context.MigrateTo(destination);

                    //上一行应该发生异常，否则会执行到这一步
                    AssertIsTrue(false);
                }
                catch (DbMigrationException) { }
            }
        }

        [TestMethod]
        public void DMT_DataLoss_DropColumn()
        {
            using (var context = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                context.IgnoreDataLoss = false;

                var destination = context.ClassMetaReader.Read();
                var taskTable = destination.FindTable("Task");
                taskTable.Columns.Remove(taskTable.FindColumn("AllTime"));

                try
                {
                    context.MigrateTo(destination);

                    //上一行应该发生异常，否则会执行到这一步
                    AssertIsTrue(false);
                }
                catch (DbMigrationException) { }
            }
        }

        [TestMethod]
        public void DMT_ManualMigrate()
        {
            using (var context = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                try
                {
                    context.ManualMigrations.Clear();
                    context.ManualMigrations.Add(new DMT_ManualMigrateEntity());
                    context.ManualMigrations.Add(new DMT_ManualMigrateTest());

                    //手工更新
                    context.MigrateManually();

                    //历史记录
                    var histories = context.GetHistories();
                    Assert.IsTrue(histories.Count == 2);
                    Assert.IsTrue(histories[0] is DMT_ManualMigrateEntity || histories[1] is DMT_ManualMigrateEntity);
                    Assert.IsTrue(histories[0] is DMT_ManualMigrateTest || histories[1] is DMT_ManualMigrateTest);

                    //数据库结构
                    var database = context.DatabaseMetaReader.Read();
                    var table = database.Tables.First(t => t.Name == "TestingTable");
                    Assert.IsTrue(table.Columns.Count == 2);
                    var pk = table.FindPrimaryColumn();
                    Assert.IsTrue(pk.Name == "Id");
                    Assert.IsTrue(pk.DataType == DbType.Int32);

                    //数据库数据
                    using (var db = new DBAccesser(UnitTestEntity.ConnectionString))
                    {
                        var rows = db.QueryDataTable("select * from TestingTable", CommandType.Text);
                        AssertIsTrue(rows.Rows.Count == 2);
                    }
                    var repo = RF.Create<TestUser>();
                    AssertIsTrue(repo.GetAll().Count == 10);
                }
                finally
                {
                    //回滚
                    context.RollbackAll(RollbackAction.DeleteHistory);

                    var database = context.DatabaseMetaReader.Read();
                    Assert.IsTrue(database.Tables.All(t => t.Name != "TestingTable"));
                }
            }
        }

        #region public class DMT_ManualMigrateTest

        public class DMT_ManualMigrateTest : ManualDbMigration
        {
            public override string DbSetting
            {
                get { return UnitTestEntity.ConnectionString; }
            }

            protected override void Up()
            {
                this.CreateTable("TestingTable", "Id", DbType.Int32);
                this.CreateNormalColumn("TestingTable", "Name", DbType.String);

                this.RunSql("insert into [TestingTable] values('TestingName1')");
                this.RunSql("insert into [TestingTable] values('TestingName2')");
            }

            protected override void Down()
            {
                this.RunSql("delete from [TestingTable] where id = 1 or id = 2");

                this.DropNormalColumn("TestingTable", "Name", DbType.String);
                this.DropTable("TestingTable", "Id", DbType.Int32);
            }

            protected override DateTime GetTimeId()
            {
                return DateTime.Now;
                //return new DateTime(2012, 1, 6, 22, 20, 00);
            }

            protected override string GetDescription()
            {
                return "单元测试 - 数据库手工升级 - 建立一张表，一个字段，两行数据";
            }

            public override ManualMigrationType Type
            {
                get { return ManualMigrationType.Schema; }
            }
        }

        #endregion

        #region public class DMT_ManualMigrateEntity

        public class DMT_ManualMigrateEntity : ManualDbMigration
        {
            public override string DbSetting
            {
                get { return UnitTestEntity.ConnectionString; }
            }

            protected override void Up()
            {
                this.RunCode(db =>
                {
                    var repo = RF.Create<TestUser>();
                    for (int i = 0; i < 10; i++)
                    {
                        var user = new TestUser();
                        repo.Save(user);
                    }
                });
            }

            protected override void Down()
            {
                this.RunCode(db =>
                {
                    var repo = RF.Create<TestUser>();
                    var all = repo.GetAll();
                    foreach (var user in all) { user.MarkDeleted(); }
                    repo.Save(all);
                });
            }

            protected override DateTime GetTimeId()
            {
                return DateTime.Now;
                //return new DateTime(2012, 1, 6, 22, 25, 00);
            }

            protected override string GetDescription()
            {
                return "单元测试 - 数据库手工升级 - 使用实体类创建数据";
            }

            public override ManualMigrationType Type
            {
                get { return ManualMigrationType.Data; }
            }
        }

        #endregion
    }
}