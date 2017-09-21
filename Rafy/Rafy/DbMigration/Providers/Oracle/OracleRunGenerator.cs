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
    public sealed class OracleRunGenerator : SqlRunGenerator
    {
        public OracleRunGenerator()
        {
            this.IdentifierQuoter = OracleIdentifierQuoter.Instance;
        }

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
                    SequenceName = SequenceName(op.TableName, op.PKName)
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
                    Sql = "DROP SEQUENCE " + SequenceName(op.TableName, op.PKName)
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

        /// <summary>
        /// 返回指定的表对应的序列的名称。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="pkName"></param>
        /// <returns></returns>
        public string SequenceName(string tableName, string pkName)
        {
            var name = string.Format("SEQ_{0}_{1}", this.Prepare(tableName), this.Prepare(pkName));
            name = this.Prepare(name);
            return name;
        }
    }
}