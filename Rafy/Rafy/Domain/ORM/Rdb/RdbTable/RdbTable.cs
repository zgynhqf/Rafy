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
using Rafy;
using Rafy.Data;
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
    /// 数据表的 ORM 运行时对象
    /// </summary>
    internal abstract class RdbTable
    {
        #region 私有字段

        private IRepositoryInternal _repository;

        private EntityMeta _meta;

        private IPersistanceTableInfo _tableInfo;

        private List<RdbColumn> _columns;

        private RdbColumn _identityColumn;

        private RdbColumn _pkColumn;

        private string _insertSQL;

        private string _deleteSQL;

        #endregion

        internal RdbTable(IRepositoryInternal repository)
        {
            _repository = repository;
            _meta = repository.EntityMeta;
            _tableInfo = repository.TableInfo;
            _columns = new List<RdbColumn>();
        }

        internal IRepositoryInternal Repository
        {
            get { return _repository; }
        }

        internal virtual RdbColumn CreateColumn(IPersistanceColumnInfo columnInfo)
        {
            return new RdbColumn(this, columnInfo);
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
        public IPersistanceTableInfo Info
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

        private bool _hasLOB;
        private string _updateSQL;

        /// <summary>
        /// 执行 sql 插入一个实体到数据库中。
        /// 基类的默认实现中，只是简单地实现了 sql 语句的生成和执行。
        /// </summary>
        /// <param name="dba"></param>
        /// <param name="item"></param>
        public virtual void Insert(IDbAccesser dba, Entity item)
        {
            EnsureMappingTable();

            if (this._insertSQL == null) { this._insertSQL = this.GenerateInsertSQL(); }

            var parameters = new List<object>(10);
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (column.CanInsert)
                {
                    var value = column.ReadParameterValue(item);
                    parameters.Add(value);
                }
            }

            dba.ExecuteText(this._insertSQL, parameters.ToArray());
        }

        /// <summary>
        /// 生成Insert 语句
        /// </summary>
        /// <param name="isManualIdentity">是否手动赋值identity</param>
        /// <returns></returns>
        internal string GenerateInsertSQL(bool isManualIdentity = false)
        {
            var sql = new StringWriter();
            sql.Write("INSERT INTO ");
            sql.AppendQuote(this, this.Name).Write(" (");

            var values = new StringBuilder();
            bool comma = false;
            var index = 0;
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (column.CanInsert || (column.Info.IsIdentity && isManualIdentity))
                {
                    if (comma)
                    {
                        sql.Write(',');
                        values.Append(',');
                    }
                    else { comma = true; }

                    sql.AppendQuote(this, column.Name);
                    values.Append('{').Append(index++).Append('}');
                }
            }

            sql.Write(") VALUES (");
            sql.Write(values.ToString());
            sql.Write(")");

            return sql.ToString();
        }

        internal int Delete(IDbAccesser dba, Entity item)
        {
            EnsureMappingTable();

            if (this._deleteSQL == null) { this._deleteSQL = this.GenerateDeleteSQL(); }

            var result = dba.ExecuteText(this._deleteSQL, item.Id);

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
            generator.Generate(where as SqlNode);
            var whereSql = generator.Sql;
            sql.Write(whereSql.ToString());

            return dba.ExecuteText(sql.ToString(), whereSql.Parameters);
        }

        internal int Update(IDbAccesser dba, Entity item)
        {
            EnsureMappingTable();

            var parameters = new List<object>(10);
            string updateSql = null;

            //是否有需要更新的 lob 字段。
            bool hasUpdatedLOBColumns = false;
            List<RdbColumn> lobColumns = null;

            if (_hasLOB)
            {
                lobColumns = new List<RdbColumn>();

                for (int i = 0, c = _columns.Count; i < c; i++)
                {
                    var column = _columns[i];
                    //如果一个 lob 属性的值存在，则表示需要更新。
                    //（可能被设置了，也可能只是简单地读取了一下，没有变更值。这时也简单地处理。）
                    if (column.IsLOB && item.FieldExists(column.Info.Property))
                    {
                        lobColumns.Add(column);
                        hasUpdatedLOBColumns = true;
                    }
                }
            }

            if (!hasUpdatedLOBColumns)
            {
                //如果没有 LOB，则直接缓存上更新语句。
                if (_updateSQL == null) { _updateSQL = this.GenerateUpdateSQL(null); }
                updateSql = _updateSQL;
            }
            else
            {
                updateSql = this.GenerateUpdateSQL(lobColumns);
            }

            //更新所有非 lob 的字段
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (!column.Info.IsPrimaryKey && !column.IsLOB)
                {
                    var value = column.ReadParameterValue(item);
                    parameters.Add(value);
                }
            }

            //更新需要更新的 lob 字段
            if (hasUpdatedLOBColumns)
            {
                for (int i = 0, c = lobColumns.Count; i < c; i++)
                {
                    var column = lobColumns[i];
                    if (!column.Info.IsPrimaryKey)
                    {
                        var value = column.ReadParameterValue(item);
                        parameters.Add(value);
                    }
                }
            }

            //Id 最后加入
            parameters.Add(item.Id);

            int res = dba.ExecuteText(updateSql, parameters.ToArray());
            return res;
        }

        private string GenerateUpdateSQL(IList<RdbColumn> lobColumns)
        {
            var sql = new StringWriter();
            sql.Write("UPDATE ");
            sql.AppendQuoteName(this);
            sql.Write(" SET ");

            bool comma = false;
            var paramIndex = 0;

            //先更新所有非 lob 字段。
            for (int i = 0, c = _columns.Count; i < c; i++)
            {
                var column = _columns[i];
                if (!column.Info.IsPrimaryKey && !column.IsLOB)
                {
                    if (comma) { sql.Write(','); }
                    else { comma = true; }

                    sql.AppendQuote(this, column.Name).Write(" = {");
                    sql.Write(paramIndex++);
                    sql.Write('}');
                }
            }
            //再更新所有 lob 字段。
            if (lobColumns != null)
            {
                for (int i = 0, c = lobColumns.Count; i < c; i++)
                {
                    if (comma) { sql.Write(','); }
                    else { comma = true; }

                    var column = lobColumns[i];
                    sql.AppendQuote(this, column.Name);
                    sql.Write(" = {");
                    sql.Write(paramIndex++);
                    sql.Write('}');
                }
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

        #endregion

        #region 查询

        /// <summary>
        /// 判断指定的分页操作，支持在哪个层面进行分页。
        /// </summary>
        /// <param name="pagingInfo">The paging information.</param>
        /// <returns></returns>
        protected abstract PagingLocation GetPagingLocation(PagingInfo pagingInfo);

        /// <summary>
        /// 使用 IQuery 条件进行查询。
        /// 分页默认实现为使用内存进行分页。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        public virtual void QueryList(IDbAccesser dba, IEntitySelectArgs args)
        {
            var query = args.Query;

            var autoSelection = AutoSelectionForLOB(query);

            var generator = this.CreateSqlGenerator();
            QueryFactory.Instance.Generate(generator, query);
            var sql = generator.Sql;

            this.QueryDataReader(dba, args, autoSelection ? ReadDataType.ByIndex : ReadDataType.ByName, sql);
        }

        /// <summary>
        /// 执行 Sql 并读取 DataReader 中的值到实体。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="readType">Type of the read.</param>
        /// <param name="sql">The SQL.</param>
        protected void QueryDataReader(IDbAccesser dba, IEntitySelectArgs args, ReadDataType readType, FormattedSql sql)
        {
            //查询数据库
            using (var reader = dba.QueryDataReader(sql, sql.Parameters))
            {
                //填充到列表中。
                this.FillDataIntoList(
                    reader,
                    readType,
                    args.List,
                    args.FetchingFirst,
                    args.PagingInfo,
                    args.MarkTreeFullLoaded
                    );
            }
        }

        /// <summary>
        /// 如果没有选择项，而且有 LOB 字段时，Selection 需要被自动生成，则按生成的属性的顺序来生成列的获取。
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected bool AutoSelectionForLOB(IQuery query)
        {
            if (query.Selection is AutoSelectionColumns) return true;

            if (query.Selection == null && !query.IsCounting && _hasLOB)
            {
                //加载所有不是 LOB 的列。
                var allColumns = (query.From.FindTable(_repository) as TableSource).LoadAllColumns();
                var columns = allColumns.Where(n => (n as ColumnNode).DbColumn.Property.Category != PropertyCategory.LOB);

                query.Selection = QueryFactory.Instance.AutoSelectionColumns(columns);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 使用 Sql 进行查询。
        /// 分页默认实现为使用内存进行分页。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        public virtual void QueryList(IDbAccesser dba, ISqlSelectArgs args)
        {
            if (_hasLOB)
            {
                args.FormattedSql = this.ReplaceLOBColumns(args.FormattedSql);
            }

            using (var reader = dba.QueryDataReader(args.FormattedSql, args.Parameters))
            {
                this.FillDataIntoList(
                    reader, ReadDataType.ByName,
                    args.List, args.FetchingFirst, args.PagingInfo, args.MarkTreeFullLoaded
                    );
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
            if (_hasLOB)
            {
                args.FormattedSql = this.ReplaceLOBColumns(args.FormattedSql);
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

        private const string LOBColumnsToken = "{*}";

        private string ReplaceLOBColumns(string sql)
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

        #endregion

        #region 从数据行读取、创建实体

        /// <summary>
        /// 在内存中对 IDataReader 进行读取。
        /// 注意！！！
        /// 此方法中会释放 Reader。外层不能再用 Using。
        /// </summary>
        /// <param name="reader">表格类数据。</param>
        /// <param name="readType">是否索引还是名称去读取 IDataReader。</param>
        /// <param name="list">需要把读取的实体，加入到这个列表中。</param>
        /// <param name="fetchingFirst">是否只读取一条数据即返回。</param>
        /// <param name="pagingInfo">如果不是只取一行数据，则这个参数表示列表内存分页的信息。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true ，才可以把整个树标记为完整加载。</param>
        protected void FillDataIntoList(
            IDataReader reader, ReadDataType readType,
            IList<Entity> list, bool fetchingFirst, PagingInfo pagingInfo, bool markTreeFullLoaded
            )
        {
            if (_repository.SupportTree)
            {
                this.FillTreeIntoList(reader, readType, list, markTreeFullLoaded, pagingInfo);
                return;
            }

            //如果正在分页，而且支持数据库层面的分页，则不使用内存分页。
            if (!PagingInfo.IsNullOrEmpty(pagingInfo) && this.GetPagingLocation(pagingInfo) == PagingLocation.Database)
            {
                pagingInfo = null;
            }

            var readByIndex = readType == ReadDataType.ByIndex;
            Action<IDataReader> rowReader = dr =>
            {
                var entity = readByIndex ? this.CreateByIndex(dr) : this.CreateByName(dr);
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
                PagingHelper.MemoryPaging(reader, rowReader, pagingInfo);
            }
        }

        /// <summary>
        /// 在内存中对 IDataReader 进行读取，并以树的方式进行节点的加载。
        /// </summary>
        /// <param name="reader">表格类数据。</param>
        /// <param name="readType">是否索引还是名称去读取 IDataReader。</param>
        /// <param name="list">需要把读取的实体中的第一级的节点，加入到这个列表中。</param>
        /// <param name="markTreeFullLoaded">如果某次查询结果是一棵完整的子树，那么必须设置此参数为 true ，才可以把整个树标记为完整加载。</param>
        /// <param name="pagingInfo">对根节点进行分页的信息。</param>
        private void FillTreeIntoList(
            IDataReader reader, ReadDataType readType, IList<Entity> list,
            bool markTreeFullLoaded, PagingInfo pagingInfo)
        {
            var entities = this.ReadToEntity(reader, readType);
            if (PagingInfo.IsNullOrEmpty(pagingInfo))
            {
                TreeHelper.LoadTreeData(list, entities, _repository.TreeIndexOption);
            }
            else
            {
                //在内存中分页。
                var tempList = new List<Entity>();
                TreeHelper.LoadTreeData(tempList, entities, _repository.TreeIndexOption);
                var paged = tempList.JumpToPage(pagingInfo);
                foreach (var item in paged) { list.Add(item); }
            }
            if (markTreeFullLoaded)
            {
                TreeHelper.MarkTreeFullLoaded(list);
            }
        }

        private IEnumerable<Entity> ReadToEntity(IDataReader reader, ReadDataType readType)
        {
            var readByIndex = readType == ReadDataType.ByIndex;

            //最后一次添加的节点。
            while (reader.Read())
            {
                var entity = readByIndex ? this.CreateByIndex(reader) : this.CreateByName(reader);
                yield return entity;
            }
        }

        /// <summary>
        /// 把某一行翻译成一个实体对象
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Entity CreateByIndex(IDataReader reader)
        {
            int i = 0;
            return this.CreateByIndex(reader, ref i);
        }

        private Entity CreateByIndex(IDataReader reader, ref int indexFrom)
        {
            var entity = this.CreateEntity();

            foreach (var column in this._columns)
            {
                if (!column.IsLOB)
                {
                    object val = reader.GetValue(indexFrom++);
                    column.LoadValue(entity, val);
                }
            }

            entity = this.TryReplaceByContext(entity) as Entity;

            return entity;
        }

        private Entity CreateByName(IDataReader reader)
        {
            var entity = this.CreateEntity();

            foreach (var column in _columns)
            {
                object val = reader[column.Name];
                column.LoadValue(entity, val);
            }

            entity = this.TryReplaceByContext(entity) as Entity;

            return entity;
        }

        private Entity CreateEntity()
        {
            var entity = Entity.New(this._meta.EntityType);

            entity.PersistenceStatus = PersistenceStatus.Unchanged;

            return entity;
        }

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
                var typeContext = current.GetOrCreateTypeContext(this.Info.Class);

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
        internal void NotifyLoaded(IEntity entity)
        {
            var current = EntityContext.Current;
            if (current != null)
            {
                var typeContext = current.GetOrCreateTypeContext(this.Info.Class);
                typeContext.Set(entity.Id, entity);
            }
        }

        #endregion

        #region 其它方法

        /// <summary>
        /// 把属性名转换为列名
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal string Translate(IManagedProperty property)
        {
            return this.Translate(property.Name);
        }

        /// <summary>
        /// 把属性名转换为列名
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal string Translate(string propertyName)
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
        internal abstract void AppendQuote(TextWriter sql, string identifier);

        /// <summary>
        /// 每个标记符被 SQL 语句使用前都需要使用此语句进行准备。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="identifier"></param>
        internal virtual void AppendPrepare(TextWriter sql, string identifier)
        {
            sql.Write(identifier);
        }

        #endregion

        public abstract SqlGenerator CreateSqlGenerator();

        protected enum ReadDataType { ByIndex, ByName }
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