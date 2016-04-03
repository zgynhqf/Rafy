/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Rafy.DbMigration.SqlServer
{
    internal static class SqlDbTypeHelper
    {
        /// <summary>
        /// 把 DbType 转换为 SqlServer 中的数据类型
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static string ConvertToSQLTypeString(DbType fieldType, string length = null)
        {
            switch (fieldType)
            {
                case DbType.String:
                case DbType.AnsiString:
                    if (string.IsNullOrEmpty(length)) { length = "MAX"; }
                    return "NVARCHAR(" + length + ")";
                case DbType.Int32:
                    return "INT";
                case DbType.Int64:
                    return "BIGINT";
                case DbType.DateTime:
                    return "DATETIME";
                case DbType.Guid:
                    return "UNIQUEIDENTIFIER";
                case DbType.Double:
                    return "FLOAT";
                case DbType.Decimal:
                    if (!string.IsNullOrWhiteSpace(length))
                    {
                        return "DECIMAL(" + length + ")";
                    }
                    return "DECIMAL(18,2)";
                case DbType.Binary:
                    return "VARBINARY(MAX)";
                case DbType.Boolean:
                    return "BIT";
                case DbType.Byte:
                    return "BYTE";
                case DbType.Xml:
                    return "XML";
                default:
                    break;
            }
            throw new NotSupportedException(string.Format("不支持生成列类型：{0}。", fieldType));
        }

        /// <summary>
        /// 把 SqlServer 中的数据类型 转换为 DbType
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static DbType ConvertFromSQLTypeString(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "uniqueidentifier":
                    return DbType.Guid;
                case "nvarchar":
                case "varchar":
                case "nchar":
                case "char":
                case "ntext":
                case "text":
                    return DbType.String;
                case "xml":
                    return DbType.Xml;
                case "smallint":
                case "int":
                    return DbType.Int32;
                case "bigint":
                    return DbType.Int64;
                case "bit":
                    return DbType.Boolean;
                case "float":
                case "real":
                    return DbType.Double;
                case "numeric":
                case "decimal":
                case "money":
                    return DbType.Decimal;
                case "binary":
                case "varbinary":
                case "image":
                    return DbType.Binary;
                case "byte":
                    return DbType.Byte;
                case "date":
                case "datetime":
                case "datetimeoffset":
                case "time":
                    return DbType.DateTime;
                default:
                    throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", sqlType));
            }
        }
    }
}
