/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20250815
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20250815 20:43
 * 
*******************************************************/

using Rafy.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace Rafy.DbMigration.PgSql
{
    internal class PgSqlDbTypeConverter : DbTypeConverter
    {
        public static readonly PgSqlDbTypeConverter Instance = new PgSqlDbTypeConverter();

        private PgSqlDbTypeConverter() { }

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
                    return "TIMESTAMP";
                case DbType.Single:
                    return "FLOAT4";
                case DbType.Double:
                    return "FLOAT8";
                case DbType.Decimal:
                    if (!string.IsNullOrWhiteSpace(length))
                    {
                        return "DECIMAL(" + length + ")";
                    }
                    return "DECIMAL(18,2)";
                case DbType.Binary:
                    return "BLOB";
                case DbType.Boolean:
                    return "BOOL";
                default:
                    break;
            }
            throw new NotSupportedException(string.Format("不支持生成列类型：{0}。", fieldType));
        }

        public override DbType ConvertToDbType(string databaseTypeName)
        {
            throw new NotImplementedException();
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
                if (value.GetType().IsEnum)
                {
                    value = TypeHelper.CoerceValue(typeof(int), value);
                }
            }

            return value;
        }
    }
}
