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

namespace Rafy.DbMigration.SQLite
{
    /// <summary>
    /// SQLite数据库结构的读取器
    /// https://www.cnblogs.com/wuhuacong/archive/2012/03/09/2387817.html
    /// </summary>
    public sealed class SQLiteMetaReader : DbMetaReader
    {
        /// <summary>
        /// 构造函数 初始化配置
        /// </summary>
        /// <param name="dbSetting">数据库配置信息</param>
        public SQLiteMetaReader(DbSetting dbSetting) : base(dbSetting) { }

        private bool _hasSequence = false;

        /// <summary>
        /// 加载指定数据库的所有的数据表
        /// </summary>
        /// <param name="database">待加载表的数据库对象</param>
        protected override void LoadAllTables(Database database)
        {
            using (var reader = this.Db.QueryDataReader(@"SELECT * FROM SQLITE_MASTER WHERE TYPE='table';"))
            {
                while (reader.Read())
                {
                    string tableName = reader["TBL_NAME"].ToString();
                    if (tableName.EqualsIgnoreCase("SQLITE_SEQUENCE"))
                    {
                        _hasSequence = true;
                    }
                    else
                    {
                        Table table = new Table(tableName, database);
                        database.Tables.Add(table);
                    }
                }
            }
        }

        /// <summary>
        /// 加载指定数据库中的每个表的所有列
        /// </summary>
        /// <param name="database">需要加载列的数据库对象</param>
        protected override void LoadAllColumns(Database database)
        {
            foreach (Table table in database.Tables)
            {
                //1.https://www.cnblogs.com/wuhuacong/archive/2012/03/09/2387817.html
                //2.select sql from sqlite_master where name = 'zzzDbMigrationVersion'
                //3.pragma table_info(zzzDbMigrationVersion)
                using (var columnsReader = this.Db.QueryDataReader(@"PRAGMA TABLE_INFO(`" + table.Name + "`)"))
                {
                    while (columnsReader.Read())
                    {
                        string columnName = columnsReader["name"].ToString();
                        string sqlType = columnsReader["type"].ToString();

                        DbType dbType = SQLiteDbTypeConverter.Instance.ConvertToDbType(sqlType);
                        Column column = new Column(columnName, dbType, null, table);
                        column.IsRequired = Convert.ToInt32(columnsReader["notnull"]) == 1;
                        column.IsPrimaryKey = Convert.ToInt32(columnsReader["pk"]) == 1;

                        table.Columns.Add(column);
                    }
                }
            }
        }

        /// <summary>
        /// 加载指定数据库的所有表中的自增列。
        /// </summary>
        /// <param name="database">指定的数据库对象</param>
        protected override void LoadAllIdentities(Database database)
        {
            //https://www.cnblogs.com/z5337/p/3637388.html
            //SQLite 中只支持使用 int 类型的主键作为自增列。

            if (!_hasSequence) return;

            var tablesHasId = this.Db.QueryLiteDataTable(@"SELECT NAME FROM SQLITE_SEQUENCE")
                .Rows.Select(r => r["NAME"].ToString()).ToArray();
            foreach (var table in database.Tables)
            {
                if (tablesHasId.Any(t => t.EqualsIgnoreCase(table.Name)))
                {
                    var pk = table.FindPrimaryColumn();
                    if (pk.DbType == DbType.Int32)
                    {
                        pk.IsIdentity = true;
                    }
                }
            }
        }

        /// <summary>
        /// 加载所有的约束
        /// </summary>
        /// <returns>以列表的形式返回所有约束数据</returns>
        protected override IList<Constraint> ReadAllConstrains(Database database)
        {
            List<Constraint> allConstrains = new List<Constraint>();

            //https://www.runoob.com/sqlite/sqlite-pragma.html
            //PRAGMA foreign_key_list

            return allConstrains;
        }
    }
}