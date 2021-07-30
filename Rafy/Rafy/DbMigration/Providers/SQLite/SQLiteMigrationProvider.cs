/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161226 14:39
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.DbMigration.Model;

namespace Rafy.DbMigration.SQLite
{
    /// <summary>
    /// SQLite的数据库迁移提供程序
    /// 
    /// .NET5 中不支持 SQLCE，所以开始支持单机版数据库 SQLite，但是 SQLite 数据库在使用时有以下限制：
    /// * DDL 只支持有限的操作：创建表、删除表、添加列；其它全部不支持；
    /// * 自增列：只能是 Int32 的主键才支持自增列；
    /// * 外键：不支持生成外键及级联删除；（在实体配置中，可以设置：Meta.DeletingChildrenInMemory = true，打开内存中级联删除功能）
    /// * 字段：数据库的所有字段，都是可空的；
    /// * 不支持迁移历史（因为无法按照迁移历史来进行回滚）；
    /// 
    /// 所以在日常开发并同步到数据库时，添加字段会自动同步到数据库，其它操作请手工操作。
    /// </summary>
    public sealed class SQLiteMigrationProvider : DbMigrationProvider
    {
        /// <summary>
        /// 创建一个数据库备份器
        /// </summary>
        /// <returns>暂不支持SQLite数据的备份</returns>
        public override IDbBackuper CreateDbBackuper()
        {
            throw new NotSupportedException("暂时不支持 SQLite 数据库的备份。");
        }

        /// <summary>
        /// 创建一个执行生成器
        /// </summary>
        /// <returns>返回SQLiteRunGenerator的实例对象</returns>
        public override RunGenerator CreateRunGenerator()
        {
            return new SQLiteRunGenerator();
        }

        /// <summary>
        /// 创建一个数据库结构读取器
        /// </summary>
        /// <returns>返回SQLiteMetaReader的实例对象</returns>
        public override IMetadataReader CreateSchemaReader()
        {
            return new SQLiteMetaReader(this.DbSetting);
        }
    }
}