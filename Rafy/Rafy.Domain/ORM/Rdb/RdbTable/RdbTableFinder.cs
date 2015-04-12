/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120605 18:24
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120605 18:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// DbTable 查找器。
    /// </summary>
    internal static class RdbTableFinder
    {
        /// <summary>
        /// 获取该实体对应的 ORM 运行时对象。
        /// 
        /// 如果该实体没有对应的实体元数据或者该实体没有被配置为映射数据库，
        /// 则本方法则无法创建对应的 ORM 运行时，此时会返回 null。
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns></returns>
        internal static RdbTable TableFor(Type entityType)
        {
            var repo = RepositoryFactoryHost.Factory.FindByEntity(entityType);
            return RdbDataProvider.Get(repo).DbTable;
        }

        internal static TextWriter AppendQuote(this TextWriter sql, RdbTable table, string identifier)
        {
            table.AppendQuote(sql, identifier);
            return sql;
        }

        internal static TextWriter AppendQuoteName(this TextWriter sql, RdbTable table)
        {
            table.AppendQuote(sql, table.Name);
            return sql;
        }

        internal static FormattedSql AppendQuote(this FormattedSql sql, RdbTable table, string identifier)
        {
            table.AppendQuote(sql.InnerWriter, identifier);
            return sql;
        }

        internal static FormattedSql AppendQuoteName(this FormattedSql sql, RdbTable table)
        {
            table.AppendQuote(sql.InnerWriter, table.Name);
            return sql;
        }
    }
}
