/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210819
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210819 00:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// Rafy ORM 中可以进行的全局性配置。
    /// </summary>
    public abstract class ORMSettings
    {
        /// <summary>
        /// 如果在查询时，Sql 中没有给出实体需要映射的列时，是否抛出异常？
        /// 默认为 true。这是因为目前 Update 只支持全字段更新，所以这里必须查询部分的列。否则会造成数据丢失。
        /// </summary>
        public static bool ErrorIfColumnNotFoundInSql { get; set; } = true;
    }
}