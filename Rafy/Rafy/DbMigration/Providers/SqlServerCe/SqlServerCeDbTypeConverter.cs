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
        public static readonly new SqlServerCeDbTypeConverter Instance = new SqlServerCeDbTypeConverter();

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
                default:
                    return base.ConvertToDatabaseTypeName(fieldType, length);
            }
        }
    }
}
