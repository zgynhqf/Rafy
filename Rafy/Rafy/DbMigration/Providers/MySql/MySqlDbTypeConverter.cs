﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170921 23:32
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Reflection;

namespace Rafy.DbMigration.MySql
{
    /// <summary>
    /// MySql 数据库字段类型的转换器。
    /// </summary>
    /// <seealso cref="Rafy.DbMigration.DbTypeConverter" />
    public class MySqlDbTypeConverter : DbTypeConverter
    {
        public static readonly MySqlDbTypeConverter Instance = new MySqlDbTypeConverter();

        private MySqlDbTypeConverter() { }

        /// <summary>
        /// 把 DbType的类型值 转换为 MySql 数据库中兼容的数据类型
        /// MySql数据精度 http://qimo601.iteye.com/blog/1622368
        /// </summary>
        /// <param name="fieldType">字段的DbType类型值</param>
        /// <param name="length">数据类型的长度</param>
        /// <returns>返回MySql数据库中的具体类型值</returns>
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
                    return "SMALLINT";
                case DbType.UInt32:
                case DbType.Int32:
                    return "INT";
                case DbType.UInt64:
                case DbType.Int64:
                    return "BIGINT";
                case DbType.Time:
                    return "TIME";
                case DbType.Date:
                    return "DATE";
                case DbType.DateTime:
                case DbType.DateTimeOffset:
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
        /// 将从数据库 Schema Meta 中读取出来的列的类型名称，转换为其对应的 DbType。
        /// </summary>
        /// <param name="databaseTypeName">从数据库 Schema Meta 中读取出来的列的类型名称。</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override DbType ConvertToDbType(string databaseTypeName)
        {
            if (TypeContains(databaseTypeName, "CHAR")) { return DbType.String; }
            if (TypeContains(databaseTypeName, "VARCHAR")) { return DbType.String; }
            if (TypeContains(databaseTypeName, "TINYTEXT")) { return DbType.String; }
            if (TypeContains(databaseTypeName, "MEDIUMTEXT")) { return DbType.String; }
            if (TypeContains(databaseTypeName, "TEXT")) { return DbType.String; }
            if (TypeContains(databaseTypeName, "LONGTEXT")) { return DbType.String; }
            if (TypeContains(databaseTypeName, "BIT")) { return DbType.Boolean; }
            if (TypeContains(databaseTypeName, "TINYINT(1)")) { return DbType.Boolean; }
            if (TypeContains(databaseTypeName, "INT"))
            {
                if (databaseTypeName.IndexOf('(') > 0)
                {
                    databaseTypeName = databaseTypeName.Substring(0, databaseTypeName.IndexOf('('));
                }
                if (string.Compare(databaseTypeName, "TINYINT", true) == 0) { return DbType.Byte; }
                if (string.Compare(databaseTypeName, "SMALLINT", true) == 0) { return DbType.Int16; }
                if (string.Compare(databaseTypeName, "BIGINT", true) == 0) { return DbType.Int64; }
                if ((string.Compare(databaseTypeName, "INT", true) == 0) || (string.Compare(databaseTypeName, "YEAR", true) == 0)) { return DbType.Int32; }
            }
            if (TypeContains(databaseTypeName, "FLOAT")) { return DbType.Single; }
            if (TypeContains(databaseTypeName, "DOUBLE")) { return DbType.Double; }
            if (TypeContains(databaseTypeName, "DECIMAL")) { return DbType.Decimal; }
            if (TypeContains(databaseTypeName, "BLOB")) { return DbType.Binary; }
            if (TypeContains(databaseTypeName, "TIME"))
            {
                if (databaseTypeName.IndexOf('(') > 0)
                {
                    databaseTypeName = databaseTypeName.Substring(0, databaseTypeName.IndexOf('('));
                }
                if (string.Compare(databaseTypeName, "TIME", true) == 0) { return DbType.Time; }
                if (string.Compare(databaseTypeName, "DATETIME", true) == 0) { return DbType.DateTime; }
                if (string.Compare(databaseTypeName, "TIMESTAMP", true) == 0) { return DbType.DateTime; }
            }
            if (TypeContains(databaseTypeName, "DATE")) { return DbType.Date; }

            throw new NotSupportedException(string.Format("不支持读取数据库中的列类型：{0}。", databaseTypeName));
        }

        /// <summary>
        /// 将指定的值转换为一个兼容数据库类型的值。
        /// 该值可用于与下层的 ADO.NET 交互。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        /// 将指定的值转换为一个 CLR 类型的值。
        /// </summary>
        /// <param name="dbValue">The database value.</param>
        /// <param name="clrType">Type of the color.</param>
        /// <returns></returns>
        public override object ToClrValue(object dbValue, Type clrType)
        {
            dbValue = base.ToClrValue(dbValue, clrType);

            if (dbValue == null && clrType == typeof(string))
            {
                dbValue = string.Empty;//null 转换为空字符串
            }

            // DateTime to DateTimeOffset
            // https://msdn.microsoft.com/zh-cn/library/bb546101.aspx
            // DateTime DateTime2 区别 http://www.studyofnet.com/news/1050.html
            if (clrType == typeof(DateTimeOffset) && dbValue != null)
            {
                DateTime dateTime = (DateTime)dbValue;

                if (dateTime == DateTime.MinValue)
                {
                    dbValue = DateTimeOffset.MinValue;
                }
                else
                {
                    dbValue = (DateTimeOffset)DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
                }
            }

            return dbValue;
        }

        private static bool TypeContains(string mySqlType, string targetType)
        {
            return CultureInfo.InvariantCulture.CompareInfo
                .IndexOf(mySqlType, targetType, CompareOptions.IgnoreCase) >= 0;
        }
    }
}
