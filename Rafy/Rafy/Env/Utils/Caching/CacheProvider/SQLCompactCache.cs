/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：使用SQLCe和FileSystem的缓存提供器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 修改文件 宋军瑞 20170112 -- 重构 Caching 模块。
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.Common;
using System.Data;
using System.IO;
using Rafy.Data;
using Rafy.Serialization;
using Rafy.DbMigration.SqlServerCe;

namespace Rafy.Utils.Caching
{
    /// <summary>
    /// 使用 SqlCe 的缓存提供器。
    /// </summary>
    public class SQLCompactCache : Cache
    {
        /// <summary>
        /// 所有未提供Region的项，都加入到这个Region中。
        /// </summary>
        private static readonly string _commonRegionName = "CommonRegion";

        /// <summary>
        /// SQL CE 3.5 不支持锁进制，但是ObjectCache必须实现多线程间同步。
        /// </summary>
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        /// <summary>
        /// 由于SQLCE不支持多链接，所以打开/关闭链接时，需要手工进行同步。
        /// </summary>
        private object _syncRoot = new object();

        /// <summary>
        /// 文件缓存的文件夹名。
        /// 如果某行的数据超过SQLCE的最大行长，则需要使用文件来缓存。
        /// </summary>
        private string _cacheDir;

        private IDbAccesser _db;

        public SQLCompactCache(string dbFile)
        {
            if (string.IsNullOrWhiteSpace(dbFile)) throw new ArgumentNullException("dbFile");

            this._cacheDir = @"CacheItems\";
            //this._cacheDir = Path.Combine(Path.GetDirectoryName(dbFile), @"CacheItems\");

            this._db = new DbAccesser("Case Sensitive=True;DataSource=" + dbFile, DbSetting.Provider_SqlCe);
        }

        protected internal override StoredValue GetCacheItemCore(string region, string key)
        {
            EnsureDbCreated();

            try
            {
                this._locker.EnterReadLock();

                object value = null;
                object memoto = null;
                this.Db_Read(key, region, out value, out memoto);

                if (value != null)
                {
                    var result = new StoredValue();
                    result.Value = value;

                    if (memoto != null)
                    {
                        //把Memoto转换为原始的Checker
                        var checkerMemoto = memoto as CheckerMemoto;
                        result.Checker = checkerMemoto.Restore();
                    }

                    return result;
                }
            }
            catch { }
            finally
            {
                this._locker.ExitReadLock();
            }

            return null;
        }

