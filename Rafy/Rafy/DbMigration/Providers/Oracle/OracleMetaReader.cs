/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120427
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120427
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

namespace Rafy.DbMigration.Oracle
{
    /// <summary>
    /// Oracle 数据库的元数据读取器
    /// </summary>
    public class OracleMetaReader : DbMetaReader
    {
        public OracleMetaReader(DbSetting dbSetting) : base(dbSetting) { }

        protected override void LoadAllTables(Database database)
        {
            using (var reader = this.Db.QueryDataReader(@"select * from user_tables"))
            {
                while (reader.Read())
                {
                    string tableName = reader["TABLE_NAME"].ToString();

                    Table table = new Table(tableName, database);

                    database.Tables.Add(table);
                }
            }
        }

        protected override void LoadAllColumns(Database database)
        {
            foreach (Table table in database.Tables)
            {
                using (var columnsReader = this.Db.QueryDataReader(
@"SELECT * FROM user_tab_columns WHERE TABLE_NAME = {0}",
table.Name))
                {
                    while (columnsReader.Read())
                    {
                        string columnName = columnsReader["COLUMN_NAME"].ToString();
                        string sqlType = columnsReader["DATA_TYPE"].ToString().ToLower();
                        if (sqlType == "number")
                        {
                            var dataScale = columnsReader["DATA_SCALE"].ToString();
                            if (dataScale == "0") { sqlType = "integer"; }
                        }

                        DbType dbType = OracleDbTypeHelper.ConvertFromOracleTypeString(sqlType);
                        Column column = new Column(columnName, dbType, null, table);
                        column.IsRequired = columnsReader["NULLABLE"].ToString() == "N";

                        table.Columns.Add(column);
                    }

                    table.SortColumns();
                }
            }
        }

        protected override List<Constraint> ReadAllConstrains()
        {
            List<Constraint> allConstrains = new List<Constraint>();

            #region 缓存数据库中的所有约束

            using (var constraintReader = this.Db.QueryDataReader(
@"SELECT 
C.CONSTRAINT_NAME, C.CONSTRAINT_TYPE, C.TABLE_NAME, C.COLUMN_NAME, CR.TABLE_NAME FK_TABLE_NAME, CR.COLUMN_NAME FK_COLUMN_NAME,
CR.PREP, CR.TABLE_NAME PK_TABLE_NAME, CR.COLUMN_NAME PK_COLUMN_NAME, CR.CONSTRAINT_NAME UNIQUE_CONSTRAINT_NAME, C.DELETE_RULE
FROM 
(
--外键或主键表列
SELECT C.CONSTRAINT_NAME, C.CONSTRAINT_TYPE, C.TABLE_NAME, COL.COLUMN_NAME, C.R_CONSTRAINT_NAME, C.DELETE_RULE
FROM USER_CONSTRAINTS C INNER JOIN USER_CONS_COLUMNS COL ON C.CONSTRAINT_NAME = COL.CONSTRAINT_NAME
WHERE (C.CONSTRAINT_TYPE = 'P' OR C.CONSTRAINT_TYPE= 'R') AND INSTR(C.CONSTRAINT_NAME, '$') = 0
) C LEFT JOIN 
(
--主键表列
SELECT C.CONSTRAINT_NAME, C.TABLE_NAME, COL.COLUMN_NAME, 'TO' PREP
FROM USER_CONSTRAINTS C INNER JOIN USER_CONS_COLUMNS COL ON C.CONSTRAINT_NAME = COL.CONSTRAINT_NAME
WHERE C.CONSTRAINT_TYPE = 'P' AND INSTR(C.CONSTRAINT_NAME, '$') = 0
) CR ON C.R_CONSTRAINT_NAME = CR.CONSTRAINT_NAME"))
            {
                while (constraintReader.Read())
                {
                    var c = new Constraint()
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
                    };

                    if (c.CONSTRAINT_TYPE == "P") { c.CONSTRAINT_TYPE = "PRIMARY KEY"; }
                    else if (c.CONSTRAINT_TYPE == "R") { c.CONSTRAINT_TYPE = "FOREIGN KEY"; }

                    allConstrains.Add(c);
                }
            }

            #endregion

            return allConstrains;
        }
    }
}