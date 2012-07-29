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
using System.Text;
using hxy.Common.Data;
using OEA.Library;
using OEA.MetaModel;
using OEA.Utils;
using OEA.ManagedProperty;
using System.Runtime;

namespace OEA.ORM
{
    internal abstract class DbTable : ITable
    {
        #region 私有字段

        private EntityMeta _meta;

        private string _name;

        private List<DbColumn> _columns;

        private DbColumn _pkId;

        private string _insertSQL;

        private string _updateSQL;

        private string _deleteSQL;

        private string _selectSQL;

        #endregion

        public DbTable(EntityMeta meta)
        {
            this._meta = meta;
            this._name = meta.TableMeta.TableName;
            this._columns = new List<DbColumn>();
        }

        #region 属性 及 元数据

        public EntityMeta ClassMeta
        {
            get { return this._meta; }
        }

        public Type Class
        {
            get { return this._meta.EntityType; }
        }

        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Primary Key & Identity
        /// 
        /// 目前 OEA 中所有的表都由一个自增列来担任主键。
        /// </summary>
        public DbColumn PKID
        {
            get { return _pkId; }
        }

        /// <summary>
        /// 本表中可用的所有字段信息。
        /// </summary>
        public IEnumerable<IColumn> Columns
        {
            get { return this._columns; }
        }

        #endregion

        #region 数据操作 CUD 及相应的 SQL 生成。

        public virtual int Insert(IDb db, IEntity item)
        {
            if (this._insertSQL == null) { this._insertSQL = this.GenerateInsertSQL(); }

            var parameters = new List<object>();
            foreach (DbColumn column in _columns)
            {
                if (this.CanInsert(column))
                {
                    var value = column.ReadParameterValue(item);
                    parameters.Add(value);
                }
            }

            var result = db.DBA.ExecuteText(this._insertSQL, parameters.ToArray());
            return result;
        }

        protected virtual string GenerateInsertSQL()
        {
            var sql = new StringBuilder("INSERT INTO ")
                .Append(this.Quote(this._name))
                .Append(" (");
            StringBuilder values = new StringBuilder();
            bool comma = false;
            var index = 0;
            foreach (DbColumn column in _columns)
            {
                if (this.CanInsert(column))
                {
                    if (comma)
                    {
                        sql.Append(',');
                        values.Append(',');
                    }
                    else { comma = true; }

                    sql.Append(this.Quote(column.Name));
                    values.Append('{').Append(index++).Append('}');
                }
            }
            sql.Append(") VALUES (").Append(values.ToString()).Append(")");

            return sql.ToString();
        }

        protected virtual bool CanInsert(DbColumn column)
        {
            return column.IsReadable && !column.IsPKID;
        }

        public virtual int Delete(IDb db, IEntity item)
        {
            if (this._deleteSQL == null) { this._deleteSQL = this.GenerateDeleteSQL(); }

            var result = db.DBA.ExecuteText(this._deleteSQL, item.Id);

            return result;
        }

        protected virtual string GenerateDeleteSQL()
        {
            return "DELETE FROM " + this.Quote(this._name) + " WHERE ID = {0}";
        }

        public virtual int Delete(IDb db, IQuery query)
        {
            var dbQuery = query as DbQuery;

            var parameters = new FormatSqlParameter();
            string sql = this.GenerateDeleteSQL(dbQuery, parameters);

            if (dbQuery != null)
            {
                return db.DBA.ExecuteText(sql, parameters.ToArray());
            }

            return db.DBA.ExecuteTextNormal(sql);
        }

        protected virtual string GenerateDeleteSQL(DbQuery query, FormatSqlParameter parameters)
        {
            var sql = new StringBuilder("DELETE FROM ")
                .Append(this.Quote(this._name));

            if (query != null)
            {
                sql.Append(' ').Append(query.GetSqlWhereOrder(this, parameters));
            }

            return sql.ToString();
        }

        public virtual int Update(IDb db, IEntity item)
        {
            if (this._updateSQL == null) { this._updateSQL = this.GenerateUpdateSQL(null); }

            var parameters = new List<object> { item.Id };
            foreach (DbColumn column in _columns)
            {
                if (column.IsReadable && !column.IsPKID)
                {
                    var value = column.ReadParameterValue(item);
                    parameters.Add(value);
                }
            }

            return db.DBA.ExecuteText(this._updateSQL, parameters.ToArray());
        }

        protected virtual string GenerateUpdateSQL(IList<string> updateColumns)
        {
            var sql = new StringBuilder("UPDATE ").Append(this.Quote(this._name)).Append(" SET ");

            bool comma = false;
            var index = 1;
            foreach (DbColumn column in _columns)
            {
                if (column.IsReadable && !column.IsPKID &&
                    (null == updateColumns || updateColumns.Contains(column.Name.ToLower()))
                    )
                {
                    if (comma) { sql.Append(','); }
                    else { comma = true; }

                    sql.Append(this.Quote(column.Name)).Append(" = {").Append(index++).Append('}');
                }
            }
            sql.Append(" WHERE ID = {0}");

            return sql.ToString();
        }

