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

namespace Rafy.DbMigration.MongoDb
{
    /// <summary>
    /// SqlServer 数据库字段类型的转换器。
    /// </summary>
    internal class MongoDbDbTypeConverter : DbTypeConverter
    {
        public static MongoDbDbTypeConverter Instance = new MongoDbDbTypeConverter();

        protected MongoDbDbTypeConverter() { }

        public override string ConvertToDatabaseTypeName(DbType fieldType, string length = null)
        {
            //数据库类型是 BonsValue 中的类型，不需要与字符串名称进行转换。
            throw new NotImplementedException();
        }

        public override DbType ConvertToDbType(string databaseTypeName)
        {
            throw new NotImplementedException();
        }
    }
}
