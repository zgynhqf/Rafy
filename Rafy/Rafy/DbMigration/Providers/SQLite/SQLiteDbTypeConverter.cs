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

using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration.SQLite
{
    /// <summary>
    /// SQLite数据类型的帮助器
    /// </summary>
    public class SQLiteDbTypeConverter : DbTypeConverter
    {
        public static SQLiteDbTypeConverter Instance = new SQLiteDbTypeConverter();

        protected SQLiteDbTypeConverter() { }

        public override object ToClrValue(object dbValue, Type clrType)
        {
            var value = base.ToClrValue(dbValue, clrType);

            if (value != null)
            {
                if (clrType == typeof(bool))
                {
                    value = Convert.ToInt32(value) == 1 ? BooleanBoxes.True : BooleanBoxes.False;
                }
                else if (clrType == typeof(DateTimeOffset))
                {
                    DateTime dateTime = (DateTime)dbValue;

                    if (dateTime == DateTime.MinValue)
                    {
                        value = DateTimeOffset.MinValue;
                    }
                    else
                    {
                        value = (DateTimeOffset)DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                    }
                }
            }
            else
            {
                if (clrType == typeof(string))
                {
                    value = string.Empty;//null 转换为空字符串
                }
            }

            return value;
        }

        public override object ToDbParameterValue(object value)
        {
            value = base.ToDbParameterValue(value);

            if (value != DBNull.Value)
            {
                if (value is bool)
                {
                    value = Convert.ToInt32(value);
                }
                else if (value.GetType().IsEnum)
                {
                    value = TypeHelper.CoerceValue(typeof(int), value);
                }
                else if (value is DateTimeOffset)
                {
                    value = ((DateTimeOffset)value).DateTime;
                }
            }

            return value;
        }

        /// <summary>
        /// 将CLR类型的Type值转换为CLR的DbType类型值
        /// </summary>
        /// <param name="clrType">CLR类型的Type值</param>
        /// <returns>返回DbType类型对应的值</returns>
        public override DbType FromClrType(Type clrType)
        {
            var value = base.FromClrType(clrType);
            if (value == DbType.Boolean)
            {
                value = DbType.String;
            }
            return value;
        }

        /// <summary>
        /// 把 DbType的类型值 转换为 SQLite 数据库中兼容的数据类型
        /// </summary>
        /// <param name="fieldType">字段的DbType类型值</param>
        /// <param name="length">数据类型的长度</param>
        /// <returns>返回SQLite数据库中的具体类型值</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override string ConvertToDatabaseTypeName(DbType fieldType, string length = null)
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
                    return "INTEGER";
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
        /// 把 SQLite 数据库中的数据类型 转换为 CLR中的 DbType的类型值
        /// </summary>
        /// <param name="databaseTypeName">SQLite数据库中的具体类型</param>
        /// <returns>返回CLR的DbType的类型值</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public override DbType ConvertToDbType(string databaseTypeName)
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "CHAR", CompareOptions.IgnoreCase) != -1) return DbType.String;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "VARCHAR", CompareOptions.IgnoreCase) != -1) return DbType.String;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "TINYTEXT", CompareOptions.IgnoreCase) != -1) return DbType.String;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "MEDIUMTEXT", CompareOptions.IgnoreCase) != -1) return DbType.String;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "TEXT", CompareOptions.IgnoreCase) != -1) return DbType.String;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "LONGTEXT", CompareOptions.IgnoreCase) != -1) return DbType.String;

            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "INT", CompareOptions.IgnoreCase) != -1)
            {
                //针对布尔类型特殊处理
                if (string.Compare(databaseTypeName, "TINYINT(1)", true) == 0) return DbType.Boolean;

                if (databaseTypeName.IndexOf('(') > 0)
                {
                    databaseTypeName = databaseTypeName.Substring(0, databaseTypeName.IndexOf('('));
                }
                if (string.Compare(databaseTypeName, "TINYINT", true) == 0) return DbType.Byte;
                if (string.Compare(databaseTypeName, "SMALLINT", true) == 0) return DbType.Int16;
                if (string.Compare(databaseTypeName, "BIGINT", true) == 0) return DbType.Int64;
                if ((string.Compare(databaseTypeName, "INT", true) == 0) || (string.Compare(databaseTypeName, "INTEGER", true) == 0) ||
                    (string.Compare(databaseTypeName, "YEAR", true) == 0)) return DbType.Int32;

                throwNotSupportedException();
            }
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "FLOAT", CompareOptions.IgnoreCase) != -1) return DbType.Single;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "DOUBLE", CompareOptions.IgnoreCase) != -1) return DbType.Double;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "DECIMAL", CompareOptions.IgnoreCase) != -1) return DbType.Decimal;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "BLOB", CompareOptions.IgnoreCase) != -1) return DbType.Binary;
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "DATE", CompareOptions.IgnoreCase) != -1) return DbType.Date;

            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(databaseTypeName, "TIME", CompareOptions.IgnoreCase) != -1)
            {
                if (databaseTypeName.IndexOf('(') > 0)
                {
                    databaseTypeName = databaseTypeName.Substring(0, databaseTypeName.IndexOf('('));
                }
                if (string.Compare(databaseTypeName, "TIME", true) == 0) return DbType.Time;
                if (string.Compare(databaseTypeName, "DATETIME", true) == 0) return DbType.DateTime;
                if (string.Compare(databaseTypeName, "TIMESTAMP", true) == 0) return DbType.DateTimeOffset;
                throwNotSupportedException();
            }

            throwNotSupportedException();

            void throwNotSupportedException()
            {
                throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", databaseTypeName));
            };
            return DbType.String;
        }
    }
}