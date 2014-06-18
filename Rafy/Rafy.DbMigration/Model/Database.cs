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
using Rafy;
using System.Diagnostics;
using Rafy.DbMigration.History;

namespace Rafy.DbMigration.Model
{
    /// <summary>
    /// 表示一个数据库的 Schema 定义
    /// </summary>
    [DebuggerDisplay("Database: {Name}")]
    public class Database : Extendable
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
        /// 通过表名找到对应的表。
        /// 忽略大小写。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Table FindTable(string name)
        {
            return this.Tables.FirstOrDefault(t => t.Name.EqualsIgnoreCase(name));
        }

        /// <summary>
        /// 根据外键关系为表排序
        /// 有外键关系的表放在后面，没有关系的表放在前面。（被引用的表放在引用表的前面。）
        /// </summary>
        public void OrderByRelations()
        {
            var count = 0;

            var tables = this.Tables;
            for (int i = 0, l = tables.Count; i < l; i++)
            {
                var left = tables[i];
                var foreignTables = left.GetForeignTables();
                for (int j = i + 1; j < l; j++)
                {
                    var right = tables[j];
                    if (foreignTables.Contains(right))
                    {
                        tables[i] = right;
                        tables[j] = left;
                        count++;
                        if (count > 100000)
                        {
                            //由于目前本方法很难实现环状外键的排序，所以暂时只有不支持了。
                            throw new InvalidProgramException(string.Format(
                                "在表 {0} 的外键链条中出现了环（例如：A->B->C->A），目前不支持此类型的数据库迁移。\r\n你可以使用手工迁移完成。",
                                left.Name));
                        }
                        i--;
                        break;
                    }
                }
            }
        }
    }
}
