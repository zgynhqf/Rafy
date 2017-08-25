/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170824
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170824 17:48
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// SqlServer 下 Sql 语句生成的一些配置项。
    /// </summary>
    public static class SqlServerSqlGeneratorConfiguration
    {
        /// <summary>
        /// 分页时默认使用的排序的字段。
        /// 在分页时，Sql 中必须有指定的排序语句。所以如果给定的 Sql 中没有 OrderBy 语句，则使用任意一个表的这个字段来进行分页。
        /// </summary>
        public static string DefaultPagingSqlOrderbyColumn = EntityConvention.IdColumnName;
    }
}
