using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;
using System.Data.SqlClient;
using OEA.ORM.sqlserver;
using System.Data;
using System.Collections;
using OEA.Utils;
using hxy.Common.Data;

namespace OEA.Library
{
    public static class DbExtension
    {
        public static IDbTable QueryTable(this IDb db, string sql)
        {
            var table = db.QueryDataTable(sql);

            return new DbTable(table);
        }

        public static DataTable QueryDataTable(this IDb db, string sql)
        {
            using (var adapter = new SqlDataAdapter(sql, db.Connection as SqlConnection))
            {
                var table = new DataTable();
                adapter.Fill(table);

                SQLTrace.Trace(sql);

                return table;
            }
        }

        public static IResultSet ExecSql(this IDb db, string sql)
        {
            return db.Exec("sys.sp_sqlexec", new object[] { sql });
        }

        public static IEnumerable ExecSql(this IDb db, Type type, string sql)
        {
            return db.Exec(type, "sys.sp_sqlexec", new object[] { sql });
        }

        public static List<TEntity> ExecSql<TEntity>(this IDb db, string sql)
        {
            return ExecSql(db, typeof(TEntity), sql).Cast<TEntity>().ToList();
        }
    }
}