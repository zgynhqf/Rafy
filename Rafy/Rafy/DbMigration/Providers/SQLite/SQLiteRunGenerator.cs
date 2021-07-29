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
using Rafy.DbMigration.MySql;
using Rafy.DbMigration.Operations;
using Rafy.DbMigration.Oracle;

namespace Rafy.DbMigration.SQLite
{
    /// <summary>
    /// SQLite的执行项生成器
    /// </summary>
    public sealed class SQLiteRunGenerator : SqlRunGenerator
    {
        public SQLiteRunGenerator()
        {
            this.IdentifierQuoter = MySqlIdentifierQuoter.Instance;
            this.DbTypeCoverter = SQLiteDbTypeConverter.Instance;
        }

        /// <summary>
        /// 生成创建数据库的语句
        /// </summary>
        /// <param name="op">创建数据库的实例对象</param>
        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new SQLiteCreateDbMigrationRun { Database = op.Database });
        }

        /// <summary>
        /// 生成删除数据库的语句
        /// </summary>
        /// <param name="op">删除数据库的实例对象</param>
        protected override void Generate(DropDatabase op)
        {
            this.AddRun(new SQLiteDropDbMigrationRun() { Database = op.Database });
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
                var pkType = op.PKIdentity ? DbType.Int32 : op.PKDbType;//sqllite:AUTOINCREMENT is only allowed on an INTEGER PRIMARY KEY
                this.GenerateColumnDeclaration(sql, op.PKName, pkType, op.PKLength, true, true);
                sql.Write(" PRIMARY KEY");
                if (op.PKIdentity) { sql.Write(" AUTOINCREMENT"); }
                sql.WriteLine();
                sql.Indent--;
                sql.Write(")");

                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 生成创建普通的数据列的语句
        /// </summary>
        /// <param name="op">创建普通列的实例对象</param>
        protected override void Generate(CreateNormalColumn op)
        {
            //忽略一般列的自增状态。
            //if (op.IsIdentity)
            //{
            //    throw new NotSupportedException($"SQLite 数据库不支持创建自增列：{op.TableName}.{op.ColumnName}！");
            //}

            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("ADD ");
                this.GenerateColumnDeclaration(sql, op.ColumnName, op.DbType, op.Length, false, op.IsForeignKey);
                this.AddRun(sql);
            }
        }

        #region 不支持

        protected override void Generate(AddNotNullConstraint op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 增加不允许为空的约束
        /// </summary>
        /// <param name="op">列操作对象的实体对象</param>
        protected override void AddNotNullConstraint(ColumnOperation op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 生成删除非空的列约束的语句
        /// </summary>
        /// <param name="op">列操作的实例对象</param>
        protected override void RemoveNotNullConstraint(ColumnOperation op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 生成增加外键约束的语句
        /// </summary>
        /// <param name="op">增加外键约束的对象</param>
        protected override void Generate(AddFKConstraint op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 生成删除外键约束的语句
        /// </summary>
        /// <param name="op">删除外键约束对象</param>
        protected override void Generate(RemoveFKConstraint op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 生成增加主键约束的语句
        /// </summary>
        /// <param name="op">增加逐渐约束的对象</param>
        protected override void Generate(AddPKConstraint op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 生成删除主键约束的语句
        /// </summary>
        /// <param name="op">删除主键约束的对象</param>
        protected override void Generate(RemovePKConstraint op)
        {
            //SQLite 不支持
        }

        protected override void Generate(DropNormalColumn op)
        {
            //SQLite 不支持
        }

        /// <summary>
        /// 生成更新备注信息
        /// </summary>
        /// <param name="op">更新备注的实体对象</param>
        protected override void Generate(UpdateComment op)
        {
            this.AddRun(new GenerationExceptionRun
            {
                Message = $"SQLite 不支持列的修改和删除语句，请手动操作列：{op.TableName}.{op.ColumnName}。"
            });
        }

        /// <summary>
        /// 生成修改列的类型的语句
        /// </summary>
        /// <param name="op">修改列类型的实例对象</param>
        protected override void Generate(AlterColumnType op)
        {
            //SQLite 不支持
            if (op.DbType == DbType.Int32 && op.NewType == DbType.Int64)
            {
                //可能是自增列的类型在创建时不支持 Int64，而是以 Int32 生成了，所以这里选择忽略此种情况。
            }
            else
            {
                this.AlterColumnNotSupport(op);
            }
        }

        protected override void GenerateAddPKConstraint(IndentedTextWriter sql, string tableName, string columnName)
        {
            //SQLite 不支持
        }

        private void AlterColumnNotSupport(ColumnOperation op)
        {
            //https://blog.csdn.net/jaycee110905/article/details/39586817
            //https://www.runoob.com/sqlite/sqlite-truncate-table.html
            //https://www.baidu.com/s?ie=utf-8&f=8&rsv_bp=1&ch=3&tn=98010089_dg&wd=sqlite%20%E5%88%A0%E9%99%A4%E5%88%97&oq=sqlite%2520alter%2520table&rsv_pq=e3e1382a001eec8b&rsv_t=647bzZl%2FgReQOf4SytHN7djqaiBYMaloAKnqBUXpTLSJzCpwLXenj%2BqkuF%2BnoYk0h%2B0&rqlang=cn&rsv_enter=1&rsv_dl=tb&rsv_btype=t&inputT=2081&rsv_sug3=19&rsv_sug1=20&rsv_sug7=100&bs=sqlite%20alter%20table
            this.AddRun(new GenerationExceptionRun
            {
                Message = $"SQLite 不支持列的修改和删除语句，请手动操作列：{op.TableName}.{op.ColumnName}。"
            });
        }

        #endregion
    }
}