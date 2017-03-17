/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:48
 * 
*******************************************************/

using System;

namespace Rafy.DataTableMigration.Exceptions
{
    /// <summary>
    /// 提供一个数据归档的异常。
    /// </summary>
    public class DataTableMigrationException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="DataTableMigrationException"/> 类的新实例。
        /// </summary>
        /// <param name="message">表示异常描述。</param>
        /// <param name="innerException">表示一个内部异常信息。</param>
        public DataTableMigrationException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}