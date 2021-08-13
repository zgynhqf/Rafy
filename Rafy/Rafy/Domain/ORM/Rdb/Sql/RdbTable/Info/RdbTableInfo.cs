/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150313
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150313 17:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    class RdbTableInfo : IRdbTableInfo
    {
        public RdbTableInfo(string name, Type entityType)
        {
            this.Name = name;
            this.EntityType = entityType;
            this.Columns = new List<RdbColumnInfo>();
        }

        /// <summary>
        /// 对应的实体类型
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 主键列（每个表肯定有一个主键列）
        /// </summary>
        public RdbColumnInfo PKColumn { get; set; }

        /// <summary>
        /// 所有的列
        /// </summary>
        public List<RdbColumnInfo> Columns { get; private set; }

        IReadOnlyList<IRdbColumnInfo> IRdbTableInfo.Columns
        {
            get { return this.Columns; }
        }

        IRdbColumnInfo IRdbTableInfo.PKColumn
        {
            get { return this.PKColumn; }
        }
    }
}