        #endregion

        #region 查询 及 查询 SQL 生成

        public virtual void Select(IDb db, IQuery query, ICollection<Entity> list)
        {
            var dba = db.DBA;

            var dbQuery = query as DbQuery;

            var parameters = new FormatSqlParameter();
            string sql = GenerateSelectSQL(dbQuery, parameters);

            var reader = dba.QueryDataReader(sql, parameters.ToArray());
            FastFillByColumnIndex(reader, list, dbQuery);
        }

        /// <summary>
        /// 子类可实现此方法实现自己的 SQL 生成逻辑。
        /// </summary>
        /// <param name="dbQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual string GenerateSelectSQL(DbQuery dbQuery, FormatSqlParameter parameters)
        {
            var tableMeta = this._meta.TableMeta;
            if (this._selectSQL == null)
            {
                var sql = new StringBuilder("SELECT ");

                this.AppendSelectColumns(sql);

                sql.AppendLine();
                if (tableMeta.IsMappingView)
                {
                    sql.Append("FROM (").Append(tableMeta.ViewSql)
                        .AppendLine(") ")
                        .AppendLine(this.QuoteName);
                }
                else
                {
                    sql.Append("FROM ");
                    sql.AppendLine(this.QuoteName);
                }

                this._selectSQL = sql.ToString();
            }

            if (dbQuery == null || dbQuery.IsEmpty) { return this._selectSQL; }

            //生成：SELECT，INNER JOIN
            var selectSql = new StringBuilder();

            //有 Join，要把所有表的数据都带上，此时重新生成 Select。
            if (dbQuery.HasInnerJoin)
            {
                if (tableMeta.IsMappingView) { throw new NotSupportedException("View 目前不支持使用 Join。"); }

                selectSql.Append("SELECT ");
                this.AppendSelectColumns(selectSql);
                foreach (var refItem in dbQuery.RefItems)
                {
                    selectSql.Append(',');
                    refItem.RefTable.AppendSelectColumns(selectSql);
                }

                selectSql.AppendLine().Append("FROM ").AppendLine(this.QuoteName);
                selectSql.Append(dbQuery.GetSqlInnerJoin(this));
            }
            else
            {
                selectSql.Append(this._selectSQL);
            }

            //WHERE，ORDER BY
            selectSql.Append(dbQuery.GetSqlWhereOrder(this, parameters));

            return selectSql.ToString();
        }

        /// <summary>
        /// 子类可实现此方法实现对应 GenerateSelectSQL 的数据加载逻辑。
        /// 
        /// 注意！！！
        /// 此方法中会释放 Reader。外层不能再用 Using。
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="list"></param>
        /// <param name="dbQuery"></param>
        protected virtual void FastFillByColumnIndex(IDataReader reader, ICollection<Entity> list, DbQuery dbQuery)
        {
            var allEntities = new List<Entity>(20);

            if (dbQuery == null || !dbQuery.HasInnerJoin)
            {
                using (reader)
                {
                    while (reader.Read())
                    {
                        int i = 0;
                        var entity = this.CreateByIndex(reader, ref i);

                        entity.Status = PersistenceStatus.Unchanged;

                        allEntities.Add(entity);
                        list.Add(entity);
                    }
                }
            }
            else
            {
                using (reader)
                {
                    List<Entity> entitiesPerRow = new List<Entity>(dbQuery.RefItems.Count + 1);
                    //有 Join 时，把关系对象也加载进来。
                    while (reader.Read())
                    {
                        entitiesPerRow.Clear();

                        int i = 0;
                        var entity = this.CreateByIndex(reader, ref i);
                        entitiesPerRow.Add(entity);

                        foreach (var refItem in dbQuery.RefItems)
                        {
                            //如果创建的对象是关联中主表对应的实体类型，则表示找到数据。此时设置关联属性。
                            for (int j = 0, c = entitiesPerRow.Count; j < c; j++)
                            {
                                var created = entitiesPerRow[j];
                                if (refItem.PropertyOwnerType.IsInstanceOfType(created))
                                {
                                    var refEntity = refItem.RefTable.CreateByIndex(reader, ref i);
                                    created.GetLazyRef(refItem.RefProperty).Entity = refEntity;
                                    entitiesPerRow.Add(refEntity);
                                    allEntities.Add(refEntity);

                                    break;
                                }
                            }
                        }

                        allEntities.Add(entity);
                        list.Add(entity);
                    }
                }
            }

            for (int i = 0, c = allEntities.Count; i < c; i++)
            {
                var item = allEntities[i];
                item.Status = PersistenceStatus.Unchanged;

                //由于 OnDbLoaded 中可能会使用到关系，所以不能放在 Reader 中。
                item.OnDbLoaded();
            }
        }

