/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161228
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161228 13:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Data.Providers
{
    /// <summary>
    /// MySql数据库链接字符串转换器
    /// </summary>
    public sealed class MySqlServerProvider : ISqlProvider
    {
        /// <summary>
        /// 存储过程返回值的参数名称
        /// </summary>
        public string ProcudureReturnParameterName
        {
            get
            {
                throw new NotImplementedException("暂不支持存储过程返回参数！");
                //return "ReturnValue";
            }
        }

        /// <summary>
        /// 返回针对MySql的数据库链接字符串
        /// </summary>
        /// <param name="commonSql"></param>
        /// <returns>返回针对MySql的数据库链接字符串</returns>
        public string ConvertToSpecialDbSql(string commonSql)
        {
            return ConverterFactory.ReParameterName.Replace(commonSql, "?p${number}");
        }

        /// <summary>
        /// 根据索引获取参数名称
        /// </summary>
        /// <param name="number">索引数据</param>
        /// <returns>返回指定位置的参数名称</returns>
        public string GetParameterName(int number)
        {
            return "?p" + number;
        }
    }
}