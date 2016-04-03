using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Data.Providers
{
    /// <summary>
    /// 此接口用于把可用于String.Format格式的字符串转换为特定数据库格式的字符串
    /// </summary>
    internal interface ISqlProvider
    {
        /// <summary>
        /// 把可用于String.Format格式的字符串转换为特定数据库格式的字符串
        /// </summary>
        /// <param name="commonSql">可用于String.Format格式的字符串</param>
        /// <returns>可用于特定数据库的sql语句</returns>
        string ConvertToSpecialDbSql(string commonSql);

        /// <summary>
        /// 返回用于
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        string GetParameterName(int number);

        string ProcudureReturnParameterName { get; }
    }
}
