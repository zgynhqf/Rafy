using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace hxy.Common.Data
{
    public class LongTimeDbAccesser : DBAccesser
    {
        public LongTimeDbAccesser(DbSetting setting) : base(setting) { }

        public LongTimeDbAccesser(string connectionStringSettingName) : base(connectionStringSettingName) { }

        public LongTimeDbAccesser(string connectionString, string connectionProvider)
            : base(connectionString, connectionProvider) { }

        public LongTimeDbAccesser(IDbConnection dbConnection, string connectionProvider)
            : base(dbConnection, connectionProvider) { }

        protected override IDbCommand PrepareCommand(string strSql, CommandType type, IDbDataParameter[] parameters)
        {
            var cmd = base.PrepareCommand(strSql, type, parameters);
            cmd.CommandTimeout = 500;
            return cmd;
        }
    }
}
