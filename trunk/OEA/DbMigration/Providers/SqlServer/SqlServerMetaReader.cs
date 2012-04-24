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
using DbMigration.Model;
using hxy.Common.Data;
using System.Data;
using System.Data.Common;
using hxy.Common;

namespace DbMigration.SqlServer
{
    /// <summary>
    /// SqlServer 2005 数据库的元数据读取器
    /// </summary>
    public class SqlServerMetaReader : IMetadataReader
    {
        private DbSetting _dbSetting;

        private IDBAccesser _db;

        public SqlServerMetaReader(DbSetting dbSetting)
        {
            if (dbSetting == null) throw new ArgumentNullException("dbSetting");

            this._dbSetting = dbSetting;
            this._db = new DBAccesser(dbSetting);
        }

        protected IDBAccesser Db
        {
            get { return this._db; }
        }

        public Database Read()
        {
            //string name = Regex.Match(this._db.Connection.ConnectionString, @"Initial Catalog=\s*(?<dbName>\w+)\s*")
            //    .Groups["dbName"].Value;

            Database database = new Database(this._dbSetting.Database);

            var conn = this._db.Connection;

            try
            {
                conn.Open();

                //this._db.ExecuteText(@"USE " + this._dbName);

                this.LoadAllTables(database);

                this.LoadAllColumns(database);

                this.LoadAllForeignConstraints(database);
            }
            catch (DbException)// ex)
            {
                database.Removed = true;

                //conn.Close();
                //throw ex;
            }
            finally
            {
                conn.Close();
            }

            return database;
        }

        /// <summary>
        /// 添加所有表
        /// </summary>
        /// <param name="database"></param>
        protected virtual void LoadAllTables(Database database)
        {
            using (var reader = this._db.QueryDataReader(@"select * from INFORMATION_SCHEMA.TABLES"))
            {
                while (reader.Read())
                {
                    try
                    {
                        string tableName = reader["TABLE_NAME"].ToString();
                        string tableType = reader["TABLE_TYPE"].ToString().ToLower();

                        //SqlServer 中是 "BASE TABLE"
                        //SQLCE 中是 "TABLE"
                        if (tableType.Contains("table"))
                        {
                            //string schemaName = reader["SCHEMA_NAME"].ToString();

                            Table table = new Table(tableName, database);

                            database.Tables.Add(table);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        /// <summary>
        /// 加载每个表的所有列
        /// </summary>
        /// <param name="database"></param>
        protected virtual void LoadAllColumns(Database database)
        {
            foreach (Table table in database.Tables)
            {
                using (var columnsReader = this._db.QueryDataReader(@"
SELECT C.COLUMN_NAME, C.IS_NULLABLE, C.DATA_TYPE, C.CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS C
WHERE C.TABLE_NAME = {0}
", table.Name))
                {
                    while (columnsReader.Read())
                    {
                        try
                        {
                            string columnName = columnsReader["COLUMN_NAME"].ToString();
                            string sqlType = columnsReader["DATA_TYPE"].ToString();

                            DbType dbType = DbTypeHelper.ConvertFromSQLTypeString(sqlType);
                            Column column = new Column(dbType, columnName, table);

                            column.IsRequired = string.Compare(columnsReader["IS_NULLABLE"].ToString(), "no", true) == 0;

                            table.Columns.Add(column);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    table.SortColumns();
                }
            }
        }

        /// <summary>
        /// 加载外键
        /// </summary>
        /// <param name="database"></param>
        private void LoadAllForeignConstraints(Database database)
        {
            List<Constraint> allConstrains = this.ReadAllConstrains();
            foreach (var table in database.Tables)
            {
                foreach (var column in table.Columns)
                {
                    this.DealColumnConstraints(column, allConstrains);
                }
            }
        }

        /// <summary>
        /// 处理主键和外键
        /// </summary>
        /// <param name="column"></param>
        /// <param name="allConstraints">所有的约束</param>
        protected virtual void DealColumnConstraints(Column column, IList<Constraint> allConstraints)
        {
            var database = column.Table.DataBase;

            var constraints = allConstraints.Where(c => c.COLUMN_NAME == column.Name && c.TABLE_NAME == column.Table.Name).ToList();

            foreach (var constraint in constraints)
            {
                //主键
                if (string.Compare(constraint.CONSTRAINT_TYPE, "PRIMARY KEY", true) == 0)
                {
                    column.IsPrimaryKey = true;
                }
                else if (string.Compare(constraint.CONSTRAINT_TYPE, "FOREIGN KEY", true) == 0)
                {
                    #region 外键

                    if (string.IsNullOrWhiteSpace(constraint.PREP)) throw new ArgumentNullException("constraint.PREP");
                    bool deleteCascade = string.Compare(constraint.DELETE_RULE, "CASCADE", true) == 0;
                    var pkTable = database.FindTable(constraint.PK_TABLE_NAME);
                    if (pkTable == null) throw new ArgumentNullException("pkTable");
                    var pkColumn = pkTable.FindColumn(constraint.PK_COLUMN_NAME);

                    column.ForeignConstraint = new ForeignConstraint(pkColumn)
                    {
                        NeedDeleteCascade = deleteCascade,
                        ConstraintName = constraint.CONSTRAINT_NAME
                    };

                    #endregion
                }
            }
        }

        protected virtual List<Constraint> ReadAllConstrains()
        {
            List<Constraint> allConstrains = new List<Constraint>();

            #region 缓存数据库中的所有约束

            using (var constraintReader = this._db.QueryDataReader(@"
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
                        PREP = constraintReader["PREP"].ToString(),
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

        protected class Constraint
        {
            public string CONSTRAINT_NAME;
            public string CONSTRAINT_TYPE;
            public string TABLE_NAME;
            public string COLUMN_NAME;
            public string FK_TABLE_NAME;
            public string FK_COLUMN_NAME;
            public string PREP;
            public string PK_TABLE_NAME;
            public string PK_COLUMN_NAME;
            public string UNIQUE_CONSTRAINT_NAME;
            public string DELETE_RULE;
        }
    }
}