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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Operations;
using Rafy.Data;

namespace Rafy.DbMigration.SqlServer
{
    /// <summary>
    /// SqlServer 的执行项生成器
    /// </summary>
    public class SqlServerRunGenerator : TSqlRunGenerator
    {
        protected override string ConvertToTypeString(DbType dataType, string length)
        {
            return SqlDbTypeHelper.ConvertToSQLTypeString(dataType, length);
        }

        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new CreateDbMigrationRun { Database = op.Database });
        }

        protected override void Generate(DropDatabase op)
        {
            this.AddRun(new DropDbMigrationRun { Database = op.Database });
        }

        protected override void Generate(CreateTable op)
        {
            if (string.IsNullOrWhiteSpace(op.PKName))
            {
                this.AddRun(new GenerationExceptionRun
                {
                    Message = "不支持生成没有主键的表：" + op.TableName
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

                if (op.PKIdentity) { sql.Write(" IDENTITY(1,1)"); }

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

                //自增列必然是不可空的，在创建列时必须同时把不可空约束给创建好了。
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, op.IsIdentity, op.IsForeignKey);

                if (op.IsIdentity)
                {
                    sql.Write(" IDENTITY(1,1)");
                }

                this.AddRun(sql);
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
                sql.Write("ALTER COLUMN ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, true, op.IsForeignKey);

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
                sql.Write("ALTER COLUMN ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, false, op.IsForeignKey);

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
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.NewType, op.Length, op.IsRequired, op.IsForeignKey);

                this.AddRun(sql);
            }
        }

        protected override void Generate(UpdateComment op)
        {
            //参考：
            //http://www.cnblogs.com/xdp-gacl/p/3506099.html
            //http://blog.sina.com.cn/s/blog_8b7263d10101d7ak.html
            //http://blog.sina.com.cn/s/blog_8fe8076e01019ik7.html

            if (string.IsNullOrEmpty(op.ColumnName))
            {
                this.AddRun(new SafeSqlMigrationRun
                {
                    Sql = string.Format(@"EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'{0}'", op.TableName)
                });
                this.AddRun(new SqlMigrationRun
                {
                    Sql = string.Format(@"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{1}', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'{0}'", op.TableName, op.Comment)
                });
            }
            else
            {
                this.AddRun(new SafeSqlMigrationRun
                {
                    Sql = string.Format(@"EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'{0}', @level2type=N'COLUMN', @level2name=N'{1}'", op.TableName, op.ColumnName)
                });
                this.AddRun(new SqlMigrationRun
                {
                    Sql = string.Format(@"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{2}', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=N'{0}', @level2type=N'COLUMN', @level2name=N'{1}'", op.TableName, op.ColumnName, op.Comment, op.Comment)
                });
            }
        }
    }
}