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

namespace DbMigration
{
    public static class DbTypeHelper
    {
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