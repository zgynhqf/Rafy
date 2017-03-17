using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.DictionaryAdapter;
using Castle.Core.Internal;
using Rafy.Domain;
using Rafy.Domain.Stamp;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.DataTableMigration
{
    /// <summary>
    /// 数据归档服务的默认实现。
    /// </summary>
    public class DefaultDataTableMigrationService : IDataTableMigrationService
    {
        private readonly DataTableMigrationContext _context;
        private Dictionary<string, ReferenceInfo> _archivingEntity = new Dictionary<string, ReferenceInfo>();
        private Dictionary<int, List<Entity>> _needArchivingData = new Dictionary<int, List<Entity>>();
        private List<ArchivingDataCahceInfo> _archiveDataCache = new EditableList<ArchivingDataCahceInfo>();

        private static readonly object _syncRoot = new object();

        /// <summary>
        /// 初始化 <see cref="DefaultDataTableMigrationService"/> 类的新实例。
        /// </summary>
        /// <param name="context">表示数据归档的上下文。</param>
        public DefaultDataTableMigrationService(DataTableMigrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this._context = context;
        }

        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        public void Report(DataTableMigrationEventArgs value)
        {
            var handler = this.ProgressChanged;
            
            handler?.Invoke(this, value);
        }

        /// <summary>
        /// 当数据归档状态发生变化时引发此事件。
        /// </summary>
        public event EventHandler<DataTableMigrationEventArgs> ProgressChanged;

        /// <summary>
        /// 执行数据归档。
        /// </summary>
        /// <param name="entityTypes">表示要迁移的实体集合。</param>
        public void ExecuteArchivingData(List<Type> entityTypes)
        {
            if (!entityTypes.Any())
            {
                throw new ArgumentException($"{nameof(entityTypes)}至少需要包含一个类型.");
            }

            this.ConvertToDictionary(entityTypes);
            this.AnalysisEntityMeta(entityTypes);

            this._archivingEntity = this._archivingEntity.OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            this.Report(new DataTableMigrationEventArgs("实体按引用次数排序完成。") {NeedNewLine = true});
            this.Report(new DataTableMigrationEventArgs($"-------------- 引用关系结构图： --------------\n{this}") {NeedNewLine = true});

            var zeroReferenceEntities = this._archivingEntity.Where(kv => kv.Value.ReferenceTimes == 0).Select(kv => kv.Key).ToList();
            foreach(var entityType in zeroReferenceEntities)
            {
                this.StartArchivingTask(entityType);
            }
        }

        public void SaveToHistory(IDomainComponent component)
        {
            throw new NotImplementedException();
        }

        private void StartArchivingTask(string entityTypeFullName)
        {
            ReferenceInfo referenceInfo;
            if(!this._archivingEntity.TryGetValue(entityTypeFullName, out referenceInfo))
            {
                return;
            }

            var condition = new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(EntityStampExtension.CreatedTimeProperty, PropertyOperator.LessEqual, this._context.DateOfArchiving)
            };
            var totalCount = referenceInfo.Repository.CountBy(condition);

            this.Report(new DataTableMigrationEventArgs($"需要归档总数据量：{totalCount}") { NeedNewLine = true });

            var pagingInfo = new PagingInfo(1, this._context.PageSize);
            this.ArchivingDataCore(pagingInfo, referenceInfo.Repository);
        }

        /// <summary>
        /// 数据归档。
        /// </summary>
        /// <param name="pagingInfo">表示一个批次的分页信息。</param>
        /// <param name="repository">当前对象对应的仓库。</param>
        private void ArchivingDataCore(PagingInfo pagingInfo, IRepository repository)
        {
            var condition = new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(EntityStampExtension.CreatedTimeProperty, PropertyOperator.LessEqual, this._context.DateOfArchiving)
            };
            condition.PagingInfo = pagingInfo;
            var step = 1;

            var entityList = repository.GetBy(condition);

            while (entityList != null && entityList.Count > 0)
            {
                this.Report(new DataTableMigrationEventArgs($"批次 {step} 总数量", entityList.Count) { NeedNewLine = true });

                //
                // TODO: {entityList} 存储到历史表里面
                //
                // this.SaveToHistory(entityList);

                //this._needArchivingData.Clear();
                this._archiveDataCache.Clear();

                foreach(var entity in entityList)
                {
                    entity.PersistenceStatus = PersistenceStatus.Deleted;

                    this.ArchivingReferenceData(entity, repository.EntityMeta);

                    this.Report(new DataTableMigrationEventArgs($"Archiving Completed. EntityId: {entity.Id}", step));
                    ++step;
                }

                try
                {
                    this._archiveDataCache = this._archiveDataCache.OrderByDescending(c => c.Level).ToList();
                    foreach(var archivingDataCahceInfo in this._archiveDataCache)
                    {
                        archivingDataCahceInfo.EntityList.ForEach(e=>e.PersistenceStatus = PersistenceStatus.Deleted);
                        archivingDataCahceInfo.Repository.Save(archivingDataCahceInfo.EntityList);
                    }
                    //
                    // TODO: 批量删除原始表里面的数据
                     repository.Save(entityList);
                    //
                }
                catch(Exception exception)
                {
                    throw new DataTableMigrationException("删除原始数据失败。", exception);
                }

                this.Report(new DataTableMigrationEventArgs($"-------------- 批次 {pagingInfo.PageNumber} 归档完成 --------------"));
            }
        }

        /// <summary>
        /// 归档当前实体所引用属性中需要归档的数据。
        /// </summary>
        /// <param name="entity">需要归档数据的实体，用于查询外键的值。</param>
        /// <param name="refTypeMeta">当前实体需要归档的引用属性的元数据信息。</param>
        /// <param name="level">表示递归尝试。</param>
        private void ArchivingReferenceData(Entity entity, EntityMeta refTypeMeta, int level = 1)
        {
            var properties = this.FindReferenceManagedProperty(refTypeMeta);
            foreach(var property in properties)
            {
                var foreignKeyValue = entity.GetRefId(((IRefEntityProperty)property.ManagedProperty).RefIdProperty);

                if(foreignKeyValue == null || foreignKeyValue.ToString() == "0")
                {
                    continue;
                }
#if DEBUG
                Console.WriteLine($"\tForeignKey: {foreignKeyValue}, Entity: {property.Name}");
#endif
                this.RecursiveQueryReferenceProperties(foreignKeyValue, property, level);
            }
        }

        /// <summary>
        /// 通过外键递归查找引用实体的数据并执行数据归档操作。
        /// </summary>
        /// <param name="forginKeyValue">外键的值。</param>
        /// <param name="propertyMetaData">外键属性元数据。</param>
        /// <param name="level">表示递归深度。</param>
        private void RecursiveQueryReferenceProperties(object forginKeyValue, EntityPropertyMeta propertyMetaData, int level)
        {
            ReferenceInfo referenceInfo;
            if (this._archivingEntity.TryGetValue(propertyMetaData.ReferenceInfo.RefType.FullName, out referenceInfo))
            {
                var entity = referenceInfo.Repository.GetById(forginKeyValue);
                if (entity == null)
                {
                    return;
                }

                //
                // TODO:  存储到历史表里面
                //
                // this.SaveToHistory(entity);

                entity.PersistenceStatus = PersistenceStatus.Deleted;
                
                var newLevel = level + 1;
                this.ArchivingReferenceData(entity, propertyMetaData.ReferenceInfo.RefTypeMeta, newLevel);


                var key = referenceInfo.Repository.GetType().FullName + "_" + level;
                var cacheItem = this._archiveDataCache.FirstOrDefault(c => c.Key == key);
                if (cacheItem == null)
                {
                    cacheItem = new ArchivingDataCahceInfo(level, referenceInfo.Repository);
                    cacheItem.EntityList.Add(entity);
                    this._archiveDataCache.Add(cacheItem);
                }
                else
                {
                    cacheItem.EntityList.Add(entity);
                }

                //this.CacheNeedArchivingData(level, entity);

                try
                {
                    // TODO: 删除原始表里面的数据
                    // referenceInfo.Repository.Save(entity);
                }
                catch (Exception exception)
                {
                    throw new DataTableMigrationException("删除原始数据失败。", exception);
                }
            }
        }

        private void CacheNeedArchivingData(int level, Entity entity)
        {
            List<Entity> entities;
            if (this._needArchivingData.TryGetValue(level, out entities))
            {
                entities.Add(entity);
            }
            else
            {
                this._needArchivingData.Add(level, new List<Entity> {entity});
            }
        }

        /// <summary>
        /// 查找当前实体包含的引用属性。
        /// </summary>
        /// <param name="metaData">当前实体的元数据信息。</param>
        /// <returns>当前实体包含的引用属性的可枚举集合。</returns>
        private IEnumerable<EntityPropertyMeta> FindReferenceManagedProperty(EntityMeta metaData)
        {
            var properties = new List<EntityPropertyMeta>();

            foreach (var property in metaData.EntityProperties)
            {
                var referenceProperty = property.ManagedProperty as IRefEntityProperty;
                if (referenceProperty == null)
                {
                    continue;
                }

                if (!this._archivingEntity.ContainsKey(property.ReferenceInfo.RefType.FullName))
                {
                    continue;
                }

                properties.Add(property);
            }

            return properties;
        }

        /// <summary>
        /// 分析要归档数据的实体的元数据。
        /// </summary>
        /// <param name="entityTypes">表示要归档的实体集合。</param>
        private void AnalysisEntityMeta(IEnumerable<Type> entityTypes)
        {
            foreach(var entityType in entityTypes)
            {
                var repository = RepositoryFactory.Find(entityType);
                if (!this._archivingEntity.ContainsKey(entityType.FullName))
                {
                    lock (_syncRoot)
                    {
                        this._archivingEntity.Add(entityType.FullName, new ReferenceInfo
                        {
                            Repository = repository
                        });
                    }
                }
                else
                {
                    this._archivingEntity[entityType.FullName].Repository = repository;
                }

                this.ScanRelationByEntityMeta(repository.EntityMeta);

                this.Report(new DataTableMigrationEventArgs($"Entity: {repository.EntityMeta.Name} scan completed."));
            }

            this.Report(new DataTableMigrationEventArgs("------------ Scan all entity completed. ------------") {NeedNewLine = true});
        }

        /// <summary>
        /// 扫描实体元数据信息，更新实体被引用次数。
        /// </summary>
        /// <param name="metaData">表示一个实体的元数据。</param>
        private void ScanRelationByEntityMeta(EntityMeta metaData)
        {
            foreach(var property in metaData.EntityProperties)
            {
                if(property.ReferenceInfo == null)
                {
                    continue;
                }

                var refTypeFullName = property.ReferenceInfo.RefType.FullName;
                ReferenceInfo referenceInfo;
                if(this._archivingEntity.TryGetValue(refTypeFullName, out referenceInfo))
                {
                    referenceInfo.ReferenceTimes = referenceInfo.ReferenceTimes + 1;
                    referenceInfo.AddReferenceType(metaData.EntityType);

                    this.ScanRelationByEntityMeta(property.ReferenceInfo.RefTypeMeta);
                }
            }
        }

        /// <summary>
        /// 将要归档数据的实体集合转换为字典进行存储。
        /// </summary>
        /// <param name="entityTypes">表示要归档数据的实体集合。</param>
        private void ConvertToDictionary(List<Type> entityTypes)
        {
            entityTypes.ForEach(t => {
                lock (_syncRoot)
                {
                    if (this._archivingEntity.ContainsKey(t.FullName))
                    {
                        return;
                    }

                    this._archivingEntity.Add(t.FullName, new ReferenceInfo());
                }
            });
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var key in this._archivingEntity.Keys)
            {
                var referenceInfo = this._archivingEntity[key];
                builder.AppendLine($"(Entity: {key},  ReferenceTimes: {referenceInfo.ReferenceTimes}) \t<---{string.Join("-",referenceInfo.ReferencePropertyTypes.Select(t=>t.Name))}");
            }

            return builder.ToString();
        }
    }

    internal class ArchivingDataCahceInfo
    {
        public ArchivingDataCahceInfo(int level, IRepository repository)
        {
            this.Level = level;
            this.Repository = repository;
            this.EntityList = this.Repository.NewList();
            this.Key = this.Repository.GetType().FullName + "_" + level;
        }

        public string Key { get; set; }

        public int Level { get; set; }

        public IRepository Repository { get; set; }

        public EntityList EntityList { get; set; }
    }
}
