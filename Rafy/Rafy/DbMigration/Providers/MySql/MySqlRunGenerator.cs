/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161226 14:32
 * 
*******************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.DbMigration.Operations;
using Rafy.DbMigration.Oracle;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql的执行项生成器
    /// </summary>
    public sealed class MySqlRunGenerator : TSqlRunGenerator
    {
        /// <summary>
        /// 增加不允许为空的约束
        /// </summary>
        /// <param name="op">列操作对象的实体对象</param>
        protected override void AddNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.Write(" MODIFY ");

                sql.Indent++;
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, true, op.IsForeignKey);
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 把Clr的数据类型转型为MySql的数据类型
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="length">数据长度</param>
        /// <returns>返回MySql的数据类型</returns>
        protected override string ConvertToTypeString(DbType dataType, string length)
        {
            return MySqlDbTypeHelper.ConvertToMySqlTypeString(dataType, length);
        }

        /// <summary>
        /// 生成创建数据库的语句
        /// </summary>
        /// <param name="op">创建数据库的实例对象</param>
        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new MySqlCreateDbMigrationRun { Database = op.Database });
        }

        /// <summary>
        /// 生成删除数据库的语句
        /// </summary>
        /// <param name="op">删除数据库的实例对象</param>
        protected override void Generate(DropDatabase op)
        {
            this.AddRun(new MySqlDropDbMigrationRun() {Database=op.Database });
        }
        
        /// <summary>
        /// 生成更新备注信息
        /// </summary>
        /// <param name="op">更新备注的实体对象</param>
        protected override void Generate(UpdateComment op)
        {
            if (string.IsNullOrEmpty(op.ColumnName))
            {
                this.AddRun(new SqlMigrationRun
                {
                    Sql = string.Format(@"ALTER TABLE `{0}` COMMENT '{1}';", this.Prepare(op.TableName), op.Comment)
                });
            }
            else
            {
                //MySql 不支持外键修改备注，所以过滤掉外键修改备注
                if (string.Compare(op.ColumnName, "id", true) != 0 && string.Compare(op.TableName, "BlogUser") != 0)
                {
                    this.AddRun(new SqlMigrationRun
                    {
                        Sql = string.Format(@"ALTER TABLE `{0}` MODIFY COLUMN `{1}` {2} COMMENT '{3}';", this.Prepare(op.TableName), this.Prepare(op.ColumnName), MySqlDbTypeHelper.ConvertToMySqlTypeString(op.ColumnDataType), op.Comment)
                    });
                }
            }
        }

        /// <summary>
        /// 生成增加外键约束的语句
        /// </summary>
        /// <param name="op">增加外键约束的对象</param>
        protected override void Generate(AddFKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.DependentTable));
                sql.Write(@" ADD CONSTRAINT ");
                sql.Write(this.Quote(op.ConstraintName));
                sql.Write(@" FOREIGN KEY ");
                sql.Write(this.Quote(op.DependentTable));
                sql.Write(@" (");
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
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成删除外键约束的语句
        /// </summary>
        /// <param name="op">删除外键约束对象</param>
        protected override void Generate(RemoveFKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write(@"ALTER TABLE ");
                sql.Write(this.Quote(op.DependentTable));
                sql.Write(@" DROP FOREIGN KEY ");
                sql.Write(this.Quote(op.ConstraintName));
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成增加主键约束的语句
        /// </summary>
        /// <param name="op">增加逐渐约束的对象</param>
        protected override void Generate(AddPKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.Write(" ADD CONSTRAINT PK_");
                sql.Write(op.TableName.ToUpper());
                sql.Write(" PRIMARY KEY ");
                sql.Write(this.Quote(op.TableName));
                sql.Write("(");
                sql.Write(this.Quote(op.ColumnName));
                sql.Write(");");
                this.AddRun(sql);
            }
            //using (var sql = this.Writer())
            //{
            //    sql.Write("ALTER TABLE ");
            //    sql.Write(this.Quote(op.TableName));
            //    sql.Write(" DROP PRIMARY KEY,ADD PRIMARY KEY(");
            //    sql.Write(this.Quote(op.ColumnName));
            //    sql.Write(")");
            //    sql.Write(";");
            //    this.AddRun(sql);
            //}
        }

        /// <summary>
        /// 生成删除主键约束的语句
        /// </summary>
        /// <param name="op">删除主键约束的对象</param>
        protected override void Generate(RemovePKConstraint op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.Write(" DROP PRIMARY KEY;");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成删除数据表的语句
        /// </summary>
        /// <param name="op">删除表对象</param>
        protected override void Generate(DropTable op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("DROP TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成创建表的语句
        /// </summary>
        /// <param name="op">创建表的对象实例</param>
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
                sql.Write("CREATE TABLE IF NOT EXISTS ");
                sql.WriteLine(this.Quote(op.TableName));
                sql.WriteLine("(");
                sql.Indent++;
                this.GenerateColumnDeclaration(sql, op.PKName, op.PKDataType, op.PKLength, true, true);
                if (op.PKIdentity) { sql.Write(" auto_increment"); }
                sql.Write(" primary key");
                sql.WriteLine();
                sql.Indent--;
                sql.Write(");");

                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成修改列的类型的语句
        /// </summary>
        /// <param name="op">修改列类型的实例对象</param>
        protected override void Generate(AlterColumnType op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("MODIFY ");

                this.GenerateColumnDeclaration(sql, op.ColumnName, op.NewType, op.Length, op.IsRequired, op.IsForeignKey);
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成创建普通的数据列的语句
        /// </summary>
        /// <param name="op">创建普通列的实例对象</param>
        protected override void Generate(CreateNormalColumn op)
        {
            if (op.IsIdentity)
            {
                throw new NotImplementedException("MySql 数据库不支持创建自增列!");
            }

            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ADD ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, false, op.IsForeignKey);
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成删除非空的列约束的语句
        /// </summary>
        /// <param name="op">列操作的实例对象</param>
        protected override void RemoveNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("MODIFY ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DataType, op.Length, false, op.IsForeignKey);
                sql.Write(";");
                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 防止命名冲突而增加的引用
        /// </summary>
        /// <param name="identifier">需要被引用的内容</param>
        /// <returns>返回增加了指定字符应用的字符串</returns>
        protected override string Quote(string identifier)
        {
            return "`"+identifier+"`";
        }
    }
}