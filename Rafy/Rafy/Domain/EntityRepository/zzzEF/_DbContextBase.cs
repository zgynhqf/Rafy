//*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110317
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100317
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//////using System.Data.Entity.ModelConfiguration.Configuration;
////using System.Data.Common;
//using System.Collections;
//using Rafy.Data;
//using System.Data.SqlClient;

//namespace Rafy.Library.Internal
//{
//    /// <summary>
//    /// 此类不需要被外界直接调用。
//    /// 公有化的目的是为了动态程序集可以生成它的子类。
//    /// </summary>
//    public abstract class _DbContextBase : DbContext
//    {
//        private ConnectionManager _connectionManager;

//        public _DbContextBase(ConnectionManager connectionManager)
//            : base(connectionManager.Connection as DbConnection, false)
//        {
//            this._connectionManager = connectionManager;

//            var config = this.Configuration;
//            config.AutoDetectChangesEnabled = false;
//            config.LazyLoadingEnabled = false;
//            config.ProxyCreationEnabled = false;
//            config.ValidateOnSaveEnabled = false;
//        }

//        public _DbContextBase(string settingName)
//            : base(settingName)
//        {
//            var config = this.Configuration;
//            config.AutoDetectChangesEnabled = false;
//            config.LazyLoadingEnabled = false;
//            config.ProxyCreationEnabled = false;
//            config.ValidateOnSaveEnabled = false;
//        }

//        protected override void Dispose(bool disposing)
//        {
//            base.Dispose(disposing);

//            if (disposing)
//            {
//                //Connection，使用原有的 ConnectionManager 模式。（一直处于打开状态）
//                if (this._connectionManager != null)
//                {
//                    var con = this._connectionManager.Connection;
//                    if (con.State != System.Data.ConnectionState.Open) { con.Open(); }

//                    this._connectionManager.Dispose();
//                }
//            }
//        }

//        #region 动态加载配置

//        protected override void OnModelCreating(DbModelBuilder modelBuilder)
//        {
//            foreach (var config in ORMContext.DbContextFactoryStore.ConfigInstances)
//            {
//                //Create a IConfigAdder
//                var entityType = config.GetType().BaseType.GetGenericArguments()[0];
//                var adderType = typeof(ConfigAdder<>).MakeGenericType(entityType);
//                var adder = Activator.CreateInstance(adderType) as IConfigAdder;

//                //Add Config into modelBuilder
//                adder.ConfigurationRegistrar = modelBuilder.Configurations;
//                adder.AddConfig(config);
//            }

//            base.OnModelCreating(modelBuilder);
//        }

//        private interface IConfigAdder
//        {
//            ConfigurationRegistrar ConfigurationRegistrar { get; set; }

//            void AddConfig(object instance);
//        }

//        private class ConfigAdder<TEntity> : IConfigAdder
//            where TEntity : class
//        {
//            public ConfigurationRegistrar ConfigurationRegistrar { get; set; }

//            void IConfigAdder.AddConfig(object instance)
//            {
//                var type = instance as EntityTypeConfiguration<TEntity>;
//                ConfigurationRegistrar.Add(type);
//            }
//        }

//        #endregion
//    }
//}
