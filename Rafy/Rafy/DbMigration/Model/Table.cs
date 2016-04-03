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
using System.Data;

namespace Rafy.DbMigration.Model
{
    /// <summary>
    /// 表示数据库表的 Schema 定义
    /// </summary>
    [DebuggerDisplay("TableName: {Name}")]
    public class Table : Extendable
    {
        public Table(string name, Database dataBase)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (dataBase == null) throw new ArgumentNullException("dataBase");

            this.Columns = new List<Column>();
            this.Name = name;
            this.DataBase = dataBase;
        }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 表的注释。
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 所在的数据库 Schema
        /// </summary>
        public Database DataBase { get; private set; }

        /// <summary>
        /// 表中的所有列定义
        /// </summary>
        public IList<Column> Columns { get; private set; }

        /// <summary>
        /// 找到第一个主键定义
        /// </summary>
        /// <returns></returns>
        public Column FindPrimaryColumn()
        {
            return this.Columns.FirstOrDefault(c => c.IsPrimaryKey);
        }

        /// <summary>
        /// 找到除主键外的所有列
        /// </summary>
        /// <returns></returns>
        public IList<Column> FindNormalColumns()
        {
            return this.Columns.Where(c => !c.IsPrimaryKey).ToList();
        }

        //public IList<Column> FindPrimaryColumns()
        //{
        //    return this.Columns.Where(c => c.IsPrimaryKey).ToList();
        //}

        /// <summary>
        /// 这个表引用的外键表
        /// </summary>
        public IList<Table> GetForeignTables()
        {
            var tables = new List<Table>();

            foreach (var column in this.Columns)
            {
                if (column.IsForeignKey)
                {
                    if (!tables.Contains(column.ForeignConstraint.PKTable))
                    {
                        tables.Add(column.ForeignConstraint.PKTable);
                    }
                }
            }

            return tables;
        }

        /// <summary>
        /// 通过列名找到对应的列
        /// 忽略大小写。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Column FindColumn(string name)
        {
            return this.Columns.FirstOrDefault(c => c.Name.EqualsIgnoreCase(name));
        }

        /// <summary>
        /// 添加一列到这个表中。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="length">The length.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="isPrimaryKey">if set to <c>true</c> [is primary key].</param>
        /// <param name="foreignConstraint">The foreign constraint.</param>
        /// <returns></returns>
        public Column AddColumn(string name, DbType type,
            string length = null,
            bool isRequired = false,
            bool isPrimaryKey = false,
            ForeignConstraint foreignConstraint = null
            )
        {
            var column = new Column(name, type, length, this)
            {
                IsRequired = isRequired,
                IsPrimaryKey = isPrimaryKey,
                ForeignConstraint = foreignConstraint
            };

            this.Columns.Add(column);

            return column;
        }

        public void SortColumns()
        {
            (this.Columns as List<Column>).Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
        }
    }
}
