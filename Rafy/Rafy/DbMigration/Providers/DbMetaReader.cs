﻿/*******************************************************
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
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using Rafy;
using Rafy.Data;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 数据库的元数据读取器
    /// </summary>
    public abstract class DbMetaReader : IMetadataReader
    {
        private DbSetting _dbSetting;

        private IDbAccesser _db;

        public DbMetaReader(DbSetting dbSetting)
        {
            if (dbSetting == null) throw new ArgumentNullException("dbSetting");

            this._dbSetting = dbSetting;
            this._db = new DbAccesser(dbSetting);
        }

        public IDbAccesser Db
        {
            get { return this._db; }
        }

        public Database Read()
        {
            Database database = new Database(this._dbSetting.Database);

            var conn = this._db.Connection;

            try
            {
                conn.Open();

                this.LoadAllTables(database);

                this.LoadAllColumns(database);

                foreach (var table in database.Tables)
                {
                    table.SortColumns();
                }

                this.LoadAllConstraints(database);

                this.LoadAllIdentities(database);
            }
            catch (DbException)
            {
                database.Removed = true;
            }
            finally
            {
                conn.Close();
            }

            return database;
        }

        /// <summary>
        /// 加载指定数据库的所有的数据表
        /// </summary>
        /// <param name="database">待加载表的数据库对象</param>
        protected abstract void LoadAllTables(Database database);

        /// <summary>
        /// 加载指定数据库中的每个表的所有列
        /// </summary>
        /// <param name="database">需要加载列的数据库对象</param>
        protected abstract void LoadAllColumns(Database database);

        /// <summary>
        /// 加载指定数据库的所有表中的自增列。
        /// </summary>
        /// <param name="database">指定的数据库对象</param>
        protected abstract void LoadAllIdentities(Database database);

        /// <summary>
        /// 加载主键、外键等约束。
        /// </summary>
        /// <param name="database">需要加载约束的数据库对象</param>
        protected virtual void LoadAllConstraints(Database database)
        {
            var allConstrains = this.ReadAllConstrains(database);

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
        private void DealColumnConstraints(Column column, IList<Constraint> allConstraints)
        {
            var database = column.Table.DataBase;

            var constraints = allConstraints.Where(c => c.COLUMN_NAME == column.Name && c.TABLE_NAME == column.Table.Name).ToList();

            foreach (var constraint in constraints)
            {
                if (string.Compare(constraint.CONSTRAINT_TYPE, "PRIMARY KEY", true) == 0)
                {
                    #region 主键

                    column.IsPrimaryKey = true;

                    #endregion
                }
                else if (string.Compare(constraint.CONSTRAINT_TYPE, "FOREIGN KEY", true) == 0)
                {
                    #region 外键

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

        /// <summary>
        /// 子类实现此方法，实现从数据库中读取出指定数据库的所有约束。
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>以列表的形式返回所有约束数据</returns>
        protected abstract IList<Constraint> ReadAllConstrains(Database database);

        protected class Constraint
        {
            public string CONSTRAINT_NAME;
            public string CONSTRAINT_TYPE;
            public string TABLE_NAME;
            public string COLUMN_NAME;
            public string FK_TABLE_NAME;
            public string FK_COLUMN_NAME;
            public string PK_TABLE_NAME;
            public string PK_COLUMN_NAME;
            public string UNIQUE_CONSTRAINT_NAME;
            public string DELETE_RULE;
        }
    }
}