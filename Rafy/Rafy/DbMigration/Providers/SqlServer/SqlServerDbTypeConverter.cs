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

namespace Rafy.DbMigration.SqlServer
{
    /// <summary>
    /// SqlServer 数据库字段类型的转换器。
    /// </summary>
    public class SqlServerDbTypeConverter : DbTypeConverter
    {
        public static readonly SqlServerDbTypeConverter Instance = new SqlServerDbTypeConverter();

        protected SqlServerDbTypeConverter() { }

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
                    if (string.IsNullOrEmpty(length)) { length = "MAX"; }
                    return "NVARCHAR(" + length + ")";
                case DbType.Int32:
                    return "INT";
                case DbType.Int64:
                    return "BIGINT";
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTime:
                case DbType.DateTimeOffset:
                    return "DATETIME";
                case DbType.Guid:
                    return "UNIQUEIDENTIFIER";
                case DbType.Double:
                case DbType.Single:
                    return "FLOAT";
                case DbType.Decimal:
                    if (!string.IsNullOrWhiteSpace(length))
                    {
                        return "DECIMAL(" + length + ")";
                    }
                    return "DECIMAL(18,2)";
                case DbType.Binary:
                    return "VARBINARY(MAX)";
                case DbType.Boolean:
                    return "BIT";
                case DbType.Byte:
                    return "TINYINT";
                case DbType.Xml:
                    return "XML";
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
                case "uniqueidentifier":
                    return DbType.Guid;
                case "nvarchar":
                case "varchar":
                case "nchar":
                case "char":
                case "ntext":
                case "text":
                    return DbType.String;
                case "xml":
                    return DbType.Xml;
                case "smallint":
                case "int":
                    return DbType.Int32;
                case "bigint":
                    return DbType.Int64;
                case "bit":
                    return DbType.Boolean;
                case "float":
                case "real":
                    return DbType.Double;
                case "numeric":
                case "decimal":
                case "money":
                    return DbType.Decimal;
                case "binary":
                case "varbinary":
                case "image":
                    return DbType.Binary;
                case "tinyint":
                    return DbType.Byte;
                case "date":
                case "datetime":
                case "datetimeoffset":
                case "time":
                    return DbType.DateTime;
                default:
                    throw new NotSupportedException($"不支持读取数据库中的列类型：{databaseTypeName}。");
            }
        }
    }
}
