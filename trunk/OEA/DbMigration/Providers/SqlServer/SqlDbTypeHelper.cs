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

namespace DbMigration.SqlServer
{
    internal static class SqlDbTypeHelper
    {
        /// <summary>
        /// 把 DbType 转换为 SqlServer 中的数据类型
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static string ConvertToSQLTypeString(DbType fieldType)
        {
            switch (fieldType)
            {
                case DbType.String:
                case DbType.AnsiString:
                    return "NVARCHAR(MAX)";
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
                case DbType.Binary:
                    return "VARBINARY(MAX)";
                case DbType.Boolean:
                    return "BIT";
                case DbType.Byte:
                    return "BYTE";
                default:
                    break;
            }
            throw new NotSupportedException(string.Format("不支持以下列类型：{0}", fieldType));
        }

        /// <summary>
        /// 把 SqlServer 中的数据类型 转换为 DbType
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static DbType ConvertFromSQLTypeString(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "uniqueidentifier":
                    return DbType.Guid;
                case "nvarchar":
                case "varchar":
                    return DbType.String;
                case "int":
                    return DbType.Int32;
                case "bigint":
                    return DbType.Int64;
                case "bit":
                    return DbType.Boolean;
                case "float":
                    return DbType.Double;
                case "varbinary":
                    return DbType.Binary;
                case "byte":
                    return DbType.Byte;
                case "datetime":
                    return DbType.DateTime;
                case "money":
                    return DbType.Double;
                default:
                    throw new NotSupportedException(string.Format("不支持以下列类型：{0}", sqlType));
            }
        }
    }
}
