/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 15:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Oracle;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.Oracle
{
    //暂时没有处理：
    //TOP、!=、
    class OracleSqlGenerator : SqlGenerator
    {
        protected override void QuoteAppend(string identifier)
        {
            if (this.AutoQuota)
            {
                identifier = this.PrepareIdentifier(identifier);
                Sql.Append("\"").Append(identifier).Append("\"");
            }
            else
            {
                base.QuoteAppend(identifier);
            }
        }

        protected override string PrepareIdentifier(string identifier)
        {
            return identifier.ToUpper();
        }

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

        public override object PrepareConstraintValue(object value)
        {
            value = base.PrepareConstraintValue(value);

            value = PrepareConstraintValueInternal(value);

            return value;
        }

        internal static object PrepareConstraintValueInternal(object value)
        {
            if (value != DBNull.Value)
            {
                if (value is bool)
                {
                    value = OracleDbTypeHelper.ToDbBoolean((bool)value);
                }
                else if (value.GetType().IsEnum)
                {
                    value = TypeHelper.CoerceValue(typeof(int), value);
                }
            }

            return value;
        }

        /// <summary>
        /// 使用 ROWNUM 来进行分页。
        /// </summary>
        /// <param name="raw">The raw.</param>
        /// <param name="pagingInfo">The paging information.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">pagingInfo</exception>
        /// <exception cref="System.InvalidProgramException">必须排序后才能使用分页功能。</exception>
        protected override ISqlSelect ModifyToPagingTree(SqlSelect raw, PagingInfo pagingInfo)
        {
            var startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
            var endRow = startRow + pagingInfo.PageSize - 1;

            var res = MakePagingTree(raw, startRow, endRow);

            return res;
        }

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