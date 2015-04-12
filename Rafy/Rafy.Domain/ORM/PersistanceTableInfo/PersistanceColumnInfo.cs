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

namespace Rafy.Domain.ORM
{
    class PersistanceColumnInfo : IPersistanceColumnInfo
    {
        public PersistanceTableInfo Table { get; set; }

        public string Name { get; set; }

        public Type DataType { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsPrimaryKey { get; set; }

        public IProperty Property { get; set; }

        IPersistanceTableInfo IPersistanceColumnInfo.Table
        {
            get { return this.Table; }
        }
    }
}
