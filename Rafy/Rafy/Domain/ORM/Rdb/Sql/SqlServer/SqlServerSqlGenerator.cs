﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 15:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.SqlServer;
using Rafy.Domain.ORM.SqlTree;
using System.Collections;

namespace Rafy.Domain.ORM
{
    class SqlServerSqlGenerator : SqlGenerator
    {
        public SqlServerSqlGenerator()
        {
            this.IdentifierProvider = SqlServerIdentifierQuoter.Instance;
            this.DbTypeCoverter = SqlServerDbTypeConverter.Instance;
        }

        /// <summary>
        /// Sql Server 中没有限制 In 语句中的项的个数。（但是如果使用参数的话，则最多只能使用 2000 个参数。）
        /// 
        /// In 语句中可以承受的最大的个数。
        /// 如果超出这个个数，则会抛出 TooManyItemsInInClauseException。
        /// </summary>
        protected override int MaxItemsInInClause => int.MaxValue;

        protected override void AppendNameCast()
        {
            Sql.Append(" AS ");
        }

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        protected override ISqlSelect ModifyToPagingTree(SqlSelect raw, PagingInfo pagingInfo)
        {
            //如果是第一页，则只需要使用 TOP 语句即可。
            if (pagingInfo.PageNumber == 1)
            {
                return new SqlSelect
                {
                    Selection = new SqlNodeList
                    {
                        new SqlLiteral { FormattedSql = "TOP " + pagingInfo.PageSize + " " },
                        raw.Selection ?? SqlSelectAll.Default
                    },
                    From = raw.From,
                    Where = raw.Where,
                    OrderBy = raw.OrderBy
                };
            }

            return this.ModifyToPagingTree_With_RowNumber(raw, pagingInfo);
        }

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        private ISqlSelect ModifyToPagingTree_With_NotIn(SqlSelect raw, PagingInfo pagingInfo)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 转换方案：
             * 
             * SELECT * 
             * FROM ASN
             * WHERE ASN.Id > 0
             * ORDER BY ASN.AsnCode ASC
             * 
             * 转换分页后：
             * 
             * SELECT TOP 10 * 
             * FROM ASN
             * WHERE ASN.Id > 0 AND ASN.Id NOT IN(
             *     SELECT TOP 20 Id
             *     FROM ASN
             *     WHERE ASN.Id > 0 
             *     ORDER BY ASN.AsnCode ASC
             * )
             * ORDER BY ASN.AsnCode ASC
             * 
            **********************************************************************/

            //先要找到主表的 PK，分页时需要使用此主键列来生成分页 Sql。
            //这里约定 Id 为主键列名。
            var finder = new FirstTableFinder();
            var pkTable = finder.Find(raw.From);
            var pkColumn = new SqlColumn { Table = pkTable, ColumnName = EntityConvention.IdColumnName };

            //先生成内部的 Select
            var excludeSelect = new SqlSelect
            {
                Selection = new SqlNodeList
                {
                    new SqlLiteral { FormattedSql = "TOP " + (pagingInfo.PageNumber - 1) * pagingInfo.PageSize + " " },
                    pkColumn
                },
                From = raw.From,
                Where = raw.Where,
                OrderBy = raw.OrderBy,
            };

            var res = new SqlSelect
            {
                Selection = new SqlNodeList
                {
                    new SqlLiteral { FormattedSql = "TOP " + pagingInfo.PageSize + " " },
                    raw.Selection ?? SqlSelectAll.Default
                },
                From = raw.From,
                OrderBy = raw.OrderBy,
            };

            //where
            var newWhere = new SqlColumnConstraint
            {
                Column = pkColumn,
                Operator = SqlColumnConstraintOperator.NotIn,
                Value = excludeSelect
            };
            if (raw.Where != null)
            {
                res.Where = new SqlBinaryConstraint
                {
                    Left = raw.Where,
                    Opeartor = SqlBinaryConstraintType.And,
                    Right = newWhere
                };
            }
            else
            {
                res.Where = newWhere;
            }

            return res;
        }

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        private ISqlSelect ModifyToPagingTree_With_RowNumber(SqlSelect raw, PagingInfo pagingInfo)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 转换方案：
             * 使用 ROW_NUMBER() 函数。（此函数 SqlServer、Oracle 都可使用。）
             * 
             * SELECT * 
             * FROM ASN
             * WHERE ASN.Id > 0
             * ORDER BY ASN.AsnCode ASC
             * 
             * 转换分页后：
             * 
             * SELECT * FROM
             * (
             *     SELECT A.*, ROW_NUMBER() OVER (order by  Id) _RowNumber
             *     FROM  A
             * ) T
             * WHERE _RowNumber BETWEEN 1 AND 10
            **********************************************************************/

