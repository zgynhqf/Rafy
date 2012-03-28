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
using hxy.Common;
using System.Diagnostics;
using DbMigration.History;

namespace DbMigration.Model
{
    /// <summary>
    /// 表示一个数据库的 Schema 定义
    /// </summary>
    [DebuggerDisplay("Database: {Name}")]
    public class Database
    {
        public Database(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            this.Name = name;
            this.Tables = new List<Table>();
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 数据库并不存在
        /// </summary>
        public bool Removed { get; set; }

        /// <summary>
        /// 数据库中所包含的表
        /// </summary>
        public IList<Table> Tables { get; private set; }

        /// <summary>
        /// 通过表名找到对应的表
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Table FindTable(string name)
        {
            return this.Tables.FirstOrDefault(t => t.Name.EqualsIgnoreCase(name));
        }

        /// <summary>
        /// 根据外键关系为表排序
        /// </summary>
        internal void OrderByRelations()
        {
            var table = this.Tables;
            for (int i = 0, l = table.Count; i < l; i++)
            {
                var left = table[i];
                var foreignTables = left.GetForeignTables();
                for (int j = i + 1; j < l; j++)
                {
                    var right = table[j];
                    if (foreignTables.Contains(right))
                    {
                        table[i] = right;
                        table[j] = left;
                        i--;
                        break; ;
                    }
                }
            }
        }
    }
}
