/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Rafy.DbMigration
{
    /// <summary>
    /// Operation 表示一个数据库升级的最小操作。
    /// 同时，Operation 同样可以是一个单独的数据库升级项，所以它继承自 DbMigration。
    /// 
    /// Operation 存在的意义是把数据库操作抽象化，以方便跨库。
    /// 
    /// 同时，所有的 MigrationOperation 作为迁移对象时，表示自动迁移。
    /// </summary>
    public abstract class MigrationOperation : DbMigration
    {
        internal override MigrationType GetMigrationType()
        {
            return MigrationType.AutoMigration;
        }

        internal DateTime RuntimeTimeId { get; set; }

        public override string Description
        {
            get { return this.GetType().Name; }
        }

        public override DateTime TimeId
        {
            get { return this.RuntimeTimeId; }
        }

        /// <summary>
        /// 升级时，生成的操作即是本身。
        /// </summary>
        protected override sealed void Up()
        {
            this.AddOperation(this);
        }
    }
}