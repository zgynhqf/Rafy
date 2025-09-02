/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20250815
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20250815 21:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Data.Providers
{
    /// <summary>
    /// CLR 值与数据库值的转换器。
    /// </summary>
    public interface IDbValueConverter
    {
        /// <summary>
        /// 将指定的值转换为一个兼容数据库类型的值。
        /// 该值可用于与下层的 ADO.NET 交互。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        object ToDbParameterValue(object value);
        /// <summary>
        /// 将指定的值转换为一个 CLR 类型的值。
        /// </summary>
        /// <param name="dbValue">The database value.</param>
        /// <param name="clrType">Type of the color.</param>
        /// <returns></returns>
        object ToClrValue(object dbValue, Type clrType);
    }

    public class DbValueConverter : IDbValueConverter
    {
        /// <summary>
        /// 将指定的值转换为一个兼容数据库类型的值。
        /// 该值可用于与下层的 ADO.NET 交互。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual object ToDbParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }

        /// <summary>
        /// 将指定的值转换为一个 CLR 类型的值。
        /// </summary>
        /// <param name="dbValue">The database value.</param>
        /// <param name="clrType">Type of the color.</param>
        /// <returns></returns>
        public virtual object ToClrValue(object dbValue, Type clrType)
        {
            return dbValue == DBNull.Value ? null : dbValue;
        }
    }
}