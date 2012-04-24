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

namespace OEA.ORM.sqlserver
{
    public class SqlTable : ITable
    {
        protected EntityMeta meta;
        protected string _name;
        protected List<SqlColumn> _columns;
        protected SqlColumn _identity;

        private string _insertSQL;
        private string _updateSQL;
        private string _deleteSQL;
        private string _selectSQL;

        public SqlTable(EntityMeta meta, string name)
        {
            this.meta = meta;
            this._name = name;
            this._columns = new List<SqlColumn>();
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

        public IColumn[] Columns
        {
            get { return _columns.ToArray(); }
        }

        #endregion

        public virtual void Add(SqlColumn column)
        {
            if (column.IsID)
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
            column.Ordinal = _columns.Count;
        }

        #region 数据操作 CRUD

        public virtual int Insert(IDb db, object item)
        {
            var dba = db.DBA;

            var parameters = new List<IDbDataParameter>();
            foreach (SqlColumn column in _columns)
            {
                if (column.IsReadable && !column.IsID)
                {
                    var p = dba.ParameterFactory.CreateParameter();
                    p.ParameterName = string.Format("@{0}", column.Ordinal);

                    SqlUtils.PrepParam(p, column.DataType);
                    parameters.Add(p);

                    //设置值
                    column.SetParameterValue(p, item);
                }
            }

            string sql = GetInsertSql();
            var result = dba.ExecuteTextNormal(sql, parameters.ToArray());

            // write generated identity value back into the object
            if (this._identity != null)
            {
                var value = dba.QueryValueNormal("select @@identity;", CommandType.Text);
                this._identity.SetValue(item, value);
            }

            return result;
        }

        public virtual int Delete(IDb db, object item)
        {
            var dba = db.DBA;

            var parameters = new List<IDbDataParameter>();

            foreach (SqlColumn column in _columns)
            {
                if (column.IsPK)
                {
                    IDbDataParameter p = dba.ParameterFactory.CreateParameter();
                    p.ParameterName = string.Format("@pk{0}", column.Ordinal);
                    SqlUtils.PrepParam(p, column.DataType);
                    parameters.Add(p);

                    column.SetParameterValue(p, item);
                }
            }

            string sql = GetDeleteSql();
            return dba.ExecuteTextNormal(sql, parameters.ToArray());
        }

        public virtual int Delete(IDb db, IQuery query)
        {
            var dba = db.DBA;

            var parameters = new List<IDbDataParameter>();

            SqlQuery sqlquery = (SqlQuery)query; //as SqlQuery;
            string sql = GetDeleteSql(sqlquery);

            if (sqlquery != null) sqlquery.SetParameters(dba.ParameterFactory, parameters);

            return dba.ExecuteTextNormal(sql, parameters.ToArray());
        }

        public virtual int Update(IDb db, object item)
        {
            var dba = db.DBA;

            var parameters = new List<IDbDataParameter>();
            foreach (SqlColumn column in _columns)
            {
                if (column.IsReadable && !(column.IsID && !column.IsPK))
                {
                    IDbDataParameter p = dba.ParameterFactory.CreateParameter();
                    p.ParameterName = string.Format("{0}{1}", column.IsPK ? "@pk" : "@", column.Ordinal);
                    SqlUtils.PrepParam(p, column.DataType);
                    parameters.Add(p);

                    column.SetParameterValue(p, item);
                }
            }

            string sql = GetUpdateSql(null);
            return dba.ExecuteTextNormal(sql, parameters.ToArray());
        }

        public virtual void Select(IDb db, IQuery query, IList list)
        {
            var dba = db.DBA;

            var sqlquery = query as SqlQuery;
            string sql = GetSelectSql(sqlquery);
            var parameters = new List<IDbDataParameter>();
            if (sqlquery != null)
            {
                sqlquery.SetParameters(dba.ParameterFactory, parameters);
            }

            using (var reader = dba.QueryDataReaderNormal(sql, CommandType.Text, parameters.ToArray()))
            {
                FillByIndex(reader, list);
            }
        }

        #endregion

        #region SQL 生成

        protected virtual string GetInsertSql()
        {
            if (_insertSQL == null)
            {
                StringBuilder sql = new StringBuilder("insert into ");
                StringBuilder values = new StringBuilder();
                sql.Append("[").Append(_name).Append("] (");
                bool comma = false;
                foreach (SqlColumn column in _columns)
                {
                    if (column.IsReadable && !column.IsID)
                    {
                        if (comma)
                        {
                            sql.Append(",");
                            values.Append(",");
                        }
                        else
                            comma = true;
                        sql.Append("[").Append(column.Name).Append("]");
                        values.Append("@").Append(column.Ordinal);
                    }
                }
                sql.Append(") values (").Append(values.ToString()).Append(");");
                _insertSQL = sql.ToString();
            }
            return _insertSQL;
        }

        protected virtual string GetUpdateSql(IList<string> updateColumns)
        {
            if ((_updateSQL == null) || (null != updateColumns))
            {
                StringBuilder sql = new StringBuilder("update ");
                sql.Append("[").Append(_name).Append("] set ");
                bool comma = false;
                foreach (SqlColumn column in _columns)
                {
                    if ((column.IsReadable && !column.IsPK && !column.IsID && ((null == updateColumns) || (updateColumns.Contains(column.Name.ToLower())))))
                    {
                        if (comma)
                            sql.Append(",");
                        else
                            comma = true;
                        sql.Append("[").Append(column.Name).Append("]=@").Append(column.Ordinal);
                    }
                }
                sql.Append(" where ");
                comma = false;
                foreach (SqlColumn column in _columns)
                {
                    if (column.IsReadable && column.IsPK)
                    {
                        if (comma)
                            sql.Append(" and ");
                        sql.Append("[").Append(column.Name).Append("]=@pk").Append(column.Ordinal);
                    }
                }
                sql.Append(";");
                _updateSQL = sql.ToString();
            }
            return _updateSQL;
        }

        protected virtual string GetDeleteSql()
        {
            if (_deleteSQL == null)
            {
                StringBuilder sql = new StringBuilder("delete from ");
                sql.Append("[").Append(_name).Append("] where 1=1");
                foreach (SqlColumn column in _columns)
                {
                    if (column.IsPK)
                        sql.Append(" and [").Append(column.Name).Append("]=@pk").Append(column.Ordinal);
                }
                sql.Append(";");
                _deleteSQL = sql.ToString();
            }
            return _deleteSQL;
        }

        protected virtual string GetDeleteSql(SqlQuery query)
        {
            StringBuilder buf = new StringBuilder("delete from ");
            buf.Append("[").Append(_name).Append("]");
            if (query != null)
                buf.Append(" ").Append(query.GetSql(this));
            buf.Append(";");
            return buf.ToString();
        }

        protected virtual string GetSelectSql(SqlQuery query)
        {
            if (_selectSQL == null)
            {
                StringBuilder buf = new StringBuilder("select ");
                bool comma = false;
                foreach (SqlColumn column in _columns)
                {
                    if (comma)
                        buf.Append(",");
                    else
                        comma = true;
                    buf.Append("[").Append(column.Name).Append("]");
                }
                buf.Append(" from ");
                buf.Append("[").Append(_name).Append("]");
                _selectSQL = buf.ToString();
            }
            if (query != null)
            {
                //edit by zhoujg
                string sQL = "";
                if (null == query.Columns)
                    sQL = _selectSQL;
                else
                {
                    StringBuilder sQLBuf = new StringBuilder("select ");
                    bool comma = false;
                    foreach (SqlColumn column in _columns)
                    {
                        if (comma)
                            sQLBuf.Append(",");
                        else
                            comma = true;

                        if (query.Columns.Contains(column.Name.ToLower()))
                            sQLBuf.Append("[").Append(column.Name).Append("]");
                        else
                            sQLBuf.Append(" null as [").Append(column.Name).Append("]");
                    }
                    sQLBuf.Append(" from ");
                    sQLBuf.Append("[").Append(_name).Append("]");
                    sQL = sQLBuf.ToString();
                }

                StringBuilder buf = new StringBuilder(sQL);
                buf.Append(" ").Append(query.GetSql(this));
                buf.Append(";");
                return buf.ToString();
            }
            else
                return string.Format("{0};", _selectSQL);
        }

        #endregion

        public void FillByIndex(IDataReader reader, IList list)
        {
            while (reader.Read())
            {
                var entity = CreateEntity();
                int i = 0;
                foreach (SqlColumn column in _columns)
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
                foreach (SqlColumn column in _columns)
                {
                    object val = reader[column.Name];
                    column.SetValue(entity, val);
                }
                list.Add(entity);
            }
        }

        private Entity CreateEntity()
        {
            var entity = Entity.New(this.meta.EntityType);

            entity.Status = PersistenceStatus.Unchanged;

            return entity;
        }

        internal SqlColumn FindByColumnName(string name)
        {
            if (name == null || name.Length == 0)
                return null;
            string target = name.ToLower();
            foreach (SqlColumn column in _columns)
            {
                if (column.Name.Equals(target))
                    return column;
            }
            return null;
        }

        internal SqlColumn FindByPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            string target = propertyName.ToLower();

            foreach (SqlColumn column in _columns)
            {
                if (column.PropertyName == target) return column;
                if (column.RefPropertyName == target) return column;
            }

            return null;
        }

