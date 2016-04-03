using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using Rafy.Data;

namespace Rafy.Data.Providers
{
    public class SqlServer2005Backuper : SqlServerBackuper
    {
        public SqlServer2005Backuper(IDbAccesser masterDBAccesser)
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