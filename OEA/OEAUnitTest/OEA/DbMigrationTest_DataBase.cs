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

namespace OEAUnitTest.OEA
{
    [TestClass]
    public class DbMigrationDataBaseTest : TestBase
    {
        [ClassInitialize]
        public static void DbMigrationTest_ClassInitialize(TestContext context)
        {
            ClassInitialize(context, false);
        }

        [TestMethod]
        public void DMDBT_CreateDatabase()
        {
            using (var context = new OEADbMigrationContext("TestingDataBase"))
            {
                if (!context.DatabaseExists())
                {
                    var destination = new DestinationDatabase("TestingDataBase");
                    var tmpTable = new Table("TestingTable", destination);
                    tmpTable.AddColumn("Id", DbType.Int32, isPrimaryKey: true);
                    tmpTable.AddColumn("Name", DbType.String);
                    destination.Tables.Add(tmpTable);

                    context.MigrateTo(destination);

                    //历史记录
                    var histories = context.GetHistories();
                    Assert.IsTrue(histories.Count == 3);
                    Assert.IsTrue(histories[2] is CreateDatabase);

                    //数据库结构
                    Assert.IsTrue(context.DatabaseExists());
                }
            }
        }

        [TestMethod]
        public void DMDBT_DropDatabase_单独运行_可能失败()
        {
            //以下代码不能运行，会提示数据库正在被使用
            using (var context = new OEADbMigrationContext("TestingDataBase"))
            {
                if (context.DatabaseExists())
                {
                    //context.DeleteDatabase();
                    var database = new DestinationDatabase("TestingDataBase") { Removed = true };
                    context.MigrateTo(database);

                    //历史记录
                    var histories = context.GetHistories();
                    Assert.IsTrue(histories[0] is DropDatabase);

                    //数据库结构
                    Assert.IsTrue(!context.DatabaseExists());
                }

                context.ResetHistory();
            }

            //using (var context = new OEADbMigrationContext("TestingDataBase"))
            //{
            //    context.ManualMigrations.Add(new DMDBT_DropDatabase_Migration());
            //    context.MigrateManually();

            //    //历史记录
            //    var histories = context.GetHistories();
            //    Assert.IsTrue(histories[0] is DMDBT_DropDatabase_Migration);

            //    //数据库结构
            //    var database = context.DatabaseMetaReader.Read();
            //    Assert.IsTrue(database == null);

            //    context.ResetHistory();
            //}
        }

        //public class DMDBT_DropDatabase_Migration : ManualDbMigration
        //{
        //    public override string Database
        //    {
        //        get { return "TestingDataBase"; }
        //    }

        //    protected override void Up()
        //    {
        //        this.AddOperation(new DropDatabase
        //        {
        //            Database = this.Database
        //        });
        //    }

        //    protected override void Down() { }

        //    protected override DateTime GetTimeId()
        //    {
        //        return DateTime.Now;
        //    }

        //    protected override string GetDescription()
        //    {
        //        return "单元测试 - 数据库手工升级 - 删除测试数据库";
        //    }

        //    public override ManualMigrationType Type
        //    {
        //        get { return ManualMigrationType.Schema; }
        //    }
        //}

        //[TestMethod]
        //public void DMT_Backup()
        //{
        //    var fileName = @"D:\OEAUnitTest.bak";

        //    var context = new OEADbMigrationContext(ConnectionStringNames.OEA);

        //    var res = context.DbBackuper.BackupDatabase(UnitTestEntity.ConnectionString, fileName, true);

        //    Assert.IsTrue(res.Success);
        //}

        //[TestMethod]
        //public void DMT_Restore()
        //{
        //    var fileName = @"D:\OEAUnitTest.bak";

        //    if (File.Exists(fileName))
        //    {
        //        var context = new OEADbMigrationContext(ConnectionStringNames.OEA);

        //        var res = context.DbBackuper.RestoreDatabase(UnitTestEntity.ConnectionString, fileName);

        //        Assert.IsTrue(res.Success);
        //    }
        //}
    }
}