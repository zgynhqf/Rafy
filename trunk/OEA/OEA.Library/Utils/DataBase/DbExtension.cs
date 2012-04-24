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
        public static IDbTable QueryTable(this IDb db, string tSql)
        {
            var table = db.DBA.QueryDataTableNormal(tSql, CommandType.Text);

            return new DbTable(table);
        }
    }
}