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
using Rafy;
using Rafy.Data;
using System.Data.Common;
using Rafy.DbMigration.Oracle;
using System.Data;
using Rafy.DbMigration.SqlServer;

namespace Rafy.DbMigration.History
{
    /// <summary>
    /// 在当前数据库中直接嵌入一张表作为版本号存储地址的提供程序。
    /// </summary>
    public class EmbadedDbVersionProvider : DbVersionProvider
    {
        public const string TableName = "zzzDbMigrationVersion";

        internal IDbAccesser DBA;

        protected override DateTime GetDbVersionCore()
        {
            try
            {
                this.CheckDbValid();

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

        protected override Result SetDbVersionCore(DateTime version)
        {
            try
            {
                this.CheckDbValid();

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
                    return "无法存储数据库版本号信息！";
                }
            }
        }

        private void CheckDbValid()
        {
            var conDb = this.DBA.Connection.Database;
            if (conDb != this.DbSetting.Database && !string.IsNullOrWhiteSpace(conDb)) throw new InvalidOperationException("连接使用的数据库与正在创建的数据不一致！");
        }

        private DateTime Get()
        {
            var value = this.DBA.QueryValue("SELECT VALUE FROM zzzDbMigrationVersion WHERE ID = 1");
            if (value != DBNull.Value) { return new DateTime(Convert.ToInt64(value)); }

            return DefaultMinTime;
        }

        private void Set(DateTime version)
        {
            this.DBA.ExecuteText("UPDATE zzzDbMigrationVersion SET VALUE = {0} WHERE ID = 1", version.Ticks);
        }

        private void CreateTable()
        {
            //不再使用 Date 类型，因为 Oracle 和 SQLServer 里面的数据的精度不一样。改为使用 LONG
            //Oracle 中的 DateTime 类型为 Date
            var timeType = this.DbSetting.ProviderName == DbSetting.Provider_Oracle ?
                OracleDbTypeHelper.ConvertToOracleTypeString(DbType.Int64) :
                SqlDbTypeHelper.ConvertToSQLTypeString(DbType.Int64);

            this.DBA.RawAccesser.ExecuteText(string.Format(@"
CREATE TABLE zzzDbMigrationVersion
(
    ID INT NOT NULL,
    Value {0} NOT NULL,
    PRIMARY KEY (ID)
)
", timeType));
            this.DBA.ExecuteText("INSERT INTO zzzDbMigrationVersion (ID,VALUE) VALUES (1, {0})", DefaultMinTime.Ticks);
        }

        protected internal override bool IsEmbaded()
        {
            return true;
        }
    }
}
