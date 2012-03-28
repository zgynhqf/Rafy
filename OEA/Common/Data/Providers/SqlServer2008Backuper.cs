using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using hxy.Common.Data.Providers;
using hxy.Common.Data;

namespace hxy.Common.Data.Providers
{
    public class SqlServer2008Backuper : SqlServerBackuper
    {
        public SqlServer2008Backuper(IDBAccesser masterDBAccesser)
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
