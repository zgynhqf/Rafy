/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 01:17
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 关系数据库使用的数据保存器。
    /// </summary>
    public class RdbDataSaver : DataSaver
    {
        private RdbDataProvider _dataProvider;

        protected internal override void Init(RepositoryDataProvider dataProvider)
        {
            base.Init(dataProvider);

            _dataProvider = dataProvider as RdbDataProvider;
        }

        internal protected override void SubmitComposition(IDomainComponent component)
        {
            //以下事务代码，不需要区分是否使用分布式缓存的情况来做事务处理，
            //而是直接使用 SingleConnectionTransactionScope 类来管理不同数据库的事务，
            //因为这个类会保证不同的库使用不同的事务。
            using (var tran = new SingleConnectionTransactionScope(_dataProvider.DbSetting))
            {
                base.SubmitComposition(component);

                //最后提交事务。前面的代码，如果出现异常，则会回滚整个事务。
                tran.Complete();
            }
        }

        /// <summary>
        /// 创建一个关系数据库的冗余属性更新器。
        /// </summary>
        /// <returns></returns>
        public override RedundanciesUpdater CreateRedundanciesUpdater()
        {
            return new RdbRedundanciesUpdater(_dataProvider);
        }

        /// <summary>
        /// 插入这个实体到仓库中。
        /// 
        /// 子类重写此方法来实现非关系型数据库的插入逻辑。
        /// 重写时，注意：
        /// 在插入完成后，把为实体新生成的 Id 赋值到实体中。否则组合子将插入失败。
        /// </summary>
        /// <param name="data"></param>
        public override void InsertToPersistence(Entity data)
        {
            using (var dba = _dataProvider.CreateDbAccesser())
            {
                var table = _dataProvider.DbTable;

                table.Insert(dba, data);

                //放到 Insert 语句之后，否则 Id 不会有值。
                table.NotifyLoaded(data);
            }
        }

        /// <summary>
        /// 更新这个实体到仓库中。
        /// 
        /// 子类重写此方法来实现非关系型数据库的更新逻辑。
        /// </summary>
        /// <param name="data"></param>
        public override void UpdateToPersistence(Entity data)
        {
            using (var dba = _dataProvider.CreateDbAccesser())
            {
                var table = _dataProvider.DbTable;
                table.Update(dba, data);
                table.NotifyLoaded(data);
            }
        }

        /// <summary>
        /// 从仓库中删除这个实体。
        /// 
        /// 子类重写此方法来实现非关系型数据库的删除逻辑。
        /// </summary>
        /// <param name="data"></param>
        public override void DeleteFromPersistence(Entity data)
        {
            using (var dba = _dataProvider.CreateDbAccesser())
            {
                _dataProvider.DbTable.Delete(dba, data);
            }
        }

        /// <summary>
        /// 实现删除关联数据的逻辑。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="refProperty"></param>
        protected override void DeleteRefCore(Entity entity, IRefProperty refProperty)
        {
            var f = QueryFactory.Instance;
            var table = f.Table(_dataProvider.Repository);
            var where = f.Constraint(table.Column(refProperty.RefIdProperty), entity.Id);

            using (var dba = _dataProvider.CreateDbAccesser())
            {
                _dataProvider.DbTable.Delete(dba, where);
            }
        }
    }
}
