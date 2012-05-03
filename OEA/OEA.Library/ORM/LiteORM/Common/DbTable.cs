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

namespace OEA.ORM
{
    public abstract class DbTable : ITable
    {
        protected EntityMeta meta;

        protected string _name;

        protected List<DbColumn> _columns;

        protected DbColumn _identity;

        private string _insertSQL;

        protected string _updateSQL;

        protected string _deleteSQL;

        protected string _selectSQL;

        public DbTable(EntityMeta meta, string name)
        {
            this.meta = meta;
            this._name = name;
            this._columns = new List<DbColumn>();
        }

        #region 元数据属性

        public Type Class
        {
            get { return this.meta.EntityType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEnumerable<IColumn> Columns
        {
            get { return this._columns; }
        }

        #endregion

        #region 数据操作 CRUD

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

            string sql = this.GenerateDeleteSQL(dbQuery);

            if (dbQuery != null)
            {
                var parameters = new List<object>();
                dbQuery.ReadParameters(parameters);
                return db.DBA.ExecuteText(sql, parameters.ToArray());
            }

            return db.DBA.ExecuteTextNormal(sql);
        }

        protected virtual string GenerateDeleteSQL(DbQuery query)
        {
            var sql = new StringBuilder("DELETE FROM ")
                .Append(this.Quote(this._name));
            if (query != null)
            {
                sql.Append(' ').Append(query.GetSql(this));
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

        public virtual void Select(IDb db, IQuery query, IList list)
        {
            var dba = db.DBA;

            var dbQuery = query as DbQuery;
            string sql = GenerateSelectSQL(dbQuery);

            var parameters = new List<object>();
            if (dbQuery != null) { dbQuery.ReadParameters(parameters); }

            using (var reader = dba.QueryDataReader(sql, parameters.ToArray()))
            {
                FillByIndex(reader, list);
            }
        }

        protected virtual string GenerateSelectSQL(DbQuery dbQuery)
        {
            if (this._selectSQL == null)
            {
                var sql = new StringBuilder("SELECT ");
                bool comma = false;
                foreach (DbColumn column in this._columns)
                {
                    if (comma) { sql.Append(','); }
                    else { comma = true; }

                    sql.Append(this.Quote(column.Name));
                }
                sql.Append(" FROM ");
                sql.Append(this.Quote(this._name));
                this._selectSQL = sql.ToString();
            }

            if (dbQuery == null || dbQuery.IsEmpty) { return this._selectSQL; }

            var buf = new StringBuilder(this._selectSQL)
                .Append(' ').Append(dbQuery.GetSql(this));
            return buf.ToString();
        }

        #endregion

        public virtual void Add(DbColumn column)
        {
            if (column.IsPKID)
            {
                if (_identity != null)
                {
                    throw new LightException(string.Format(
                        "cannot add idenity column {0} to table {1}, it already has an identity column {2}",
                        column.Name, this.Name, _identity.Name));
                }
                _identity = column;
            }
            _columns.Add(column);
        }

        public void FillByIndex(IDataReader reader, IList list)
        {
            while (reader.Read())
            {
                var entity = CreateEntity();
                int i = 0;
                foreach (DbColumn column in _columns)
                {
                    object val = reader[i++];
                    column.SetValue(entity, val);
                }
                list.Add(entity);
            }
        }

        public void FillByName(IDataReader reader, IList list)
        {
            while (reader.Read())
            {
                var entity = CreateEntity();
                foreach (DbColumn column in _columns)
                {
                    object val = reader[column.Name];
                    column.SetValue(entity, val);
                }
                list.Add(entity);
            }
        }

        protected Entity CreateEntity()
        {
            var entity = Entity.New(this.meta.EntityType);

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
                throw new LightException(string.Format("column {0} not found in table {1}", propertyName, this.Name));
            }

            return name;
        }

        internal virtual IDataBridge CreateBridge(IManagedProperty managedProperty)
        {
            return new ManagedPropertyBridge(managedProperty);
        }

        /// <summary>
        /// 把某一行翻译成一个实体对象
        /// </summary>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public Entity Translate(DataRow row)
        {
            var entity = this.CreateEntity();

            foreach (var column in this.Columns)
            {
                object val = row[column.Name];
                column.SetValue(entity, val);
            }

            return entity;
        }

        /// <summary>
        /// 直接把data中的数据读取到entity中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        public void SetValues(object entity, IResultSet data)
        {
            foreach (var column in this._columns)
            {
                object val = data[column.Name];
                column.SetValue(entity, val);
            }
        }

        #region 帮助方法

        internal abstract string Quote(string identifier);

        internal virtual string Prepare(string identifier)
        {
            return identifier;
        }

        #endregion
    }
}
