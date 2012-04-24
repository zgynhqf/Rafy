using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OEA.ORM;
using System.Data.SqlClient;
using System.Data.Common;
using hxy.Common.Data;
using OEA.Library;
using OEA.MetaModel;

namespace OEA.ORM.sqlserver
{
    public class SqlDb : IDb
    {
        public IDBAccesser DBA { get; private set; }

        public SqlDb(IDBAccesser dba)
        {
            this.DBA = dba;
        }

        #region Dispose Pattern

        ~SqlDb()
        {
            this.Dispose(false);
        }

        public virtual void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.DBA.Dispose();
            }
        }

        #endregion

        public ITable GetTable(Type type)
        {
            return TableFor(type);
        }

        private SqlTable TableFor(IEntity entity)
        {
            return entity.GetRepository().EntityMeta.GetORMTable();
        }

        private SqlTable TableFor(Type type)
        {
            var em = CommonModel.Entities.Get(type);
            return em.GetORMTable();
        }

        public int Insert(IEntity item)
        {
            SqlTable table = TableFor(item);
            return table.Insert(this, item);
        }

        public int Update(IEntity item)
        {
            SqlTable table = TableFor(item);
            return table.Update(this, item);
        }

        public int Delete(IEntity item)
        {
            SqlTable table = TableFor(item);
            return table.Delete(this, item);
        }

        public int Delete(Type type, IQuery query)
        {
            SqlTable table = TableFor(type);
            return table.Delete(this, query);
        }

        public IList Select(IQuery typedQuery)
        {
            IList list = new ArrayList();
            DoSelect(typedQuery.EntityType, typedQuery, list);
            return list;
        }

        protected virtual void DoSelect(Type type, IQuery query, IList list)
        {
            SqlTable table = TableFor(type);
            table.Select(this, query, list);
        }

        public IList Select(Type type, string sql)
        {
            SqlTable table = TableFor(type);
            IList list = new ArrayList();
            using (var reader = this.DBA.QueryDataReaderNormal(sql, CommandType.Text))
            {
                table.FillByIndex(reader, list);
            }
            return list;
        }

        //public IResultSet Exec(string procName, object[] parameters, int[] outputs)
        //{
        //    parameters = parameters ?? new object[0];

        //    SqlResultSet rs = null;

        //    int[] sortedOutputs = null;
        //    if (outputs != null)
        //        sortedOutputs = SortedCopy(outputs);

        //    int size = (outputs == null) ? 0 : outputs.Length;
        //    IDbDataParameter[] outputParams = new IDbDataParameter[size];
        //    IDbDataParameter[] dbParameters = new IDbDataParameter[parameters.Length];
        //    int j = 0;
        //    if (parameters != null)
        //    {
        //        for (int i = 0; i < parameters.Length; i++)
        //        {
        //            int x = i + 1;
        //            if (i > 0)
        //                buf.Append(",");
        //            buf.Append("@").Append(x);

        //            IDbDataParameter p = cmd.CreateParameter();
        //            p.ParameterName = string.Format("@{0}", x);
        //            SqlUtils.PrepParam(p, parameters[i]);
        //            if (outputs != null && Array.IndexOf<int>(outputs, i) > -1)
        //            {
        //                buf.Append(" output");
        //                p.Direction = ParameterDirection.InputOutput;
        //                outputParams[j++] = p;
        //            }
        //            dbParameters[i] = p;
        //        }
        //    }

        //    var reader = this.DBA.QueryDataReaderNormal(
        //        procName, CommandType.StoredProcedure,
        //        dbParameters
        //        );

        //    // setup the result set
        //    int sz = 0;
        //    string[] names = null;
        //    DataTable meta = reader.GetSchemaTable();
        //    if (meta != null)
        //    {
        //        names = new string[meta.Rows.Count];
        //        foreach (DataRow datarow in meta.Rows)
        //        {
        //            // NOTE: this assumes that the name of the column
        //            // is in the first field of the schema table.
        //            // This is the case for .NET 2.0.
        //            names[sz++] = (string)datarow[0];
        //        }
        //        meta.Dispose();
        //        meta = null;
        //    }
        //    rs = new SqlResultSet(names);

        //    while (reader.Read())
        //    {
        //        object[] row = new object[sz];
        //        reader.GetValues(row);
        //        rs.AddRow(row);
        //    }
        //    // this is necessary to get output values
        //    reader.Close();

        //    if (sortedOutputs != null)
        //    {
        //        for (int i = 0; i < sortedOutputs.Length; i++)
        //        {
        //            int index = sortedOutputs[i];
        //            object val = outputParams[i].Value;
        //            if (val == DBNull.Value)
        //                val = null;
        //            parameters[index] = val;
        //        }
        //    }

        //    return rs;
        //}

        //private static int[] SortedCopy(int[] src)
        //{
        //    int sz = src.Length;
        //    int[] dest = new int[sz];
        //    Array.Copy(src, 0, dest, 0, sz);
        //    Array.Sort<int>(dest);
        //    return dest;
        //}

        public IQuery Query(Type entityType)
        {
            return new SqlQuery(entityType);
        }
    }
}
