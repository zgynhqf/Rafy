using System;
using System.Collections.Generic;
using System.Text;
using OEA.ORM.sqlserver;
using OEA.ORM;
using System.Data;

namespace OEA.ORM
{
    public class DbFactory
    {
        public static readonly DbFactory Instance = new DbFactory();

        private SqlProvider provider;

        private DbFactory()
        {
            provider = new SqlProvider();
        }

        public IDb GetDb(IDbConnection cn)
        {
            return provider.OpenDb(cn);
        }
    }
}
