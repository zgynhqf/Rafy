/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140507
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140507 15:05
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 数据迁移的一些默认配置。
    /// </summary>
    public static class DbMigrationSettings
    {
        /// <summary>
        /// 可设置所有主键及外键的长度。默认为 40。
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/2863993/is-of-a-type-that-is-invalid-for-use-as-a-key-column-in-an-index
        /// SqlServer 主键最大 450、Oracle 主键最大 400。
        /// </remarks>
        public static string PKFKDataTypeLength = "40";

        /// <summary>
        /// 可设置所有一般字符串字段的默认长度。默认为 2000。
        /// </summary>
        public static string StringColumnDataTypeLength = "2000";
    }
}