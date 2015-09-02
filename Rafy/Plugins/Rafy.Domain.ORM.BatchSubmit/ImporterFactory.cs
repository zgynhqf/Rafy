/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150816
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150816 12:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.BatchSubmit;
using Rafy.Domain.ORM.BatchSubmit.Oracle;
using Rafy.Domain.ORM.BatchSubmit.SqlServer;

namespace Rafy.Domain
{
    /// <summary>
    /// 批量导入器工具。
    /// </summary>
    public static class ImporterFactory
    {
        /// <summary>
        /// 为指定的仓库创建批量导入器。
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <returns></returns>
        public static IBatchImporter CreateImporter(this IRepository repository)
        {
            var rdp = RdbDataProvider.Get(repository);
            return CreateImporter(rdp.DbSetting);
        }

        /// <summary>
        /// 根据数据库配置来创建对应数据库的批量导入器。
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static IBatchImporter CreateImporter(DbSetting setting)
        {
            switch (setting.ProviderName)
            {
                case DbSetting.Provider_SqlClient:
                    return new SqlBatchImporter();
                default:
                    if (DbSetting.IsOracleProvider(setting))
                    {
                        return new OracleBatchImporter();
                    }
                    throw new NotSupportedException("目前不支持该类型数据库的批量导入：" + setting.ProviderName);
            }
        }
    }
}
