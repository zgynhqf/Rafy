/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120428
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120428
 * 
*******************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Operations;
using Rafy.Data;

namespace Rafy.DbMigration.Oracle
{
    /// <summary>
    /// Oracle 的执行项生成器
    /// </summary>
    public class OracleRunGenerator : TSqlRunGenerator
    {
        protected override string ConvertToTypeString(DbType dataType, string length)
        {
            return OracleDbTypeHelper.ConvertToOracleTypeString(dataType, length);
        }

        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new GenerationExceptionRun
            {
                Message = "由于未连接上指定的数据库，所以需要创建数据库。由于 Rafy 不支持对 Oracle 数据库进行生成，请使用 DCA 工具手工创建指定库。"
            });
        }

        protected override void Generate(CreateNormalColumn op)
        {
            if (op.IsIdentity)
            {
                throw new NotImplementedException("Oracle 数据库暂时不支持创建自增列。");
            }

            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ADD ");

                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, false, op.IsForeignKey);

                this.AddRun(sql);
            }
        }

        protected override void Generate(DropDatabase op)
        {
            this.AddRun(new GenerationExceptionRun { Message = "Oracle 数据不支持数据库删除，请使用 DCA 工具手工完成。" });
        }

        protected override void Generate(CreateTable op)
        {
            if (string.IsNullOrWhiteSpace(op.PKName))
            {
                this.AddRun(new GenerationExceptionRun
                {
                    Message = "暂时不支持生成没有主键的表：" + op.TableName
                });
                return;
            }

            using (var sql = this.Writer())
            {
                sql.Write("CREATE TABLE ");
                sql.WriteLine(this.Quote(op.TableName));
                sql.WriteLine("(");
                sql.Indent++;
                this.GenerateColumnDeclaration(sql, op.PKName, op.PKDataType, op.PKLength, true, true);

                sql.WriteLine();
                sql.Indent--;
                sql.Write(")");

                this.AddRun(sql);
            }

            using (var sql = this.Writer())
            {
                this.GenerateAddPKConstraint(sql, op.TableName, op.PKName);

                this.AddRun(sql);
            }

            if (op.PKIdentity)
            {
                this.AddRun(new TryCreateTableSequenceRun
                {
                    SequenceName = this.SEQName(op)
                    //Sql = sql
                });
            }
        }

        protected override void GenerateAddPKConstraint(IndentedTextWriter sql, string tableName, string columnName)
        {
            var pkName = string.Format("PK_{0}_{1}",
                this.Prepare(tableName), this.Prepare(columnName)
                );
            pkName = OracleMigrationProvider.LimitOracleIdentifier(pkName);

            sql.Write(@"
ALTER TABLE ");
            sql.Write(this.Quote(tableName));
            sql.Write(@"
    ADD CONSTRAINT ");
            sql.Write(this.Quote(pkName));
            sql.Write(@"
    PRIMARY KEY (");
            sql.Write(this.Quote(columnName));
            sql.Write(")");
        }

        protected override void Generate(DropTable op)
        {
            base.Generate(op);

            if (op.PKIdentity)
            {
                this.AddRun(new SqlMigrationRun
                {
                    Sql = "DROP SEQUENCE " + this.SEQName(op)
                });
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
                sql.Write("MODIFY ");//ORACLE 中，MODIFY 关键字后没有 COLUMN 关键字。

                //Oracle 中如果可空性没有变化时，不能加到 Modify Column 之后。
                bool? isRequired = null;
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.NewType, op.Length, isRequired, op.IsForeignKey);

                this.AddRun(sql);
            }
        }

        protected override void Generate(AddFKConstraint op)
        {
            var oldName = op.ConstraintName;

            PrepareFKConstraintName(op);
            base.Generate(op);

            op.ConstraintName = oldName;
        }

        protected override void Generate(RemoveFKConstraint op)
        {
            var oldName = op.ConstraintName;

            PrepareFKConstraintName(op);
            base.Generate(op);

            op.ConstraintName = oldName;
        }

        protected override void Generate(UpdateComment op)
        {
            if (string.IsNullOrEmpty(op.ColumnName))
            {
                this.AddRun(new SqlMigrationRun
                {
                    Sql = string.Format(@"COMMENT ON TABLE ""{0}"" IS '{1}'", this.Prepare(op.TableName), op.Comment)
                });
            }
            else
            {
                this.AddRun(new SqlMigrationRun
                {
                    Sql = string.Format(@"COMMENT ON COLUMN ""{0}"".""{1}"" IS '{2}'", this.Prepare(op.TableName), this.Prepare(op.ColumnName), op.Comment)
                });
            }
        }

        /// <summary>
        /// 由于 Oracle 中 FK 最长是 30 个字符，所以这里需要对多余的字符串做截取操作。
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private static void PrepareFKConstraintName(FKConstraintOperation op)
        {
            var constraintName = op.ConstraintName;
            if (constraintName.StartsWith("FK_"))
            {
                constraintName = constraintName.Substring(3);
            }

            op.ConstraintName = OracleMigrationProvider.LimitOracleIdentifier(constraintName);
        }

        protected override void AddNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("MODIFY ");
                sql.Write(this.Quote(op.ColumnName));
                sql.Write(" NOT NULL");

                this.AddRun(sql);
            }
        }

        protected override void RemoveNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("MODIFY ");
                sql.Write(this.Quote(op.ColumnName));
                sql.Write(" NULL");

                this.AddRun(sql);
            }
        }

        protected override string Quote(string identifier)
        {
            return "\"" + this.Prepare(identifier) + "\"";
        }

        protected override string Prepare(string identifier)
        {
            return OracleMigrationProvider.PrepareIdentifier(identifier);
        }

        private string SEQName(TableOperation op)
        {
            return OracleMigrationProvider.SequenceName(op.TableName, op.PKName);
        }
    }
}