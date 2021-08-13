/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170104 10:36
 * 
*******************************************************/

using Rafy.DbMigration.MySql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.MySql
{
    /// <summary>
    /// MySql列的实例对象
    /// </summary>
    internal sealed class MySqlColumn : RdbColumn
    {
        /// <summary>
        /// 构造函数 初始化表和 持久列信息
        /// </summary>
        /// <param name="table">表对象</param>
        /// <param name="columnInfo">持久列对象</param>
        public MySqlColumn(RdbTable table, IRdbColumnInfo columnInfo) : base(table, columnInfo) { }
    }
}