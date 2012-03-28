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
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Threading;
using System.Data.SqlServerCe;
using System.Data.Common;
using System.Data;
using System.IO;

namespace OEA.Utils.Caching
{
    public class SQLCompactProvider : CacheProvider
    {
        /// <summary>
        /// 所有未提供Region的项，都加入到这个Region中。
        /// </summary>
        private static readonly string _commonRegionName = "CommonRegion";

        /// <summary>
        /// SQL CE 3.5 不支持锁进制，但是ObjectCache必须实现多线程间同步。
        /// </summary>
        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        private ISQLCompactSerializer _serializer;

        private string _conString;

        /// <summary>
        /// 由于SQLCE不支持多链接，所以打开/关闭链接时，需要手工进行同步。
        /// </summary>
        private object _syncRoot = new object();

        /// <summary>
        /// 文件缓存的文件夹名。
        /// 如果某行的数据超过SQLCE的最大行长，则需要使用文件来缓存。
        /// </summary>
        private string _cacheDir;

        public SQLCompactProvider(string dbFile, ISQLCompactSerializer serializer)
        {
            if (string.IsNullOrWhiteSpace(dbFile)) throw new ArgumentNullException("dbFile");
            if (serializer == null) throw new ArgumentNullException("serializer");

            this._conString = "DataSource=" + dbFile;
            this._cacheDir = Path.Combine(Path.GetDirectoryName(dbFile), @"CacheItems\");

            this._serializer = serializer;
        }

        internal protected override StoredValue GetCacheItemCore(string region, string key)
        {
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

        internal protected override bool AddCore(string region, string key, StoredValue value)
        {
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

        internal protected override void RemoveCore(string region, string key)
        {
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

        protected internal override void ClearCore()
        {
            try
            {
                this.Db_Clear();
            }
            catch { }
        }

        #region 硬盘访问
        //以下方法使用真实的硬盘访问操作。

        private void Db_Clear()
        {
            this.ExecuteSQL("delete from [Entities]");
            Directory.Delete(this._cacheDir, true);
        }

        private void Db_Add(string region, string key, object value, object changeChecker)
        {
            if (region == null) region = _commonRegionName;

            //序列化对象
            byte[] valueData = Serialize(value);
            byte[] checkerData = Serialize(changeChecker);
            if (checkerData.Length > 8000) throw new InvalidOperationException("checkerData.Length > 8000 must be false.");

            //如果数据超过了SQLCE的最大行长，则需要使用文件来缓存。
            string valuePath = string.Empty;
            if (valueData.Length > 8000)
            {
                var dir = Path.Combine(this._cacheDir, region);
                Directory.CreateDirectory(dir);
                valuePath = Path.Combine(dir, DateTime.Now.Ticks.ToString());
                File.WriteAllBytes(valuePath, valueData);
                valueData = new byte[0];
            }

            var cmd = this.CreateCommand("Insert into [Entities]([Region], [Key], [Value], [ChangeChecker], [Time], [ValuePath]) values(?, ?, ?, ?, ?, ?)");

            var parameters = cmd.Parameters;

            var p = cmd.CreateParameter();
            p.ParameterName = "p1";
            p.Value = region;
            p.DbType = DbType.String;
            parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "p2";
            p.Value = key;
            p.DbType = DbType.String;
            parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "p3";
            p.Value = valueData;
            p.DbType = DbType.Binary;
            parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "p4";
            p.Value = checkerData;
            p.DbType = DbType.Binary;
            parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "p5";
            p.Value = DateTime.Now;
            p.DbType = DbType.DateTime;
            parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "p6";
            p.Value = valuePath;
            p.DbType = DbType.String;
            parameters.Add(p);

            this.ExeNonquery(cmd);
        }

        private void Db_Remove(string region, string key)
        {
            if (region == null) region = _commonRegionName;

            //先查询一次，看是否把内容存储为文件了。
            var con = this.CreateConnection();
            var cmd = this.CreateCommand(
                string.Format(@"select ValuePath from Entities where [Region] = '{0}' and [Key] = '{1}'", region, key),
                con
                );

            //处理文件
            bool hasRecord = false;
            try
            {
                this.OpenConnection(con);
                using (var reader = cmd.ExecuteReader())
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
            }
            finally
            {
                this.CloseConnection(con);
            }

            //处理数据库
            if (hasRecord)
            {
                this.ExecuteSQL(string.Format(@"delete from [Entities] where [Region] = '{0}' and [Key] = '{1}'", region, key));
            }
        }

        private void Db_Read(string key, string region, out object value, out object changeChecker)
        {
            value = changeChecker = null;

            if (region == null) region = _commonRegionName;

            var con = this.CreateConnection();
            var cmd = this.CreateCommand(string.Format(@"
Select Value, ValuePath, ChangeChecker 
from [Entities]
where [Region] = '{0}' and [Key] = '{1}'
", region, key), con);
            try
            {
                this.OpenConnection(con);
                using (var reader = cmd.ExecuteReader())
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
            finally
            {
                this.CloseConnection(con);
            }
        }

        #region 数据库访问的基础操作封装

        private void ExecuteSQL(string sql)
        {
            var cmd = this.CreateCommand(sql);
            this.ExeNonquery(cmd);
        }

        private DbCommand CreateCommand(string sql, DbConnection con = null)
        {
            con = con ?? this.CreateConnection();
            var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            return cmd;
        }

        private void ExeNonquery(DbCommand cmd)
        {
            try
            {
                this.OpenConnection(cmd.Connection);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                this.CloseConnection(cmd.Connection);
            }
        }

        private void OpenConnection(DbConnection con)
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
        }

        private void CloseConnection(DbConnection con)
        {
            if (con.State != ConnectionState.Closed)
            {
                con.Close();
            }
        }

        private DbConnection CreateConnection()
        {
            return new SqlCeConnection(this._conString);
        }

        #endregion

        #endregion

        private byte[] Serialize(object value)
        {
            return this._serializer.Serialize(value);
        }

        private object Deserialize(byte[] data)
        {
            return this._serializer.Deserialize(data);
        }
    }

    public interface ISQLCompactSerializer
    {
        byte[] Serialize(object value);

        object Deserialize(byte[] data);
    }
}
