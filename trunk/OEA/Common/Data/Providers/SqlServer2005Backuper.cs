using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using hxy.Common.Data;

namespace hxy.Common.Data.Providers
{
    public class SqlServer2005Backuper : SqlServerBackuper
    {
        public SqlServer2005Backuper(IDBAccesser masterDBAccesser)
            : base(masterDBAccesser)
        {
        }

        protected override string DatabaseIdColumnName
        {
            get
            {
                return "bdid";
            }
        }
    }
}