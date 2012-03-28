using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OEA.Utils;
using OEA.Library;
using hxy.Common.Data;

namespace OEA.ORM.sqlserver
{
    public class SqlTable : ITable
    {
        protected Type type;
        protected string name;
        protected string schema;
        protected List<SqlColumn> columns;
        protected SqlColumn identity;
        protected bool isSPResult;

        private string insertSQL;
        private string updateSQL;
        private string deleteSQL;
        private string selectSQL;

        public SqlTable(Type type, string name, string schema)
        {
            this.type = type;
            if (name != null)
                this.name = name;
            if (schema != null)
                this.schema = schema.ToLower();
            columns = new List<SqlColumn>();
            isSPResult = (name == null || name.Length == 0);
        }

        public Type Class
        {
            get { return type; }
        }

        public string Name
        {
            get { return name; }
        }

        public string Schema
        {
            get { return schema; }
        }

        public IColumn[] Columns
        {
            get { return columns.ToArray(); }
        }

        public virtual void Add(SqlColumn column)
        {
            if (column.IsID)
            {
                if (identity != null)
                {
                    throw new LightException(string.Format(
                        "cannot add idenity column {0} to table {1}, it already has an identity column {2}",
                        column.Name, this.Name, identity.Name));
                }
                identity = column;
            }
            columns.Add(column);
            column.Ordinal = columns.Count;
        }

        public SqlColumn FindByColumnName(string name)
        {
            if (name == null || name.Length == 0)
                return null;
            string target = name.ToLower();
            foreach (SqlColumn column in columns)
            {
                if (column.Name.Equals(target))
                    return column;
            }
            return null;
        }

        public SqlColumn FindByPropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;

            string target = propertyName.ToLower();

            foreach (SqlColumn column in columns)
            {
                if (column.PropertyName == target) return column;
                if (column.RefPropertyName == target) return column;
            }

            return null;
        }

        public string Translate(string propertyName)
        {
            SqlColumn column = FindByPropertyName(propertyName);
            if (column == null)
                return null;
            return column.Name;
        }

        private void ErrorIfSPResult()
        {
            if (isSPResult)
            {
                string format = "type {0} is marked with SPResultAttribute, not TableAttribute";
                throw new LightException(string.Format(format, type.FullName));
            }
        }

