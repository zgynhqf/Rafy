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
        /// 加载指定数据库中的每个表的所有列
        /// </summary>
        /// <param name="database">需要加载列的数据库对象</param>
        protected override void LoadAllColumns(Database database)
        {
            foreach (Table table in database.Tables)
            {
                using (var columnsReader = this.Db.QueryDataReader(@"SHOW FULL COLUMNS FROM `" + table.Name + "`;"))
                {
                    while (columnsReader.Read())
                    {
                        string columnName = columnsReader["Field"].ToString();
                        string sqlType = columnsReader["Type"].ToString();

                        DbType dbType = MySqlDbTypeHelper.ConvertFromMySqlTypeString(sqlType);
                        Column column = new Column(columnName, dbType, null, table);
                        column.IsRequired = string.Compare(columnsReader["Null"].ToString(), "Yes", true) != 0;
                        column.IsIdentity = string.Equals(columnsReader["Extra"].ToString(), "auto_increment", StringComparison.CurrentCultureIgnoreCase);
                        table.Columns.Add(column);
                    }
                    table.SortColumns();
                }
            }
        }

        /// <summary>
        /// 加载指定数据库的所有的数据表
        /// </summary>
        /// <param name="database">待加载表的数据库对象</param>
        protected override void LoadAllTables(Database database)
        {
            using (var reader = this.Db.QueryDataReader(@"select table_name from information_schema.tables where table_schema='" + database.Name + "';"))
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
        /// 加载所有的约束
        /// </summary>
        /// <returns>以列表的形式返回所有约束数据</returns>
        protected override List<Constraint> ReadAllConstrains()
        {
            List<Constraint> allConstrains = new List<Constraint>();

            #region 缓存数据库中的所有约束

            using (var constraintReader = this.Db.QueryDataReader(
@"select O.CONSTRAINT_SCHEMA,O.CONSTRAINT_NAME,O.TABLE_SCHEMA,O.TABLE_NAME,O.COLUMN_NAME,O.REFERENCED_TABLE_SCHEMA,O.REFERENCED_TABLE_NAME,O.REFERENCED_COLUMN_NAME,O.UPDATE_RULE,O.DELETE_RULE,O.UNIQUE_CONSTRAINT_NAME,T.CONSTRAINT_TYPE
from (
    select K.CONSTRAINT_SCHEMA,K.CONSTRAINT_NAME,K.TABLE_SCHEMA,K.TABLE_NAME,K.COLUMN_NAME,K.REFERENCED_TABLE_SCHEMA,K.REFERENCED_TABLE_NAME,K.REFERENCED_COLUMN_NAME,R.UPDATE_RULE,R.DELETE_RULE,R.UNIQUE_CONSTRAINT_NAME
        from information_schema.KEY_COLUMN_USAGE K 
            LEFT join information_schema.REFERENTIAL_CONSTRAINTS R on K.CONSTRAINT_NAME=R.CONSTRAINT_NAME
) as O 
inner join Information_schema.TABLE_CONSTRAINTS T on O.Table_Name=T.TABLE_NAME and T.CONSTRAINT_NAME=O.CONSTRAINT_NAME
where O.CONSTRAINT_SCHEMA!='mysql' and O.CONSTRAINT_SCHEMA!='sys' and O.CONSTRAINT_SCHEMA='" + this.Db.Connection.Database + "'"))
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

            #endregion

            return allConstrains;
        }
    }
}