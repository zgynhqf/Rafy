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
using Rafy.DbMigration.Model;
using Rafy.Data;
using System.Data;
using System.Data.Common;
using Rafy;

namespace Rafy.DbMigration.SqlServer
{
    /// <summary>
    /// SqlServer 2005 数据库的元数据读取器
    /// </summary>
    public class SqlServerMetaReader : DbMetaReader
    {
        public SqlServerMetaReader(DbSetting dbSetting) : base(dbSetting) { }

        /// <summary>
        /// 添加所有表
        /// </summary>
        /// <param name="database"></param>
        protected override void LoadAllTables(Database database)
        {
            using (var reader = this.Db.QueryDataReader(@"select * from INFORMATION_SCHEMA.TABLES"))
            {
                while (reader.Read())
                {
                    string tableName = reader["TABLE_NAME"].ToString();
                    string tableType = reader["TABLE_TYPE"].ToString().ToLower();

                    //SqlServer 中是 "BASE TABLE"，同时还会把一些系统表也查询出来，例如：sysdiagrams
                    //SQLCE 中是 "TABLE"
                    if (tableType.Contains("table") && !tableName.StartsWith("sys"))
                    {
                        //string schemaName = reader["SCHEMA_NAME"].ToString();

                        Table table = new Table(tableName, database);

                        database.Tables.Add(table);
                    }
                }
            }
        }

        /// <summary>
        /// 加载每个表的所有列
        /// </summary>
        /// <param name="database"></param>
        protected override void LoadAllColumns(Database database)
        {
            foreach (Table table in database.Tables)
            {
                using (var columnsReader = this.Db.QueryDataReader(@"
SELECT C.COLUMN_NAME, C.IS_NULLABLE, C.DATA_TYPE, C.CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS C
WHERE C.TABLE_NAME = {0}
", table.Name))
                {
                    while (columnsReader.Read())
                    {
                        string columnName = columnsReader["COLUMN_NAME"].ToString();
                        string sqlType = columnsReader["DATA_TYPE"].ToString();

                        //不再读取 Length
                        //string length = null;
                        //var lengthObj = columnsReader["CHARACTER_MAXIMUM_LENGTH"];
                        //if (lengthObj != null && !DBNull.Value.Equals(lengthObj) && lengthObj.ToString() != "-1")
                        //{
                        //    length = lengthObj.ToString();
                        //}

                        DbType dbType = SqlDbTypeHelper.ConvertFromSQLTypeString(sqlType);
                        Column column = new Column(columnName, dbType, null, table);

                        column.IsRequired = string.Compare(columnsReader["IS_NULLABLE"].ToString(), "no", true) == 0;

                        table.Columns.Add(column);
                    }

                    table.SortColumns();
                }
            }

            this.LoadIsIdentity(database);
        }

        protected override List<Constraint> ReadAllConstrains()
        {
            List<Constraint> allConstrains = new List<Constraint>();

            #region 缓存数据库中的所有约束

            using (var constraintReader = this.Db.QueryDataReader(@"
SELECT 
T1.CONSTRAINT_NAME, T1.CONSTRAINT_TYPE, T1.TABLE_NAME, T1.COLUMN_NAME,
T2.FK_TABLE_NAME, T2.FK_COLUMN_NAME, T2.PREP, T2.PK_TABLE_NAME, T2.PK_COLUMN_NAME, T2.UNIQUE_CONSTRAINT_NAME, T2.DELETE_RULE
FROM 
(
--外键或主键表列
    SELECT c.CONSTRAINT_NAME, c.CONSTRAINT_TYPE, c.TABLE_NAME, K.COLUMN_NAME
    FROM 
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS c
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k ON 
        c.CONSTRAINT_NAME = k.CONSTRAINT_NAME 
) T1
LEFT JOIN 
(
--主键表列
    SELECT 
    R.CONSTRAINT_NAME, 
    FKC.TABLE_NAME FK_TABLE_NAME, 
    FKC.COLUMN_NAME FK_COLUMN_NAME,
    ' TO ' PREP,
    PKC.TABLE_NAME PK_TABLE_NAME,
    PKC.COLUMN_NAME PK_COLUMN_NAME,
    R.UNIQUE_CONSTRAINT_NAME, 
    R.DELETE_RULE
    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS R
        INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE FKC ON R.CONSTRAINT_NAME = FKC.CONSTRAINT_NAME
        INNER JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE PKC ON R.UNIQUE_CONSTRAINT_NAME = PKC.CONSTRAINT_NAME
) T2
ON T1.CONSTRAINT_NAME = T2.CONSTRAINT_NAME
"))
            {
                while (constraintReader.Read())
                {
                    allConstrains.Add(new Constraint()
                    {
                        CONSTRAINT_NAME = constraintReader["CONSTRAINT_NAME"].ToString(),
                        CONSTRAINT_TYPE = constraintReader["CONSTRAINT_TYPE"].ToString(),
                        TABLE_NAME = constraintReader["TABLE_NAME"].ToString(),
                        COLUMN_NAME = constraintReader["COLUMN_NAME"].ToString(),
                        FK_TABLE_NAME = constraintReader["FK_TABLE_NAME"].ToString(),
                        FK_COLUMN_NAME = constraintReader["FK_COLUMN_NAME"].ToString(),
                        PK_TABLE_NAME = constraintReader["PK_TABLE_NAME"].ToString(),
                        PK_COLUMN_NAME = constraintReader["PK_COLUMN_NAME"].ToString(),
                        UNIQUE_CONSTRAINT_NAME = constraintReader["UNIQUE_CONSTRAINT_NAME"].ToString(),
                        DELETE_RULE = constraintReader["DELETE_RULE"].ToString()
                    });
                }
            }

            #endregion

            return allConstrains;
        }

        #region 读取列的 Identity 信息

        private void LoadIsIdentity(Database database)
        {
            var identities = this.ReadAllIdentities();
            foreach (var identity in identities)
            {
                var table = database.FindTable(identity.TableName);
                if (table != null)
                {
                    var column = table.FindColumn(identity.ColumnName);
                    if (column != null)
                    {
                        column.IsIdentity = true;
                    }
                }
            }
        }

        /// <summary>
        /// 查询当前数据库中所有的 Identity 列。
        /// </summary>
        /// <returns></returns>
        protected virtual List<IdentityColumn> ReadAllIdentities()
        {
            var list = new List<IdentityColumn>();

            #region 缓存数据库中的所有约束

            //all_objects.type：U 是用户表、IT 是内部表，对应的 type_desc 是 USER_TABLE 和 INTERNAL_TABLE
            using (var reader = this.Db.QueryDataReader(@"
SELECT c.name columnName, obj.name tableName, obj.type, obj.type_desc
    FROM [sys].[columns] c
    left join [sys].all_objects obj on c.object_id = obj.object_id
    where c.is_identity = 1 and obj.type = 'U'
"))
            {
                while (reader.Read())
                {
                    list.Add(new IdentityColumn()
                    {
                        ColumnName = reader["columnName"].ToString(),
                        TableName = reader["tableName"].ToString(),
                    });
                }
            }

            #endregion

            return list;
        }

        protected struct IdentityColumn
        {
            public string ColumnName;
            public string TableName;
        }

        #endregion
    }
}