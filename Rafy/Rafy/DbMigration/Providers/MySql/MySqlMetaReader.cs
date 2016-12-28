/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161226 15:23
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
    /// MySql数据库结构的读取器
    /// </summary>
    public sealed class MySqlMetaReader : DbMetaReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbSetting"></param>
        public MySqlMetaReader(DbSetting dbSetting) : base(dbSetting)
        {
        }

        /// <summary>
        /// 加载每个表的所有列
        /// </summary>
        /// <param name="database"></param>
        protected override void LoadAllColumns(Database database)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 添加所有表
        /// </summary>
        /// <param name="database"></param>
        protected override void LoadAllTables(Database database)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 加载外键
        /// </summary>
        /// <returns></returns>
        protected override List<Constraint> ReadAllConstrains()
        {
            throw new NotImplementedException();
        }
    }
}