        public virtual int Insert(IDb db, ICollection items)
        {
            ErrorIfSPResult();
            int result = 0;
            string sql = GetInsertSql();
            Trace(sql);
            IDbCommand cmd = null;
            IDbDataParameter idP = null;
            try
            {
                cmd = db.Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 600000; //add by zhoujg
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = db.Transaction;
                foreach (SqlColumn column in columns)
                {	// add parameters
                    if (column.IsReadable && !column.IsID)
                    {
                        IDbDataParameter p = cmd.CreateParameter();
                        p.ParameterName = string.Format("@{0}", column.Ordinal);
                        SqlUtils.PrepParam(p, column.DataType);
                        cmd.Parameters.Add(p);
                    }
                }
                if (identity != null)
                {	// add output parameter for identity column
                    idP = cmd.CreateParameter();
                    idP.ParameterName = "@id";
                    SqlUtils.PrepParam(idP, identity.DataType);
                    idP.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(idP);
                }
                cmd.Prepare();
                foreach (object item in items)
                {
                    if (item == null)
                        continue;
                    foreach (SqlColumn column in columns)
                    {	// assign values to all parameters
                        if (column.IsReadable && !column.IsID)
                        {
                            string pname = string.Format("@{0}", column.Ordinal);
                            IDbDataParameter p = (IDbDataParameter)cmd.Parameters[pname];
                            column.SetParameterValue(p, item);
                        }
                    }

                    TraceObject.Instance.TraceCommand(cmd);

                    result += cmd.ExecuteNonQuery();
                    if (identity != null)
                    {	// write generated identity value back into the object
                        identity.SetValue(item, idP.Value);
                    }
                }
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
            return result;
        }

        //columns param add by zhoujg
        public virtual int Update(IDb db, ICollection items, IList<string> updateColumns)
        {
            ErrorIfSPResult();
            int result = 0;
            string sql = GetUpdateSql(updateColumns);
            Trace(sql);
            IDbCommand cmd = null;
            try
            {
                cmd = db.Connection.CreateCommand();
                cmd.CommandTimeout = 600000;  //add by zhoujg
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = db.Transaction;
                foreach (SqlColumn column in columns)
                {	// add parameters
                    if (column.IsReadable && !(column.IsID && !column.IsPK) && ((null == updateColumns) || (updateColumns.Contains(column.Name.ToLower()))))
                    {
                        string prefix = column.IsPK ? "@pk" : "@";
                        IDbDataParameter p = cmd.CreateParameter();
                        p.ParameterName = string.Format("{0}{1}", prefix, column.Ordinal);
                        SqlUtils.PrepParam(p, column.DataType);
                        cmd.Parameters.Add(p);
                    }
                }
                cmd.Prepare();
                foreach (object item in items)
                {
                    if (item != null)
                    {
                        foreach (SqlColumn column in columns)
                        {	// assign values to parameters
                            if (column.IsReadable && !(column.IsID && !column.IsPK) && ((null == updateColumns) || (updateColumns.Contains(column.Name.ToLower()))))
                            {
                                string prefix = column.IsPK ? "@pk" : "@";
                                string pname = string.Format("{0}{1}", prefix, column.Ordinal);
                                IDbDataParameter p = (IDbDataParameter)cmd.Parameters[pname];
                                column.SetParameterValue(p, item);
                            }
                        }

                        TraceObject.Instance.TraceCommand(cmd);

                        result += cmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
            return result;
        }

        public virtual int Delete(IDb db, ICollection items)
        {
            ErrorIfSPResult();
            int result = 0;
            string sql = GetDeleteSql();
            Trace(sql);
            IDbCommand cmd = null;
            try
            {
                cmd = db.Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = db.Transaction;
                foreach (SqlColumn column in columns)
                {
                    if (column.IsPK)
                    {
                        IDbDataParameter p = cmd.CreateParameter();
                        p.ParameterName = string.Format("@pk{0}", column.Ordinal);
                        SqlUtils.PrepParam(p, column.DataType);
                        cmd.Parameters.Add(p);
                    }
                }
                cmd.Prepare();
                foreach (object item in items)
                {
                    if (item != null)
                    {
                        foreach (SqlColumn column in columns)
                        {
                            if (column.IsPK)
                            {
                                string pname = string.Format("@pk{0}", column.Ordinal);
                                IDbDataParameter p = (IDbDataParameter)cmd.Parameters[pname];
                                column.SetParameterValue(p, item);
                            }
                        }

                        TraceObject.Instance.TraceCommand(cmd);

                        result += cmd.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
            return result;
        }

        public virtual int Delete(IDb db, IQuery query)
        {
            ErrorIfSPResult();
            int result = 0;
            SqlQuery sqlquery = (SqlQuery)query; //as SqlQuery;
            string sql = GetDeleteSql(sqlquery);
            Trace(sql);
            IDbCommand cmd = null;
            try
            {
                cmd = db.Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 600000;  //add by zhoujg
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = db.Transaction;
                if (sqlquery != null)
                    sqlquery.SetParameters(cmd);
                cmd.Prepare();

                TraceObject.Instance.TraceCommand(cmd);

                result = cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
            return result;
        }

        public virtual void Select(IDb db, IQuery query, IList list)
        {
            ErrorIfSPResult();
            SqlQuery sqlquery = (SqlQuery)query;
            string sql = GetSelectSql(sqlquery);
            Trace(sql);
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = db.Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = db.Transaction;
                if (sqlquery != null)
                    sqlquery.SetParameters(cmd);
                cmd.Prepare();

                TraceObject.Instance.TraceCommand(cmd);

                reader = cmd.ExecuteReader();
                FillByIndex(reader, list);
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                        reader.Close();
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
        }

        public virtual object Find(IDb session, object key)
        {
            ErrorIfSPResult();
            SqlColumn pk = null;
            object result = null;
            foreach (SqlColumn c in columns)
            {
                if (c.IsPK)
                {
                    pk = c;
                    break;
                }
            }
            if (pk != null)
            {
                SqlQuery q = new SqlQuery();
                q.Constrain(pk.PropertyName).Equal(key);
                IList list = new ArrayList(1);
                Select(session, q, list);
                if (list.Count > 0)
                    result = list[0];
            }
            return result;
        }

        public virtual void Exec(IDb db, string procName, object[] parameters, IList list)
        {
            TraceProc(procName, parameters);

            int len = (parameters == null) ? 0 : parameters.Length;
            string sql = GetExecSql(procName, len);
            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = db.Connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = db.Transaction;
                for (int i = 0; i < len; i++)
                {
                    int j = i + 1;
                    IDbDataParameter p = cmd.CreateParameter();
                    p.ParameterName = string.Format("@{0}", j);
                    SqlUtils.PrepParam(p, parameters[i]);
                    cmd.Parameters.Add(p);
                }

                TraceObject.Instance.TraceCommand(cmd);

                reader = cmd.ExecuteReader();
                FillByName(reader, list);
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                        reader.Close();
                    reader.Dispose();
                    reader = null;
                }
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
        }

        protected virtual string GetInsertSql()
        {
            if (insertSQL == null)
            {
                StringBuilder sql = new StringBuilder("insert into ");
                StringBuilder values = new StringBuilder();
                if (schema != null && schema.Length > 0)
                    sql.Append("[").Append(schema).Append("].");
                sql.Append("[").Append(name).Append("] (");
                bool comma = false;
                foreach (SqlColumn column in columns)
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
                if (identity != null)
                    sql.Append("set @id=scope_identity();");
                insertSQL = sql.ToString();
            }
            return insertSQL;
        }

        protected virtual string GetUpdateSql(IList<string> updateColumns)
        {
            if ((updateSQL == null) || (null != updateColumns))
            {
                StringBuilder sql = new StringBuilder("update ");
                if (schema != null && schema.Length > 0)
                    sql.Append("[").Append(schema).Append("].");
                sql.Append("[").Append(name).Append("] set ");
                bool comma = false;
                foreach (SqlColumn column in columns)
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
                foreach (SqlColumn column in columns)
                {
                    if (column.IsReadable && column.IsPK)
                    {
                        if (comma)
                            sql.Append(" and ");
                        sql.Append("[").Append(column.Name).Append("]=@pk").Append(column.Ordinal);
                    }
                }
                sql.Append(";");
                updateSQL = sql.ToString();
            }
            return updateSQL;
        }

        protected virtual string GetDeleteSql()
        {
            if (deleteSQL == null)
            {
                StringBuilder sql = new StringBuilder("delete from ");
                if (schema != null && schema.Length > 0)
                    sql.Append("[").Append(schema).Append("].");
                sql.Append("[").Append(name).Append("] where 1=1");
                foreach (SqlColumn column in columns)
                {
                    if (column.IsPK)
                        sql.Append(" and [").Append(column.Name).Append("]=@pk").Append(column.Ordinal);
                }
                sql.Append(";");
                deleteSQL = sql.ToString();
            }
            return deleteSQL;
        }

        protected virtual string GetDeleteSql(SqlQuery query)
        {
            StringBuilder buf = new StringBuilder("delete from ");
            if (schema != null && schema.Length > 0)
                buf.Append("[").Append(schema).Append("].");
            buf.Append("[").Append(name).Append("]");
            if (query != null)
                buf.Append(" ").Append(query.GetSql(this));
            buf.Append(";");
            return buf.ToString();
        }

        protected virtual string GetSelectSql(SqlQuery query)
        {
            if (selectSQL == null)
            {
                StringBuilder buf = new StringBuilder("select ");
                bool comma = false;
                foreach (SqlColumn column in columns)
                {
                    if (comma)
                        buf.Append(",");
                    else
                        comma = true;
                    buf.Append("[").Append(column.Name).Append("]");
                }
                buf.Append(" from ");
                if (schema != null && schema.Length > 0)
                    buf.Append("[").Append(schema).Append("].");
                buf.Append("[").Append(name).Append("]");
                selectSQL = buf.ToString();
            }
            if (query != null)
            {
                //edit by zhoujg
                string sQL = "";
                if (null == query.Columns)
                    sQL = selectSQL;
                else
                {
                    StringBuilder sQLBuf = new StringBuilder("select ");
                    bool comma = false;
                    foreach (SqlColumn column in columns)
                    {
                        if (comma)
                            sQLBuf.Append(",");
                        else
                            comma = true;
                        //add by zhoujg: 如果不包含当前字段,则取空
                        if (query.Columns.Contains(column.Name.ToLower()))
                            sQLBuf.Append("[").Append(column.Name).Append("]");
                        else
                            sQLBuf.Append(" null as [").Append(column.Name).Append("]");
                    }
                    sQLBuf.Append(" from ");
                    if (schema != null && schema.Length > 0)
                        sQLBuf.Append("[").Append(schema).Append("].");
                    sQLBuf.Append("[").Append(name).Append("]");
                    sQL = sQLBuf.ToString();
                }

                StringBuilder buf = new StringBuilder(sQL);
                buf.Append(" ").Append(query.GetSql(this));
                buf.Append(";");
                return buf.ToString();
            }
            else
                return string.Format("{0};", selectSQL);
        }

        protected virtual string GetExecSql(string proc, int nParams)
        {
            StringBuilder buf = new StringBuilder("exec ");
            buf.Append(proc);
            if (nParams > 0)
                buf.Append(" ");
            for (int i = 0; i < nParams; i++)
            {
                int j = i + 1;
                if (i > 0)
                    buf.Append(",");
                buf.Append("@").Append(j);
            }
            buf.Append(";");
            return buf.ToString();
        }

        private void FillByIndex(IDataReader reader, IList list)
        {
            while (reader.Read())
            {
                var entity = CreateEntity();
                int i = 0;
                foreach (SqlColumn column in columns)
                {
                    object val = reader[i++];
                    column.SetValue(entity, val);
                }
                list.Add(entity);
            }
        }

        private void FillByName(IDataReader reader, IList list)
        {
            while (reader.Read())
            {
                var entity = CreateEntity();
                foreach (SqlColumn column in columns)
                {
                    object val = reader[column.Name];
                    column.SetValue(entity, val);
                }
                list.Add(entity);
            }
        }

        private Entity CreateEntity()
        {
            return ITableExtension.CreateEntity(type);
        }

        #region OEA

        /// <summary>
        /// 直接把data中的数据读取到entity中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="data"></param>
        public void SetValues(object entity, IResultSet data)
        {
            foreach (SqlColumn column in columns)
            {
                object val = data[column.Name];
                column.SetValue(entity, val);
            }
        }

        #region 用于测试SQL

        [System.Diagnostics.Conditional("DEBUG")]
        public static void TraceProc(string proc, object[] parameters)
        {
            if (proc == "sys.sp_sqlexec")
            {
                Trace(parameters[0] as string);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Trace(string sql)
        {
            SQLTrace.Trace(sql);
        }

        #endregion

        #endregion
    }
}
