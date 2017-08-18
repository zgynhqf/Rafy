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
        /// <summary>
        /// 将CLR的布尔类型转换为MySql数据库兼容的类型
        /// </summary>
        /// <param name="value">CLR的布尔类型值</param>
        /// <returns>转换为1或者0的数值</returns>
        public static int ToDbBoolean(bool value)
        {
            //数据库使用 tinyint(1)来存储 Boolean 类型数据。
            return value ? 1 : 0;
        }

        /// <summary>
        /// 转换为CLR的布尔类型
        /// </summary>
        /// <param name="value">待转换的对象值</param>
        /// <returns>返回CLR的true或者false</returns>
        public static bool ToCLRBoolean(object value)
        {
            return value.ToString() == "1" ? true : false;
        }

        /// <summary>
        /// 将CLR类型的Type值转换为CLR的DbType类型值
        /// </summary>
        /// <param name="clrType">CLR类型的Type值</param>
        /// <returns>返回DbType类型对应的值</returns>
        public static DbType ConvertFromCLRType(Type clrType)
        {
            var value = DbTypeHelper.ConvertFromCLRType(clrType);
            if (value == DbType.Boolean)
            {
                value = DbType.String;
            }
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
                    return "DECIMAL";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "TINYINT(1)";
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
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "CHAR", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.String;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "VARCHAR", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.String;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "TINYTEXT", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.String;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "MEDIUMTEXT", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.String;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "TEXT", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.String;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "LONGTEXT", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.String;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "INT", CompareOptions.IgnoreCase) != -1)
            {
                //针对布尔类型特殊处理
                if (string.Compare(mySqlType, "TINYINT(1)", true) == 0)
                {
                    return DbType.Boolean;
                }
                if (mySqlType.IndexOf('(') > 0)
                {
                    mySqlType = mySqlType.Substring(0, mySqlType.IndexOf('('));
                }
                if (string.Compare(mySqlType, "TINYINT", true) == 0)
                {
                    return DbType.Byte;
                }
                else if (string.Compare(mySqlType, "SMALLINT", true) == 0)
                {
                    return DbType.Int16;
                }
                else if (string.Compare(mySqlType, "BIGINT", true) == 0)
                {
                    return DbType.Int64;
                }
                else if ((string.Compare(mySqlType, "INT", true) == 0) || (string.Compare(mySqlType, "YEAR", true) == 0))
                {
                    return DbType.Int32;
                }
                else
                {
                    throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", mySqlType));
                }
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "FLOAT", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.Single;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "DOUBLE", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.Double;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "DECIMAL", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.Decimal;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "BLOB", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.Binary;
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "TIME", CompareOptions.IgnoreCase) != -1)
            {
                if (mySqlType.IndexOf('(') > 0)
                {
                    mySqlType = mySqlType.Substring(0, mySqlType.IndexOf('('));
                }
                if (string.Compare(mySqlType, "TIME", true) == 0)
                {
                    return DbType.Time;
                }
                else if (string.Compare(mySqlType, "DATETIME", true) == 0)
                {
                    return DbType.DateTime;
                }
                else if (string.Compare(mySqlType, "TIMESTAMP", true) == 0)
                {
                    return DbType.DateTimeOffset;
                }
                else
                {
                    throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", mySqlType));
                }
            }
            else if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(mySqlType, "DATE", CompareOptions.IgnoreCase) != -1)
            {
                return DbType.Date;
            }
            else
            {
                throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", mySqlType));
            }
        }
    }
}