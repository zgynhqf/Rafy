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
    public static class DbTypeHelper
    {
        /// <summary>
        /// 把DbType转换为sql2005中的数据类型
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
            throw new NotSupportedException();
        }

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
                    throw new NotSupportedException();
            }
        }

        public static DbType ConvertFromCLRType(Type fieldRuntimeType)
        {
            if (fieldRuntimeType.IsEnum) { return DbType.Int32; }
            if (fieldRuntimeType == typeof(string)) { return DbType.String; }
            if (fieldRuntimeType == typeof(int)) { return DbType.Int32; }
            if (fieldRuntimeType == typeof(bool)) { return DbType.Boolean; }
            if (fieldRuntimeType == typeof(DateTime)) { return DbType.DateTime; }
            if (fieldRuntimeType == typeof(Guid)) { return DbType.Guid; }
            if (fieldRuntimeType == typeof(double)) { return DbType.Double; }
            if (fieldRuntimeType == typeof(byte)) { return DbType.Byte; }
            if (fieldRuntimeType == typeof(short)) { return DbType.Int16; }
            if (fieldRuntimeType == typeof(char)) { return DbType.StringFixedLength; }
            if (fieldRuntimeType == typeof(decimal)) { return DbType.Decimal; }
            if (fieldRuntimeType == typeof(float)) { return DbType.Single; }
            if (fieldRuntimeType == typeof(uint)) { return DbType.UInt32; }
            if (fieldRuntimeType == typeof(ulong)) { return DbType.UInt64; }
            if (fieldRuntimeType == typeof(ushort)) { return DbType.UInt16; }
            if (fieldRuntimeType == typeof(sbyte)) { return DbType.SByte; }
            if (fieldRuntimeType == typeof(float)) { return DbType.Single; }
            if (fieldRuntimeType == typeof(byte[])) { return DbType.Binary; }

            if (fieldRuntimeType.IsGenericType &&
                fieldRuntimeType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return ConvertFromCLRType(fieldRuntimeType.GetGenericArguments()[0]);
            }

            return DbType.String;
        }

        /// <summary>
        /// 获取type的默认值sql表达式
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static string GetDefaultValue(DbType type)
        {
            switch (type)
            {
                case DbType.String:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return "''";
                case DbType.DateTime:
                    return "'2000-1-1 0:0:0'";
                case DbType.Guid:
                    return "'" + Guid.Empty + "'";
                case DbType.Int32:
                case DbType.Binary:
                case DbType.Double:
                case DbType.Boolean:
                case DbType.Byte:
                    return "0";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
