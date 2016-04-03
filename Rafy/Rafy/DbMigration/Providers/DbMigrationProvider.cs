/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120424
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120424
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using Rafy.Data;
using Rafy.DbMigration.SqlServer;
using Rafy.Data.Providers;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 数据库迁移的提供器。
    /// 
    /// 各种不同的数据库使用不同的提供器程序。
    /// </summary>
    public abstract class DbMigrationProvider
    {
        /// <summary>
        /// 该提供器可用的数据库信息
        /// </summary>
        public DbSetting DbSetting { get; internal set; }

        /// <summary>
        /// 创建一个数据库结构读取器
        /// </summary>
        /// <returns></returns>
        public abstract IMetadataReader CreateSchemaReader();

        /// <summary>
        /// 创建一个执行生成器
        /// </summary>
        /// <returns></returns>
        public abstract RunGenerator CreateRunGenerator();

        /// <summary>
        /// 创建一个数据库备份器
        /// </summary>
        /// <returns></returns>
        public abstract IDbBackuper CreateDbBackuper();
    }
}