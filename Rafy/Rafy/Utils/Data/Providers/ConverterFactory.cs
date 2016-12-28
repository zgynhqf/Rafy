using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rafy.Data.Providers
{
    /// <summary>
    /// 根据提供的字符串类型数据库提供器转换类型，可以获取指定的DbProviderFactory和ISqlProvider类型的具体实例
    /// </summary>
    internal class ConverterFactory
    {
        private static DbProviderFactory _sql, _sqlCe, _oracle,_mySql;
        private static ISqlProvider _sqlConverter, _oracleConverter, _odbcConverter,_mySqlConverter;

        /// <summary>   
        /// 以快速键值对照来获取 DbProviderFactory。
        /// </summary>
        /// <param name="provider"></param>
        /// <returns>返回DbProviderFactory类型的具体对象实例</returns>
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
                //PatrickLiu增加的有关获取MySql的Provider工厂类
                case DbSetting.Provider_MySql:
                    if (_mySql == null) { _mySql = DbProviderFactories.GetFactory(DbSetting.Provider_MySql); }
                    return _mySql;
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

        /// <summary>
        /// 创建指定的链接字符串的转换器
        /// </summary>
        /// <param name="provider">指定的数据库的提供程序</param>
        /// <returns>返回针对指定数据库的ISqlProvider类型的具体对象实例</returns>
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
                //PatrickLiu增加的有关MySql的代码
                case DbSetting.Provider_MySql:
                    if (_mySqlConverter == null) _mySqlConverter = new MySqlServerProvider();
                    return _mySqlConverter;

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