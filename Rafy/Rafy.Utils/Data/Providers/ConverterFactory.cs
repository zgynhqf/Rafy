using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rafy.Data.Providers
{
    internal class ConverterFactory
    {
        private static DbProviderFactory _sql, _sqlCe, _oracle;

        /// <summary>
        /// 以快速键值对照来获取 DbProviderFactory。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(string provider)
        {
            //ISqlConverter Factory
            switch (provider)
            {
                case DbSetting.Provider_SqlClient:
                    if (_sql == null) { _sql = DbProviderFactories.GetFactory(DbSetting.Provider_SqlClient); }
                    return _sql;
                case DbSetting.Provider_SqlCe:
                    if (_sqlCe == null) { _sqlCe = DbProviderFactories.GetFactory(DbSetting.Provider_SqlCe); }
                    return _sqlCe;
                default:
                    if (DbSetting.IsOracleProvider(provider))
                    {
                        if (_oracle == null) { _oracle = DbProviderFactories.GetFactory(provider); }
                        return _oracle;
                    }
                    return DbProviderFactories.GetFactory(provider);
                //throw new NotSupportedException("This type of database is not supportted now:" + provider);
            }
        }

        private static ISqlProvider _sqlConverter, _oracleConverter, _odbcConverter;
        public static ISqlProvider Create(string provider)
        {
            //ISqlConverter Factory
            switch (provider)
            {
                case DbSetting.Provider_Odbc:
                    if (_odbcConverter == null) _odbcConverter = new ODBCProvider();
                    return _odbcConverter;

                case DbSetting.Provider_SqlClient:
                case DbSetting.Provider_SqlCe:
                    if (_sqlConverter == null) _sqlConverter = new SqlServerProvider();
                    return _sqlConverter;

                default:
                    if (DbSetting.IsOracleProvider(provider))
                    {
                        if (_oracleConverter == null) _oracleConverter = new OracleProvider();
                        return _oracleConverter;
                    }

                    if (_sqlConverter == null) _sqlConverter = new SqlServerProvider();
                    return _sqlConverter;
            }
        }

        /// <summary>
        /// 在 FormatSQL 中的参数格式定义。
        /// </summary>
        internal static readonly Regex ReParameterName = new Regex(@"{(?<number>\d+)}", RegexOptions.Compiled);
    }
}
