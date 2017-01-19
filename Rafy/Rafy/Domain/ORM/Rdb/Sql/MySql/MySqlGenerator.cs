/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170104 11:08
 * 
*******************************************************/

using Rafy.DbMigration.MySql;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.MySql
{
    /// <summary>
    /// MySql的Sql语句生成器
    /// </summary>
    internal sealed class MySqlGenerator:SqlGenerator
    {
        /// <summary>
        /// 名称别名设置
        /// </summary>
        protected override void AppendNameCast()
        {
            Sql.Append(" AS ");
        }

        /// <summary>
        /// 为指定标识符增加引用符号
        /// </summary>
        /// <param name="identifier">标识符</param>
        protected override void QuoteAppend(string identifier)
        {
            if (this.AutoQuota)
            {
                identifier = this.PrepareIdentifier(identifier);
                Sql.Append("`").Append(identifier).Append("`");
            }
            else
            {
                base.QuoteAppend(identifier);
            }
        }

        /// <summary>
        /// 将小写的标识符转换成大写
        /// </summary>
        /// <param name="identifier">待转换的标识符</param>
        /// <returns>返回大写的标识符</returns>
        protected override string PrepareIdentifier(string identifier)
        {
            return identifier.ToUpper();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override SqlColumnConstraint VisitSqlColumnConstraint(SqlColumnConstraint node)
        {
            switch (node.Operator)
            {
                case SqlColumnConstraintOperator.Equal:
                case SqlColumnConstraintOperator.NotEqual:
                    //在 Oracle 中，空字符串的对比，需要转换为对 Null 值的对比。
                    var strValue = node.Value as string;
                    if (strValue != null && strValue.Length == 0)
                    {
                        node.Value = null;
                    }
                    break;
                default:
                    break;
            }

            return base.VisitSqlColumnConstraint(node);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object PrepareConstraintValue(object value)
        {
            value = base.PrepareConstraintValue(value);

            value = PrepareConstraintValueInternal(value);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static object PrepareConstraintValueInternal(object value)
        {
            if (value != DBNull.Value)
            {
                if (value is bool)
                {
                    value = MySqlDbTypeHelper.ToDbBoolean((bool)value);
                }
                else if (value.GetType().IsEnum)
                {
                    value = TypeHelper.CoerceValue(typeof(int), value);
                }
            }

            return value;
        }

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        protected override ISqlSelect ModifyToPagingTree(SqlSelect raw, PagingInfo pagingInfo)
        {
            if (PagingInfo.IsNullOrEmpty(pagingInfo)) { throw new ArgumentNullException("pagingInfo"); }
            if (!raw.HasOrdered()) { throw new InvalidProgramException("必须排序后才能使用分页功能。"); }

            var startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
            var endRow = startRow + pagingInfo.PageSize - 1;

            var res = MakePagingTree(raw, startRow, endRow);

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <returns></returns>
        private static ISqlSelect MakePagingTree(SqlSelect raw, long startRow, long endRow)
        {
            /*********************** 代码块解释 *********************************
             * 以下转换使用 ORACLE 行号字段来实现分页。只需要简单地在查询的 WHERE 语句中加入等号的判断即可。
             *
             * 源格式：
             *     SELECT *
             *     FROM A
             *     WHERE A.Id > 0
             *     ORDER BY A.NAME ASC
             * 
             * 目标格式：
             *      SELECT * FROM
             *      (
             *          SELECT T.*, ROWNUM RN
             *          FROM (
             *              SELECT *
             *              FROM A
             *              WHERE A.Id > 0
             *              ORDER BY A.NAME ASC
             *          ) T
             *          WHERE ROWNUM <= 20
             *      )
             *      WHERE RN >= 10
            **********************************************************************/

            return new SqlNodeList
            {
                new SqlLiteral(
@"SELECT * FROM
(
    SELECT T.*, ROWNUM RN
    FROM 
    (
"),
                raw,
                new SqlLiteral(
@"
    ) T
    WHERE ROWNUM <= " + endRow + @"
)
WHERE RN >= " + startRow)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <returns></returns>
        private static ISqlSelect MakePagingTree_ReserveMethod(SqlSelect raw, int startRow, int endRow)
        {
            /*********************** 代码块解释 *********************************
             * 源格式：
             *     SELECT *
             *     FROM A
             *     WHERE A.Id > 0
             *     ORDER BY A.NAME ASC
             * 
             * 目标格式：
             *      SELECT * FROM
             *      (SELECT A.*, ROWNUM RN
             *     FROM A
             *     WHERE A.Id > 0 AND ROWNUM <= 20
             *     ORDER BY A.NAME ASC)
             *     WHERE RN >= 10
             *     
             * 这种方法可能存在问题：
             * 因为源 Sql 可能是：Select * From A Join B，这时表示结果集需要显示 A 和 B 的所有字段，
             * 但是此方法会转换为：Select A.* From A Join B。比较麻烦，暂不处理
            **********************************************************************/
            var innerSelect = new SqlSelect
            {
                IsDistinct = raw.IsDistinct,
                From = raw.From,
                Where = AppendWhere(raw.Where, new SqlLiteral("ROWNUM <= " + endRow)),
                OrderBy = raw.OrderBy
            };

            //内部的 Select 子句中，不能简单地使用 "*, ROWNUM"，而是需要使用 "A.*, ROWNUM"
            var rawSelection = raw.Selection;
            if (rawSelection == null)
            {
                //默认约定第一张表，就是
                var table = new FirstTableFinder().Find(raw.From);
                rawSelection = new SqlSelectAll
                {
                    Table = table
                };
            }
            innerSelect.Selection = new SqlNodeList
            {
                rawSelection,
                new SqlLiteral(", ROWNUM RN")
            };

            var res = new SqlSelect
            {
                Selection = SqlSelectAll.Default,
                From = new SqlSubSelect
                {
                    Select = innerSelect
                },
                Where = new SqlLiteral("RN >= " + startRow)
            };
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newConstraint"></param>
        /// <returns></returns>
        private static ISqlConstraint AppendWhere(ISqlConstraint old, ISqlConstraint newConstraint)
        {
            if (old != null)
            {
                newConstraint = new SqlBinaryConstraint
                {
                    Left = old,
                    Opeartor = SqlBinaryConstraintType.And,
                    Right = newConstraint
                };
            }
            return newConstraint;
        }
    }
}