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
    class PersistanceTableInfo : IPersistanceTableInfo
    {
        private IRepositoryInternal _repo;

        public PersistanceTableInfo(IRepositoryInternal repo)
        {
            _repo = repo;
            this.Class = repo.EntityType;
            this.Name = repo.EntityMeta.TableMeta.TableName;
            this.Columns = new List<PersistanceColumnInfo>();
        }

        /// <summary>
        /// 对应的实体类型
        /// </summary>
        public Type Class { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 主键列（每个表肯定有一个主键列）
        /// </summary>
        public PersistanceColumnInfo PKColumn { get; set; }

        /// <summary>
        /// 所有的列
        /// </summary>
        public List<PersistanceColumnInfo> Columns { get; private set; }

        IReadOnlyList<IPersistanceColumnInfo> IPersistanceTableInfo.Columns
        {
            get { return this.Columns; }
        }

        IPersistanceColumnInfo IPersistanceTableInfo.PKColumn
        {
            get { return this.PKColumn; }
        }
    }
}
