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
using System.Text.RegularExpressions;
using Rafy.DbMigration.Operations;
using System.Data;
using Rafy.Data;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 表示一个可升级、可回滚的用户数据库升级项。
    /// 
    /// 该类及该类的子类需要支持 Xml 序列化，以支持存储到历史库中。
    /// </summary>
    public abstract class ManualDbMigration : DbMigration
    {
        internal override MigrationType GetMigrationType()
        {
            return MigrationType.ManualMigration;
        }

        /// <summary>
        /// 对应的数据库
        /// </summary>
        public abstract string DbSetting { get; }

        /// <summary>
        /// 手工迁移的类型：结构/数据。
        /// </summary>
        public abstract ManualMigrationType Type { get; }

        /// <summary>
        /// 从类的命名中获取该更新的时间点。手工更新必须使用以下格式命名类：“_20110107_093040_ClassName”。
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">手工更新必须使用以下格式命名类：“_20110107_093040_ClassName”。</exception>
        public override DateTime TimeId
        {
            get
            {
                var name = this.GetType().Name;
                var m = Regex.Match(name, @"^(?<time>_\d{8}_\d{6}_)");
                if (!m.Success)
                {
                    throw new InvalidOperationException("手工更新必须使用以下格式命名类：“_20110107_093040_ClassName”。");
                }
                var time = m.Groups["time"].Value;

                var value = DateTime.ParseExact(time, "_yyyyMMdd_HHmmss_", null);

                return value;
            }
        }

        #region 方便的 API

        /*********************** 代码块解释 *********************************
         * 
         * 此内的 API 主要方便用户写手工更新时使用。
         * 暂时写两个意思一下好了，以后看场景的运用再添加。
         * 
         **********************************************************************/

        protected void RunSql(string sql)
        {
            this.AddOperation(new RunSql()
            {
                Sql = sql
            });
        }

        protected void RunCode(Action<IDbAccesser> action)
        {
            this.AddOperation(new RunAction()
            {
                Action = action
            });
        }

        protected void CreateTable(string tableName, string pkName, DbType pkDbType, string length, bool isPkIdentity)
        {
            this.AddOperation(new CreateTable
            {
                TableName = tableName,
                PKName = pkName,
                PKDataType = pkDbType,
                PKLength = length,
                PKIdentity = isPkIdentity,
            });
        }

        protected void DropTable(string tableName, string pkName, DbType pkDbType, string length, bool isPkIdentity)
        {
            this.AddOperation(new DropTable
            {
                TableName = tableName,
                PKName = pkName,
                PKDataType = pkDbType,
                PKLength = length,
                PKIdentity = isPkIdentity,
            });
        }

        protected void CreateNormalColumn(string tableName, string columnName, DbType dataType, string length = null, bool isPrimaryKey = false, bool isIdentity = false)
        {
            this.AddOperation(new CreateNormalColumn
            {
                TableName = tableName,
                ColumnName = columnName,
                DataType = dataType,
                Length = length,
                IsPrimaryKey = isPrimaryKey,
                IsIdentity = isIdentity,
            });
        }

        protected void DropNormalColumn(string tableName, string columnName, DbType dataType, string length = null, bool isPrimaryKey = false, bool isIdentity = false)
        {
            this.AddOperation(new DropNormalColumn
            {
                TableName = tableName,
                ColumnName = columnName,
                DataType = dataType,
                Length = length,
                IsPrimaryKey = isPrimaryKey,
                IsIdentity = isIdentity,
            });
        }

        protected void AddPKConstraint(string tableName, string pkName, DbType pkDbType, string pkLength)
        {
            this.AddOperation(new AddPKConstraint
            {
                TableName = tableName,
                ColumnName = pkName,
                DataType = pkDbType,
                Length = pkLength,
            });
        }

        protected void RemovePKConstraint(string tableName, string pkName, DbType pkDbType, string pkLength)
        {
            this.AddOperation(new RemovePKConstraint
            {
                TableName = tableName,
                ColumnName = pkName,
                DataType = pkDbType,
                Length = pkLength,
            });
        }

        #endregion
    }

    /// <summary>
    /// 手动升级的类型
    /// </summary>
    public enum ManualMigrationType
    {
        /// <summary>
        /// 手动结构升级
        /// </summary>
        Schema,

        /// <summary>
        /// 手动数据升级
        /// </summary>
        Data
    }
}