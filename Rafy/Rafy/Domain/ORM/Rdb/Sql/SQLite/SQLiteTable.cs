/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210729
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210729 00:01
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.ORM.MySql;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.SQLite
{
    /// <summary>
    /// MySql的表对象
    /// </summary>
    internal sealed class SQLiteTable : MySqlTable
    {
        /// <summary>
        /// 构造函数 初始化仓库对象
        /// </summary>
        /// <param name="repository">仓库对象</param>
        /// <param name="dbProvider"></param>
        public SQLiteTable(IRepositoryInternal repository, string dbProvider) : base(repository, dbProvider)
        {
            _insertSql = new Lazy<string>(() =>
            {
                //https://www.cnblogs.com/keitsi/p/5558985.html
                var generatedSql = this.GenerateInsertSQL(false);
                return $@"{generatedSql};
SELECT LAST_INSERT_ROWID() FROM [{this.Name}]";
            });
        }
    }
}