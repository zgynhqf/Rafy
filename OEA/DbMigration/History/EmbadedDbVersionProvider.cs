/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110109
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110109
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hxy.Common;
using hxy.Common.Data;
using System.Data.Common;

namespace DbMigration.History
{
    /// <summary>
    /// 在当前数据库中直接嵌入一张表作为版本号存储地址的提供程序。
    /// </summary>
    public class EmbadedDbVersionProvider : DbVersionProvider
    {
        public const string TableName = "__DbMigrationVersion";

        internal IDBAccesser DBA;

        protected override DateTime GetDbVersionCore(string database)
        {
            if (this.DBA.Connection.Database != database) throw new InvalidOperationException();

            try
            {
                return this.Get();
            }
            catch (DbException)
            {
                try
                {
                    this.CreateTable();
                    return this.Get();
                }
                catch (DbException)
                {
                    return DefaultMinTime;
                }
            }
        }

        protected override Result SetDbVersionCore(string database, DateTime version)
        {
            if (this.DBA.Connection.Database != database) throw new InvalidOperationException();

            try
            {
                this.Set(version);
                return true;
            }
            catch (DbException)
            {
                try
                {
                    this.CreateTable();

                    this.Set(version);
                    return true;
                }
                catch (DbException)
                {
                    return false;
                }
            }
        }

        private DateTime Get()
        {
            var value = this.DBA.QueryValue("SELECT [VALUE] FROM [__DbMigrationVersion] WHERE [ID] = 1");
            if (value != DBNull.Value) { return (DateTime)value; }

            return DefaultMinTime;
        }

        private void Set(DateTime version)
        {
            this.DBA.ExecuteText("UPDATE [__DbMigrationVersion] SET [VALUE] = {0} WHERE [ID] = 1", version);
        }

        private void CreateTable()
        {
            this.DBA.ExecuteText(@"
CREATE TABLE [__DbMigrationVersion]
(
    [ID] INT NOT NULL,
    [Value] DATETIME NOT NULL,
    PRIMARY KEY ([ID])
)
");
            this.DBA.ExecuteText("INSERT INTO [__DbMigrationVersion] ([ID],[VALUE]) VALUES (1, {0})", DefaultMinTime);
        }

        protected internal override bool IsEmbaded()
        {
            return true;
        }
    }
}
