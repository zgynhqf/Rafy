using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OEA.ORM;
using System.Data.SqlClient;
using System.Data.Common;

namespace OEA.ORM.sqlserver
{
    public class SqlDb : IDb
    {
        protected SqlProvider provider;
        protected IDbConnection connection;
        protected IDbTransaction transaction;
        protected bool autocommit = true;
        private bool adapter = false;
        private bool closed = false;

        public SqlDb(SqlProvider provider, IDbConnection cn)
        {
            this.provider = provider;
            this.connection = cn;
        }

        public IDbConnection Connection
        {
            get { return connection; }
            set { connection = value; }
        }

        public IDbTransaction Transaction
        {
            get { return transaction; }
        }

        public bool IsAutoCommit
        {
            get { return autocommit; }
        }

        public bool IsAdapter
        {
            get { return adapter; }
            set { adapter = value; }
        }

        public virtual void Dispose()
        {
            if (!IsClosed)
            {
                RollbackInternal();
                provider.CloseDb(this);
                IsClosed = true;
            }
        }

        public bool IsClosed
        {
            get { return closed; }
            set { closed = value; }
        }

        private void ErrorIfClosed()
        {
            if (IsClosed)
                throw new LightException("db is closed");
        }

        public virtual ITable GetTable(Type type)
        {
            return TableFor(type);
        }

        private SqlTable TableFor(Type type)
        {
            return SqlTableFactory.Instance.Build(type);
        }

        public virtual int Insert(Type type, ICollection items)
        {
            ErrorIfClosed();
            if (items.Count == 0)
                return 0;
            int result = 0;
            try
            {
                SqlTable table = TableFor(type);
                if (autocommit)
                    BeginInternal();
                result = table.Insert(this, items);
                if (autocommit)
                    CommitInternal();
            }
            catch (DbException)
            {
                if (autocommit)
                    RollbackInternal();
                throw;
            }
            return result;
        }

        public virtual int Insert<T>(ICollection<T> items)
        {
            return Insert(typeof(T), (ICollection)items);
        }

        public virtual int Insert(object item)
        {
            if (item == null)
                return 0;
            object[] items = new object[] { item };
            int result = Insert(item.GetType(), items);
            return result;
        }

        public virtual int Update(Type type, ICollection items)
        {
            return Update(type, items, null);
        }

        //columns param add by zhoujg
        public virtual int Update(Type type, ICollection items, IList<string> columns)
        {
            ErrorIfClosed();
            if (items.Count == 0)
                return 0;
            int result = 0;
            try
            {
                SqlTable table = TableFor(type);
                if (autocommit)
                    BeginInternal();
                result = table.Update(this, items, columns);
                if (autocommit)
                    CommitInternal();
            }
            catch (Exception)
            {
                if (autocommit)
                    RollbackInternal();
                throw;
            }
            return result;
        }

        public virtual int Update<T>(ICollection<T> items)
        {
            return Update(typeof(T), (ICollection)items, null);
        }

        public virtual int Update<T>(ICollection<T> items, IList<string> columns)
        {
            return Update(typeof(T), (ICollection)items, columns);
        }

        //columns param add by zhoujg
        public virtual int Update(object item, IList<string> columns)
        {
            if (item == null)
                return 0;
            object[] items = new object[] { item };
            int result = Update(item.GetType(), items, columns);
            return result;
        }

        public virtual int Update(object item)
        {
            return Update(item, null);
        }

        public virtual int Delete(Type type, ICollection items)
        {
            ErrorIfClosed();
            if (items.Count == 0)
                return 0;
            int result = 0;
            try
            {
                SqlTable table = TableFor(type);
                if (autocommit)
                    BeginInternal();
                result = table.Delete(this, items);
                if (autocommit)
                    CommitInternal();
            }
            catch (Exception)
            {
                if (autocommit)
                    RollbackInternal();
                throw;
            }
            return result;
        }

        public virtual int Delete<T>(ICollection<T> items)
        {
            return Delete(typeof(T), (ICollection)items);
        }

        public virtual int Delete(object item)
        {
            if (item == null)
                return 0;
            object[] items = new object[] { item };
            int result = Delete(item.GetType(), items);
            return result;
        }

        public virtual int Delete(Type type, IQuery query)
        {
            ErrorIfClosed();
            int result = 0;
            try
            {
                SqlTable table = TableFor(type);
                if (autocommit)
                    BeginInternal();
                result = table.Delete(this, query);
                if (autocommit)
                    CommitInternal();
            }
            catch (Exception)
            {
                if (autocommit)
                    RollbackInternal();
                throw;
            }
            return result;
        }

        public virtual int Delete<T>(IQuery query)
        {
            return Delete(typeof(T), query);
        }

        protected virtual void DoSelect(Type type, IQuery query, IList list)
        {
            ErrorIfClosed();
            SqlTable table = TableFor(type);
            table.Select(this, query, list);
        }

        public IList Select(IQuery typedQuery)
        {
            return this.Select(typedQuery.EntityType, typedQuery);
        }

        public virtual IList Select(Type type, IQuery query)
        {
            IList list = new ArrayList();
            DoSelect(type, query, list);
            return list;
        }

        public virtual IList<T> Select<T>(IQuery query)
        {
            IList<T> list = new List<T>();
            var type = typeof(T);
            DoSelect(typeof(T), query, (IList)list);
            return list;
        }

        public virtual object Find(Type type, object key)
        {
            ErrorIfClosed();
            SqlTable table = TableFor(type);
            return table.Find(this, key);
        }

