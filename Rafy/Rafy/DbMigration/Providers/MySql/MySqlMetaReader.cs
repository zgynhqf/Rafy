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
using System.Data;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql数据库结构的读取器
    /// </summary>
    public sealed class MySqlMetaReader : DbMetaReader
    {
        /// <summary>
        /// 构造函数 初始化配置
        /// </summary>
        /// <param name="dbSetting">数据库配置信息</param>
        public MySqlMetaReader(DbSetting dbSetting) : base(dbSetting) { }

        /// <summary>
        /// 加载指定数据库的所有的数据表
        /// </summary>
        /// <param name="database">待加载表的数据库对象</param>
        protected override void LoadAllTables(Database database)
        {
            using (var reader = this.Db.QueryDataReader(
@"SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = '" + database.Name + "'"))
            {
                while (reader.Read())
                {
                    string tableName = reader["TABLE_NAME"].ToString();
                    Table table = new Table(tableName, database);
                    database.Tables.Add(table);
                }
            }
        }

        /// <summary>
        /// 加载指定数据库中的每个表的所有列
        /// </summary>
        /// <param name="database">需要加载列的数据库对象</param>
        protected override void LoadAllColumns(Database database)
        {
            //用一句 Sql 将所有的表的所有字段都一次性查询出来。
            //不再使用 @"SHOW FULL COLUMNS FROM `" + table.Name + "`;"
            var sql = new StringBuilder(
@"SELECT TABLE_NAME, COLUMN_NAME, IS_NULLABLE, COLUMN_TYPE, EXTRA
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (");
            sql.Append(string.Join(",", database.Tables.Select(t => "'" + t.Name + "'")));
            sql.Append(") AND TABLE_SCHEMA = '").Append(database.Name).Append(@"'
ORDER BY TABLE_NAME");

            using (var columnsReader = this.Db.QueryDataReader(sql.ToString()))
            {
                Table currentTable = null;//当前正在处理的表。（放在循环外，有缓存的作用。）
                while (columnsReader.Read())
                {
                    //找到该列所对应的表。
                    var tableName = columnsReader["TABLE_NAME"].ToString();
                    if (currentTable == null || !currentTable.Name.EqualsIgnoreCase(tableName))
                    {
                        currentTable = database.FindTable(tableName);
                    }

                    string columnName = columnsReader["COLUMN_NAME"].ToString();
                    string sqlType = columnsReader["COLUMN_TYPE"].ToString();
                    DbType dbType = MySqlDbTypeConverter.Instance.ConvertToDbType(sqlType);

                    Column column = new Column(columnName, dbType, null, currentTable);

                    column.IsRequired = "NO".EqualsIgnoreCase(columnsReader["IS_NULLABLE"].ToString());
                    column.IsIdentity = columnsReader["EXTRA"].ToString().ToLower().Contains("auto_increment");

                    currentTable.Columns.Add(column);
                }
            }
        }

        /// <summary>
        /// 子类实现此方法，实现从数据库中读取出指定数据库的所有约束。
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>
        /// 以列表的形式返回所有约束数据
        /// </returns>
        protected override IList<Constraint> ReadAllConstrains(Database database)
        {
            var allConstrains = new List<Constraint>();

            using (var constraintReader = this.Db.QueryDataReader(
@"SELECT O.CONSTRAINT_SCHEMA, O.CONSTRAINT_NAME, O.TABLE_SCHEMA, O.TABLE_NAME, O.COLUMN_NAME, O.REFERENCED_TABLE_SCHEMA, O.REFERENCED_TABLE_NAME, O.REFERENCED_COLUMN_NAME, O.UPDATE_RULE, O.DELETE_RULE, O.UNIQUE_CONSTRAINT_NAME, T.CONSTRAINT_TYPE
FROM (
    SELECT K.CONSTRAINT_SCHEMA, K.CONSTRAINT_NAME, K.TABLE_SCHEMA, K.TABLE_NAME, K.COLUMN_NAME, K.REFERENCED_TABLE_SCHEMA, K.REFERENCED_TABLE_NAME, K.REFERENCED_COLUMN_NAME, R.UPDATE_RULE, R.DELETE_RULE, R.UNIQUE_CONSTRAINT_NAME
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE K 
        LEFT JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS R ON K.CONSTRAINT_NAME = R.CONSTRAINT_NAME
) AS O 
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS T ON O.TABLE_NAME = T.TABLE_NAME AND T.CONSTRAINT_NAME = O.CONSTRAINT_NAME
WHERE O.CONSTRAINT_SCHEMA != 'mysql' AND O.CONSTRAINT_SCHEMA != 'sys' AND O.CONSTRAINT_SCHEMA = '" + database.Name + "'"))
            {
                while (constraintReader.Read())
                {
                    var c = new Constraint()
                    {
                        CONSTRAINT_NAME = constraintReader["CONSTRAINT_NAME"].ToString(),
                        CONSTRAINT_TYPE = constraintReader["CONSTRAINT_TYPE"].ToString(),
                        TABLE_NAME = constraintReader["TABLE_NAME"].ToString(),
                        COLUMN_NAME = constraintReader["COLUMN_NAME"].ToString(),

                        FK_TABLE_NAME = constraintReader["TABLE_NAME"].ToString(),
                        FK_COLUMN_NAME = constraintReader["COLUMN_NAME"].ToString(),
                        PK_TABLE_NAME = constraintReader["REFERENCED_TABLE_NAME"].ToString(),
                        PK_COLUMN_NAME = constraintReader["REFERENCED_COLUMN_NAME"].ToString(),

                        UNIQUE_CONSTRAINT_NAME = constraintReader["UNIQUE_CONSTRAINT_NAME"].ToString(),
                        DELETE_RULE = constraintReader["DELETE_RULE"].ToString()
                    };

                    allConstrains.Add(c);
                }
            }

            return allConstrains;
        }

        protected override void LoadAllIdentities(Database database)
        {
            //do nothing.
            //自增列，已经在 LoadAllColumns 方法中直接加载了。
        }
    }
}