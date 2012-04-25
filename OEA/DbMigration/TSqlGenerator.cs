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

        protected abstract string GetDefaultValue(DbType dataType);

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
            var text = string.Format(@"
ALTER TABLE [{0}]
    ADD CONSTRAINT [PK_{0}_{1}] 
    PRIMARY KEY ([{1}])", tableName, columnName);
            sql.Write(text);
        }

        protected override void Generate(RemovePKConstraint op)
        {
            using (var sql = this.Writer())
            {
                var text = string.Format(@"
ALTER TABLE [{0}]
    DROP CONSTRAINT [PK_{0}_{1}]", op.TableName, op.ColumnName);
                sql.Write(text);

                this.AddRun(sql);
            }
        }

        protected override void Generate(AddNotNullConstraint op)
        {
            using (var sql = this.Writer())
            {
                string columnDefaultValue = this.GetDefaultValue(op.DataType);

                var text = string.Format(@"UPDATE [{0}] SET [{1}] = {2} WHERE [{1}] IS NULL", op.TableName, op.ColumnName, columnDefaultValue);
                sql.Write(text);

                this.AddRun(sql);
            }

            this.AddNotNullConstraint(op);
        }

        protected override void Generate(AddNotNullConstraintFK op)
        {
            this.AddNotNullConstraint(op);
        }

        protected void AddNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ALTER COLUMN ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, true);

                this.AddRun(sql);
            }
        }

        protected override void Generate(RemoveNotNullConstraint op)
        {
            this.RemoveNotNullConstraint(op);
        }

        protected override void Generate(RemoveNotNullConstraintFK op)
        {
            this.RemoveNotNullConstraint(op);
        }

        protected void RemoveNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ALTER COLUMN ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, false);

                this.AddRun(sql);
            }
        }

        protected override void Generate(AddFKConstraint op)
        {
            using (var sql = this.Writer())
            {
                var text = string.Format(@"
ALTER TABLE [{0}]
    ADD CONSTRAINT [{4}] 
    FOREIGN KEY ([{1}]) REFERENCES [{2}]([{3}])", op.DependentTable, op.DependentTableColumn, op.PrincipleTable, op.PrincipleTableColumn, op.ConstraintName);
                sql.Write(text);

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
                var text = string.Format(@"
ALTER TABLE [{0}]
    DROP CONSTRAINT [{1}]", op.DependentTable, op.ConstraintName);
                sql.Write(text);

                this.AddRun(sql);
            }
        }

        protected override void Generate(AlterColumnType op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ALTER COLUMN ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.NewType, op.IsRequired);

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

        protected string Quote(string identifier)
        {
            return "[" + identifier + "]";
        }
    }
}