        internal string Translate(string propertyName)
        {
            SqlColumn column = FindByPropertyName(propertyName);
            if (column == null)
                return null;
            return column.Name;
        }

        //存储过程的支持，暂时不需要。
        //public virtual object Find(IDb session, object key)
        //{
        //    
        //    SqlColumn pk = null;
        //    object result = null;
        //    foreach (SqlColumn c in columns)
        //    {
        //        if (c.IsPK)
        //        {
        //            pk = c;
        //            break;
        //        }
        //    }
        //    if (pk != null)
        //    {
        //        SqlQuery q = new SqlQuery();
        //        q.Constrain(pk.PropertyName).Equal(key);
        //        IList list = new ArrayList(1);
        //        Select(session, q, list);
        //        if (list.Count > 0)
        //            result = list[0];
        //    }
        //    return result;
        //}

        //public virtual void Exec(IDb db, string procName, object[] parameters, IList list)
        //{
        //    TraceProc(procName, parameters);

        //    int len = (parameters == null) ? 0 : parameters.Length;
        //    string sql = GetExecSql(procName, len);
        //    IDbCommand cmd = null;
        //    IDataReader reader = null;
        //    try
        //    {
        //        cmd = db.Connection.CreateCommand();
        //        cmd.CommandText = sql;
        //        cmd.CommandType = CommandType.Text;
        //        //cmd.Transaction = db.Transaction;
        //        for (int i = 0; i < len; i++)
        //        {
        //            int j = i + 1;
        //            IDbDataParameter p = cmd.CreateParameter();
        //            p.ParameterName = string.Format("@{0}", j);
        //            SqlUtils.PrepParam(p, parameters[i]);
        //            cmd.Parameters.Add(p);
        //        }

        //        reader = cmd.ExecuteReader();
        //        FillByName(reader, list);
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //        {
        //            if (!reader.IsClosed)
        //                reader.Close();
        //            reader.Dispose();
        //            reader = null;
        //        }
        //        if (cmd != null)
        //        {
        //            cmd.Dispose();
        //            cmd = null;
        //        }
        //    }
        //}

        //protected virtual string GetExecSql(string proc, int nParams)
        //{
        //    StringBuilder buf = new StringBuilder("exec ");
        //    buf.Append(proc);
        //    if (nParams > 0)
        //        buf.Append(" ");
        //    for (int i = 0; i < nParams; i++)
        //    {
        //        int j = i + 1;
        //        if (i > 0)
        //            buf.Append(",");
        //        buf.Append("@").Append(j);
        //    }
        //    buf.Append(";");
        //    return buf.ToString();
        //}
    }
}
