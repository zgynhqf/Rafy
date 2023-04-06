/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 10:49
 * 
*******************************************************/

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Rafy;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 为 SqlNode 语法树生成相应 Sql 的生成器。
    /// </summary>
    internal abstract class SqlGenerator : SqlNodeVisitor
    {
        internal const string WILDCARD_ALL = "%";
        internal const string WILDCARD_SINGLE = "_";
        internal const string ESCAPE_CHAR = "/";
        internal static readonly string WILDCARD_ALL_ESCAPED = ESCAPE_CHAR + WILDCARD_ALL;
        internal static readonly string WILDCARD_SINGLE_ESCAPED = ESCAPE_CHAR + WILDCARD_SINGLE;

        private IDbIdentifierQuoter _identifierProvider;
        private FormattedSql _sql;

        protected SqlGenerator()
        {
            _sql = new FormattedSql();
            _sql.InnerWriter = new IndentedTextWriter(_sql.InnerWriter);
            this.EmbedParameters = ORMSettings.EmbedParametersInQuerySql;
        }

        /// <summary>
        /// 数据库字段类型的转换器。
        /// </summary>
        public DbTypeConverter DbTypeCoverter { get; protected set; }

        /// <summary>
        /// 数据库标识符的处理器。
        /// </summary>
        public IDbIdentifierQuoter IdentifierProvider { get => _identifierProvider; protected set => _identifierProvider = value; }

        /// <summary>
        /// 当前需要的缩进量。
        /// </summary>
        protected int Indent
        {
            get { return (_sql.InnerWriter as IndentedTextWriter).Indent; }
            set { (_sql.InnerWriter as IndentedTextWriter).Indent = value; }
        }

        /// <summary>
        /// 是否自动添加标识符的括号。默认为 true。
        /// </summary>
        public bool AutoQuota { get; set; } = true;

        /// <summary>
        /// 参数是否需要嵌入到 SQL 中来执行。默认为 false。
        /// 对于非索引的参数，都会以参数化方式来执行，而不论此属性是否为真。
        /// </summary>
        public bool EmbedParameters { get; set; }

        /// <summary>
        /// 生成完毕后的 Sql 语句及参数。
        /// </summary>
        public FormattedSql Sql
        {
            get { return _sql; }
        }

        /// <summary>
        /// 单个 In 条件语句中可以承受的最大的项的个数。
        /// 
        /// 这个数表示各个不同类型的数据库中能接受的个数的最小值。
        /// 
        /// Oracle: 1000.
        /// Sql server: 未限制。
        /// MySql: ???
        /// </summary>
        internal const int CampatibleMaxItemsInInClause = 1000;

        /// <summary>
        /// In 语句中可以承受的最大的个数。
        /// 如果超出这个个数，则会抛出 TooManyItemsInInClauseException。
        /// </summary>
        protected virtual int MaxItemsInInClause => CampatibleMaxItemsInInClause;

        #region 分页支持

        /// <summary>
        /// 为指定的原始查询生成指定分页效果的新查询。
        /// </summary>
        /// <param name="raw">原始查询</param>
        /// <param name="pagingInfo">分页信息。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">pagingInfo</exception>
        /// <exception cref="System.InvalidProgramException">必须排序后才能使用分页功能。</exception>
        protected abstract ISqlSelect ModifyToPagingTree(SqlSelect raw, PagingInfo pagingInfo);

        #endregion

        /// <summary>
        /// 访问 sql 语法树中的每一个结点，并生成相应的 Sql 语句。
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="pagingInfo">The paging information.</param>
        public void Generate(SqlSelect tree, PagingInfo pagingInfo = null)
        {
            ISqlSelect res = tree;
            if (!PagingInfo.IsNullOrEmpty(pagingInfo))
            {
                res = this.ModifyToPagingTree(tree, pagingInfo);
            }

            base.Visit(res);
        }

        /// <summary>
        /// 访问 sql 语法树中的每一个结点，并生成相应的 Sql 语句。
        /// </summary>
        /// <param name="tree">The tree.</param>
        public void Generate(ISqlNode tree)
        {
            base.Visit(tree);
        }

        protected override SqlLiteral VisitSqlLiteral(SqlLiteral sqlLiteral)
        {
            var parameters = sqlLiteral.Parameters;
            if (parameters != null && parameters.Length > 0)
            {
                Dictionary<int, int> indeces = null;
                var sql = Regex.Replace(sqlLiteral.FormattedSql, @"\{(?<index>\d+)\}", m =>
                {
                    var index = Convert.ToInt32(m.Groups["index"].Value);
                    var value = parameters[index];
                    if (this.EmbedParameters)
                    {
                        return this.PrepareSqlEmbedParameter(value).ToString();
                    }
                    else
                    {
                        if (indeces == null) { indeces = new Dictionary<int, int>(); }
                        if (!indeces.TryGetValue(index, out int targetIndex))
                        {
                            targetIndex = _sql.Parameters.Add(value);
                            indeces[index] = targetIndex;
                        }
                        return "{" + targetIndex + "}";
                    }
                });
                _sql.Append(sql);
            }
            else
            {
                _sql.Append(sqlLiteral.FormattedSql);
            }

            return sqlLiteral;
        }

        protected override SqlBinaryConstraint VisitSqlBinaryConstraint(SqlBinaryConstraint node)
        {
            var leftBinary = node.Left as SqlBinaryConstraint;
            var rightBinary = node.Right as SqlBinaryConstraint;
            var isLeftOr = leftBinary != null && leftBinary.Opeartor == SqlBinaryConstraintType.Or;
            var isRightOr = rightBinary != null && rightBinary.Opeartor == SqlBinaryConstraintType.Or;

            switch (node.Opeartor)
            {
                case SqlBinaryConstraintType.And:
                    if (isLeftOr) _sql.Append("(");
                    this.Visit(node.Left);
                    if (isLeftOr) _sql.Append(")");
                    _sql.AppendAnd();
                    if (isRightOr) _sql.Append("(");
                    this.Visit(node.Right);
                    if (isRightOr) _sql.Append(")");
                    break;
                case SqlBinaryConstraintType.Or:
                    this.Visit(node.Left);
                    _sql.AppendOr();
                    this.Visit(node.Right);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return node;
        }

        protected override SqlSelect VisitSqlSelect(SqlSelect sqlSelect)
        {
            _sql.Append("SELECT ");

            //SELECT
            this.GenerateSelection(sqlSelect);

            //FROM
            _sql.AppendLine();
            _sql.Append("FROM ");
            this.Visit(sqlSelect.From);

            //WHERE
            if (sqlSelect.Where != null)
            {
                _sql.AppendLine();
                _sql.Append("WHERE ");
                this.Visit(sqlSelect.Where);
            }

            //ORDER BY
            if (!sqlSelect.IsCounting && sqlSelect.OrderBy != null && sqlSelect.OrderBy.Count > 0)
            {
                _sql.AppendLine();
                this.Visit(sqlSelect.OrderBy);
            }

            return sqlSelect;
        }

        /// <summary>
        /// 生成 Selection 中的语句
        /// </summary>
        /// <param name="sqlSelect"></param>
        protected virtual void GenerateSelection(SqlSelect sqlSelect)
        {
            if (sqlSelect.IsCounting)
            {
                _sql.Append("COUNT(0)");
            }
            else
            {
                if (sqlSelect.IsDistinct)
                {
                    _sql.Append("DISTINCT ");
                }

                if (sqlSelect.Selection == null)
                {
                    _sql.Append("*");
                }
                else
                {
                    this.Visit(sqlSelect.Selection);
                }
            }
        }

        protected override SqlTable VisitSqlTable(SqlTable sqlTable)
        {
            this.QuoteAppend(sqlTable.TableName);
            if (!string.IsNullOrEmpty(sqlTable.Alias))
            {
                this.AppendNameCast();
                this.QuoteAppend(sqlTable.Alias);
            }

            return sqlTable;
        }

        protected override ISqlSelectionColumn VisitSqlColumn(ISqlSelectionColumn sqlColumn)
        {
            this.AppendColumnUsage(sqlColumn);

            if (!string.IsNullOrEmpty(sqlColumn.Alias))
            {
                this.AppendNameCast();
                this.QuoteAppend(sqlColumn.Alias);
            }

            return sqlColumn;
        }

        protected virtual void AppendColumnUsage(ISqlSelectionColumn sqlColumn)
        {
            var table = sqlColumn.Table;
            if (table != null)
            {
                this.QuoteAppend(table.Name);
                _sql.Append(".");
            }
            this.QuoteAppend(sqlColumn.ColumnName);
        }

        protected override SqlJoin VisitSqlJoin(SqlJoin sqlJoin)
        {
            this.Visit(sqlJoin.Left);

            switch (sqlJoin.JoinType)
            {
                //case SqlJoinType.Cross:
                //    _sql.Append(", ");
                //    break;
                case SqlJoinType.Inner:
                    _sql.AppendLine();
                    this.Indent++;
                    _sql.Append("INNER JOIN ");
                    this.Indent--;
                    break;
                case SqlJoinType.LeftOuter:
                    _sql.AppendLine();
                    this.Indent++;
                    _sql.Append("LEFT OUTER JOIN ");
                    this.Indent--;
                    break;
                default:
                    throw new NotSupportedException();
            }

            this.Visit(sqlJoin.Right);

            _sql.Append(" ON ");

            this.Visit(sqlJoin.Condition);

            return sqlJoin;
        }

        protected override SqlArray VisitSqlArray(SqlArray sqlArray)
        {
            for (int i = 0, c = sqlArray.Items.Count; i < c; i++)
            {
                var item = sqlArray.Items[i] as ISqlNode;
                if (i > 0)
                {
                    _sql.Append(", ");
                }
                this.Visit(item);
            }

            return sqlArray;
        }

        protected override SqlColumnConstraint VisitSqlColumnConstraint(SqlColumnConstraint node)
        {
            var op = node.Operator;
            var value = node.Value;

            value = this.DbTypeCoverter.ToDbParameterValue(value);

            #region 处理一些特殊的值

            switch (op)
            {
                case SqlColumnConstraintOperator.Like:
                    if (IsEmptyOrWildcard(value as string))
                    {
                        _sql.Append("1 = 1");
                        return node;
                    }
                    break;
                case SqlColumnConstraintOperator.NotLike:
                    //如果是空字符串的模糊对比操作，直接认为是假。
                    if (IsEmptyOrWildcard(value as string))
                    {
                        _sql.Append("1 != 1");
                        return node;
                    }
                    break;
                case SqlColumnConstraintOperator.Contains:
                case SqlColumnConstraintOperator.StartsWith:
                case SqlColumnConstraintOperator.EndsWith:
                    //如果是空字符串的模糊对比操作，直接认为是真。
                    if (string.IsNullOrEmpty(value as string))
                    {
                        _sql.Append("1 = 1");
                        return node;
                    }
                    break;
                case SqlColumnConstraintOperator.NotContains:
                case SqlColumnConstraintOperator.NotStartsWith:
                case SqlColumnConstraintOperator.NotEndsWith:
                    //如果是空字符串的模糊对比操作，直接认为是假。
                    if (string.IsNullOrEmpty(value as string))
                    {
                        _sql.Append("1 != 1");
                        return node;
                    }
                    break;
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
                                _sql.Append("0 = 1");
                            }
                            else
                            {
                                _sql.Append("1 = 1");
                            }

                            return node;
                        }
                    }
                    break;
                default:
                    break;
            }

            #endregion

            var column = node.Column;
            this.AppendColumnUsage(node.Column);

            //根据不同的操作符，来生成不同的_sql。
            switch (op)
            {
                case SqlColumnConstraintOperator.Equal:
                    if (value == null || value == DBNull.Value)
                    {
                        _sql.Append(" IS NULL");
                    }
                    else
                    {
                        _sql.Append(" = ");
                        this.AppendParameter(value, column);
                    }
                    break;
                case SqlColumnConstraintOperator.NotEqual:
                    if (value == null || value == DBNull.Value)
                    {
                        _sql.Append(" IS NOT NULL");
                    }
                    else
                    {
                        _sql.Append(" != ");
                        this.AppendParameter(value, column);
                    }
                    break;
                case SqlColumnConstraintOperator.Greater:
                    _sql.Append(" > ");
                    this.AppendParameter(value, column);
                    break;
                case SqlColumnConstraintOperator.GreaterEqual:
                    _sql.Append(" >= ");
                    this.AppendParameter(value, column);
                    break;
                case SqlColumnConstraintOperator.Less:
                    _sql.Append(" < ");
                    this.AppendParameter(value, column);
                    break;
                case SqlColumnConstraintOperator.LessEqual:
                    _sql.Append(" <= ");
                    this.AppendParameter(value, column);
                    break;
                case SqlColumnConstraintOperator.Like:
                    _sql.Append(" LIKE ");
                    this.AppendParameter(value, column);
                    break;
                case SqlColumnConstraintOperator.NotLike:
                    _sql.Append(" NOT LIKE ");
                    this.AppendParameter(value, column);
                    break;
                case SqlColumnConstraintOperator.Contains:
                    _sql.Append(" LIKE ");
                    this.AppendParameter(WILDCARD_ALL + this.Escape(value) + WILDCARD_ALL, column);
                    this.AppendEscapePlause(value);
                    break;
                case SqlColumnConstraintOperator.NotContains:
                    _sql.Append(" NOT LIKE ");
                    this.AppendParameter(WILDCARD_ALL + this.Escape(value) + WILDCARD_ALL, column);
                    this.AppendEscapePlause(value);
                    break;
                case SqlColumnConstraintOperator.StartsWith:
                    _sql.Append(" LIKE ");
                    this.AppendParameter(this.Escape(value) + WILDCARD_ALL, column);
                    this.AppendEscapePlause(value);
                    break;
                case SqlColumnConstraintOperator.NotStartsWith:
                    _sql.Append(" NOT LIKE ");
                    this.AppendParameter(this.Escape(value) + WILDCARD_ALL, column);
                    this.AppendEscapePlause(value);
                    break;
                case SqlColumnConstraintOperator.EndsWith:
                    _sql.Append(" LIKE ");
                    this.AppendParameter(WILDCARD_ALL + this.Escape(value), column);
                    this.AppendEscapePlause(value);
                    break;
                case SqlColumnConstraintOperator.NotEndsWith:
                    _sql.Append(" NOT LIKE ");
                    this.AppendParameter(WILDCARD_ALL + this.Escape(value), column);
                    this.AppendEscapePlause(value);
                    break;
                case SqlColumnConstraintOperator.In:
                case SqlColumnConstraintOperator.NotIn:
                    var opSql = op == SqlColumnConstraintOperator.In ? "IN" : "NOT IN";
                    _sql.Append(" ").Append(opSql).Append(" (");
                    if (value is IEnumerable)
                    {
                        bool first = true;
                        bool needDelimiter = false;
                        int i = 0;
                        foreach (var item in value as IEnumerable)
                        {
                            if (++i > this.MaxItemsInInClause) throw new TooManyItemsInInClauseException();

                            if (first)
                            {
                                first = false;
                                needDelimiter = item is string || item is DateTime || item is Guid;
                            }
                            else { _sql.Append(','); }

                            if (needDelimiter)
                            {
                                _sql.Append('\'').Append(EscapeSpecialChar(item)).Append('\'');
                            }
                            else
                            {
                                _sql.Append(item);
                            }

                            //由于集合中的数据可能过多，所以这里不要使用参数化的查询。
                            //_sql.AppendParameter(item);
                        }
                    }
                    else if (value is ISqlNode)
                    {
                        _sql.AppendLine();
                        this.Indent++;
                        this.Visit(value as ISqlNode);
                        this.Indent--;
                        _sql.AppendLine();
                    }
                    _sql.Append(')');
                    break;
                default:
                    throw new NotSupportedException();
            }

            return node;
        }

        private static bool IsEmptyOrWildcard(string strValue)
        {
            return
                string.IsNullOrEmpty(strValue) ||
                !strValue.Contains(WILDCARD_ALL_ESCAPED) &&
                !strValue.Contains(WILDCARD_SINGLE_ESCAPED) &&
                strValue.Replace(WILDCARD_ALL, string.Empty).Replace(WILDCARD_SINGLE, string.Empty).Length == 0;
        }

        private void AppendParameter(object value, SqlColumn column)
        {
            //如果只传入参数的值，那么 DbDataParameter 中是没有设置 DbType 的，这会造成索引无效。
            if (column.HasIndex || value == null)
            {
                value = new DbAccesserParameter(value, column.DbType);
            }

            if (value is DbAccesserParameter || !this.EmbedParameters)
            {
                _sql.AppendParameter(value);
            }
            else
            {
                value = this.PrepareSqlEmbedParameter(value);
                _sql.InnerWriter.Write(value);
            }
        }

        protected virtual object PrepareSqlEmbedParameter(object value)
        {
            if (value == null)
            {
                value = "NULL";
            }
            else if (value is DateTime)
            {
                value = '\'' + ((DateTime)value).ToString() + '\'';
            }
            else if (value is string)
            {
                value = '\'' + value.ToString() + '\'';
            }
            else if (value is bool)
            {
                value = Convert.ToByte(value);//bool 使用的是 1 或 0
            }
            return value;
        }

        /// <summary>
        /// in 和 not in 没用参数化 所以要转义特殊字符 
        /// </summary>
        /// <param name="value">参数</param>
        /// <returns>value double 单引号</returns>
        protected virtual object EscapeSpecialChar(object value)
        {
            if (value is string)
            {
                value = value.ToString().Replace("'", "''");
            }
            return value;
        }

        private string Escape(object value)
        {
            return value.ToString()
                .Replace(WILDCARD_ALL, WILDCARD_ALL_ESCAPED)
                .Replace(WILDCARD_SINGLE, WILDCARD_SINGLE_ESCAPED);
        }

        private void AppendEscapePlause(object value)
        {
            //http://blog.sina.com.cn/s/blog_415bd707010006qv.html
            var strValue = value as string;
            if (!string.IsNullOrWhiteSpace(strValue) &&
                (strValue.Contains(WILDCARD_ALL) || strValue.Contains(WILDCARD_SINGLE))
                )
            {
                _sql.Append(" ESCAPE '").Append(ESCAPE_CHAR).Append('\'');
            }
        }

        protected override SqlSelectAll VisitSqlSelectAll(SqlSelectAll sqlSelectStar)
        {
            if (sqlSelectStar.Table != null)
            {
                this.QuoteAppend(sqlSelectStar.Table.Name);
                _sql.Append(".*");
            }
            else
            {
                _sql.Append("*");
            }

            return sqlSelectStar;
        }

        protected override SqlColumnsComparisonConstraint VisitSqlColumnsComparisonConstraint(SqlColumnsComparisonConstraint node)
        {
            this.AppendColumnUsage(node.LeftColumn);
            switch (node.Operator)
            {
                case SqlColumnConstraintOperator.Equal:
                    _sql.Append(" = ");
                    break;
                case SqlColumnConstraintOperator.NotEqual:
                    _sql.Append(" != ");
                    break;
                case SqlColumnConstraintOperator.Greater:
                    _sql.Append(" > ");
                    break;
                case SqlColumnConstraintOperator.GreaterEqual:
                    _sql.Append(" >= ");
                    break;
                case SqlColumnConstraintOperator.Less:
                    _sql.Append(" < ");
                    break;
                case SqlColumnConstraintOperator.LessEqual:
                    _sql.Append(" <= ");
                    break;
                default:
                    throw new NotSupportedException("两个属性之间的对比，只能使用 6 类基本对比。");
            }
            this.AppendColumnUsage(node.RightColumn);

            return node;
        }

        protected override SqlExistsConstraint VisitSqlExistsConstraint(SqlExistsConstraint sqlExistsConstraint)
        {
            _sql.Append("EXISTS (");
            _sql.AppendLine();

            this.Indent++;
            this.Visit(sqlExistsConstraint.Select);
            this.Indent--;

            _sql.AppendLine();
            _sql.Append(")");

            return sqlExistsConstraint;
        }

        protected override SqlNotConstraint VisitSqlNotConstraint(SqlNotConstraint sqlNotConstraint)
        {
            _sql.Append("NOT (");
            this.Visit(sqlNotConstraint.Constraint);
            _sql.Append(")");

            return sqlNotConstraint;
        }

        protected override SqlSubSelect VisitSqlSubSelect(SqlSubSelect sqlSelectRef)
        {
            _sql.Append("(");
            _sql.AppendLine();
            this.Indent++;
            this.Visit(sqlSelectRef.Select);
            this.Indent--;
            _sql.AppendLine();
            _sql.Append(")");
            this.AppendNameCast();
            _sql.Append(sqlSelectRef.Alias);

            return sqlSelectRef;
        }

        protected override SqlOrderBy VisitSqlOrderBy(SqlOrderBy sqlOrderBy)
        {
            this.AppendColumnUsage(sqlOrderBy.Column);
            _sql.Append(" ");
            _sql.Append(sqlOrderBy.Direction == OrderDirection.Ascending ? "ASC" : "DESC");

            return sqlOrderBy;
        }

        protected override SqlOrderByList VisitSqlOrderByList(SqlOrderByList sqlOrderByList)
        {
            _sql.Append("ORDER BY ");

            for (int i = 0, c = sqlOrderByList.Items.Count; i < c; i++)
            {
                if (i > 0)
                {
                    _sql.Append(", ");
                }
                this.Visit(sqlOrderByList.Items[i] as SqlOrderBy);
            }

            return sqlOrderByList;
        }

        /// <summary>
        /// 把标识符添加到 Sql 语句中。
        /// 子类可重写此方法来为每一个标识符添加引用符。
        /// SqlServer 生成 [identifier]
        /// Oracle 生成 "IDENTIFIER"
        /// </summary>
        /// <param name="identifier"></param>
        protected void QuoteAppend(string identifier)
        {
            identifier = this.Prepare(identifier);

            if (this.AutoQuota && _identifierProvider.QuoteStart != char.MinValue)
            {
                _sql.Append(_identifierProvider.QuoteStart);
                _sql.Append(identifier);
                _sql.Append(_identifierProvider.QuoteEnd);
            }
            else
            {
                _sql.Append(identifier);
            }
        }

        /// <summary>
        /// 准备所有标识符。
        /// Oracle 可重写此方法，使得标识符都是大写的。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        protected string Prepare(string identifier)
        {
            return _identifierProvider.Prepare(identifier);
        }

        protected virtual void AppendNameCast()
        {
            _sql.Append(" ");
        }
    }

    /// <summary>
    /// 表示在 In 语句中使用了过多的参数。
    /// </summary>
    [Serializable]
    public class TooManyItemsInInClauseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyItemsInInClauseException"/> class.
        /// </summary>
        public TooManyItemsInInClauseException() : base("在 In 语句中使用了过多的参数") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyItemsInInClauseException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected TooManyItemsInInClauseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
