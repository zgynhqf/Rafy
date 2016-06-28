/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration;
using Rafy.DbMigration.History;
using Rafy.DbMigration.Model;
using Rafy;
using Rafy.Data;

namespace Rafy.Domain.ORM.DbMigration
{
    /// <summary>
    /// 在 DbMigrationContext 的基础上实现以下功能：
    /// * 读取元数据，获取目标数据库 Schema
    /// * 使用 DbMigrationHistory 库来支持历史日志
    /// </summary>
    public class RafyDbMigrationContext : DbMigrationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RafyDbMigrationContext"/> class.
        /// </summary>
        /// <param name="dbSetting">The database setting.</param>
        public RafyDbMigrationContext(string dbSetting) : this(DbSetting.FindOrCreate(dbSetting)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RafyDbMigrationContext"/> class.
        /// </summary>
        /// <param name="dbSetting">The database setting.</param>
        public RafyDbMigrationContext(DbSetting dbSetting)
            : base(dbSetting)
        {
            this.ManualMigrations = new RafyUserMigrations();
            this.ClassMetaReader = new ClassMetaReader(this.DbSetting);

            //this.DbVersionProvider = new RafyDbVersionProvider();

            //如果需要使用 DbHistoryHistory 库来记录升级日志，可使用以下代码。
            //this.HistoryRepository = new DbHistoryRepository();
        }

        /// <summary>
        /// 从实体类型元数据中读取数据库结构的读取器。
        /// </summary>
        public ClassMetaReader ClassMetaReader { get; private set; }

        /// <summary>
        /// 使用实体类型的数据库映射元数据来自动更新数据库。
        /// </summary>
        /// <returns></returns>
        public void AutoMigrate()
        {
            this.ClassMetaReader.ReadComment = false;

            var dbInClassMeta = this.ClassMetaReader.Read();

            this.MigrateTo(dbInClassMeta);
        }

        /// <summary>
        /// 使用实体类中的注释来更新数据库中的相关注释内容。
        /// 注意，要成功使用此方法，需要在编译领域实体所在的程序集时，同时生成对应的 XML 注释文件。
        /// </summary>
        public void RefreshComments()
        {
            this.ClassMetaReader.ReadComment = true;

            var dbInClassMeta = this.ClassMetaReader.Read();

            this.RefreshComments(dbInClassMeta);
        }
    }
}