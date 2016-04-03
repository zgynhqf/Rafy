/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120423
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120423
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

namespace Rafy.DbMigration.SqlServerCe
{
    /// <summary>
    /// SqlServer CE 数据库的元数据读取器
    /// </summary>
    public class SqlServerCeMetaReader : SqlServer.SqlServerMetaReader
    {
        public SqlServerCeMetaReader(DbSetting dbSetting) : base(dbSetting) { }

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
    SELECT c.CONSTRAINT_NAME, c.CONSTRAINT_TYPE, c.TABLE_NAME, K.COLUMN_NAME
    FROM 
        INFORMATION_SCHEMA.TABLE_CONSTRAINTS c
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k ON 
        c.CONSTRAINT_NAME = k.CONSTRAINT_NAME 
) T1
LEFT JOIN 
(

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
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE FKC ON R.CONSTRAINT_NAME = FKC.CONSTRAINT_NAME
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PKC ON R.UNIQUE_CONSTRAINT_NAME = PKC.CONSTRAINT_NAME
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

        protected override List<SqlServer.SqlServerMetaReader.IdentityColumn> ReadAllIdentities()
        {
            var list = new List<IdentityColumn>();

            #region 缓存数据库中的所有约束

            using (var reader = this.Db.QueryDataReader(@"
SELECT c.TABLE_NAME, c.COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS c 
    WHERE AUTOINC_SEED IS NOT NULL
"))
            {
                while (reader.Read())
                {
                    list.Add(new IdentityColumn()
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString(),
                        TableName = reader["TABLE_NAME"].ToString(),
                    });
                }
            }

            #endregion

            return list;
        }
    }
}