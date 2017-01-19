/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150926
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150926 18:47
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.DbMigration.Operations
{
    /// <summary>
    /// 更新数据库中表或列的注释的操作。
    /// </summary>
    public class UpdateComment : MigrationOperation
    {
        /// <summary>
        /// 要添加注释的表的名字
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 要添加注释的表的字段的名字。
        /// 如果本字段为空，则表示给表加注释，而不是给字段加注释。
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 修改字段注释信息的字段类型
        /// </summary>
        public DbType ColumnDataType { get; set; }

        /// <summary>
        /// 注释内容
        /// </summary>
        public string Comment { get; set; }

        protected override void Down()
        {
            this.AddOperation(new UpdateComment
            {
                TableName = this.TableName,
                ColumnName = this.ColumnName,
                ColumnDataType = this.ColumnDataType,
                Comment = string.Empty
            });
        }
    }
}
