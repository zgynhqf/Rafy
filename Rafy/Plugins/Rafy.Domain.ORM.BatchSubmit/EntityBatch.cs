/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150815
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150815 19:02
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;

namespace Rafy.Domain.ORM.BatchSubmit
{
    /// <summary>
    /// 表示需要批量处理的某一个实体类型的所有对象实体。
    /// </summary>
    public class EntityBatch
    {
        private IDbAccesser _dba;
        private RdbTable _table;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBatch"/> class.
        /// </summary>
        public EntityBatch()
        {
            this.InsertBatch = new List<Entity>(100);
            this.UpdateBatch = new List<Entity>(100);
            this.DeleteBatch = new List<Entity>(100);
        }

        /// <summary>
        /// 实体的类型
        /// </summary>
        public Type EntityType;
        /// <summary>
        /// 实体对应的仓库。
        /// </summary>
        public EntityRepository Repository;
        /// <summary>
        /// 要批量插入的实体列表。
        /// </summary>
        public IList<Entity> InsertBatch;
        /// <summary>
        /// 要批量更新的实体列表。
        /// </summary>
        public IList<Entity> UpdateBatch;
        /// <summary>
        /// 要删除的实体列表。
        /// </summary>
        public IList<Entity> DeleteBatch;

        /// <summary>
        /// 数据访问组件。
        /// </summary>
        public IDbAccesser DBA
        {
            get
            {
                if (_dba == null)
                {
                    var dp = RdbDataProvider.Get(this.Repository);
                    _dba = dp.CreateDbAccesser();
                }
                return _dba;
            }
        }

        internal RdbTable Table
        {
            get
            {
                if (_table == null)
                {
                    var dp = RdbDataProvider.Get(this.Repository);
                    _table = dp.DbTable;
                }
                return _table;
            }
        }
    }
}