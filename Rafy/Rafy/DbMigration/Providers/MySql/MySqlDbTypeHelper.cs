/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161226 15:22
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql数据类型的帮助器
    /// </summary>
    internal static class MySqlDbTypeHelper
    {
        public static DbType ConvertFromCLRType(Type clrType)
        {
            var value = DbTypeHelper.ConvertFromCLRType(clrType);
            return value;
        }

        /// <summary>
        /// 把 DbType的类型值 转换为 MySql 数据库中兼容的数据类型
        /// </summary>
        /// <param name="fieldType">字段的DbType类型值</param>
        /// <param name="length">数据类型的长度</param>
        /// <returns>返回MySql数据库中的具体类型值</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static string ConvertToMySqlTypeString(DbType fieldType, string length = null)
        {
            switch (fieldType)
            {
                case DbType.String:
                case DbType.AnsiString:
                    if (!string.IsNullOrEmpty(length) && !string.Equals("max", length, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return "VARCHAR(" + length + ")";
                    }
                    return "TEXT";
                case DbType.Xml:
                    return "TEXT";
                case DbType.SByte:
                case DbType.Byte:
                    return "TINYINT";
                case DbType.UInt16:
                case DbType.Int16:
                    return "SMALLINT ";
                case DbType.UInt32:
                case DbType.Int32:
                    return "INT";
                case DbType.UInt64:
                case DbType.Int64:
                    return "BIGINT";
                case DbType.DateTimeOffset:
                    return "TIMESTAMP";
                case DbType.Time:
                    return "TIME";
                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                    return "DATETIME";
                case DbType.Single:
                    return "FLOAT";
                case DbType.Double:
                    return "DOUBLE";
                case DbType.Decimal:
                    if (!string.IsNullOrWhiteSpace(length))
                    {
                        return "DECIMAL(" + length + ")";
                    }
                    return "DECIMAL(18,2)";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "TINYINT(1)";
                //return "BIT";// Boolean 映射为 BIT 时，批量导入会出错。
                default:
                    break;
            }
            throw new NotSupportedException(string.Format("不支持生成列类型：{0}。", fieldType));
        }

        /// <summary>
        /// 把 MySql 数据库中的数据类型 转换为 CLR中的 DbType的类型值
        /// </summary>
        /// <param name="mySqlType">MySql数据库中的具体类型</param>
        /// <returns>返回CLR的DbType的类型值</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public static DbType ConvertFromMySqlTypeString(string mySqlType)
        {
            if (TypeContains(mySqlType, "CHAR")) { return DbType.String; }
            if (TypeContains(mySqlType, "VARCHAR")) { return DbType.String; }
            if (TypeContains(mySqlType, "TINYTEXT")) { return DbType.String; }
            if (TypeContains(mySqlType, "MEDIUMTEXT")) { return DbType.String; }
            if (TypeContains(mySqlType, "TEXT")) { return DbType.String; }
            if (TypeContains(mySqlType, "LONGTEXT")) { return DbType.String; }
            if (TypeContains(mySqlType, "BIT")) { return DbType.Boolean; }
            if (TypeContains(mySqlType, "TINYINT(1)")) { return DbType.Boolean; }
            if (TypeContains(mySqlType, "INT"))
            {
                if (mySqlType.IndexOf('(') > 0)
                {
                    mySqlType = mySqlType.Substring(0, mySqlType.IndexOf('('));
                }
                if (string.Compare(mySqlType, "TINYINT", true) == 0) { return DbType.Byte; }
                if (string.Compare(mySqlType, "SMALLINT", true) == 0) { return DbType.Int16; }
                if (string.Compare(mySqlType, "BIGINT", true) == 0) { return DbType.Int64; }
                if ((string.Compare(mySqlType, "INT", true) == 0) || (string.Compare(mySqlType, "YEAR", true) == 0)) { return DbType.Int32; }
            }
            if (TypeContains(mySqlType, "FLOAT")) { return DbType.Single; }
            if (TypeContains(mySqlType, "DOUBLE")) { return DbType.Double; }
            if (TypeContains(mySqlType, "DECIMAL")) { return DbType.Decimal; }
            if (TypeContains(mySqlType, "BLOB")) { return DbType.Binary; }
            if (TypeContains(mySqlType, "TIME"))
            {
                if (mySqlType.IndexOf('(') > 0)
                {
                    mySqlType = mySqlType.Substring(0, mySqlType.IndexOf('('));
                }
                if (string.Compare(mySqlType, "TIME", true) == 0) { return DbType.Time; }
                if (string.Compare(mySqlType, "DATETIME", true) == 0) { return DbType.DateTime; }
                if (string.Compare(mySqlType, "TIMESTAMP", true) == 0) { return DbType.DateTimeOffset; }
            }
            if (TypeContains(mySqlType, "DATE")) { return DbType.Date; }

            throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", mySqlType));
        }

        private static bool TypeContains(string mySqlType, string targetType)
        {
            return CultureInfo.InvariantCulture.CompareInfo
                .IndexOf(mySqlType, targetType, CompareOptions.IgnoreCase) >= 0;
        }
    }
}