/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170921 23:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Reflection;

namespace Rafy.DbMigration.Oracle
{
    /// <summary>
    /// Oracle 数据库字段类型的转换器。
    /// </summary>
    public class OracleDbTypeConverter : DbTypeConverter
    {
        private const string CLOBTypeName = "CLOB";

        public static readonly OracleDbTypeConverter Instance = new OracleDbTypeConverter();

        private OracleDbTypeConverter() { }

        /// <summary>
        /// 将 DbType 转换为数据库中的列的类型名称。
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public override string ConvertToDatabaseTypeName(DbType fieldType, string length = null)
        {
            switch (fieldType)
            {
                case DbType.String:
                case DbType.AnsiString:
                    if (!string.IsNullOrEmpty(length))
                    {
                        if (CLOBTypeName.EqualsIgnoreCase(length) || "MAX".EqualsIgnoreCase(length))
                        {
                            return CLOBTypeName;
                        }
                        return "VARCHAR2(" + length + ')';
                    }
                    return "VARCHAR2(4000)";
                case DbType.Xml:
                    return "XMLTYPE";
                case DbType.Int32:
                    return "INTEGER";
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                    return "DATE";
                case DbType.Byte:
                case DbType.Single:
                case DbType.Int64:
                case DbType.Double:
                case DbType.Decimal:
                    return "NUMBER";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "CHAR(1)";
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
            switch (databaseTypeName.ToLower())
            {
                case "nvarchar2":
                case "varchar2":
                case "clob":
                    return DbType.String;
                case "xmltype":
                    return DbType.Xml;
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
                    return DbType.Date;
                default:
                    break;
            }
            throw new NotSupportedException($"不支持读取数据库中的列类型：{databaseTypeName}。");
        }

        /// <summary>
        /// 返回 CLR 类型默认映射的数据库的类型。
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public override DbType FromClrType(Type clrType)
        {
            if (clrType == typeof(DateTime)) { return DbType.Date; }

            var value = base.FromClrType(clrType);

            if (value == DbType.Boolean)
            {
                value = DbType.String;
            }

            return value;
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
                    //数据库使用 CHAR(1) 来存储 Boolean 类型数据。
                    value = (bool)value ? "1" : "0";
                }
                else if (value.GetType().IsEnum)
                {
                    value = TypeHelper.CoerceValue(typeof(int), value);
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

            if (dbValue != null)
            {
                if (clrType == typeof(bool))
                {
                    dbValue = dbValue.ToString() == "1" ? true : false;
                }
            }
            else
            {
                if (clrType == typeof(string))
                {
                    dbValue = string.Empty;//null 转换为空字符串
                }
            }

            return dbValue;
        }
    }
}