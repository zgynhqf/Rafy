/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120424
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120424
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using Rafy;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.Query.Impl;
using Rafy.Domain.ORM.SqlTree;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Utils;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 数据表的 CDUQ 最终实现类。
    /// </summary>
    internal abstract class RdbTable
    {
        #region 私有字段

        private IRepositoryInternal _repository;

        private EntityMeta _meta;

        private IRdbTableInfo _tableInfo;

        private List<RdbColumn> _columns;

        private RdbColumn _identityColumn;

        private RdbColumn _pkColumn;

        #endregion

        internal RdbTable(IRepositoryInternal repository, string dbProvider)
        {
            _repository = repository;
            _meta = repository.EntityMeta;
            _tableInfo = RdbTableInfoFactory.FindOrCreateTableInfo(_meta, dbProvider);
            _columns = new List<RdbColumn>();

            _deleteSql = new Lazy<string>(this.GenerateDeleteSQL);
            _insertSql = new Lazy<string>(() => this.GenerateInsertSQL(false));
            _insertSqlWithId = new Lazy<string>(() => this.GenerateInsertSQL(true));
        }

        internal IRepositoryInternal Repository
        {
            get { return _repository; }
        }

        /// <summary>
        /// Gets or sets the identifier provider.
        /// </summary>
        /// <value>
        /// The identifier provider.
        /// </value>
        public IDbIdentifierQuoter IdentifierProvider { get; internal set; }

        /// <summary>
        /// Gets or sets the database type converter.
        /// </summary>
        /// <value>
        /// The database type converter.
        /// </value>
        public DbTypeConverter DbTypeConverter { get; internal set; }

        internal virtual RdbColumn CreateColumn(IRdbColumnInfo columnInfo)
        {
            return new RdbColumn(this, columnInfo);
        }

        public abstract SqlGenerator CreateSqlGenerator();

        /// <summary>
        /// 此方法用于调试。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal string DebugSql(IQueryNode node)
        {
            var generator = this.CreateSqlGenerator();
            generator.Generate(node as ISqlNode);
            return generator.Sql.ToString();
        }

        #region 属性 及 元数据

        /// <summary>
        /// 表名
        /// </summary>
        public string Name
        {
            get { return _tableInfo.Name; }
        }

        /// <summary>
        /// 表的信息
        /// </summary>
        public IRdbTableInfo Info
        {
            get { return _tableInfo; }
        }

        /// <summary>
        /// 主键列。
        /// </summary>
        public RdbColumn PKColumn
        {
            get { return _pkColumn; }
        }

        /// <summary>
        /// 自增列。
        /// （某些表可以会没有自增列。）
        /// </summary>
        public RdbColumn IdentityColumn
        {
            get { return _identityColumn; }
        }

        /// <summary>
        /// 本表中可用的所有字段信息。
        /// </summary>
        public IReadOnlyList<RdbColumn> Columns
        {
            get { return this._columns; }
        }

        internal void Add(RdbColumn column)
        {
            if (column.Info.IsIdentity)
            {
                if (_identityColumn != null)
                {
                    throw new ORMException(string.Format(
                        "cannot add idenity column {0} to table {1}, it already has an identity column {2}",
                        column.Name, this.Name, _identityColumn.Name));
                }
                _identityColumn = column;
            }

            if (column.Info.IsPrimaryKey)
            {
                if (_pkColumn != null)
                {
                    throw new ORMException(string.Format(
                        "cannot add primary key column {0} to table {1}, it already has an primary key column {2}",
                        column.Name, this.Name, _pkColumn.Name));
                }

                _pkColumn = column;
            }

            if (column.IsLOB)
            {
                _hasLOB = true;
            }

            _columns.Add(column);
        }

        #endregion

        #region 数据操作 CUD 及相应的 SQL 生成。

        /// <summary>
        /// 插入实体的 Sql 语句。（插入语句中不含 Identity 列的插入。）
        /// </summary>
        protected Lazy<string> _insertSql;

        /// <summary>
        /// 插入实体的 Sql 语句。（插入语句中包含 Identity 列的插入。）
        /// </summary>
        protected Lazy<string> _insertSqlWithId;

        private Lazy<string> _deleteSql;

        private bool _hasLOB;

        /// <summary>
        /// 执行 sql 插入一个实体到数据库中。
        /// 基类的默认实现中，只是简单地实现了 sql 语句的生成和执行。
        /// </summary>
        /// <param name="dba"></param>
        /// <param name="item"></param>
        public virtual void Insert(IDbAccesser dba, Entity item)
        {
            EnsureMappingTable();

            var parameters = this.GenerateInsertSqlParameters(item, false);

            dba.ExecuteText(_insertSql.Value, parameters);
        }

        /// <summary>
        /// 生成 Insert 语句
        /// </summary>
        /// <param name="withIdentity">是否将 Identity 列也生成到 Insert 语句中。</param>
        /// <returns></returns>
        internal string GenerateInsertSQL(bool withIdentity)
        {
            var sql = new StringWriter();
            sql.Write("INSERT INTO ");
            sql.AppendQuote(this, this.Name).Write(" (");

            var values = new StringBuilder();
            var index = 0;
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (column.ShouldInsert(withIdentity))
                {
                    if (index > 0)
                    {
                        sql.Write(',');
                        values.Append(',');
                    }

                    sql.AppendQuote(this, column.Name);
                    values.Append('{').Append(index++).Append('}');
                }
            }

            sql.Write(") VALUES (");
            sql.Write(values.ToString());
            sql.Write(")");

            return sql.ToString();
        }

        /// <summary>
        /// 生成 <see cref="GenerateInsertSQL(bool)"/> 方法所对应的参数列表。
        /// </summary>
        /// <param name="item">被插件的实体。（从这个实体提取参数的值。）</param>
        /// <param name="withIdentity">是否将 Identity 列的值，也生成到参数数组中。</param>
        /// <returns></returns>
        protected object[] GenerateInsertSqlParameters(Entity item, bool withIdentity)
        {
            var parameters = new List<object>(_columns.Count);
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (column.ShouldInsert(withIdentity))
                {
                    parameters.Add(ReadParamater(item, column));
                }
            }
            return parameters.ToArray();
        }

        internal int Delete(IDbAccesser dba, Entity item)
        {
            EnsureMappingTable();

            var result = dba.ExecuteText(_deleteSql.Value, item.Id);

            return result;
        }

        private string GenerateDeleteSQL()
        {
            var sql = new StringWriter();
            sql.Write("DELETE FROM ");
            sql.AppendQuoteName(this);
            sql.Write(" WHERE ");
            sql.AppendQuote(this, this.PKColumn.Name).Write(" = {0}");
            return sql.ToString();
        }

        internal int Delete(IDbAccesser dba, IConstraint where)
        {
            if (where == null) throw new ArgumentNullException("where");

            EnsureMappingTable();

            var sql = new StringWriter();
            sql.Write("DELETE FROM ");
            sql.AppendQuoteName(this);
            sql.Write(" WHERE ");

            var generator = this.CreateSqlGenerator();
            generator.Generate(where as ISqlNode);
            var whereSql = generator.Sql;
            sql.Write(whereSql.ToString());

            return dba.ExecuteText(sql.ToString(), whereSql.Parameters);
        }

        internal int DeleteAll(IDbAccesser dba)
        {
            EnsureMappingTable();

            var sql = new StringWriter();
            sql.Write("DELETE FROM ");
            sql.AppendQuoteName(this);

            return dba.ExecuteText(sql.ToString());
        }

        public virtual int Update(IDbAccesser dba, Entity item, bool updateChangedPropertiesOnly)
        {
            EnsureMappingTable();

            var updateColumns = new List<RdbColumn>();
            var parameters = new List<object>(10);

            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (!column.Info.IsPrimaryKey)
                {
                    var field = item.GetField(column.Info.Property);
                    if (field.IsDisabled) continue;

                    if (!updateChangedPropertiesOnly || field.IsChanged)
                    {
                        updateColumns.Add(column);
                        parameters.Add(ReadParamater(item, column));
                    }
                }
            }

            if (updateColumns.Count == 0) return 1;

            //sql
            var updateSql = this.GenerateUpdateSQL(updateColumns);
            parameters.Add(item.Id);

            //execute
            int res = dba.ExecuteText(updateSql, parameters.ToArray());

            return res;
        }

        private string GenerateUpdateSQL(IList<RdbColumn> updateColumns)
        {
            var sql = new StringWriter();
            sql.Write("UPDATE ");
            sql.AppendQuoteName(this);
            sql.Write(" SET ");

            bool comma = false;
            var paramIndex = 0;

            for (int i = 0, c = updateColumns.Count; i < c; i++)
            {
                var column = updateColumns[i];

                if (comma) { sql.Write(','); }
                else { comma = true; }

                sql.AppendQuote(this, column.Name);
                sql.Write(" = {");
                sql.Write(paramIndex++);
                sql.Write('}');
            }

            sql.Write(" WHERE ");
            sql.AppendQuote(this, _pkColumn.Name);
            sql.Write(" = {");
            sql.Write(paramIndex);
            sql.Write('}');

            return sql.ToString();
        }

        private void EnsureMappingTable()
        {
            if (_meta.TableMeta.IsMappingView) throw new NotSupportedException(string.Format("{0} 类映射视图，不能进行 CDU 操作。", _meta.EntityType.Name));
        }

        private static object ReadParamater(Entity item, RdbColumn column)
        {
            var value = column.ReadDbParameterValue(item);
            if (value == DBNull.Value)
            {
                value = new DbAccesserParameter(value, column.Info.DbType);
            }
            return value;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 判断指定的分页操作，支持在哪个层面进行分页。
        /// </summary>
        /// <param name="pagingInfo">The paging information.</param>
        /// <returns></returns>
        protected virtual PagingLocation GetPagingLocation(PagingInfo pagingInfo)
        {
            //虽然本类默认使用数据库分页，但是它的子类可以重写本方法来使用内存分页。
            //所以本类中的所有方法，在重新实现时，都会分辨这两种情况。
            return PagingLocation.Database;
        }

        /// <summary>
        /// 使用 IQuery 条件进行查询。
        /// 分页默认实现为使用内存进行分页。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        public virtual void QueryList(IDbAccesser dba, IORMQueryArgs args)
        {
            //检查分页条件。（如果是树状实体，也不支持在数据库中进行分页。）
            var query = args.Query;
            var selectionProperties = args.LoadOptions?.SelectionProperties;
            this.AutoSelection(query, selectionProperties);

            var pagingInfo = args.PagingInfo;
            bool isDbPaging = this.IsDbPaging(pagingInfo);

            var generator = this.CreateSqlGenerator();
            QueryFactory.Instance.Generate(generator, query, isDbPaging ? pagingInfo : PagingInfo.Empty);
            var sql = generator.Sql;

            this.QueryDataReader(dba, args, sql, sql.Parameters, selectionProperties);
            if (isDbPaging && pagingInfo.IsNeedCount)
            {
                pagingInfo.TotalCount = this.Count(dba, query);
            }
        }

        /// <summary>
        /// 判断当前是否正在进行数据库层分页。
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        protected bool IsDbPaging(PagingInfo pagingInfo)
        {
            return !PagingInfo.IsNullOrEmpty(pagingInfo) &&
                this.GetPagingLocation(pagingInfo) == PagingLocation.Database &&
                !Repository.SupportTree;
        }

        /// <summary>
        /// 执行 Sql 并读取 DataReader 中的值到实体。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="sql">The SQL.</param>
        /// <param name="sqlParamters"></param>
        /// <param name="readProperties">只读取这些属性。</param>
        protected void QueryDataReader(IDbAccesser dba, IEntityQueryArgs args, string sql, object[] sqlParamters, List<IManagedProperty> readProperties)
        {
            //查询数据库
            using (var reader = dba.QueryDataReader(sql, sqlParamters))
            {
                //填充到列表中。
                this.FillDataIntoList(
                    reader,
                    readProperties,
                    args.List,
                    args.FetchingFirst,
                    args.PagingInfo,
                    args.MarkTreeFullLoaded
                    );
            }
        }

        /// <summary>
        /// 如果没有选择项，而且有 LOB 字段或限定了只读取某些属性时，Selection 将被自动生成。
        /// </summary>
        /// <param name="query"></param>
        /// <param name="readProperties"></param>
        /// <returns></returns>
        protected void AutoSelection(IQuery query, List<IManagedProperty> readProperties)
        {
            if (query.Selection == null && !query.IsCounting && (_hasLOB || readProperties != null))
            {
                var table = query.From.FindTable(_repository) as TableSource;
                if (table != null)
                {
                    var columns = table.CacheSelectionColumnsExceptsLOB(readProperties);

                    query.Selection = QueryFactory.Instance.Array(columns);
                }
            }
        }

        /// <summary>
        /// 使用 Sql 进行查询。
        /// 分页默认实现为使用内存进行分页。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        public virtual void QueryList(IDbAccesser dba, ISqlSelectArgs args)
        {
            var pagingInfo = args.PagingInfo;
            var notPaging = !this.IsDbPaging(pagingInfo);
            if (notPaging)
            {
                if (_hasLOB)
                {
                    args.FormattedSql = this.ReplaceLOBColumnsInConvention(args.FormattedSql);
                }

                this.QueryDataReader(dba, args, args.FormattedSql, args.Parameters, args.LoadOptions?.SelectionProperties);
            }
            else
            {
                //转换为分页查询 SQL
                var parts = ParsePagingSqlParts(args.FormattedSql);
                CreatePagingSql(ref parts, pagingInfo);

                //读取分页的实体
                this.QueryDataReader(dba, args, parts.PagingSql, args.Parameters, args.LoadOptions?.SelectionProperties);

                QueryTotalCountIf(dba, pagingInfo, parts, args.Parameters);
            }
        }

        /// <summary>
        /// 使用 Sql 进行查询。
        /// 分页默认实现为使用内存进行分页。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        public virtual void QueryTable(IDbAccesser dba, ITableQueryArgs args)
        {
            var pagingInfo = args.PagingInfo;
            var notPaging = !this.IsDbPaging(pagingInfo);
            if (notPaging)
            {
                if (_hasLOB)
                {
                    args.FormattedSql = this.ReplaceLOBColumnsInConvention(args.FormattedSql);
                }

                using (var reader = dba.QueryDataReader(args.FormattedSql, args.Parameters))
                {
                    var table = args.ResultTable;
                    if (table.Columns.Count == 0)
                    {
                        LiteDataTableAdapter.AddColumns(table, reader);
                    }

                    var columnsCount = table.Columns.Count;

                    PagingHelper.MemoryPaging(reader, r =>
                    {
                        var row = table.NewRow();
                        for (int i = 0; i < columnsCount; i++)
                        {
                            var value = reader[i];
                            if (value != DBNull.Value)
                            {
                                row[i] = value;
                            }
                        }
                        table.Rows.Add(row);
                    }, args.PagingInfo);
                }
            }
            else
            {
                //转换为分页查询 SQL
                var parts = ParsePagingSqlParts(args.FormattedSql);
                CreatePagingSql(ref parts, pagingInfo);

                //读取分页的数据
                var table = args.ResultTable;
                using (var reader = dba.QueryDataReader(parts.PagingSql, args.Parameters))
                {
                    LiteDataTableAdapter.Fill(table, reader);
                }

                QueryTotalCountIf(dba, pagingInfo, parts, args.Parameters);
            }
        }

        private const string LOBColumnsToken = "{*}";

        /// <summary>
        /// 如果 sql 的 select 中使用了 {*} ，则需要将其替换为把 LOB 属性排除之后的列名。
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string ReplaceLOBColumnsInConvention(string sql)
        {
            //如果 sql 中编写了 LOBColumnsToken 这个符号，则表示需要进行列名替换。
            var tokenIndex = sql.IndexOf(LOBColumnsToken);
            if (tokenIndex < 0) return sql;

            var res = new StringWriter();

            var tableName = string.Empty;

            //如果使用了 XXX.{*}，则表示指定了表的名称，这时需要在每个字段前使用这个表的名称。
            var hasTablePrefix = sql[tokenIndex - 1] == '.';
            if (hasTablePrefix)
            {
                var tablePrefixIndex = tokenIndex - 1;
                while (true)
                {
                    tablePrefixIndex--;
                    if (tablePrefixIndex < 0) throw new InvalidOperationException("sql 语句格式有误。");
                    var c = sql[tablePrefixIndex];
                    if (c == ' ')
                    {
                        tablePrefixIndex++;
                        tableName = sql.Substring(tablePrefixIndex, tokenIndex - 1 - tablePrefixIndex);

                        var before = sql.Substring(0, tablePrefixIndex);
                        res.Write(before);

                        break;
                    }
                }
            }
            else
            {
                var before = sql.Substring(0, tokenIndex);
                res.Write(before);

                tableName = this.GetQuoteName();
            }

            //输出所有非 lob 列的列名。
            bool comma = false;
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (column.IsLOB) { continue; }
                if (comma) { res.Write(','); }
                else { comma = true; }

                res.Write(tableName);
                res.Write('.');
                res.AppendQuote(this, column.Name);
            }

            var after = sql.Substring(tokenIndex + LOBColumnsToken.Length);
            res.Write(after);

            return res.ToString();
        }

        internal long Count(IDbAccesser dba, IQuery query)
        {
            var oldCounting = query.IsCounting;
            try
            {
                query.IsCounting = true;

                var generator = this.CreateSqlGenerator();
                QueryFactory.Instance.Generate(generator, query);
                var sql = generator.Sql;

                var value = dba.QueryValue(sql, sql.Parameters);
                return ConvertCount(value);
            }
            finally
            {
                query.IsCounting = oldCounting;
            }
        }

        internal static long ConvertCount(object value)
        {
            if (value != DBNull.Value)
            {
                try
                {
                    return Convert.ToInt64(value);
                }
                catch { }
            }

            throw new InvalidOperationException("sql 语句无法查询出一个整型数值！返回的值是：" + value);
        }

        /// <summary>
        /// 如果需要统计，则生成统计语句进行查询。
        /// </summary>
        /// <param name="dba"></param>
        /// <param name="pagingInfo"></param>
        /// <param name="parts"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static void QueryTotalCountIf(IDbAccesser dba, PagingInfo pagingInfo, PagingSqlParts parts, object[] parameters)
        {
            if (pagingInfo.IsNeedCount)
            {
                var pagingCountSql = "SELECT COUNT(0) " + parts.FromWhere;

                //查询值。（由于所有参数都不会在 OrderBy、Select 语句中，所以把所有参数都传入。
                var value = dba.QueryValue(pagingCountSql, parameters);
                pagingInfo.TotalCount = Convert.ToInt64(value);
            }
        }

        #endregion

        #region 从数据行读取、创建实体

        /// <summary>
        /// 在内存中对 IDataReader 进行读取。
        /// 注意！！！
        /// 此方法中会释放 Reader。外层不能再用 Using。
        /// </summary>
        /// <param name="reader">表格类数据。</param>
        /// <param name="readProperties">如果为 null，表示尽力读取。否则，表示只读取给定的属性。</param>
        /// <param name="list">需要把读取的实体，加入到这个列表中。</param>
        /// <param name="fetchingFirst">是否只读取一条数据即返回。</param>
        /// <param name="memoryPagingInfo">内存分页信息：如果不是只取一行数据，则这个参数表示列表内存分页的信息，只在需要进行内存分页时会用到。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true ，才可以把整个树标记为完整加载。</param>
        internal protected void FillDataIntoList(
            IDataReader reader, List<IManagedProperty> readProperties, IList<Entity> list,
            bool fetchingFirst, PagingInfo memoryPagingInfo, bool markTreeFullLoaded)
        {
            //如果正在分页，而且支持数据库层面的分页，则不使用内存分页。
            if (this.IsDbPaging(memoryPagingInfo))
            {
                memoryPagingInfo = null;
            }

            if (_repository.SupportTree)
            {
                this.FillTreeIntoList(reader, readProperties, list, markTreeFullLoaded, memoryPagingInfo);
                return;
            }

            var entityReader = CreateEntityReader(readProperties);
            Action<IDataReader> rowReader = dr =>
            {
                var entity = entityReader.Read(dr);
                list.Add(entity);
            };
            if (fetchingFirst)
            {
                if (reader.Read())
                {
                    rowReader(reader);
                }
            }
            else
            {
                PagingHelper.MemoryPaging(reader, rowReader, memoryPagingInfo);
            }
        }

        /// <summary>
        /// 在内存中对 IDataReader 进行读取，并以树的方式进行节点的加载。
        /// </summary>
        /// <param name="reader">表格类数据。</param>
        /// <param name="readProperties">如果为 null，表示尽力读取。否则，表示只读取给定的属性。</param>
        /// <param name="list">需要把读取的实体中的第一级的节点，加入到这个列表中。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true ，才可以把整个树标记为完整加载。</param>
        /// <param name="pagingInfo">对根节点进行分页的信息。</param>
        private void FillTreeIntoList(
            IDataReader reader, List<IManagedProperty> readProperties, IList<Entity> list, bool markTreeFullLoaded,
            PagingInfo pagingInfo)
        {
            var entities = this.ReadToEntity(reader, readProperties);
            if (PagingInfo.IsNullOrEmpty(pagingInfo))
            {
                TreeComponentHelper.LoadTreeData(list, entities, _repository.TreeIndexOption);
            }
            else
            {
                //在内存中分页。
                var tempList = new List<Entity>();
                TreeComponentHelper.LoadTreeData(tempList, entities, _repository.TreeIndexOption);
                var paged = tempList.JumpToPage(pagingInfo);
                foreach (var item in paged) { list.Add(item); }
            }
            if (markTreeFullLoaded)
            {
                TreeComponentHelper.MarkTreeFullLoaded(list);
            }
        }

        private IEnumerable<Entity> ReadToEntity(IDataReader reader, List<IManagedProperty> readProperties)
        {
            var entityReader = this.CreateEntityReader(readProperties);

            while (reader.Read())
            {
                var entity = entityReader.Read(reader);
                yield return entity;
            }
        }

        #region EntityReader

        private EntityReader CreateEntityReader(List<IManagedProperty> readProperties)
        {
            return new CreateByNameReader
            {
                _owner = this,
                _readProperties = readProperties
            };//CreateByNameReader 由于每个 SQL 的列的顺序不一定一致，所以不能进行缓存。
        }

        private abstract class EntityReader
        {
            internal RdbTable _owner;

            public Entity Read(IDataReader reader)
            {
                var entity = Entity.New(_owner._meta.EntityType);

                this.ReadProperties(entity, reader, _owner._columns);

                entity = _owner.TryReplaceByContext(entity) as Entity;

                entity.PersistenceStatus = PersistenceStatus.Saved;

                return entity;
            }

            protected abstract void ReadProperties(Entity entity, IDataReader reader, IList<RdbColumn> columns);
        }

        //private class CreateByIndexReader : EntityReader
        //{
        //    protected override void ReadProperties(Entity entity, IDataReader reader, IList<RdbColumn> columns)
        //    {
        //        var index = 0;
        //        for (int i = 0, c = columns.Count; i < c; i++)
        //        {
        //            var column = columns[i];
        //            if (!column.IsLOB)
        //            {
        //                object val = reader.GetValue(index++);
        //                column.WritePropertyValue(entity, val);
        //            }
        //        }
        //    }
        //}

        private class CreateByNameReader : EntityReader
        {
            private int[] _columnIndeces;

            internal List<IManagedProperty> _readProperties;

            private bool NeedRead(IProperty property)
            {
                if (property == Entity.IdProperty) return true;
                if (_readProperties == null) return true;
                if (_readProperties.Contains(property)) return true;

                return false;
            }

            protected override void ReadProperties(Entity entity, IDataReader reader, IList<RdbColumn> columns)
            {
                if (_columnIndeces == null)
                {
                    //先初始化对应的索引号。
                    _columnIndeces = new int[columns.Count];
                    for (int i = 0, c = columns.Count; i < c; i++)
                    {
                        var column = columns[i];

                        if (this.NeedRead(column.Info.Property))
                        {
                            try
                            {
                                _columnIndeces[i] = reader.GetOrdinal(column.Name);
                            }
                            catch (IndexOutOfRangeException)
                            {
                                //如果 Reader 中没有这一列时，这里会抛出异常。
                                if (ORMSettings.EnablePropertiesIfNotFoundInSqlQuery && ORMSettings.ErrorIfColumnNotFoundInSql)
                                {
                                    throw new InvalidProgramException($"Sql 查询中没有给出必须的列：{column.Table}.{column.Name}，无法读取并转换实体。");
                                }
                                _columnIndeces[i] = -1;
                            }
                        }
                        else
                        {
                            _columnIndeces[i] = -1;
                        }
                    }
                }

                for (int i = 0, c = columns.Count; i < c; i++)
                {
                    var column = columns[i];
                    var index = _columnIndeces[i];
                    if (index >= 0)
                    {
                        object val = reader[index];
                        try
                        {
                            column.WritePropertyValue(entity, val);
                        }
                        catch
                        {
                            if (ORMSettings.ErrorIfColumnValueCantConvert)
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        //未读取的属性，都需要被禁用。
                        if (!ORMSettings.EnablePropertiesIfNotFoundInSqlQuery)
                        {
                            entity.Disable(column.Info.Property);
                        }
                    }
                }
            }
        }

        #endregion

        //需要同时加载引用实体的数据时，参考以下方法。
        ///// <summary>
        ///// 子类可实现此方法实现对应 GenerateSelectSQL 的数据加载逻辑。
        ///// 注意！！！
        ///// 此方法中会释放 Reader。外层不能再用 Using。
        ///// </summary>
        ///// <param name="reader">The reader.</param>
        ///// <param name="list">The list.</param>
        ///// <param name="args">The arguments.</param>
        //private void FastFillByColumnIndex(IDataReader reader, IList<Entity> list, IPropertySelectArgs args)
        //{
        //    //如果正在分页，而且不支持数据库层面的分页，则直接使用内存分页。
        //    var dbQuery = args.PropertyQuery as PropertyQuery;
        //    var memoryPagingInfo = dbQuery.PagingInfo;
        //    if (this.GetPagingLocation(memoryPagingInfo) == PagingLocation.Database) { memoryPagingInfo = null; }

        //    var fromIndex = list.Count;

        //    if (dbQuery == null || !dbQuery.HasInnerJoin)
        //    {
        //        MemoryPaging(reader, r =>
        //        {
        //            int i = 0;
        //            var entity = this.CreateByIndex(reader, ref i);
        //            list.Add(entity);
        //        }, args.FetchingFirst, memoryPagingInfo);

        //        OnDbLoaded(list, fromIndex);
        //    }
        //    else
        //    {
        //        int refItemsCount = 0;
        //        List<RefTableProperty> refItems = null;

        //        if (dbQuery.RefItems != null)
        //        {
        //            refItems = dbQuery.RefItems.Where(i => i.JoinRefType == JoinRefType.QueryData).ToList();
        //            refItemsCount = refItems.Count;
        //        }

        //        var entitiesPerRow = new List<Entity>(refItemsCount + 1);//每一行最终读取的实体列表
        //        var allRefEntities = new Dictionary<DbTable, List<Entity>>(refItemsCount + 1);

        //        MemoryPaging(reader, r =>
        //        {
        //            entitiesPerRow.Clear();

        //            int i = 0;
        //            var entity = this.CreateByIndex(reader, ref i);
        //            entitiesPerRow.Add(entity);

        //            //有 Join 时，把关系对象也加载进来。
        //            for (int j = 0; j < refItemsCount; j++)
        //            {
        //                var refItem = refItems[j];
        //                var refTable = refItem.RefTable;

        //                //如果创建的对象是关联中主表对应的实体类型，则表示找到数据。此时设置关联属性。
        //                for (int z = 0, c = entitiesPerRow.Count; z < c; z++)
        //                {
        //                    var created = entitiesPerRow[z];
        //                    if (refItem.PropertyOwner.IsInstanceOfType(created))
        //                    {
        //                        var refEntity = refTable.CreateByIndex(reader, ref i);
        //                        created.SetRefEntity(refItem.RefProperty.RefEntityProperty, refEntity);
        //                        entitiesPerRow.Add(refEntity);

        //                        //添加到 allRefEntities 中
        //                        List<Entity> refList = null;
        //                        if (!allRefEntities.TryGetValue(refTable, out refList))
        //                        {
        //                            refList = new List<Entity>();
        //                            allRefEntities.Add(refTable, refList);
        //                        }
        //                        refList.Add(refEntity);

        //                        break;
        //                    }
        //                }
        //            }

        //            list.Add(entity);
        //        }, args.FetchingFirst, memoryPagingInfo);

        //        //不同的实体，使用各自的仓库来通知加载完成。
        //        OnDbLoaded(list, fromIndex);
        //        foreach (var kv in allRefEntities)
        //        {
        //            kv.Key.OnDbLoaded(kv.Value);
        //        }
        //    }
        //}

        ///// <summary>
        ///// 把某一行翻译成一个实体对象
        ///// </summary>
        ///// <param name="row">The row.</param>
        ///// <returns></returns>
        //public Entity CreateFrom(DataRow row)
        //{
        //    var entity = this.CreateEntity();

        //    foreach (var column in this.Columns)
        //    {
        //        object val = row[column.Name];
        //        column.LoadValue(entity, val);
        //    }

        //    return entity;
        //}

        #endregion

        #region EntityContext

        /// <summary>
        /// 如果目前使用了 EntityContext，则应该使用内存中已经存在的对象。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private IEntity TryReplaceByContext(IEntity entity)
        {
            var current = EntityContext.Current;
            if (current != null)
            {
                var typeContext = current.GetOrCreateTypeContext(this.Info.EntityType);

                var id = entity.Id;
                var item = typeContext.TryGetById(id);
                if (item != null)
                {
                    entity = item;
                }
                else
                {
                    typeContext.Add(id, entity);
                }
            }

            return entity;
        }

        /// <summary>
        /// 如果目前使用了 EntityContext，则应该把加载好的对象都存储在内存中。
        /// </summary>
        /// <param name="entity"></param>
        internal void AddIntoEntityContext(IEntity entity)
        {
            var current = EntityContext.Current;
            if (current != null)
            {
                var typeContext = current.GetOrCreateTypeContext(this.Info.EntityType);
                typeContext.Set(entity.Id, entity);
            }
        }

        #endregion

        #region ToPagingSql

        protected virtual void CreatePagingSql(ref PagingSqlParts parts, PagingInfo pagingInfo)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 注意，这个方法只支持不太复杂 SQL 的转换。
             *
             * 源格式：
             * select ...... from ...... order by xxxx asc, yyyy desc
             * 不限于以上格式，只要满足没有复杂的嵌套查询，最外层是一个 Select 和 From 语句即可。
             * 
             * 目标格式：
             * select * from (select ......, row_number() over(order by xxxx asc, yyyy desc) _rowNumber from ......) x where x._rowNumber<10 and x._rowNumber>5;
            **********************************************************************/

            var startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
            var endRow = startRow + pagingInfo.PageSize - 1;

            var sql = new StringBuilder("SELECT * FROM (");

            //在 Select 和 From 之间插入：
            //,row_number() over(order by UPDDATETIME desc) rn 
            sql.AppendLine().Append(parts.Select)
                .Append(", row_number() over(")
                .Append(parts.OrderBy);
            //query.AppendSqlOrder(res, this);
            sql.Append(") dataRowNumber ").Append(parts.FromWhere)
                .Append(") x").AppendLine()
                .Append("WHERE x.dataRowNumber >= ").Append(startRow)
                .Append(" AND x.dataRowNumber <= ").Append(endRow);

            parts.PagingSql = sql.ToString();
        }

        private static Regex FromRegex = new Regex(@"[^\w]FROM[^\w]", RegexOptions.IgnoreCase);

        private static PagingSqlParts ParsePagingSqlParts(string sql)
        {
            var fromIndex = FromRegex.Match(sql).Index;
            var orderByIndex = sql.LastIndexOf("ORDER BY", StringComparison.OrdinalIgnoreCase);
            if (orderByIndex < 0) { throw new InvalidProgramException("使用数据库分页时，Sql 语句中必须指定 OrderBy 语句。"); }

            var parts = new PagingSqlParts();
            parts.RawSql = sql;
            parts.Select = sql.Substring(0, fromIndex).Trim();
            parts.FromWhere = sql.Substring(fromIndex, orderByIndex - fromIndex).Trim();
            parts.OrderBy = sql.Substring(orderByIndex).Trim();
            return parts;
        }

        protected struct PagingSqlParts
        {
            /// <summary>
            /// 原始 SQL
            /// </summary>
            public string RawSql;

            /// <summary>
            /// Select 语句
            /// </summary>
            public string Select;

            /// <summary>
            /// From 以及 Where 语句
            /// </summary>
            public string FromWhere;

            /// <summary>
            /// OrderBy 语句
            /// </summary>
            public string OrderBy;

            /// <summary>
            /// 转换后的分页 SQL
            /// </summary>
            public string PagingSql;
        }

        #endregion

        #region 其它方法

        /// <summary>
        /// 把属性名转换为列名
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public string Translate(IManagedProperty property)
        {
            return this.Translate(property.Name);
        }

        /// <summary>
        /// 把属性名转换为列名
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string Translate(string propertyName)
        {
            string name = null;

            RdbColumn column = FindByPropertyName(propertyName);
            if (column != null) { name = column.Name; }

            if (string.IsNullOrEmpty(name))
            {
                throw new ORMException(string.Format("表 {1} 中没有找到对应的列：{0}。", propertyName, this.Name));
            }

            return name;
        }

        internal RdbColumn FindByColumnName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;

            string target = name.ToLower();

            foreach (RdbColumn column in _columns)
            {
                if (column.Name == target) return column;
            }

            return null;
        }

        internal RdbColumn FindByPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            string target = propertyName;

            foreach (RdbColumn column in _columns)
            {
                if (column.Info.Property.Name == target) return column;
            }

            return null;
        }

        #endregion

        #region 帮助方法

        internal string GetQuoteName()
        {
            var sql = new StringWriter();
            sql.AppendQuoteName(this);
            return sql.ToString();
        }

        /// <summary>
        /// 引用某个标识符后，向 sql 输出。
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="identifier">The identifier.</param>
        public void AppendQuote(TextWriter sql, string identifier)
        {
            if (this.IdentifierProvider.QuoteStart != char.MinValue)
            {
                sql.Write(this.IdentifierProvider.QuoteStart);
                this.AppendPrepare(sql, identifier);
                sql.Write(this.IdentifierProvider.QuoteEnd);
            }
            else
            {
                this.AppendPrepare(sql, identifier);
            }
        }

        /// <summary>
        /// 每个标记符被 SQL 语句使用前都需要使用此语句进行准备。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="identifier"></param>
        internal void AppendPrepare(TextWriter sql, string identifier)
        {
            identifier = this.IdentifierProvider.Prepare(identifier);
            sql.Write(identifier);
        }

        #endregion
    }

    /// <summary>
    /// 分页的位置
    /// </summary>
    internal enum PagingLocation
    {
        /// <summary>
        /// 内存分页
        /// </summary>
        Memory,
        /// <summary>
        /// 数据库分页
        /// </summary>
        Database
    }
}