            var finder = new FirstTableFinder();
            var pkTable = finder.Find(raw.From);

            //在 Sql 分页的算法中，必须排序后才能使用分页功能，所以如果给定的 sql 中没有排序语句的话，则尝试使用任意表的一个默认的字段（Id）来进行排序。
            var orderBy = raw.OrderBy;
            if (!raw.HasOrdered())
            {
                if (SqlServerSqlGeneratorConfiguration.DefaultPagingSqlOrderbyColumn == null)
                {
                    throw new InvalidProgramException("必须提供排序语句后，才能使用分页功能。");
                }

                orderBy = new SqlOrderByList
                {
                    new SqlOrderBy
                    {
                        Column = new SqlColumn
                        {
                            Table = pkTable,
                            ColumnName = SqlServerSqlGeneratorConfiguration.DefaultPagingSqlOrderbyColumn
                        }
                    }
                };
            }

            var newRaw = new SqlSelect
            {
                Selection = new SqlNodeList()
                {
                    raw.Selection ?? new SqlSelectAll() { Table = pkTable },
                    new SqlLiteral { FormattedSql = ", ROW_NUMBER() OVER (" },
                    orderBy,
                    new SqlLiteral { FormattedSql = ") _RowNumber " }
                },
                From = raw.From,
                Where = raw.Where,
                IsDistinct = raw.IsDistinct,
                IsCounting = raw.IsCounting
            };
            var startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
            var endRow = startRow + pagingInfo.PageSize - 1;

            var res = new SqlNodeList
            {
                new SqlLiteral(
                @"SELECT * FROM
("),
                newRaw,
                new SqlLiteral(
                @")T WHERE _RowNumber BETWEEN " + startRow + @" AND " +endRow )
            };

            return res;
        }

        /// <summary>
        /// 重写VisitSqlColumnConstraint方法
        /// 处理In、NotIn操作，如果传入的值为字符串，则查询条件增加N前缀
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override SqlColumnConstraint VisitSqlColumnConstraint(SqlColumnConstraint node)
        {
            var op = node.Operator;
            var value = node.Value;
            
            value = this.DbTypeCoverter.ToDbParameterValue(value);

            #region 处理一些特殊的值

            switch (op)
            {
                case SqlColumnConstraintOperator.In:
                case SqlColumnConstraintOperator.NotIn:
                    //对于 In、NotIn 操作，如果传入的是空列表时，需要特殊处理：
                    //In(Empty) 表示 false，NotIn(Empty) 表示 true。
                    if (value is IEnumerable)
                    {
                        bool hasValue = false;
                        foreach (var item in value as IEnumerable)
                        {
                            hasValue = true;
                            break;
                        }
                        if (!hasValue)
                        {
                            if (op == SqlColumnConstraintOperator.In)
                            {
                                Sql.Append("0 = 1");
                            }
                            else
                            {
                                Sql.Append("1 = 1");
                            }

                            return node;
                        }
                    }
                    break;
                default:
                    break;
            }

            #endregion
            
            //根据不同的操作符，来生成不同的Sql。
            switch (op)
            {
                case SqlColumnConstraintOperator.In:
                case SqlColumnConstraintOperator.NotIn:
                    base.AppendColumnUsage(node.Column);

                    var opSql = op == SqlColumnConstraintOperator.In ? "IN" : "NOT IN";
                    Sql.Append(" ").Append(opSql).Append(" (");
                    if (value is IEnumerable)
                    {
                        bool first = true;
                        bool needDelimiter = false;
                        bool isString = false;
                        int i = 0;
                        foreach (var item in value as IEnumerable)
                        {
                            if (++i > this.MaxItemsInInClause) throw new TooManyItemsInInClauseException();

                            if (first)
                            {
                                first = false;
                                needDelimiter = item is string || item is DateTime || item is Guid;
                                isString = item is string;
                            }
                            else { Sql.Append(','); }

                            if (needDelimiter)
                            {
                                //判断值否是为字符串，如果是查询语句增加N前缀
                                if (isString) Sql.Append('N').Append('\'').Append(EscapeSpecialChar(item)).Append('\'');
                                else Sql.Append('\'').Append(EscapeSpecialChar(item)).Append('\'');
                            }
                            else
                            {
                                Sql.Append(item);
                            }
                        }
                    }
                    else if (value is ISqlNode)
                    {
                        Sql.AppendLine();
                        this.Indent++;
                        this.Visit(value as ISqlNode);
                        this.Indent--;
                        Sql.AppendLine();
                    }
                    Sql.Append(')');
                    return node;
                default:
                    break;
            }

            return base.VisitSqlColumnConstraint(node);
        }
    }
}
