/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150816
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150816 12:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Domain.Caching;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.BatchSubmit
{
    /// <summary>
    /// 批量导入基类。
    /// </summary>
    public abstract class BatchImporter : IBatchImporter
    {
        private IDomainComponent _entityOrList;

        private int _batchSize = int.MaxValue;

        internal SqlGenerator SqlGenerator;

        /// <summary>
        /// 每次导入时，一批最多同时导入多少条数据。
        /// </summary>
        public virtual int BatchSize
        {
            get { return _batchSize; }
            set { _batchSize = value; }
        }

        /// <summary>
        /// 是否在更新时，是否一并更新 LOB 属性。默认为 false。
        /// </summary>
        public bool UpdateLOB { get; set; }

        /// <summary>
        /// 批量导入指定的实体或列表。
        /// </summary>
        /// <param name="entityOrList"></param>
        public void Save(IDomainComponent entityOrList)
        {
            if (entityOrList == null) throw new ArgumentNullException("entityOrList");
            _entityOrList = entityOrList;

            //读取所有的要批处理的实体
            var reader = new EntityBatchReader(_entityOrList);
            var batchList = reader.Read();

            try
            {
                //反向先删除实体。从子表中的数据开始删除。
                for (int i = batchList.Count - 1; i >= 0; i--)
                {
                    var batch = batchList[i];
                    if (batch.DeleteBatch.Count > 0)
                    {
                        this.ImportDelete(batch);
                        this.MarkNew(batch.DeleteBatch);
                    }
                }

                //再插入、更新数据
                for (int i = 0, c = batchList.Count; i < c; i++)
                {
                    var batch = batchList[i];
                    if (batch.InsertBatch.Count > 0)
                    {
                        this.ImportInsert(batch);
                        this.MarkSaved(batch.InsertBatch);
                    }
                    if (batch.UpdateBatch.Count > 0)
                    {
                        this.ImportUpdate(batch);
                        this.MarkSaved(batch.UpdateBatch);
                    }
                }

                //更新缓存
                for (int i = 0, c = batchList.Count; i < c; i++)
                {
                    var batch = batchList[i];
                    if (VersionSyncMgr.IsEnabled)
                    {
                        VersionSyncMgr.Repository.UpdateVersion(batch.EntityType);
                    }

                    if (batch.UpdateBatch.Count > 0)
                    {
                        var redundancyUpdater = batch.Repository.DataProvider.DataSaver.CreateRedundanciesUpdater();
                        foreach (var property in batch.Repository.GetPropertiesInRedundancyPath())
                        {
                            foreach (var path in property.InRedundantPathes)
                            {
                                redundancyUpdater.RefreshRedundancy(path.Redundancy);
                            }
                        }
                    }
                }
            }
            finally
            {
                for (int i = 0, c = batchList.Count; i < c; i++)
                {
                    var batch = batchList[i];
                    batch.Dispose();
                }
            }
        }

        /// <summary>
        /// 子类重写此方法实现批量插入逻辑。
        /// </summary>
        /// <param name="batch"></param>
        protected virtual void ImportInsert(EntityBatch batch)
        {
            throw new NotSupportedException("目前不支持批量插入数据。");
        }

        /// <summary>
        /// 子类重写此方法实现批量更新逻辑。
        /// </summary>
        /// <param name="batch"></param>
        protected virtual void ImportUpdate(EntityBatch batch)
        {
            throw new NotSupportedException("目前不支持批量更新数据。");
        }

        /// <summary>
        /// 子类重写此方法实现批量删除逻辑。
        /// </summary>
        /// <param name="batch"></param>
        protected virtual void ImportDelete(EntityBatch batch)
        {
            foreach (var section in this.EnumerateAllBatches(batch.DeleteBatch, 1000))
            {
                var sqlDelete = new FormattedSql();
                if (batch.Repository.EntityMeta.IsPhantomEnabled)
                {
                    var isPhantomColumn = batch.Table.FindByPropertyName(EntityConvention.Property_IsPhantom.Name).Name;
                    var value = this.SqlGenerator.DbTypeCoverter.ToDbParameterValue(BooleanBoxes.True);

                    sqlDelete.Append("UPDATE ").Append(batch.Table.Name).Append(" SET ")
                        .Append(isPhantomColumn).Append(" = ").AppendParameter(value)
                        .Append(" WHERE ID IN (");
                }
                else
                {
                    sqlDelete.Append("DELETE FROM ").Append(batch.Table.Name).Append(" WHERE ID IN (");
                }

                bool needDelimiter = false;
                for (int i = 0, c = section.Count; i < c; i++)
                {
                    var item = section[i];
                    if (i > 0) { sqlDelete.Append(','); }
                    else
                    {
                        var keyType = (item as IEntity).IdProvider.KeyType;
                        needDelimiter = keyType.IsClass;
                    }

                    if (needDelimiter)
                    {
                        sqlDelete.Append('\'').Append(item.Id).Append('\'');
                    }
                    else
                    {
                        sqlDelete.Append(item.Id);
                    }
                }
                sqlDelete.Append(')');

                batch.DBA.ExecuteText(sqlDelete, sqlDelete.Parameters);
            }
        }

        /// <summary>
        /// 根据每批条数来切分所有的实体到不同的列表中。
        /// </summary>
        /// <param name="all"></param>
        /// <returns></returns>
        internal protected IEnumerable<IList<Entity>> EnumerateAllBatches(IList<Entity> all)
        {
            return EnumerateAllBatches(all, this.BatchSize);
        }

        internal IEnumerable<IList<Entity>> EnumerateAllBatches(IList<Entity> all, int batchSize)
        {
            var start = 0;
            while (start < all.Count)
            {
                var end = Math.Min(start + batchSize - 1, all.Count - 1);

                //拷贝子节的实体到指定的数组中。
                var section = new Entity[end - start + 1];
                for (int i = start, j = 0; i <= end; i++, j++)
                {
                    section[j] = all[i];
                }

                yield return section;

                start = end + 1;
            }
        }

        private void MarkNew(IList<Entity> list)
        {
            for (int j = 0, jc = list.Count; j < jc; j++)
            {
                list[j].PersistenceStatus = PersistenceStatus.New;
            }
        }

        private void MarkSaved(IList<Entity> list)
        {
            for (int j = 0, jc = list.Count; j < jc; j++)
            {
                list[j].MarkSaved();
            }
        }

        /// <summary>
        /// 生成 Update 语句。
        /// 注意，此方法不会更新 LOB 字段。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="updateLOB"></param>
        /// <param name="paramBegin"></param>
        /// <param name="updateColumns">需要更新的列。</param>
        /// <returns></returns>
        internal static string GenerateUpdateSQL(RdbTable table, bool updateLOB, char paramBegin, IReadOnlyList<RdbColumn> updateColumns)
        {
            //代码 90% 拷贝自： RdbTable.GenerateUpdateSQL() 方法。

            var sql = new StringWriter();
            sql.Write("UPDATE ");
            sql.AppendQuoteName(table);
            sql.Write(" SET ");

            bool comma = false;

            for (int i = 0, c = updateColumns.Count; i < c; i++)
            {
                if (comma) { sql.Write(','); }
                else { comma = true; }

                var column = updateColumns[i];
                sql.AppendQuote(table, column.Name).Write(" = ");
                sql.Write(paramBegin);
                sql.Write(column.Name);
            }

            sql.Write(" WHERE ");
            sql.AppendQuote(table, table.PKColumn.Name);
            sql.Write(" = ");
            sql.Write(paramBegin);
            sql.Write(table.PKColumn.Name);

            return sql.ToString();
        }

        internal static void ThrowInvalidPropertyException(Entity entity, IProperty property, int entityIndex)
        {
            throw new InvalidOperationException($"第 {entityIndex + 1} 个实体（{entity}，Id：{entity.Id}）的属性 {property.Name} 被禁用，无法批量保存。注意：要批量保存的所有实体，需要有完全一致的禁用属性列表。");
        }
    }
}
