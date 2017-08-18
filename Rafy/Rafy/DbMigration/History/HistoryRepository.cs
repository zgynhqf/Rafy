/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.DbMigration.History
{
    /// <summary>
    /// 历史记录提供程序
    /// </summary>
    public abstract class HistoryRepository
    {
        internal bool HasNoHistory(string database)
        {
            return this.GetHistoriesCore(database).Count == 0;
        }

        internal IList<DbMigration> GetHistories(string database)
        {
            var items = this.GetHistoriesCore(database);

            return items.Select(history => this.TryRestore(history))
                .Where(h => h != null)
                .OrderByDescending(dm => dm.TimeId)
                .ToList();
        }

        internal IList<HistoryItem> GetHistoryItems(string database)
        {
            return this.GetHistoryItems(database, DateTime.MinValue, DateTime.MaxValue);
        }

        internal IList<HistoryItem> GetHistoryItems(string database, DateTime minTime, DateTime maxTime)
        {
            return this.GetHistoriesCore(database)
                .Where(h => h.TimeId > minTime && h.TimeId <= maxTime)
                .OrderByDescending(history => history.TimeId)
                .ToList();
        }

        /// <summary>
        /// 当某个迁移操作升级完成后，为它添加相应的历史记录。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="migration"></param>
        /// <returns></returns>
        internal Result AddAsExecuted(string database, DbMigration migration)
        {
            var migrationType = migration.GetType();

            var history = new HistoryItem()
            {
                IsGenerated = false,
                TimeId = migration.TimeId,
                Description = migration.Description,
                //MigrationClass = migration.GetType().AssemblyQualifiedName,//Rafy.DbMigration.Operations.RemoveFKConstraint, Rafy, Version=3.51.3140.0, Culture=neutral, PublicKeyToken=f7937325279b37cf
                MigrationClass = migrationType.FullName + ", " + migrationType.Assembly.FullName,//忽略版本号，方便框架升级。Rafy.DbMigration.Operations.RemoveFKConstraint, Rafy
            };

            if (migration.MigrationType == MigrationType.AutoMigration)
            {
                history.IsGenerated = true;
                history.MigrationContent = SerializationHelper.XmlSerialize(migration);
            }

            return this.AddHistoryCore(database, history);
        }

        /// <summary>
        /// 从历史记录中还原迁移对象
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        internal DbMigration TryRestore(HistoryItem history)
        {
            Type type = null;
            try
            {
                type = Type.GetType(history.MigrationClass);
            }
            catch (TypeLoadException)
            {
                //如果当前这个类已经不存在了，无法还原，则直接返回 null，跳过该项。
                return null;
            }

            //如果这个类型已经被变更了，或者找不到，则直接返回 null
            if (type == null) { return null; }

            DbMigration migration = null;

            if (history.IsGenerated)
            {
                migration = SerializationHelper.XmlDeserialize(type, history.MigrationContent).CastTo<DbMigration>();

                (migration as MigrationOperation).RuntimeTimeId = history.TimeId;
            }
            else
            {
                migration = Activator.CreateInstance(type, true).CastTo<DbMigration>();
            }

            return migration;
        }

        internal Result Remove(string database, HistoryItem history)
        {
            return this.RemoveHistoryCore(database, history);
        }

        #region 子类接口

        /// <summary>
        /// 获取指定数据库的所有历史记录
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        protected abstract IList<HistoryItem> GetHistoriesCore(string database);

        /// <summary>
        /// 为指定数据库添加历史记录。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        protected abstract Result AddHistoryCore(string database, HistoryItem history);

        /// <summary>
        /// 为指定数据库删除某条历史记录。
        /// </summary>
        /// <param name="database"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        protected abstract Result RemoveHistoryCore(string database, HistoryItem history);

        #endregion
    }
}