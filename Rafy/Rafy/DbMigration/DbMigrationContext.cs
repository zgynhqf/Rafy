/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;
using Rafy.DbMigration.History;
using Rafy.DbMigration.Model;
using Rafy.DbMigration.Operations;
using Rafy;
using Rafy.Data;
using Rafy.Data.Providers;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 支持功能：
    /// * 根据目标 Schema 自动升级
    ///     此时可配置是否考虑数据丢失。
    /// * 手工更新
    /// * 升级历史日志功能
    ///     根据历史记录回滚、再次升级
    ///     客户端根据开发人员的历史记录升级自己的自己的数据库。
    /// * 防止数据丢失
    ///     配置是否执行丢失操作。
    ///     配置是否忽略数据丢失。
    /// * 数据库删除、备份、还原
    /// </summary>
    public class DbMigrationContext : IDisposable
    {
        #region Fields

        private ManualMigrationsContainer _ManualMigrations;

        private IDbBackuper _DbBackuper;

        private DbVersionProvider _DbVersionProvider;

        private DbMigrationProvider _dbProvider;

        private IDbAccesser _dba;

        private RunGenerator _runGenerator;

        /// <summary>
        /// 数据库的名称
        /// </summary>
        protected string DbName
        {
            get { return this.DbSetting.Database; }
        }

        /// <summary>
        /// 对应的数据库配置。
        /// </summary>
        public DbSetting DbSetting { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DbMigrationContext"/> class.
        /// </summary>
        /// <param name="dbSetting">The database setting.</param>
        /// <exception cref="System.ArgumentNullException">dbSetting</exception>
        public DbMigrationContext(DbSetting dbSetting)
        {
            if (dbSetting == null) throw new ArgumentNullException("dbSetting");

            this.DbSetting = dbSetting;

            this.RunDataLossOperation = DataLossOperation.None;

            this._dbProvider = DbMigrationProviderFactory.GetProvider(dbSetting);

            this._runGenerator = this._dbProvider.CreateRunGenerator();
            this.DatabaseMetaReader = this._dbProvider.CreateSchemaReader();
        }

        #region Components

        /*********************** 代码块解释 *********************************
         * Builder 模式
         * 以下组件属性基本全是公开的，应用层可以设置这些组件属性以实现不同程序的功能多样化。
        **********************************************************************/

        /// <summary>
        /// 存储所有可用的手工更新
        /// </summary>
        public ManualMigrationsContainer ManualMigrations
        {
            get
            {
                if (this._ManualMigrations == null)
                {
                    this.ManualMigrations = new ManualMigrationsContainer();
                }

                return this._ManualMigrations;
            }
            set
            {
                value.TryInitalize(this.DbSetting);
                this._ManualMigrations = value;
            }
        }

        /// <summary>
        /// 数据库元数据读取器。
        /// </summary>
        public IMetadataReader DatabaseMetaReader { get; private set; }

        /// <summary>
        /// 数据库备份工具
        /// </summary>
        public IDbBackuper DbBackuper
        {
            get
            {
                if (this._DbBackuper == null)
                {
                    this._DbBackuper = this._dbProvider.CreateDbBackuper();
                }

                return this._DbBackuper;
            }
        }

        /// <summary>
        /// 此属性如果为 null，表示不需要记录更新日志。
        /// 也就是说每次都是根据数据库当前版本号来进行完整对比升级。
        /// 默认值为 null。
        /// </summary>
        public HistoryRepository HistoryRepository { get; set; }

        /// <summary>
        /// 数据库版本号管理提供程序
        /// 当纯粹使用手工更新时，可以只重写此属性而不重写 HistoryRepository 属性。
        /// </summary>
        public DbVersionProvider DbVersionProvider
        {
            get
            {
                if (this._DbVersionProvider == null)
                {
                    this._DbVersionProvider = new EmbadedDbVersionProvider
                    {
                        DBA = this.DBA,
                        DbSetting = this.DbSetting
                    };
                }

                return this._DbVersionProvider;
            }
            set
            {
                this._DbVersionProvider = value;
                if (value != null) value.DbSetting = this.DbSetting;
            }
        }

        #endregion

        #region Configuration

        /// <summary>
        /// 是否在自动迁移过程中执行 删除表、删除列 的操作。
        /// 
        /// 默认为 None，表示不执行任何数据丢失的操作。
        /// </summary>
        public DataLossOperation RunDataLossOperation { get; set; }

        internal void NotifyDataLoss(string actionName)
        {
            //此函数中可以记录所有数据丢失操作的日志。
            //暂不实现。
        }

        #endregion

        #region AutoMigrate

        /// <summary>
        /// 自动移植到目标结构
        /// 注意，自动迁移时，同样执行相应时间段的手工迁移。
        /// </summary>
        /// <param name="destination">目标结构</param>
        public void MigrateTo(DestinationDatabase destination)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 主要更新逻辑如下：
             * 1 根据当前版本号，同步最新的历史记录。
             *      客户端被动升级：当给客户版本做更新时，一般使用开发人员的历史记录替换客户版本的历史记录，此时发生此逻辑：
             *      发现历史记录中有比当前数据库版本还要新的记录时，说明已经使用了最新版本的历史记录库，这时需要根据这些历史记录来升级数据库。
             * 2 如果是第一次创建库，则：先执行自动升级、然后再执行手工结构升级。
             * 2 如果是升级库，则：先执行手工结构升级、然后再执行自动升级。
             * 3 执行手工数据升级。
             * 
            **********************************************************************/

            if (this.SupportHistory) { Must(this.MigrateToHistory(DateTime.MaxValue)); }

            var manualPendings = this.GetManualPendings();
            var schemaPending = manualPendings.Where(m => m.Type == ManualMigrationType.Schema).ToList();
            var dataPending = manualPendings.Where(m => m.Type == ManualMigrationType.Data).ToList();

            var dbMeta = this.DatabaseMetaReader.Read();
            var changeSet = ModelDiffer.Distinguish(dbMeta, destination);

            //判断是否正处于升级阶段。（或者是处于创建阶段。）
            //不能直接用 dbMeta.Tables.Count > 0 来判断，这是因为里面可能有 IgnoreTables 中指定表。
            var updating = changeSet.ChangeType == ChangeType.Removed ||
                changeSet.ChangeType == ChangeType.Modified && changeSet.TablesChanged.Count(t => t.ChangeType == ChangeType.Added) < destination.Tables.Count;
            if (updating)
            {
                Must(this.MigrateUpBatch(schemaPending));

                Must(this.AutoMigrate(changeSet));
            }
            else
            {
                if (manualPendings.Count > 0)
                {
                    //此时，自动升级的时间都应该小于所有手工升级
                    Must(this.AutoMigrate(changeSet, manualPendings[0].TimeId));
                }
                else
                {
                    Must(this.AutoMigrate(changeSet));
                }

                Must(this.MigrateUpBatch(schemaPending));
            }

            if (dataPending.Count > 0)
            {
                /*********************** 代码块解释 *********************************
                 * 
                 * 由于之前把 结构升级 和 数据升级 分离开了，
                 * 所以有可能出现 数据升级 中的最后时间其实没有之前的 结构升级 或者 自动升级 的时间大，
                 * 这样就会导致 数据升级 后，版本号变得更小了。
                 * 所以这里需要判断如果这种情况发生，则忽略数据升级中的版本号。
                 * 
                **********************************************************************/

                var dbVersion = this.GetDbVersion();
                var maxDataPending = dataPending.Max(m => m.TimeId);

                Must(this.MigrateUpBatch(dataPending));

                if (dbVersion > maxDataPending)
                {
                    this.DbVersionProvider.SetDbVersion(dbVersion);
                }
            }
        }

        /// <summary>
        /// 保证 TimeId 之间的间隔在 10ms 以上
        /// </summary>
        private const double TimeIdSpan = 10d;

        private Result AutoMigrate(DatabaseChanges changeSet, DateTime? maxTime = null)
        {
            //生成所有自动迁移操作
            var auto = new AutomationMigration() { Context = this };
            auto.GenerateOpertions(changeSet);
            var autoMigrations = auto.Operations;

            if (autoMigrations.Count > 0)
            {
                this.GenerateTimeId(autoMigrations, maxTime);

                return this.MigrateUpBatch(autoMigrations);
            }

            return true;
        }

        private void GenerateTimeId(List<MigrationOperation> autoMigrations, DateTime? maxTime)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 如果提供了最大时间限制，则所有自动升级的时间都由该时间向前反推得出。
             * 否则，直接由当前时间向后推出。
             * 
             * 保证 TimeId 之间的间隔在 MinTimeIdSpan 以上。
             * 
            **********************************************************************/

            if (maxTime.HasValue)
            {
                var timeId = maxTime.Value;
                for (int i = autoMigrations.Count - 1; i >= 0; i--)
                {
                    timeId = timeId.AddMilliseconds(-TimeIdSpan);
                    autoMigrations[i].RuntimeTimeId = timeId;
                }
            }
            else
            {
                var timeId = DateTime.Now;
                foreach (var m in autoMigrations)
                {
                    m.RuntimeTimeId = timeId;
                    timeId = timeId.AddMilliseconds(TimeIdSpan);
                }
            }
        }

        #endregion

        #region MigrateManually

        /// <summary>
        /// 使用场景：如果只是使用手工更新，可以调用此方法完成。
        /// </summary>
        public void MigrateManually()
        {
            var pendings = this.GetManualPendings();

            Must(this.MigrateUpBatch(pendings));
        }

        private List<ManualDbMigration> GetManualPendings()
        {
            var version = this.GetDbVersion();

            var pendings = this.ManualMigrations
                .Where(m => m.TimeId > version)
                .OrderBy(m => m.TimeId)
                .ToList();

            return pendings;
        }

        #endregion

        #region RefreshComments

        /// <summary>
        /// 使用指定的注释来更新数据库中的相关注释内容。
        /// 更新注释前，请保证真实数据库中的包含了指定的库中的所有表和字段。
        /// </summary>
        public void RefreshComments(Database database)
        {
            var operations = new List<MigrationOperation>(1000);

            foreach (var table in database.Tables)
            {
                if (!string.IsNullOrWhiteSpace(table.Comment))
                {
                    operations.Add(new UpdateComment { TableName = table.Name, Comment = table.Comment });
                }

                foreach (var column in table.Columns)
                {
                    if (!string.IsNullOrWhiteSpace(column.Comment))
                    {
                        operations.Add(new UpdateComment { TableName = table.Name, ColumnName = column.Name, ColumnDataType = column.DataType, Comment = column.Comment });
                    }
                }
            }

            Must(this.MigrateUpBatch(operations));
        }

        #endregion

        #region Migrate/Rollback by history

        /*********************** 代码块解释 *********************************
         * 直接根据时间点来迁移数据库。
         * 
         * 根据历史记录功能来进行数据库的迁移
        **********************************************************************/

        /// <summary>
        /// 直接跳转到某个时间点的数据库
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Result JumpToHistory(DateTime time)
        {
            if (!this.SupportHistory) throw new InvalidOperationException("当前迁移操作不支持历史记录功能。");

            var version = this.GetDbVersion();
            if (version < time)
            {
                return this.MigrateToHistory(time);
            }
            else
            {
                this.RollbackToHistory(time);
                return true;
            }
        }

        /// <summary>
        /// 只使用历史记录来升级到指定的时间点
        /// </summary>
        /// <param name="time"></param>
        public Result MigrateToHistory(DateTime time)
        {
            var version = this.GetDbVersion();

            var histories = this.EnsureHistoryRepository().GetHistoryItems(this.DbName, version, time);
            if (histories.Count > 0)
            {
                var migrations = histories.Reverse()
                    .Select(h => this.HistoryRepository.TryRestore(h))
                    .Where(h => h != null)
                    .ToList();

                return this.MigrateUpBatch(migrations, true);
            }

            return true;
        }

        /// <summary>
        /// 回滚到指定时间
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="rollbackAction">The rollback action.</param>
        public void RollbackToHistory(DateTime time, RollbackAction rollbackAction = RollbackAction.None)
        {
            var version = this.GetDbVersion();

            var histories = this.EnsureHistoryRepository().GetHistoryItems(this.DbName, time, version);

            this.Rollback(histories, rollbackAction, time);
        }

        /// <summary>
        /// 全部回滚历史记录
        /// </summary>
        public void RollbackAll(RollbackAction rollbackAction = RollbackAction.None)
        {
            var version = this.GetDbVersion();

            var histories = this.EnsureHistoryRepository().GetHistoryItems(this.DbName, DateTime.MinValue, version);

            this.Rollback(histories, rollbackAction);
        }

        private void Rollback(IList<HistoryItem> histories, RollbackAction rollbackAction, DateTime? time = null)
        {
            var historyRepo = this.HistoryRepository;
            if (histories.Count > 0)
            {
                foreach (var history in histories)
                {
                    var migration = historyRepo.TryRestore(history);
                    if (migration != null)
                    {
                        //CreateDatabase 不能回滚，需要使用 DeleteDatabase 方法。
                        if (!(migration is CreateDatabase))
                        {
                            this.MigrateDown(migration);

                            var version = migration.TimeId.AddMilliseconds(-TimeIdSpan / 2);
                            Must(this.DbVersionProvider.SetDbVersion(version));

                            if (rollbackAction == RollbackAction.DeleteHistory) { historyRepo.Remove(this.DbName, history); }
                        }
                    }
                }
            }

            if (time.HasValue)
            {
                this.DbVersionProvider.SetDbVersion(time.Value);
            }
        }

        #region 方便的 API

        /// <summary>
        /// 直接跳转到某个时间点的数据库
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public Result JumpToHistory(string time)
        {
            return this.JumpToHistory(DateTime.Parse(time));
        }

        /// <summary>
        /// 只使用历史记录来升级到指定的时间点
        /// </summary>
        /// <param name="time"></param>
        public Result MigrateToHistory(string time)
        {
            return this.MigrateToHistory(DateTime.Parse(time));
        }

        /// <summary>
        /// 回滚到指定时间
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="rollbackAction">The rollback action.</param>
        public void RollbackToHistory(string time, RollbackAction rollbackAction = RollbackAction.None)
        {
            this.RollbackToHistory(DateTime.Parse(time), rollbackAction);
        }

        #endregion

        #endregion

        #region DbVersion & History

        /// <summary>
        /// 获取数据库当前的版本号
        /// </summary>
        /// <returns></returns>
        public DateTime GetDbVersion()
        {
            return this.DbVersionProvider.GetDbVersion();
        }

        /// <summary>
        /// 把当前数据库的版本号设置为初始状态。
        /// 
        /// 调用此方法后，会导致：所有手动迁移再次运行。
        /// </summary>
        public void ResetDbVersion()
        {
            this.DbVersionProvider.SetDbVersion(DbVersionProvider.DefaultMinTime);
        }

        /// <summary>
        /// 当前是否支持历史操作
        /// </summary>
        public bool SupportHistory
        {
            get { return this.HistoryRepository != null; }
        }

        /// <summary>
        /// 获取当前所有的历史项
        /// </summary>
        /// <returns></returns>
        public IList<DbMigration> GetHistories()
        {
            return this.EnsureHistoryRepository().GetHistories(this.DbName);
        }

        /// <summary>
        /// 判断当前库的版本号是否处于最开始的状态。
        /// 暂时把这个判断封装在方法内，以应对未来可能的 DefaultMinTime 变化
        /// </summary>
        /// <returns></returns>
        public bool HasNoHistory()
        {
            return this.EnsureHistoryRepository().HasNoHistory(this.DbName);
        }

        /// <summary>
        /// 删除所有历史记录
        /// </summary>
        public void ResetHistory()
        {
            var repo = this.EnsureHistoryRepository();

            var items = repo.GetHistoryItems(this.DbName);

            foreach (var item in items) { repo.Remove(this.DbName, item); }
        }

        private HistoryRepository EnsureHistoryRepository()
        {
            if (!this.SupportHistory) throw new InvalidOperationException("当前环境不支持历史记录功能。（请正确重写本类的 HistoryRepository 属性。）");

            return this.HistoryRepository;
        }

        #endregion

        #region Database

        /// <summary>
        /// 是否存在
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists()
        {
            var connection = this.DBA.Connection;
            if (connection.State == ConnectionState.Open) return true;

            try
            {
                connection.Open();

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        /// <summary>
        /// 删除数据库
        /// 以及它的历史信息、版本号信息。
        /// 
        /// 注意，如果需要保留整个历史库的升级信息，请使用 MigrateTo(RemovedDatabase) 方法。
        /// </summary>
        public void DeleteDatabase()
        {
            this.MigrateUp(new DropDatabase()
            {
                Database = this.DbName
            });

            bool embaded = this.DbVersionProvider is EmbadedDbVersionProvider;

            if (!embaded) { this.ResetDbVersion(); }

            this.ResetHistory();
        }

        #endregion

        #region MigrateCore

        private IDbAccesser DBA
        {
            get
            {
                if (this._dba == null)
                {
                    this._dba = new DbAccesser(this.DbSetting);
                }

                return this._dba;
            }
        }

        private Result MigrateUpBatch(IEnumerable<DbMigration> migrations, bool addByDeveloperHistory = false)
        {
            Result res = true;

            //对于每一个数据库操作，进行升级操作
            int i = 0, count = migrations.Count();
            foreach (var migration in migrations)
            {
                this.MigrateUp(migration);

                //如果不是客户端被动升级，需要创建历史记录
                if (!addByDeveloperHistory && this.SupportHistory)
                {
                    res = this.HistoryRepository.AddAsExecuted(this.DbName, migration);
                }

                //如果如果前面执行成功，那么刷新版本号
                if (res)
                {
                    //以下三种情况需要记录版本号：手工更新、自动更新且同时支持历史记录、客户端被动升级
                    if (migration.MigrationType == MigrationType.ManualMigration
                        || this.SupportHistory || addByDeveloperHistory)
                    {
                        //EmbadedDbVersionProvider 在删除数据库时就不能再存储版本号信息了。
                        if (!(migration is DropDatabase) || !this.DbVersionProvider.IsEmbaded())
                        {
                            res = this.DbVersionProvider.SetDbVersion(migration.TimeId);
                        }
                    }
                }

                if (!res)
                {
                    //升级日志添加出错，回滚，并终止之后的升级列表。
                    this.MigrateDown(migration);
                    break;
                }

                i++;
                this.OnItemMigrated(new MigratedEventArgs(i, count));
            }

            return res;
        }

        [DebuggerStepThrough]
        private void MigrateUp(DbMigration migration)
        {
            this.RunMigration(migration, true);
        }

        [DebuggerStepThrough]
        private void MigrateDown(DbMigration migration)
        {
            this.RunMigration(migration, false);
        }

        private void RunMigration(DbMigration migration, bool up)
        {
            var runList = this.GenerateRunList(migration, up);

            if (runList.Count == 0) return;

            this.ExecuteWithoutDebug(() =>
            {
                var dba = this.DBA;
                if (runList.Count > 1)
                {
                    /*********************** 代码块解释 *********************************
                     * 
                     * 如果执行多句 SQL，则需要主动打开连接，
                     * 否则 DBA 可能会打开之后再把连接关闭，造成多次打开连接，引起分布式事务。
                     * 而分布式事务在一些数据库中并不支持，例如 SQLCE。
                     * 
                    **********************************************************************/
                    bool opend = false;
                    var con = dba.Connection;

                    try
                    {
                        if (con.State != ConnectionState.Open)
                        {
                            con.Open();
                            opend = true;
                        }

                        using (var tran = new TransactionScope())
                        {
                            foreach (var item in runList)
                            {
                                this._currentRun = item as SqlMigrationRun;
                                item.Run(dba);
                            }

                            tran.Complete();
                        }
                    }
                    finally
                    {
                        if (opend)
                        {
                            con.Close();
                        }
                    }
                }
                else
                {
                    var singleRun = runList[0];
                    this._currentRun = singleRun as SqlMigrationRun;
                    singleRun.Run(dba);
                }
            });
        }

        private SqlMigrationRun _currentRun;

        [DebuggerStepThrough]
        private void ExecuteWithoutDebug(Action action)
        {
            try
            {
                action();
            }
            catch (DbException ex)
            {
                var error = DbMigrationExceptionMessageFormatter.FormatMessage(ex, _currentRun);
                throw new DbMigrationException(error, ex);
            }
        }

        private IList<MigrationRun> GenerateRunList(DbMigration migration, bool up)
        {
            migration.DatabaseMetaReader = this.DatabaseMetaReader;

            if (up)
            {
                migration.GenerateUpOperations();
            }
            else
            {
                migration.GenerateDownOperations();
            }

            var runList = _runGenerator.Generate(migration.Operations);

            return runList;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (this._dba != null) { this._dba.Dispose(); }
        }

        /// <summary>
        /// 每一个项成功升级后的通知事件。
        /// </summary>
        public event EventHandler<MigratedEventArgs> ItemMigrated;

        private void OnItemMigrated(MigratedEventArgs e)
        {
            var handler = this.ItemMigrated;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Helper

        [DebuggerStepThrough]
        private static Result Must(Result res)
        {
            if (!res) throw new DbMigrationException(res.Message);
            return res;
        }

        #endregion
    }

    /// <summary>
    /// 每一个项成功升级后的通知事件参数
    /// </summary>
    public class MigratedEventArgs : EventArgs
    {
        public MigratedEventArgs(int index, int totalCount)
        {
            this.Index = index;
            this.TotalCount = totalCount;
        }

        /// <summary>
        /// 当前项的索引
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 所有需要升级的项的总和
        /// </summary>
        public int TotalCount { get; private set; }
    }

    /// <summary>
    /// 回滚数据库时的行为
    /// </summary>
    public enum RollbackAction
    {
        /// <summary>
        /// 不回滚。
        /// </summary>
        None,
        /// <summary>
        /// 在回滚的同时删除数据库中的历史记录
        /// </summary>
        DeleteHistory
    }

    /// <summary>
    /// 造成数据丢失的操作。
    /// </summary>
    public enum DataLossOperation
    {
        /// <summary>
        /// 不执行丢失数据的操作。
        /// </summary>
        None = 0,

        /// <summary>
        /// 删除表
        /// </summary>
        DropTable = 1,
        /// <summary>
        /// 删除列
        /// </summary>
        DropColumn = 2,

        /// <summary>
        /// 所有操作。
        /// </summary>
        All = DropTable | DropColumn
    }
}