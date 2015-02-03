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

namespace Rafy.DbMigration
{
    internal static class DbTypeHelper
    {
        public static DbType ConvertFromCLRType(Type clrType)
        {
            if (clrType.IsEnum) { return DbType.Int32; }
            if (clrType == typeof(string)) { return DbType.String; }
            if (clrType == typeof(int)) { return DbType.Int32; }
            if (clrType == typeof(bool)) { return DbType.Boolean; }
            if (clrType == typeof(DateTime)) { return DbType.DateTime; }
            if (clrType == typeof(Guid)) { return DbType.Guid; }
            if (clrType == typeof(double)) { return DbType.Double; }
            if (clrType == typeof(byte)) { return DbType.Byte; }
            if (clrType == typeof(short)) { return DbType.Int16; }
            if (clrType == typeof(char)) { return DbType.StringFixedLength; }
            if (clrType == typeof(decimal)) { return DbType.Decimal; }
            if (clrType == typeof(float)) { return DbType.Single; }
            if (clrType == typeof(uint)) { return DbType.UInt32; }
            if (clrType == typeof(ulong)) { return DbType.UInt64; }
            if (clrType == typeof(ushort)) { return DbType.UInt16; }
            if (clrType == typeof(sbyte)) { return DbType.SByte; }
            if (clrType == typeof(float)) { return DbType.Single; }
            if (clrType == typeof(byte[])) { return DbType.Binary; }

            if (clrType.IsGenericType &&
                clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return ConvertFromCLRType(clrType.GetGenericArguments()[0]);
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
                case DbType.Decimal:
                    return "0";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}