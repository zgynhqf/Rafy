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
using Rafy.Data;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.Utils;
using Rafy.ManagedProperty;
using Rafy;
using System.IO;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.SqlServer
{
    internal abstract class SqlTable : SqlOraTable
    {
        public SqlTable(IRepositoryInternal repository) : base(repository) { }

        internal override void AppendQuote(TextWriter sql, string identifier)
        {
            sql.Write("[");
            this.AppendPrepare(sql, identifier);
            sql.Write("]");
        }

        protected override void CreatePagingSql(ref SqlOraTable.PagingSqlParts parts, PagingInfo pagingInfo)
        {
            //Sql Server 中，如果是第一页，直接使用 TOP 语法。
            if (pagingInfo.PageNumber == 1)
            {
                var select = "SELECT";
                var index = parts.RawSql.IndexOf(select, StringComparison.OrdinalIgnoreCase);
                if (index < 0) throw new InvalidProgramException("SQL 中没有 Select 语句。");

                var insertIndex = index + select.Length;
                parts.PagingSql = parts.RawSql.Insert(insertIndex, " TOP " + pagingInfo.PageSize);
            }
            else
            {
                base.CreatePagingSql(ref parts, pagingInfo);
            }
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

        public override SqlGenerator CreateSqlGenerator()
        {
            return new SqlServerSqlGenerator();
        }
    }
}
