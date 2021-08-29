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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.Model;
using Rafy.DbMigration.Operations;
using Rafy.DbMigration.SqlServer;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.ORM.DbMigration.Presistence;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class DbMigrationTest
    {
        private static DbTypeConverter dbTypeConverter;

        [ClassInitialize]
        public static void DbMigrationTest_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);

            //运行测试前，这个库升级到最新的内容，同时它的历史记录需要清空
            using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                c.RunDataLossOperation = DataLossOperation.All;

                if (IsTestDbSQLite())
                {
                    c.DeleteAllTables();
                }
                else
                {
                    c.HistoryRepository = new DbHistoryRepository();
                }

                c.AutoMigrate();

                c.ResetDbVersion();

                if (!IsTestDbSQLite())
                {
                    c.ResetHistory();
                }

                var dbProvider = DbMigrationProviderFactory.GetProvider(c.DbSetting);
                dbTypeConverter = (dbProvider.CreateRunGenerator() as SqlRunGenerator).DbTypeCoverter;
            };
        }

        [TestMethod]
        public void DMT_AutoMigrate()
        {
            using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                if (!IsTestDbSQLite())
                {
                    c.HistoryRepository = new DbHistoryRepository();
                }
                c.RunDataLossOperation = DataLossOperation.All;
                c.AutoMigrate();
            }
        }

        /// <summary>
        /// 测试实体数据库的迁移，实体从源库迁移到目标库
        /// </summary>
        [TestMethod]
        public void DMT_AutoMigrate_ChangeDbSetting()
        {
            using (RdbDataProvider.RedirectDbSetting(UnitTestEntityRepositoryDataProvider.DbSettingName,
                    UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
            {
                using (var c = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                {
                    /// 实体元数据默认设置的连接字符串
                    /// 如果切换新的数据库需要设置，否则不用设置
                    c.ClassMetaReader.EntityDbSettingName = UnitTestEntityRepositoryDataProvider.DbSettingName;
                    if (!IsTestDbSQLite())
                    {
                        c.HistoryRepository = new DbHistoryRepository();
                    }
                    c.RunDataLossOperation = DataLossOperation.All;
                    c.AutoMigrate();
                }
            }
        }

        [TestMethod]
        public void DMT_RollbackAll()
        {
            if (IsTestDbSQLite()) return;

            using (var context = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                context.HistoryRepository = new DbHistoryRepository();
                context.RunDataLossOperation = DataLossOperation.All;

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
                Assert.IsTrue(tmpTable2 != null);
                Assert.IsTrue(tmpTable2.Columns.Count == 2);
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
                Assert.IsTrue(result.FindTable("Task") == null);
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
                Assert.IsTrue(c1 != null);
                Assert.IsTrue(c2 != null && (c2.IsRequired || IsTestDbSQLite()));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_String()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.String, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.String));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_AnsiString()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.AnsiString, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.AnsiString));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Date()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Date, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Date));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Time()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Time, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Time));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_DateTime()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.DateTime, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.DateTime));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_DateTimeOffset()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.DateTimeOffset, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.DateTimeOffset));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Int32()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Int32, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Int32));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Int64()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Int64, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Int64));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Double()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Double, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Double));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Decimal()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Decimal, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Decimal));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Binary()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Binary, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Binary));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_Boolean()
        {
            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.AddColumn("TestingColumn", DbType.Boolean, isRequired: true);
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c1 = taskTable.FindColumn("TestingColumn");
                Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.Boolean));
            });
        }

        [TestMethod]
        public void DMT_CreateColumn_CLOB()
        {
            if (DbSetting.IsOracleProvider(UnitTest2EntityRepositoryDataProvider.DbSettingName))
            {
                this.Test(destination =>
                {
                    var taskTable = destination.FindTable("Task");
                    taskTable.AddColumn("TestingColumn", DbType.String, "CLOB", isRequired: true);
                }, result =>
                {
                    var taskTable = result.FindTable("Task");
                    var c1 = taskTable.FindColumn("TestingColumn");
                    Assert.IsTrue(c1 != null && (c1.IsRequired || IsTestDbSQLite()));
                    Assert.IsTrue(dbTypeConverter.IsCompatible(c1.DbType, DbType.String));
                });
            }
        }

        [TestMethod]
        public void DMT_DropColumn()
        {
            if (IsTestDbSQLite()) return;

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
                Assert.IsTrue(taskTable2.FindColumn("AllTime") == null);
                Assert.IsTrue(taskTable2.FindColumn("OrderNo") == null);
                Assert.IsTrue(taskTable2.FindColumn("PId") == null);
                Assert.IsTrue(taskTable2.FindColumn("TestUserId") == null);
            });
        }

        /// <summary>
        /// 在自动升级时，可以升级使用 HasDbType 定义的数据类型。
        /// </summary>
        [TestMethod]
        public void DMT_AlterColumn_DbType_AutoMigrate()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
            }, result =>
            {
                var table = result.FindTable("Task");
                var column = table.FindColumn("XmlContent");
                Assert.IsTrue(column != null);

                var p = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName).ProviderName;
                Assert.IsTrue(dbTypeConverter.IsCompatible(column.DbType, DbType.Xml));
            });
        }

        [TestMethod]
        public void DMT_AlterColumn_DbType()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.Columns.Remove(taskTable.FindColumn("Name"));
                taskTable.AddColumn("Name", DbType.Xml);
            }, result =>
            {
                var table = result.FindTable("Task");
                var column = table.FindColumn("Name");
                Assert.IsTrue(column != null);

                var p = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName).ProviderName;
                Assert.IsTrue(dbTypeConverter.IsCompatible(column.DbType, DbType.Xml));
            });
        }

        /// <summary>
        /// 测试从可空的字符串类型变到不可空的数值类型时的迁移。
        /// </summary>
        [TestMethod]
        public void DMT_AlterColumn_DbType_NullStringToDouble()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
                var taskTable = destination.FindTable("Task");
                taskTable.Columns.Remove(taskTable.FindColumn("Name"));
                taskTable.AddColumn("Name", DbType.Double, isRequired: true);
            }, result =>
            {
                var table = result.FindTable("Task");
                var column = table.FindColumn("Name");
                Assert.IsTrue(column != null);
                Assert.IsTrue(dbTypeConverter.IsCompatible(column.DbType, DbType.Double));
                Assert.IsTrue(column.IsRequired);
            });
        }

        [TestMethod]
        public void DMT_AddFK()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
                destination.FindTable("Task").AddColumn("TestingColumn3", DbType.Int32,
                    isRequired: true,
                    foreignConstraint: new ForeignConstraint(destination.FindTable("Users").FindColumn("Id"))
                    );
            }, result =>
            {
                var taskTable = result.FindTable("Task");
                var c = taskTable.FindColumn("TestingColumn3");
                Assert.IsTrue(c != null);
                var c3FK = c.ForeignConstraint.PKColumn;
                Assert.IsTrue(c3FK.Table.Name.EqualsIgnoreCase("Users") && c3FK.Name.EqualsIgnoreCase("Id"));
            });
        }

        [TestMethod]
        public void DMT_RemoveFK()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
                destination.FindTable("Task").FindColumn("TestUserId").ForeignConstraint = null;
            }, result =>
            {
                Assert.IsTrue(result.FindTable("Task").FindColumn("TestUserId").ForeignConstraint == null);
            });
        }

        [TestMethod]
        public void DMT_AddNotNull()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
                destination.FindTable("Users").FindColumn("Level").IsRequired = true;
            }, result =>
            {
                Assert.IsTrue(result.FindTable("Users").FindColumn("Level").IsRequired == true);
            });
        }

        [TestMethod]
        public void DMT_RemoveNotNull()
        {
            if (IsTestDbSQLite()) return;

            this.Test(destination =>
            {
                destination.FindTable("Task").FindColumn("AllTime").IsRequired = false;
            }, result =>
            {
                Assert.IsTrue(result.FindTable("Task").FindColumn("AllTime").IsRequired == false);
            });
        }

        private void Test(Action<Database> action, Action<Database> assertAction)
        {
            using (var context = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                if (!IsTestDbSQLite())
                {
                    context.HistoryRepository = new DbHistoryRepository();
                }
                context.RunDataLossOperation = DataLossOperation.All;

                var destination = context.ClassMetaReader.Read();
                action(destination);

                try
                {
                    context.MigrateTo(destination);

                    var result = context.DatabaseMetaReader.Read();
                    assertAction(result);

                    //SQLite 数据库，只能删除所有表，再重新生成。
                    if (IsTestDbSQLite())
                    {
                        context.DeleteAllTables();
                        context.AutoMigrate();
                    }
                }
                finally
                {
                    if (!IsTestDbSQLite())
                    {
                        context.RollbackAll(RollbackAction.DeleteHistory);
                    }
                }
            }
        }

        [TestMethod]
        public void DMT_DataLoss_DropTable()
        {
            using (var context = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                if (!IsTestDbSQLite())
                {
                    context.HistoryRepository = new DbHistoryRepository();
                }

                var destination = context.ClassMetaReader.Read();
                var taskTable = destination.FindTable("Task");
                destination.Tables.Remove(taskTable);

                try
                {
                    context.MigrateTo(destination);

                    var result = context.DatabaseMetaReader.Read();
                    var resultTable = result.FindTable("Task");
                    Assert.IsTrue(resultTable != null);
                }
                finally
                {
                    if (context.SupportHistory)
                    {
                        context.RollbackAll(RollbackAction.DeleteHistory);
                    }
                    else
                    {
                        context.DeleteAllTables();
                        context.AutoMigrate();
                    }
                }
            }
        }

        [TestMethod]
        public void DMT_DataLoss_DropColumn()
        {
            if (IsTestDbSQLite()) return;

            using (var context = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                context.HistoryRepository = new DbHistoryRepository();

                var destination = context.ClassMetaReader.Read();
                var taskTable = destination.FindTable("Task");
                taskTable.Columns.Remove(taskTable.FindColumn("AllTime"));

                try
                {
                    context.MigrateTo(destination);

                    var result = context.DatabaseMetaReader.Read();
                    var resultTable = result.FindTable("Task");
                    var column = resultTable.FindColumn("AllTime");
                    Assert.IsTrue(column != null);
                }
                finally
                {
                    context.RollbackAll(RollbackAction.DeleteHistory);
                }
            }
        }

        [TestMethod]
        public void DMT_ManualMigrate()
        {
            using (var context = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                if (!IsTestDbSQLite())
                {
                    context.HistoryRepository = new DbHistoryRepository();
                }
                context.RunDataLossOperation = DataLossOperation.All;

                try
                {
                    context.ManualMigrations.Clear();
                    context.ManualMigrations.Add(new DMT_ManualMigrate_AddEntity());
                    context.ManualMigrations.Add(new DMT_ManualMigrate_AddTable());

                    //手工更新
                    context.MigrateManually();

                    //历史记录
                    if (context.SupportHistory)
                    {
                        var histories = context.GetHistories();
                        Assert.AreEqual(2, histories.Count);
                        Assert.IsTrue(histories[0] is DMT_ManualMigrate_AddEntity);
                        Assert.IsTrue(histories[1] is DMT_ManualMigrate_AddTable);
                        //Assert.IsTrue(histories.Any(e => e is DMT_ManualMigrateEntity));
                        //Assert.IsTrue(histories.Any(e => e is DMT_ManualMigrateTest));
                    }

                    //数据库结构
                    var database = context.DatabaseMetaReader.Read();
                    var table = database.FindTable("TestingTable");
                    Assert.AreEqual(2, table.Columns.Count);
                    var pk = table.FindPrimaryColumn();
                    Assert.IsTrue(pk.Name.EqualsIgnoreCase("Id"));
                    Assert.AreEqual(DbType.Int32, pk.DbType);

                    //数据库数据
                    using (var db = DbAccesserFactory.Create(context.DbSetting))
                    {
                        var rows = db.QueryDataTable("select * from TestingTable");
                        Assert.AreEqual(2, rows.Rows.Count);
                    }
                    var repo = RF.Find<TestUser>();
                    Assert.AreEqual(10, repo.CountAll());
                }
                finally
                {
                    if (!IsTestDbSQLite())
                    {
                        //回滚
                        context.RollbackAll(RollbackAction.DeleteHistory);

                        var database = context.DatabaseMetaReader.Read();
                        Assert.IsNull(database.FindTable("TestingTable"));
                    }
                    else
                    {
                        context.DeleteAllTables();
                        context.ManualMigrations.Clear();
                        context.AutoMigrate();
                    }
                }
            }
        }

        [TestMethod]
        public void DMT_RefreshComments()
        {
            if (IsTestDbSQLite()) return;

            using (var context = new RafyDbMigrationContext(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                context.RefreshComments();
                if (context.DbSetting.ProviderName == DbSetting.Provider_MySql)
                {
                    //数据库数据
                    using (var db = DbAccesserFactory.Create(context.DbSetting))
                    {
                        Func<string, DataRow[]> queryComments = tableName =>
                        {
                            var table = db.QueryDataTable(@"show full columns from `" + tableName + "`");
                            return table.Rows.Cast<DataRow>().ToArray();
                        };
                        var rows = queryComments("ARTICLE");
                        Assert.IsTrue(rows.Any(r => r["Field"].ToString() == "Id"), "实体的标识属性。");
                        Assert.IsTrue(rows.Any(r => r["Field"].ToString() == "AdministratorId"), "文章的管理员");
                        Assert.IsTrue(rows.Any(r => r["Field"].ToString() == "CreatedTime"), "实体的创建时间。");

                        rows = queryComments("Roles");
                        var roleTypeDesc = rows.FirstOrDefault(r => r["Field"].ToString() == "RoleType");
                        Assert.IsNotNull(roleTypeDesc, "枚举属性必须有注释。");
                        var comment = roleTypeDesc["Comment"].ToString();
                        Assert.AreEqual(comment, @"角色的类型
0:(Normal, 一般)
1:(Administrator, 管理员)");
                    }
                }
                else if (!DbSetting.IsOracleProvider(context.DbSetting))
                {
                    //数据库数据
                    using (var db = DbAccesserFactory.Create(context.DbSetting))
                    {
                        Func<string, DataRow[]> queryComments = tableName =>
                        {
                            var table = db.QueryDataTable(@"select t.name tableName, c.name columnName, p.Value Comment from sys.all_columns c
                            join sys.tables t on c.object_id = t.object_id join sys.extended_properties p on p.major_id = c.object_id and p.minor_id = c.column_id
                            where t.name = '" + tableName + "'");
                            return table.Rows.Cast<DataRow>().ToArray();
                        };
                        var rows = queryComments("ARTICLE");
                        Assert.IsTrue(rows.Any(r => r["columnName"].ToString() == "Id"), "主键必须有注释。");
                        Assert.IsTrue(rows.Any(r => r["columnName"].ToString() == "AdministratorId"), "外键必须有注释。");
                        Assert.IsTrue(rows.Any(r => r["columnName"].ToString() == "CreatedTime"), "扩展属性必须有注释。");

                        rows = queryComments("Roles");
                        var roleTypeDesc = rows.FirstOrDefault(r => r["columnName"].ToString() == "RoleType");
                        Assert.IsNotNull(roleTypeDesc, "枚举属性必须有注释。");
                        var comment = roleTypeDesc["Comment"].ToString();
                        Assert.AreEqual(comment, @"角色的类型
0:(Normal, 一般)
1:(Administrator, 管理员)");

                        //WF_ 开头的动态属性。
                    }
                }
            }
        }

        internal static bool IsTestDbSQLite()
        {
            var dbSettings = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
            return dbSettings.ProviderName == DbSetting.Provider_SQLite;
        }

        #region public class DMT_ManualMigrateTest

        public class DMT_ManualMigrate_AddTable : ManualDbMigration
        {
            public override string DbSetting
            {
                get { return UnitTestEntityRepositoryDataProvider.DbSettingName; }
            }

            protected override void Up()
            {
                this.CreateTable("TestingTable", "Id", DbType.Int32, null, true);
                this.CreateNormalColumn("TestingTable", "Name", DbType.String);

                var dbSetting = Rafy.Data.DbSetting.FindOrCreate(DbSetting);
                if (dbSetting.ProviderName == Rafy.Data.DbSetting.Provider_SqlCe ||
                    dbSetting.ProviderName == Rafy.Data.DbSetting.Provider_SqlClient)
                {
                    //Sql 中使用的是 Identity 来生成主键
                    this.RunSql("insert into TestingTable (Name) values('TestingName1')");
                    this.RunSql("insert into TestingTable (Name) values('TestingName2')");
                }
                else
                {
                    this.RunSql("insert into TestingTable (Id, Name) values(1, 'TestingName1')");
                    this.RunSql("insert into TestingTable (Id, Name) values(2, 'TestingName2')");
                }
            }

            protected override void Down()
            {
                this.RunSql("delete from TestingTable where id = 1 or id = 2");

                this.DropNormalColumn("TestingTable", "Name", DbType.String);
                this.DropTable("TestingTable", "Id", DbType.Int32, null, true);
            }

            public override DateTime TimeId
            {
                get
                {
                    return DateTime.Now.AddDays(1);
                }
            }

            public override string Description
            {
                get { return "单元测试 - 数据库手工升级 - 建立一张表，一个字段，两行数据"; }
            }

            public override ManualMigrationType Type
            {
                get { return ManualMigrationType.Schema; }
            }
        }

        #endregion

        #region public class DMT_ManualMigrateEntity

        public class DMT_ManualMigrate_AddEntity : ManualDbMigration
        {
            public override string DbSetting
            {
                get { return UnitTestEntityRepositoryDataProvider.DbSettingName; }
            }

            protected override void Up()
            {
                this.RunCode(db =>
                {
                    var repo = RF.Find<TestUser>();
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
                    var repo = RF.Find<TestUser>();
                    var all = repo.GetAll();
                    foreach (var user in all) { user.PersistenceStatus = PersistenceStatus.Deleted; }
                    repo.Save(all);
                });
            }

            public override DateTime TimeId
            {
                get
                {
                    return DateTime.Now.AddDays(2);
                }
            }

            public override string Description
            {
                get { return "单元测试 - 数据库手工升级 - 使用实体类创建数据"; }
            }

            public override ManualMigrationType Type
            {
                get { return ManualMigrationType.Data; }
            }
        }

        #endregion
    }
}