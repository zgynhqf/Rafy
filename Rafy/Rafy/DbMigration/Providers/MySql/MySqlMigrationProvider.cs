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

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql的数据库迁移提供程序
    /// </summary>
    public sealed class MySqlMigrationProvider : DbMigrationProvider
    {
        /// <summary>
        /// 创建一个数据库备份器
        /// </summary>
        /// <returns>暂不支持MySql数据的备份</returns>
        public override IDbBackuper CreateDbBackuper()
        {
            throw new NotSupportedException("暂时不支持 MySql 数据库的备份。");
        }

        /// <summary>
        /// 创建一个执行生成器
        /// </summary>
        /// <returns>返回MySqlRunGenerator的实例对象</returns>
        public override RunGenerator CreateRunGenerator()
        {
            return new MySqlRunGenerator();
        }

        /// <summary>
        /// 创建一个数据库结构读取器
        /// </summary>
        /// <returns>返回MySqlMetaReader的实例对象</returns>
        public override IMetadataReader CreateSchemaReader()
        {
            return new MySqlMetaReader(this.DbSetting);
        }
    }
}