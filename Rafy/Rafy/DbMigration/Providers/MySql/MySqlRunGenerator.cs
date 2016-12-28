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
        /// <param name="op"></param>
        protected override void AddNotNullConstraint(ColumnOperation op)
        {
            using (var sql = this.Writer())
            {
                sql.Write("ALTER TABLE ");
                sql.Write(this.Quote(op.TableName));
                sql.WriteLine();

                sql.Indent++;
                sql.Write("MODIFY ");
                sql.Write(op.ColumnName);
                sql.Write(" NOT NULL");

                this.AddRun(sql);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override string ConvertToTypeString(DbType dataType, string length)
        {
            return MySqlDbTypeHelper.ConvertToOracleTypeString(dataType, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        protected override void Generate(CreateDatabase op)
        {
            this.AddRun(new GenerationExceptionRun
            {
                Message = "由于未连接上指定的数据库，所以需要创建数据库。由于 Rafy 不支持对 MySql 数据库进行生成，请使用 DCA 工具手工创建指定库。"
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        protected override void Generate(UpdateComment op)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        protected override void Generate(DropDatabase op)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 生成创建表的语句
        /// </summary>
        /// <param name="op"></param>
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        protected override void Generate(AlterColumnType op)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        protected override void Generate(CreateNormalColumn op)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op"></param>
        protected override void RemoveNotNullConstraint(ColumnOperation op)
        {
            throw new NotImplementedException();
        }
    }
}