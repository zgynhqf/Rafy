/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230304
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230304 21:31
 * 
*******************************************************/

using MongoDB.Driver;
using Rafy.Data;
using Rafy.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Sources;

namespace Rafy.MongoDb
{
    /// <summary>
    /// MongoDb 的数据层提供程序。
    /// </summary>
    public class MongoDbDataProvider : RepositoryDataProvider
    {
        public MongoDbDataProvider()
        {
            this.DataSaver = new MongoDbDataSaver();
            this.DataQueryer = new MongoDbDataQueryer();
        }

        public MongoDbDataProvider(MongoDbDataSaver dataSaver, MongoDbDataQueryer dataQueryer)
        {
            this.DataSaver = dataSaver;
            this.DataQueryer = dataQueryer;
        }

        /// <summary>
        /// 数据的保存器。
        /// </summary>
        public new MongoDbDataSaver DataSaver
        {
            get { return base.DataSaver as MongoDbDataSaver; }
            set { base.DataSaver = value; }
        }

        /// <summary>
        /// 数据的查询器。
        /// </summary>
        public new MongoDbDataQueryer DataQueryer
        {
            get { return base.DataQueryer as MongoDbDataQueryer; }
            set { base.DataQueryer = value; }
        }

        #region DbSetting && MongoClient

        private IMongoDatabase _mongoDatabase;
        /// <summary>
        /// 当前实体所在的数据库。
        /// </summary>
        public IMongoDatabase MongoDatabase
        {
            get
            {
                if (_mongoDatabase == null)
                {
                    var conString = this.DbSetting.ConnectionString;
                    if (conString.Contains("?"))
                    {
                        conString = conString.Substring(0, conString.IndexOf("?"));
                    }
                    var dbName = conString.Substring(conString.LastIndexOf("/") + 1);//最后一个 / 字符之后是
                    if (string.IsNullOrEmpty(dbName))
                    {
                        throw new InvalidProgramException("MongoDb 的连接字符串中，需要指定数据库名称。");
                    }
                    _mongoDatabase = this.MongoClient.GetDatabase(dbName);
                }
                return _mongoDatabase;
            }
        }

        private IMongoClient _mongoClient;
        /// <summary>
        /// 正在使用的 Mongo 连接客户端。
        /// </summary>
        public IMongoClient MongoClient
        {
            get
            {
                if (_mongoClient == null)
                {
                    _mongoClient = new MongoClient(this.DbSetting.ConnectionString);
                }
                return _mongoClient;
            }
        }

        /// <summary>
        /// 数据层使用的 DbSetting 的缓存。
        /// </summary>
        private DbSetting _dbSetting;

        /// <summary>
        /// 数据库配置名称（每个库有一个唯一的配置名）
        /// 
        /// 默认使用 <see cref="DbSettingNames.RafyPlugins"/> 中配置的数据库。
        /// </summary>
        internal protected virtual string ConnectionStringSettingName
        {
            get { return DbSettingNames.RafyPlugins; }
        }

        /// <summary>
        /// 数据库配置（每个库有一个唯一的配置名）
        /// </summary>
        public DbSetting DbSetting
        {
            get
            {
                //如果还没有初始化，进入初始化的逻辑。
                if (_dbSetting == null)
                {
                    string dbSettingName = this.ConnectionStringSettingName;
                    _dbSetting = DbSetting.FindOrCreate(dbSettingName);
                }

                return _dbSetting;
            }
            private set
            {
                _dbSetting = value;
                //由于 _dbSetting 已经变更，可能底层的数据库也会变更。这里需要重置 _client;
                _mongoClient = null;
            }
        }

        public override string DbProviderName => this.DbSetting.ProviderName;

        #endregion

        #region 其它

        /// <summary>
        /// 强制转换指定仓库的数据提供程序为关系数据库的数据提供程。
        /// 如果该仓库的 <see cref="IRepository.DataProvider"/> 不是此类型的子类，则会抛出异常。
        /// </summary>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static MongoDbDataProvider Get(IRepository repository)
        {
            var dp = repository.DataProvider as MongoDbDataProvider;
            if (dp == null)
            {
                throw new InvalidProgramException(string.Format(
                    "{0} 仓库类型使用的数据提供程序类型是 {1}，该类型不能转换为 MongoDb 数据库的数据提供程序。",
                    repository.GetType(),
                    repository.DataProvider.GetType()
                    ));
            }
            return dp;
        }

        #endregion
    }
}