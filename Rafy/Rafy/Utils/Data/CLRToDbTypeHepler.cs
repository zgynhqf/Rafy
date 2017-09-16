/*******************************************************
 * 
 * 作者：颜昌龙
 * 创建日期：20170916
 * 说明：CLR to DbType 处理必须的转换。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 颜昌龙 20170916 14:55
 * 
*******************************************************/

using Rafy.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Utils.Data
{
    #region 根据CLR显示指定DbType

    /// <summary>
    /// 一个CLR TO DBTYPE接口
    /// </summary>
    public interface ICLRDbTypeMapping
    {
        Dictionary<Type, DbType> CLRToDbType();
    }

    /// <summary>
    /// Sql Server
    /// </summary>
    public class SqlClientCLRMapping : ICLRDbTypeMapping
    {
        public Dictionary<Type, DbType> CLRToDbType()
        {
            return new Dictionary<Type, DbType>()
            {
                {typeof(DateTime),DbType.DateTime2 },
                {typeof(DateTime?),DbType.DateTime2 }
            };
        }
    }

    /// <summary>
    /// SqlCE
    /// </summary>
    public class SqlCeCLRMapping : ICLRDbTypeMapping
    {
        public Dictionary<Type, DbType> CLRToDbType()
        {
            return null;
        }
    }

    /// <summary>
    /// MySql
    /// </summary>
    public class MySqlCLRMapping : ICLRDbTypeMapping
    {
        public Dictionary<Type, DbType> CLRToDbType()
        {
            return null;
        }
    }

    /// <summary>
    /// Oracle
    /// </summary>
    public class OracleCLRMapping : ICLRDbTypeMapping
    {
        public Dictionary<Type, DbType> CLRToDbType()
        {
            return null;
        }
    }

    /// <summary>
    /// 默认
    /// </summary>
    public class NoneCLRMapping : ICLRDbTypeMapping
    {
        public Dictionary<Type, DbType> CLRToDbType()
        {
            return null;
        }
    }

    #endregion

    public class CLRToDbTypeHepler
    {
        /// <summary>
        /// 获取指定数据库需要手动映射的数据类型
        ///    例如SqlServer中将CLR <see cref="DateTime"/>映射为<see cref="DbType.DateTime2"/>
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static ICLRDbTypeMapping DbTypeByCLRType(string provider)
        {
            if (string.IsNullOrEmpty(provider)) throw new ArgumentNullException(nameof(provider));

            ICLRDbTypeMapping typeMapping = null;
            switch (provider)
            {
                case DbSetting.Provider_SqlClient:
                    typeMapping = new SqlClientCLRMapping();
                    break;
                case DbSetting.Provider_SqlCe:
                    typeMapping = new SqlCeCLRMapping();
                    break;
                case DbSetting.Provider_MySql:
                    typeMapping = new MySqlCLRMapping();
                    break;
                default:
                    if (DbConnectionSchema.IsOracleProvider(provider))
                    {
                        typeMapping = new OracleCLRMapping();
                        break;
                    }
                    typeMapping = new NoneCLRMapping();
                    break;
            }
            return typeMapping;
        }
    }
}
