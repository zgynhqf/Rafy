/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:44
 * 2.0 胡庆访 20171109 1655 
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Stamp;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.DataArchiver
{
    /// <summary>
    /// 聚合实体的归档器。
    /// </summary>
    public class AggregationArchiver
    {
        /// <summary>
        /// 需要迁移的数据所在的库的配置名。
        /// </summary>
        private string _orignalDataDbSettingName;

        /// <summary>
        /// 需要把数据迁移的目标数据库的配置名。
        /// </summary>
        private string _backUpDbSettingName;

        /// <summary>
        /// 执行数据归档。
        /// </summary>
        /// <param name="context">表示数据归档的上下文。</param>
        public void Archive(AggregationArchiveContext context)
        {
            if (context == null) { throw new ArgumentNullException(nameof(context)); }
            _orignalDataDbSettingName = context.OrignalDataDbSettingName;
            _backUpDbSettingName = context.BackUpDbSettingName;

            var updatedTimeCondition = new PropertyMatch(EntityStampExtension.UpdatedTimeProperty, PropertyOperator.LessEqual, context.DateOfArchiving);

            bool needProgress = this.ProgressChanged != null;
            if (needProgress) this.OnProgressChanged("-------------- 开始数据归档 --------------");

            using (StampContext.DisableAutoSetStamps())
            {
                foreach (var item in context.ItemsToArchive)
                {
                    var aggtRootType = item.AggregationRoot;
                    var archiveType = item.ArchiveType;
                    if (archiveType == ArchiveType.Copy) throw new NotSupportedException("Copy 操作暂不支持。");

                    var repository = RepositoryFacade.Find(aggtRootType);

                    #region 查询本次需要归档的总数据行数。

                    long totalCount = 0;
                    if (needProgress)
                    {
                        totalCount = repository.CountBy(new CommonQueryCriteria { updatedTimeCondition });
                        this.OnProgressChanged($"目前，共有 {totalCount} 个聚合根 {aggtRootType.FullName} 需要归档。");
                    }

                    #endregion

                    #region 构造一个查完整聚合的条件对象 CommonQueryCriteria

                    //设置 EagerLoadOptions，加载整个聚合。
                    var criteria = new CommonQueryCriteria() { updatedTimeCondition };
                    criteria.PagingInfo = new PagingInfo(1, context.BatchSize);
                    var eagerLoadOptions = new EagerLoadOptions();
                    EagerLoadAggregationRecur(eagerLoadOptions, repository.EntityMeta);
                    if (repository.SupportTree)
                    {
                        eagerLoadOptions.LoadWithTreeChildren();
                    }
                    criteria.EagerLoad = eagerLoadOptions;

                    #endregion

                    //逐页迁移历史数据表。
                    var currentProcess = 0;
                    while (true)
                    {
                        //获取最新的一页的数据。
                        var entitiesToMigrate = repository.GetBy(criteria);
                        var count = entitiesToMigrate.Count;
                        if (count == 0) { break; }

                        using (var tranOriginal = RF.TransactionScope(_orignalDataDbSettingName))
                        using (var tranBackup = RF.TransactionScope(_backUpDbSettingName))
                        {
                            //迁移到历史表。
                            this.BackupToHistory(repository, entitiesToMigrate);

                            //实体删除
                            this.DeleteOriginalData(repository, entitiesToMigrate);

                            //备份完成，才能同时提交两个库的事务。
                            tranOriginal.Complete();
                            tranBackup.Complete();
                        }

                        currentProcess += count;

                        if (needProgress) this.OnProgressChanged($"    处理进度", decimal.Round(currentProcess / totalCount * 100, 2));
                    }
                }
            }

            if (needProgress) this.OnProgressChanged("-------------- 结束数据归档 --------------");
        }

        /// <summary>
        /// 保存实体归档
        /// </summary>
        /// <param name="repository">实体仓储</param>
        /// <param name="entitiesToMigrate">实体集合</param>
        private void BackupToHistory(IRepository repository, EntityList entitiesToMigrate)
        {
            var options = CloneOptions.NewComposition();
            options.Actions |= CloneActions.IdProperty;//Id 也需要拷贝。
            options.Method = CloneValueMethod.LoadProperty;

            var newList = repository.NewList();
            newList.Clone(entitiesToMigrate, options);

            using (RdbDataProvider.RedirectDbSetting(_orignalDataDbSettingName, _backUpDbSettingName))
            {
                this.SaveList(repository, newList);
            }
        }

        /// <summary>
        /// 从原始表中移除聚合。
        /// </summary>
        /// <param name="repository">表示当前 <see cref="EntityList" /> 对应的仓库。</param>
        /// <param name="entityList">表示一个领域对象的集合。</param>
        private void DeleteOriginalData(IRepository repository, EntityList entityList)
        {
            entityList.Clear();

            this.SaveList(repository, entityList);
        }

        /// <summary>
        /// 使用仓库保存指定的列表。
        /// 
        /// 默认使用逐条备份的方式。
        /// 子类可以重写此方法来实现批量数据的导入。
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="entityList"></param>
        protected virtual void SaveList(IRepository repository, EntityList entityList)
        {
            repository.Save(entityList);
        }

        #region ProgressChanged

        /// <summary>
        /// 当数据归档状态发生变化时引发此事件。
        /// </summary>
        public event EventHandler<AggregationArchiveProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="currentProcess"></param>
        private void OnProgressChanged(string message, decimal currentProcess = 0M)
        {
            this.ProgressChanged(this, new AggregationArchiveProgressEventArgs(message, currentProcess));
        }

        #endregion

        /// <summary>
        /// 贪婪加载聚合子实体
        /// </summary>
        /// <param name="eagerLoadOptions"></param>
        /// <param name="entityMeta"></param>
        /// 
        private static void EagerLoadAggregationRecur(EagerLoadOptions eagerLoadOptions, EntityMeta entityMeta)
        {
            var childProperties = entityMeta.ChildrenProperties;
            if (childProperties.Count > 0)
            {
                foreach (var childPropertyMeta in childProperties)
                {
                    var listProperty = childPropertyMeta.ManagedProperty as IListProperty;
                    if (listProperty != null)
                    {
                        eagerLoadOptions.LoadWith(listProperty);
                    }
                    EagerLoadAggregationRecur(eagerLoadOptions, childPropertyMeta.ChildType);
                }
            }
        }
    }
}