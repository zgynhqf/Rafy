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
            this.AddRun(new GenerationExceptionRun { Message = "Oracle 数据不支持数据库生成，请使用 DCA 工具手工完成。" });
        }

        protected override void Generate(CreateNormalColumn op)
        {
            if (op.IsIdentity)
            {
                throw new NotImplementedException("Oracle 自增列的创建功能还没有完成。");
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
                var sql = string.Format(@"CREATE SEQUENCE {0}
MINVALUE 1
MAXVALUE 99999999999999999
START WITH 1
INCREMENT BY 1
NOCACHE
ORDER", this.SEQName(op));

                this.AddRun(new SqlMigrationRun
                {
                    Sql = sql
                });
            }
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

        private string SEQName(TableOperation op)
        {
            var name = string.Format("SEQ_{0}_{1}", this.Prepare(op.TableName), this.Prepare(op.PKName));
            name = OracleMigrationProvider.LimitOracleIdentifier(name);
            return name;
        }

        protected override void Generate(AlterColumnType op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("MODIFY COLUMN ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.NewType, op.Length, op.IsRequired, op.IsForeignKey);

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
            return identifier.ToUpper();
        }
    }
}