        public virtual T Find<T>(object key)
        {
            object result = Find(typeof(T), key);
            if (result != null)
                return (T)result;
            return default(T);
        }

        public virtual object Call(string procName, object[] parameters)
        {
            ErrorIfClosed();
            StringBuilder buf = new StringBuilder("exec @retval=").Append(procName);
            IDbCommand cmd = null;
            IDbDataParameter retval = null;
            try
            {
                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = transaction;
                retval = cmd.CreateParameter();
                retval.ParameterName = "@retval";
                retval.DbType = DbType.Object;
                SqlUtils.SetMaxValues(retval);
                retval.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(retval);
                SetupCommand(cmd, parameters, null, buf);
                buf.Append(";");
                cmd.CommandText = buf.ToString();
                cmd.Prepare();

                TraceObject.Instance.TraceCommand(cmd);

                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                    cmd = null;
                }
            }
            object val = retval.Value;
            if (DBNull.Value.Equals(val))
                return null;
            return val;
        }

        public virtual IList Exec(Type type, string procName, object[] parameters)
        {
            ErrorIfClosed();
            SqlTable table = TableFor(type);
            IList list = new ArrayList();
            table.Exec(this, procName, parameters, list);
            return list;
        }

        public IResultSet Exec(string procName, object[] parameters)
        {
            return Exec(procName, parameters, null);
        }

        public IResultSet Exec(string procName, object[] parameters, int[] outputs)
        {
            ErrorIfClosed();
            SqlResultSet rs = null;
            IDbCommand cmd = null;
            IDataReader reader = null;
            StringBuilder buf = new StringBuilder("exec ").Append(procName);
            try
            {
                int[] sortedOutputs = null;
                if (outputs != null)
                    sortedOutputs = SortedCopy(outputs);

                cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.Transaction = transaction;
                IDbDataParameter[] outputParams = SetupCommand(cmd, parameters, sortedOutputs, buf);
                buf.Append(";");
                cmd.CommandText = buf.ToString();
                System.Diagnostics.Debug.WriteLine(connection.State);
                cmd.Prepare();

                TraceObject.Instance.TraceCommand(cmd);
                SqlTable.TraceProc(procName, parameters);

                reader = cmd.ExecuteReader();

                // setup the result set
                int sz = 0;
                string[] names = null;
                DataTable meta = reader.GetSchemaTable();
                if (meta != null)
                {
                    names = new string[meta.Rows.Count];
                    foreach (DataRow datarow in meta.Rows)
                    {
                        // NOTE: this assumes that the name of the column
                        // is in the first field of the schema table.
                        // This is the case for .NET 2.0.
                        names[sz++] = (string)datarow[0];
                    }
                    meta.Dispose();
                    meta = null;
                }
                rs = new SqlResultSet(names);

                while (reader.Read())
                {
                    object[] row = new object[sz];
                    reader.GetValues(row);
                    rs.AddRow(row);
                }
                // this is necessary to get output values
                reader.Close();

                if (sortedOutputs != null)
                {
                    for (int i = 0; i < sortedOutputs.Length; i++)
                    {
                        int index = sortedOutputs[i];
                        object val = outputParams[i].Value;
                        if (val == DBNull.Value)
                            val = null;
                        parameters[index] = val;
                    }
                }
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
            return rs;
        }

        private IDbDataParameter[] SetupCommand(IDbCommand cmd,
            object[] parameters, int[] outputs, StringBuilder buf)
        {
            int size = (outputs == null) ? 0 : outputs.Length;
            IDbDataParameter[] outputParams = new IDbDataParameter[size];
            int j = 0;
            if (parameters != null && parameters.Length > 0)
            {
                buf.Append(" ");
                for (int i = 0; i < parameters.Length; i++)
                {
                    int x = i + 1;
                    if (i > 0)
                        buf.Append(",");
                    buf.Append("@").Append(x);

                    IDbDataParameter p = cmd.CreateParameter();
                    p.ParameterName = string.Format("@{0}", x);
                    SqlUtils.PrepParam(p, parameters[i]);
                    if (outputs != null && Array.IndexOf<int>(outputs, i) > -1)
                    {
                        buf.Append(" output");
                        p.Direction = ParameterDirection.InputOutput;
                        outputParams[j++] = p;
                    }
                    cmd.Parameters.Add(p);
                }
            }
            return outputParams;
        }

        private int[] SortedCopy(int[] src)
        {
            int sz = src.Length;
            int[] dest = new int[sz];
            Array.Copy(src, 0, dest, 0, sz);
            Array.Sort<int>(dest);
            return dest;
        }

        protected virtual void BeginInternal()
        {
            if (transaction == null)
            {
                TraceObject.Instance.TraceMessage("begin transaction;");
                transaction = connection.BeginTransaction();
            }
        }

        protected virtual void CommitInternal()
        {
            if (transaction != null)
            {
                TraceObject.Instance.TraceMessage("commit transaction;");
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
            }
        }

        protected virtual void RollbackInternal()
        {
            if (transaction != null)
            {
                TraceObject.Instance.TraceMessage("rollback transaction;");
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
            }
        }

        public virtual void Begin()
        {
            BeginInternal();
            autocommit = false;
        }

        public virtual void Commit()
        {
            CommitInternal();
            autocommit = true;
        }

        public virtual void Rollback()
        {
            RollbackInternal();
            autocommit = true;
        }

        public IQuery Query()
        {
            return new SqlQuery();
        }

        public IQuery Query(Type entityType)
        {
            return new SqlQuery(entityType);
        }
    }
}