        protected internal override bool AddCore(string region, string key, StoredValue value)
        {
            EnsureDbCreated();

            try
            {
                this._locker.EnterWriteLock();

                object checkerMemoto = null;
                if (value.Checker != null)
                {
                    //真实存储在数据库中的值，是Checker的Memoto
                    checkerMemoto = value.Checker.GetMemoto();
                }

                this.Db_Add(region, key, value.Value, checkerMemoto);

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        protected internal override void RemoveCore(string region, string key)
        {
            EnsureDbCreated();

            try
            {
                this._locker.EnterWriteLock();

                this.Db_Remove(region, key);
            }
            catch { }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        protected internal override void ClearRegionCore(string region)
        {
            EnsureDbCreated();

            try
            {
                this._locker.EnterWriteLock();

                this.Db_Remove(region);
            }
            catch { }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        protected internal override void ClearCore()
        {
            EnsureDbCreated();

            try
            {
                this._locker.EnterWriteLock();

                this.Db_Clear();
            }
            catch { }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        #region 创建数据库

        private bool _dbExists = false;

        private void EnsureDbCreated()
        {
            if (!_dbExists)
            {
                if (File.Exists(_db.Connection.Database))
                {
                    _dbExists = true;
                }
                else
                {
                    try
                    {
                        _locker.EnterWriteLock();

                        if (!File.Exists(_db.Connection.Database))
                        {
                            CreateDb();
                        }

                        _dbExists = true;
                    }
                    finally
                    {
                        _locker.ExitWriteLock();
                    }
                }
            }
        }

        private void CreateDb()
        {
            var action = new CreateDbMigrationRun();
            action.Run(_db);

            _db.RawAccesser.ExecuteText(@"
CREATE TABLE [Entities] (
    [Region] nvarchar(100) NOT NULL,
    [Key] nvarchar(100) NOT NULL,
    [ChangeChecker] varbinary(8000) NULL,
    [Time] datetime NOT NULL,
    [ValuePath] nvarchar(1000) NULL,
    [Value] varbinary(8000) NULL
);
");
            _db.RawAccesser.ExecuteText(@"
ALTER TABLE [Entities] ADD CONSTRAINT [PK__Entities__000000000000000D] PRIMARY KEY ([Region],[Key]);");
        }

        #endregion

        #region 硬盘访问
        //以下方法使用真实的硬盘访问操作。

        private const int SqlCEBytesMaxLength = 8000;

        private void Db_Clear()
        {
            this._db.RawAccesser.ExecuteText("delete from [Entities]");

            if (Directory.Exists(this._cacheDir))
            {
                Directory.Delete(this._cacheDir, true);
            }
        }

        private void Db_Add(string region, string key, object value, object changeChecker)
        {
            if (region == null) region = _commonRegionName;

            //序列化对象
            byte[] valueData = Serialize(value);
            byte[] checkerData = Serialize(changeChecker);
            if (checkerData != null && checkerData.Length > SqlCEBytesMaxLength) throw new InvalidOperationException(string.Format("checkerData.Length > {0} must be false.", SqlCEBytesMaxLength));

            //如果数据超过了SQLCE的最大行长，则需要使用文件来缓存。
            string valuePath = string.Empty;
            if (valueData.Length > SqlCEBytesMaxLength)
            {
                var dir = Path.Combine(this._cacheDir, region);
                Directory.CreateDirectory(dir);
                valuePath = Path.Combine(dir, DateTime.Now.Ticks.ToString());
                File.WriteAllBytes(valuePath, valueData);
                valueData = new byte[0];
            }

            this._db.ExecuteText(
                @"Insert into [Entities]([Region], [Key], [Value], [ChangeChecker], [Time], [ValuePath]) values({0}, {1}, {2}, {3}, {4}, {5})",
                region, key, valueData, checkerData, DateTime.Now, valuePath
                );
        }

        private void Db_Remove(string region, string key)
        {
            if (region == null) region = _commonRegionName;

            //处理文件
            bool hasRecord = false;
            //先查询一次，看是否把内容存储为文件了。
            using (var reader = this._db.QueryDataReader(
@"select ValuePath from Entities where [Region] = {0} and [Key] = {1}", region, key))
            {
                if (reader.Read())
                {
                    hasRecord = true;
                    var path = reader["ValuePath"] as string;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        if (File.Exists(path)) File.Delete(path);
                    }
                }
            }

            //处理数据库
            if (hasRecord)
            {
                this._db.ExecuteText(@"delete from [Entities] where [Region] = {0} and [Key] = {1}", region, key);
            }
        }

        private void Db_Remove(string region)
        {
            if (region == null) region = _commonRegionName;

            //处理数据库
            this._db.ExecuteText(@"delete from [Entities] where [Region] = {0}", region);

            //处理文件
            var dir = Path.Combine(this._cacheDir, region);
            Directory.Delete(dir, true);
        }

        private void Db_Read(string key, string region, out object value, out object changeChecker)
        {
            value = changeChecker = null;

            if (region == null) region = _commonRegionName;

            using (var reader = this._db.QueryDataReader(
                @"Select Value, ValuePath, ChangeChecker 
                from [Entities]
                where [Region] = {0} and [Key] = {1}
                ", region, key))
            {
                if (reader.Read())
                {
                    var checkerData = reader["ChangeChecker"] as byte[];
                    var valueData = reader["Value"] as byte[];
                    var valuePath = reader["ValuePath"] as string;
                    bool loadDataSuccess = true;

                    //如果已经存储进了文件，则在文件中读取数据。
                    if (!string.IsNullOrWhiteSpace(valuePath))
                    {
                        if (File.Exists(valuePath))
                        {
                            valueData = File.ReadAllBytes(valuePath);
                        }
                        else
                        {
                            loadDataSuccess = false;
                            this.Db_Remove(region, key);
                        }
                    }

                    //反序列化为对象。
                    if (loadDataSuccess)
                    {
                        value = Deserialize(valueData);
                        changeChecker = Deserialize(checkerData);
                    }
                }
            }
        }

        #endregion

        private byte[] Serialize(object value)
        {
            return Serializer.SerializeBytes(value);
        }

        private object Deserialize(byte[] data)
        {
            return Serializer.DeserializeBytes(data);
        }
    }
}
