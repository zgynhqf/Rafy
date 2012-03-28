using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace hxy.Common.Data.Providers
{
    internal class ConverterFactory
    {
        public static ISqlProvider Create(string provider)
        {
            //ISqlConverter Factory
            switch (provider.ToLower())
            {
                case "system.data.sqlclient":
                    return new SqlServerProvider();
                case "system.data.oracleclient":
                    return new OracleProvider();
                case "system.data.odbc":
                    return new ODBCProvider();
                default:
                    return new SqlServerProvider();
                    //throw new NotSupportedException("This type of database is not supportted now!");
            }
        }
        internal static readonly Regex ReParameterName = new Regex(@"{(?<number>\d+)}", RegexOptions.Compiled);
    }
}
