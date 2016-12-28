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
using Rafy.DbMigration.Operations;
using System.CodeDom.Compiler;
using System.Data;
using Rafy.Data;

namespace Rafy.DbMigration
{
    /// <summary>
    /// T-SQL 生成的基类
    /// </summary>
    public abstract class TSqlRunGenerator : RunGenerator
    {
        protected abstract string ConvertToTypeString(DbType dataType, string length);

        protected override void Generate(DropTable op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("DROP TABLE ");
                sql.Write(this.Quote(op.TableName));

                this.AddRun(sql);
            }
        }

        protected override void Generate(DropNormalColumn op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("DROP COLUMN ");
                sql.Write(this.Quote(op.ColumnName));

                this.AddRun(sql);
            }
        }

        protected override void Generate(AddPKConstraint op)
        {
            using (var sql = this.Writer())
            {
                this.GenerateAddPKConstraint(sql, op.TableName, op.ColumnName);

                this.AddRun(sql);
            }
        }

        protected virtual void GenerateAddPKConstraint(IndentedTextWriter sql, string tableName, string columnName)
        {
            var pkName = string.Format("PK_{0}_{1}", this.Prepare(tableName), this.Prepare(columnName));

            sql.Write(@"ALTER TABLE ");
            sql.Write(this.Quote(tableName));
            sql.Write(@" ADD CONSTRAINT ");
            sql.Write(this.Quote(pkName));
            sql.Write(@" PRIMARY KEY (");
            sql.Write(this.Quote(columnName));
            sql.Write(")");
        }

        protected override void Generate(RemovePKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write(@"
ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.Write(@"
    DROP CONSTRAINT ");
                sql.Write(this.Quote(string.Format("PK_{0}_{1}",
                    this.Prepare(op.TableName), this.Prepare(op.ColumnName)
                    )));

                this.AddRun(sql);
            }
        }

        protected override void Generate(AddNotNullConstraint op)
        {
            FormattedSql sql = string.Format(@"UPDATE {0} SET {1} = {2} WHERE {1} IS NULL",
                    this.Quote(op.TableName), this.Quote(op.ColumnName), "{0}");
            sql.Parameters.Add(DbTypeHelper.GetDefaultValue(op.DataType));

            this.AddRun(new FormattedSqlMigrationRun
            {
                Sql = sql
            });

            this.AddNotNullConstraint(op);
        }

        protected override void Generate(AddNotNullConstraintFK op)
        {
            this.AddNotNullConstraint(op);
        }

        protected abstract void AddNotNullConstraint(ColumnOperation op);

        protected override void Generate(RemoveNotNullConstraint op)
        {
            this.RemoveNotNullConstraint(op);
        }

        protected override void Generate(RemoveNotNullConstraintFK op)
        {
            this.RemoveNotNullConstraint(op);
        }

        protected abstract void RemoveNotNullConstraint(ColumnOperation op);

        protected override void Generate(AddFKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.DependentTable));
                sql.Write(@"
    ADD CONSTRAINT ");
                sql.Write(this.Quote(op.ConstraintName));
                sql.Write(@"
    FOREIGN KEY (");
                sql.Write(this.Quote(op.DependentTableColumn));
                sql.Write(") REFERENCES ");
                sql.Write(this.Quote(op.PrincipleTable));
                sql.Write("(");
                sql.Write(this.Quote(op.PrincipleTableColumn));
                sql.Write(")");

                if (op.NeedDeleteCascade)
                {
                    sql.Write(" ON DELETE CASCADE");
                }

                this.AddRun(sql);
            }
        }

        protected override void Generate(RemoveFKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write(@"
ALTER TABLE ");
                sql.Write(this.Quote(op.DependentTable));
                sql.Write(@"
    DROP CONSTRAINT ");
                sql.Write(this.Quote(op.ConstraintName));

                this.AddRun(sql);
            }
        }

        /// <summary>
        /// Generates the column declaration.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="length">The length.</param>
        /// <param name="isRequired">if set to <c>true</c> [is required].</param>
        /// <param name="isPKorFK">在没有给出字段长度的情况下，如果这个字段是一个主键或外键，则需要自动限制它的长度。</param>
        protected void GenerateColumnDeclaration(IndentedTextWriter sql,string columnName, DbType dataType, string length, bool? isRequired, bool isPKorFK)
        {
            if (string.IsNullOrEmpty(length))
            {
                if (isPKorFK)
                {
                    //http://stackoverflow.com/questions/2863993/is-of-a-type-that-is-invalid-for-use-as-a-key-column-in-an-index
                    length = DbMigrationSettings.PKFKDataTypeLength;
                }
                else if (dataType == DbType.String)
                {
                    length = DbMigrationSettings.StringColumnDataTypeLength;
                }
            }

            sql.Write(this.Quote(columnName));
            sql.Write(" ");
            sql.Write(this.ConvertToTypeString(dataType, length));

            if (isRequired.HasValue)
            {
                if (isRequired.Value)
                {
                    sql.Write(" NOT");
                }
                sql.Write(" NULL");
            }
        }

        protected virtual string Quote(string identifier)
        {
            return "[" + this.Prepare(identifier) + "]";
        }

        protected virtual string Prepare(string identifier)
        {
            return identifier;
        }
    }
}