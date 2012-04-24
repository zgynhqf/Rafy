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
            switch (provider)
            {
                case DbSetting.Provider_SqlClient:
                case DbSetting.Provider_SqlCe:
                    return new SqlServerProvider();
                case DbSetting.Provider_Oracle:
                    return new OracleProvider();
                case DbSetting.Provider_Odbc:
                    return new ODBCProvider();
                default:
                    return new SqlServerProvider();
                //throw new NotSupportedException("This type of database is not supportted now:" + provider);
            }
        }

        /// <summary>
        /// 在 FormatSQL 中的参数格式定义。
        /// </summary>
        internal static readonly Regex ReParameterName = new Regex(@"{(?<number>\d+)}", RegexOptions.Compiled);
    }
}
