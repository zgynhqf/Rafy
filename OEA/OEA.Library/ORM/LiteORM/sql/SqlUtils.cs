using System;
using System.Data;
using System.Collections;

namespace OEA.ORM.sqlserver
{
    public static class SqlUtils
    {
        public static void PrepParam(IDbDataParameter p, object val)
        {
            if (val == null || val == DBNull.Value)
            {
                PrepParam(p, (Type)null);
                p.Value = DBNull.Value;
            }
            else
            {
                PrepParam(p, val.GetType());
                p.Value = val;
            }
        }

        public static void PrepParam(IDbDataParameter p, Type type)
        {
            p.DbType = ResolveType(type);
            SetMaxValues(p);
        }

        public static void SetMaxValues(IDbDataParameter p)
        {
            p.Size = int.MaxValue;
            p.Precision = 38; //this is max for SQLServer
            //p.Scale = ...; //not required - works without it
        }

        public static DbType ResolveType(Type type)
        {
            return DbMigration.SqlServer.DbTypeHelper.ConvertFromCLRType(type);
        }
    }
}
