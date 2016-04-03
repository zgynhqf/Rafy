using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data.Providers;
using Rafy.Data;

namespace Rafy.Data.Providers
{
    public class SqlServer2008Backuper : SqlServerBackuper
    {
        public SqlServer2008Backuper(IDbAccesser masterDBAccesser)
            : base(masterDBAccesser)
        {
        }

        protected override string DatabaseIdColumnName
        {
            get
            {
                return "dbid";
            }
        }
    }
}
