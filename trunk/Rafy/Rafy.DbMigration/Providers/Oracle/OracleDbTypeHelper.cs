/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120427
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120427
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Rafy.DbMigration.Oracle
{
    internal static class OracleDbTypeHelper
    {
        /// <summary>
        /// 把 DbType 转换为 Oracle 中的数据类型
        /// </summary>
        /// <param name="fieldType">Type of the field.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static string ConvertToOracleTypeString(DbType fieldType, string length = null)
        {
            switch (fieldType)
            {
                case DbType.String:
                case DbType.AnsiString:
                    if (!string.IsNullOrEmpty(length))
                    {
                        return "VARCHAR2(" + length + ')';
                    }
                    return "VARCHAR2(4000)";
                case DbType.Int32:
                    return "INTEGER";
                case DbType.DateTime:
                    return "DATE";
                case DbType.Int64:
                case DbType.Double:
                    return "NUMBER";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "CHAR(1)";
                case DbType.Byte:
                    return "BYTE";
                default:
                    break;
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// 把 Oracle 中的数据类型 转换为 DbType
        /// </summary>
        /// <param name="lowerSqlType">Type of the lower SQL.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static DbType ConvertFromOracleTypeString(string lowerSqlType)
        {
            switch (lowerSqlType)
            {
                case "nvarchar2":
                case "varchar2":
                    return DbType.String;
                case "integer":
                    return DbType.Int32;
                case "number":
                    return DbType.Double;
                case "char":
                    return DbType.Boolean;
                case "blob":
                    return DbType.Binary;
                case "byte":
                    return DbType.Byte;
                case "date":
                    return DbType.DateTime;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}