/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170921 23:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration.SqlServerCe
{
    /// <summary>
    /// SqlServerCe 数据库字段类型的转换器。
    /// </summary>
    public class SqlServerCeDbTypeConverter : SqlServer.SqlServerDbTypeConverter
    {
        public static new SqlServerCeDbTypeConverter Instance = new SqlServerCeDbTypeConverter();

        private SqlServerCeDbTypeConverter() { }

        /// <summary>
        /// 将 DbType 转换为数据库中的列的类型名称。
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override string ConvertToDatabaseTypeName(DbType fieldType, string length = null)
        {
            switch (fieldType)
            {
                case DbType.String:
                case DbType.AnsiString:
                case DbType.Xml:
                    if (!string.IsNullOrEmpty(length) && !length.EqualsIgnoreCase("MAX"))
                    {
                        return "NVARCHAR(" + length + ')';
                    }
                    return "NVARCHAR(4000)";
                case DbType.Binary:
                    return "VARBINARY(8000)";
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return "DATETIME";
                default:
                    return base.ConvertToDatabaseTypeName(fieldType, length);
            }
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
                case "datetimeoffset":
                case "datetime2":
                case "time":
                    return DbType.DateTime;
                default:
                    return base.ConvertToDbType(databaseTypeName);
            }
        }

        /// <summary>
        /// 返回 CLR 类型默认映射的数据库的类型。
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public override DbType FromClrType(Type clrType)
        {
            if (clrType == typeof(DateTimeOffset)) { return DbType.DateTime; }

            var value = base.FromClrType(clrType);

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
                if (value is DateTimeOffset)
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

            if (dbValue != null)
            {
                if (clrType == typeof(DateTimeOffset))
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
            }

            return dbValue;
        }
    }
}
