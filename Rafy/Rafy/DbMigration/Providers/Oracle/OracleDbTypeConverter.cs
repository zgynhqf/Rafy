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

namespace Rafy.DbMigration.Oracle
{
    public class OracleDbTypeConverter : DbTypeConverter
    {
        private const string CLOBTypeName = "CLOB";

        public static readonly OracleDbTypeConverter Instance = new OracleDbTypeConverter();

        private OracleDbTypeConverter() { }

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
                case DbType.DateTime:
                case DbType.DateTime2:
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
                    return DbType.DateTime;
                default:
                    break;
            }
            throw new NotSupportedException($"不支持读取数据库中的列类型：{databaseTypeName}。");
        }

        public override DbType FromClrType(Type clrType)
        {
            var value = base.FromClrType(clrType);

            if (value == DbType.Boolean)
            {
                value = DbType.String;
            }

            return value;
        }

        public string ToDbBoolean(bool value)
        {
            //数据库使用 CHAR(1) 来存储 Boolean 类型数据。
            return value ? "1" : "0";
        }

        public bool ToCLRBoolean(object value)
        {
            return value.ToString() == "1" ? true : false;
        }
    }
}