        /// <summary>
        /// 添加一个本表对应的 列选择 Sql
        /// </summary>
        /// <param name="sql"></param>
        protected void AppendSelectColumns(StringBuilder sql)
        {
            bool comma = false;
            foreach (DbColumn column in this._columns)
            {
                if (comma) { sql.Append(','); }
                else { comma = true; }

                //由于后面可能会有关联查询，所以这里显式把表名加上。
                sql.Append(this.QuoteName).Append('.').Append(this.Quote(column.Name))
                    .Append(' ').Append(this.Name).Append('_').Append(column.Name);
            }
        }

        /// <summary>
        /// 添加一个本表对应的 列选择 Sql
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="createAlias">是否为所有列创建唯一的别名。</param>
        internal void AppendSelectColumns(StringBuilder sql, bool createAlias)
        {
            bool comma = false;
            foreach (DbColumn column in this._columns)
            {
                if (comma) { sql.Append(','); }
                else { comma = true; }

                //由于后面可能会有关联查询，所以这里显式把表名加上。
                sql.Append(this.QuoteName).Append('.').Append(this.Quote(column.Name));
                if (createAlias)
                {
                    sql.Append(' ').Append(this.Name).Append('_').Append(column.Name);
                }
            }
        }

        #endregion

        public virtual void Add(DbColumn column)
        {
            if (column.IsPKID)
            {
                if (this._pkId != null)
                {
                    throw new ORMException(string.Format(
                        "cannot add idenity column {0} to table {1}, it already has an identity column {2}",
                        column.Name, this.Name, this._pkId.Name));
                }
                this._pkId = column;
            }
            _columns.Add(column);
        }

        public void FillByName(IDataReader reader, ICollection<Entity> list)
        {
            while (reader.Read())
            {
                var entity = this.CreateEntity();

                foreach (var column in _columns)
                {
                    object val = reader[column.Name];
                    column.LoadValue(entity, val);
                }

                list.Add(entity);
            }
        }

        /// <summary>
        /// 把某一行翻译成一个实体对象
        /// </summary>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public Entity CreateFrom(DataRow row)
        {
            var entity = this.CreateEntity();

            foreach (var column in this.Columns)
            {
                object val = row[column.Name];
                column.LoadValue(entity, val);
            }

            return entity;
        }

        /// <summary>
        /// 把某一行翻译成一个实体对象
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="indexFrom"></param>
        /// <returns></returns>
        private Entity CreateByIndex(IDataReader reader, ref int indexFrom)
        {
            var entity = this.CreateEntity();

            foreach (var column in this._columns)
            {
                object val = reader.GetValue(indexFrom++);
                column.LoadValue(entity, val);
            }

            return entity;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected Entity CreateEntity()
        {
            var entity = Entity.New(this._meta.EntityType);

            entity.Status = PersistenceStatus.Unchanged;

            return entity;
        }

        internal DbColumn FindByColumnName(string name)
        {
            if (name == null || name.Length == 0)
                return null;
            string target = name.ToLower();
            foreach (DbColumn column in _columns)
            {
                if (column.Name.Equals(target))
                    return column;
            }
            return null;
        }

        internal DbColumn FindByPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            string target = propertyName.ToLower();

            foreach (DbColumn column in _columns)
            {
                if (column.PropertyName == target) return column;
                if (column.RefPropertyName == target) return column;
            }

            return null;
        }

        /// <summary>
        /// 把属性名转换为列名
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal string Translate(string propertyName)
        {
            string name = null;

            DbColumn column = FindByPropertyName(propertyName);
            if (column != null) { name = column.Name; }

            if (string.IsNullOrEmpty(name))
            {
                throw new ORMException(string.Format("表 {1} 中没有找到对应的列：{0}。", propertyName, this.Name));
            }

            return name;
        }

        /// <summary>
        /// 把属性名转换为列名
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal string Translate(IManagedProperty property)
        {
            return this.Translate(property.GetMetaPropertyName(property.OwnerType));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal IDataBridge CreateBridge(IManagedProperty managedProperty)
        {
            return new ManagedPropertyBridge(managedProperty);
        }

        #region 帮助方法

        internal string QuoteName
        {
            get { return this.Quote(this.Name); }
        }

        /// <summary>
        /// 引用某个标识符
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        internal abstract string Quote(string identifier);

        /// <summary>
        /// 每个标记符被 SQL 语句使用前都需要使用此语句进行准备。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        internal virtual string Prepare(string identifier)
        {
            return identifier;
        }

        #endregion
    }
}