/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Rafy.DataTableMigration.Contexts;
using Rafy.DataTableMigration.Exceptions;
using Rafy.DataTableMigration.Utils;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Stamp;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.DataTableMigration.Services
{
    /// <summary>
    /// 数据归档服务的默认实现。
    /// </summary>
    public class DataTableMigrationService : IDataTableMigrationService
    {
        private readonly DataTableMigrationContext _context;

        private readonly Dictionary<string, IRepository> _repositoriesCache = new Dictionary<string, IRepository>();

        /// <summary>
        /// 初始化 <see cref="DataTableMigrationService" /> 类的新实例。
        /// </summary>
        /// <param name="context">表示数据归档的上下文。</param>
        public DataTableMigrationService(DataTableMigrationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this._context = context;
        }

        /// <summary>Reports a progress update.</summary>
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
        public void ExecuteArchivingData()
        {
            var condition = new CommonQueryCriteria(BinaryOperator.And) {
                new PropertyMatch(EntityStampExtension.UpdatedTimeProperty, PropertyOperator.LessEqual, this._context.DateOfArchiving)
            };

            condition.PagingInfo = new PagingInfo(1, this._context.PageSize);

            var aggregationRootParameters = _context.ArchivingAggregationRootTypeList;
            this.Report(new DataTableMigrationEventArgs("-------------- 开始数据归档 --------------"));

            foreach (var parameter in aggregationRootParameters)
            {
                var currentProcess = 0M;
                var repository = this.FindRepository(parameter);
                var totalCount = repository.CountBy(new CommonQueryCriteria {
                    new PropertyMatch(EntityStampExtension.UpdatedTimeProperty, PropertyOperator.LessEqual, this._context.DateOfArchiving)
                });

                this.Report(new DataTableMigrationEventArgs($"共有 {totalCount} 个聚合根 {parameter.FullName} 需要归档。"));

                var eagerLoadOptions = new EagerLoadOptions();

                bool isHasChild = false;

                LoadChildListProperty(ref isHasChild, repository.EntityMeta, eagerLoadOptions);

                bool isSupportTree = repository.SupportTree;

                if (isSupportTree)
                {
                    eagerLoadOptions.LoadWithTreeChildren();
                }

                if (isHasChild||isSupportTree)
                {
                    condition.EagerLoad = eagerLoadOptions;
                }

                var entityList = repository.GetBy(condition);

                while (entityList != null && entityList.Count > 0)
                {
                    this.SaveToHistory(repository, entityList, isSupportTree);

                    //实体删除
                    this.RemoveOriginData(repository, entityList);

                    currentProcess = currentProcess + entityList.Count;
                    this.Report(new DataTableMigrationEventArgs($"\t处理进度", decimal.Round(currentProcess / totalCount * 100, 2)));

                    entityList = repository.GetBy(condition);
                }
            }

            this.Report(new DataTableMigrationEventArgs("-------------- 结束数据归档 --------------"));
        }

        /// <summary>
        /// 从原始表中移除聚合。
        /// </summary>
        /// <param name="repository">表示当前 <see cref="entityList" /> 对应的仓库。</param>
        /// <param name="entityList">表示一个领域对象的集合。</param>
        public void RemoveOriginData(IRepository repository, EntityList entityList)
        {
            entityList.EachNode(t =>
            {
                t.PersistenceStatus = PersistenceStatus.Deleted;
                t.TreeChildren?.Clear();
                return false;

            });
            try
            {
                repository.Save(entityList);
            }
            catch (Exception exception)
            {
                throw new DataTableMigrationException("删除原始表中数据失败，请查看内部异常信息。", exception);
            }
        }

        /// <summary>
        /// 保存实体归档
        /// </summary>
        /// <param name="repository">实体仓储</param>
        /// <param name="component">实体集合</param>
        /// <param name="isSupportTree">是否是树形实体</param>
        public void SaveToHistory(IRepository repository, EntityList component, bool isSupportTree)
        {
            if (isSupportTree)
            {
                var treeEntityList = TreeHelper.ConvertToList<Entity>(component);
                ChangeEntityPersistenceStatus(treeEntityList, PersistenceStatus.New);
                using (
                 RdbDataProvider.RedirectDbSetting(DataTableMigrationPlugin.DbSettingName,
                     DataTableMigrationPlugin.BackUpDbSettingName))
                {
                    foreach (var treeEntity in treeEntityList)
                    {
                        repository.Save(treeEntity);
                    }
                }
            }
            else
            {
                ChangeEntityPersistenceStatus(component, PersistenceStatus.New);
                using (
                    RdbDataProvider.RedirectDbSetting(DataTableMigrationPlugin.DbSettingName,
                        DataTableMigrationPlugin.BackUpDbSettingName))
                {
                    repository.Save(component);
                }
            }
        }

        /// <summary>
        /// 获取指定类型的仓储
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IRepository FindRepository(Type type)
        {
            if (_repositoriesCache.ContainsKey(type.FullName))
            {
                return _repositoriesCache[type.FullName];
            }

            var repository = RepositoryFacade.Find(type);

            _repositoriesCache[type.FullName] = repository;

            return repository;
        }

        /// <summary>
        /// 贪婪加载聚合子实体
        /// </summary>
        /// <param name="isHasChild"></param>
        /// <param name="entityMeta"></param>
        /// <param name="eagerLoadOptions"></param>
        private static void LoadChildListProperty(ref bool isHasChild, EntityMeta entityMeta, EagerLoadOptions eagerLoadOptions)
        {
            var childProperties = entityMeta.ChildrenProperties;
            if (childProperties != null && childProperties.Count > 0)
            {
                foreach (var childPropertyMeta in childProperties)
                {
                    var listProperty = childPropertyMeta.ManagedProperty as IListProperty;
                    if (listProperty != null)
                    {
                        if (!isHasChild)
                        {
                            isHasChild = true;
                        }
                        eagerLoadOptions.LoadWith(listProperty);
                    }
                    LoadChildListProperty(ref isHasChild, childPropertyMeta.ChildType, eagerLoadOptions);
                }
            }
        }

        /// <summary>
        /// 更改实体的状态
        /// </summary>
        /// <param name="entityList"></param>
        /// <param name="persistenceStatus"></param>
        private void ChangeEntityPersistenceStatus(IList<Entity> entityList, PersistenceStatus persistenceStatus)
        {
            entityList.ForEach(item => item.PersistenceStatus = persistenceStatus);

            foreach (var entity in entityList)
            {
                foreach (var childField in entity.GetLoadedChildren())
                {
                    var children = childField.Value as EntityList;
                    if (children != null && children.Count > 0)
                    {
                        ChangeEntityPersistenceStatus(children, persistenceStatus);
                    }
                }

            }
        }
    }
}