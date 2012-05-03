using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration.Operations;
using System.CodeDom.Compiler;
using System.Data;

namespace DbMigration
{
    /// <summary>
    /// T-SQL 生成的基类
    /// </summary>
    public abstract class TSqlGenerator : RunGenerator
    {
        protected abstract string ConvertToTypeString(DbType dataType);

        protected virtual string GetDefaultValue(DbType dataType)
        {
            return DbTypeHelper.GetDefaultValue(dataType);
        }

        protected override void Generate(DropTable op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("DROP TABLE ");
                sql.Write(this.Quote(op.TableName));

                this.AddRun(sql);
            }
        }

        protected override void Generate(CreateNormalColumn op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ADD ");

                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, false);

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

        protected void GenerateAddPKConstraint(IndentedTextWriter sql, string tableName, string columnName)
        {
            sql.Write(@"
ALTER TABLE ");
            sql.Write(this.Quote(tableName));
            sql.Write(@"
    ADD CONSTRAINT ");
            sql.Write(this.Quote(string.Format("PK_{0}_{1}",
                this.Prepare(tableName), this.Prepare(columnName)
                )));
            sql.Write(@"
    PRIMARY KEY (");
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
            using (var sql = this.Writer())
            {
                string columnDefaultValue = this.GetDefaultValue(op.DataType);

                var text = string.Format(@"UPDATE {0} SET {1} = {2} WHERE {1} IS NULL",
                    this.Quote(op.TableName), this.Quote(op.ColumnName), columnDefaultValue);
                sql.Write(text);

                this.AddRun(sql);
            }

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
                sql.Write(@"
ALTER TABLE ");
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

        protected void GenerateColumnDeclaration(IndentedTextWriter sql, string columnName, DbType dataType, bool isRequired)
        {
            sql.Write(this.Quote(columnName));
            sql.Write(" ");
            sql.Write(this.ConvertToTypeString(dataType));

            if (isRequired)
            {
                sql.Write(" NOT");
            }
            sql.Write(" NULL");
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