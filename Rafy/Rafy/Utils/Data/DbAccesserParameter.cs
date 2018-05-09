/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20171111
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20171111 12:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// DbAccesser 可使用的 Parameter（可跨库）。
    /// 
    /// <para></para>
    /// 在使用 DbAccesser 查询时，如果只传入参数的值，那么 DbDataParameter 中是没有设置 DbType 的，这会造成索引无效。
    /// 这时，在传入参数时，应该使用本类型将值进行封装，并同时将 DbType 传入。
    /// （
    /// 详情见：https://www.cnblogs.com/OpenCoder/p/4561532.html。
    /// 原因：
    /// 经过查阅资料和自己的分析后，得知原来是字符类型的不匹配造成的。
    /// 因为这个数据表是客户数据库中的，我只是提取数据，而我一般建数据表都使用NVarchar类型,而客户的这个表使用的是Char(32)，
    /// 在查询分析器中直接写字符串作为查询条件时，查询优化器认为条件中等号两边的字符类型是相同的，从而会选择聚集索引查询，
    /// 而在ADO.NET中使用SqlParameter后，因为字段类型错误导致了Sql查询从索引扫描变成了表扫描...
    /// 所以为了避免在Sql查询中由于条件字段类型不匹配而导致表扫描，记得以后在创建SqlParameter时声明SqlDbType就解决问题了。
    /// ）
    /// </summary>
    public class DbAccesserParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbAccesserParameter"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="dbType">Type of the database.</param>
        public DbAccesserParameter(object value, DbType dbType)
        {
            this.DbType = dbType;
            this.Value = value;
        }

        /// <summary>
        /// 所对应的数据库中的类型。
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// 值。
        /// </summary>
        public object Value { get; set; }
    }
}
