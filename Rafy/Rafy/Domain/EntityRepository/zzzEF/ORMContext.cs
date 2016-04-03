//*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110320
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100320
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
////using System.Collections.Concurrent;
//using System.Reflection;
//using System.Data.Entity.ModelConfiguration.Configuration;
////using Rafy.MetaModel;
//using System.Reflection.Emit;
//using Rafy.Library.Internal;
////using Rafy.Data;

//namespace Rafy.Library
//{
//    /// <summary>
//    /// ORM 环境上下文
//    /// </summary>
//    public class ORMContext
//    {
//        /// <summary>
//        /// Singleton
//        /// </summary>
//        internal static readonly ORMContext DbContextFactoryStore = new ORMContext();

//        private ORMContext() { }

//        /// <summary>
//        /// 初始化整个 ORM 环境。
//        /// </summary>
//        public static void Initialize()
//        {
//            DbContextFactoryStore.LoadConfigAndInitDb();
//        }

//        #region 字段

//        private IDictionary<string, Func<DbContext>> _dic = new Dictionary<string, Func<DbContext>>();

//        /// <summary>
//        /// 临时存放点
//        /// </summary>
//        internal IEnumerable<IDbSeeder> ConfigInstances;

//        #endregion

//        /// <summary>
//        /// 两个职责：
//        /// 1. 加载所有程序集中的ORMapping配置。
//        /// 2. 初始化所有的数据库。
//        /// </summary>
//        private void LoadConfigAndInitDb()
//        {
//            //加载所有 Mapping
//            var configTypes = SearchAllConfigTypes();

//            //按照 ConnectionStringSettingName/DbName 分组
//            var groups = configTypes.GroupBy(configType =>
//            {
//                var entityType = configType.BaseType.GetGenericArguments()[0];
//                var dbName = (Activator.CreateInstance(entityType, true) as Entity).ConnectionStringSettingName;
//                return dbName;
//            });

//            //对于每个组：
//            foreach (var group in groups)
//            {
//                var dbSetting = group.Key;
//                var contextType = EmitContextType(dbSetting);
//                //这里用 ConfigInstances 作为间接传值的工具，当调用初始化数据库时，
//                //_DbContextBase 会从这个字段中读取配置类。
//                ConfigInstances = group.Select(configType => Activator.CreateInstance(configType) as IDbSeeder).ToArray();

//                //可构造“高速对象生成方法”
//                //同时存入 ContextFactoryStore 中。
//                Func<DbContext> creator = () =>
//                {
//                    //Connection，使用原有的 ConnectionManager 模式。（一直处于打开状态）
//                    //但是，在初始化 DbContext 时，链接又必须保证处于关闭状态，否则会抛出异常。
//                    var connectionManager = ConnectionManager.GetManager(dbSetting);
//                    connectionManager.Connection.Close();

//                    var context = Activator.CreateInstance(contextType, connectionManager) as _DbContextBase;
//                    context.Database.Initialize(false);

//                    connectionManager.Connection.Open();

//                    return context;
//                };
//                //Func<DbContext> creator = () =>
//                //{
//                //    var context = Activator.CreateInstance(contextType) as _DbContextBase;
//                //    context.Database.Initialize(false);
//                //    return context;
//                //};
//                this._dic[dbSetting] = creator;

//                //设置数据库使用首次创建的方式
//                var dbSetterType = typeof(DbIntializer<>).MakeGenericType(contextType);
//                var dbSetter = Activator.CreateInstance(dbSetterType) as IDbInitizlizer;
//                foreach (var config in ConfigInstances)
//                {
//                    dbSetter.AddSeeder(config);
//                }
//                dbSetter.SetDb();

//                //用找到的所有配置，初始化数据库
//                var firstGuy = Activator.CreateInstance(contextType) as _DbContextBase;
//                firstGuy.Database.Initialize(false);

//                //var meta = creator().GetMetadata()
//                //    .GetItems<EntityType>(DataSpace.OSpace)
//                //    .OrderBy(e => e.FullName).ToList();
//            }

//            ConfigInstances = null;
//        }

//        internal Func<DbContext> FindCreator(string connectionStringSettingName)
//        {
//            Func<DbContext> result = null;
//            this._dic.TryGetValue(connectionStringSettingName, out result);
//            return result;
//        }

//        #region 类到表名的映射

//        private IDictionary<Type, string> _classToTable = new Dictionary<Type, string>();

//        internal void MapClassToTable(Type entityType, string tableName)
//        {
//            this._classToTable[entityType] = tableName;
//        }

//        public string GetMappingTableName(Type entityType)
//        {
//            string tableName = null;

//            if (!this._classToTable.TryGetValue(entityType, out tableName))
//            {
//                tableName = entityType.Name;
//            }

//            return tableName;
//        }

//        #endregion

//        /// <summary>
//        /// 获取所有 ORMapping 的子类。
//        /// </summary>
//        /// <returns></returns>
//        private static IEnumerable<Type> SearchAllConfigTypes()
//        {
//            foreach (var lib in RafyEnvironment.GetAllLibraries())
//            {
//                var configTypes = lib.Assembly.GetTypes()
//                    .Where(t =>
//                        !t.IsAbstract &&
//                        t.BaseType != null &&
//                        t.BaseType.IsGenericType &&
//                        t.BaseType.GUID == typeof(ORMapping<>).GUID
//                    );

//                foreach (var configType in configTypes) { yield return configType; }
//            }
//        }

//        #region 数据库初始化器

//        private interface IDbInitizlizer
//        {
//            void AddSeeder(IDbSeeder seeder);

//            void SetDb();
//        }

//        private class DbIntializer<TContext> : IDbInitizlizer
//            where TContext : DbContext
//        {
//            private List<IDbSeeder> _seeders = new List<IDbSeeder>();

//            public void AddSeeder(IDbSeeder seeder)
//            {
//                this._seeders.Add(seeder);
//            }

//            public void SetDb()
//            {
//                IDatabaseInitializer<TContext> db = null;

//                if (_seeders.Any(s => s.DbCreationType == DbCreationType.CreateIfModelChanges))
//                {
//                    db = new CreateDatabaseIfModelChangedWithSeed<TContext>()
//                    {
//                        Seeders = _seeders
//                    };
//                }
//                else if (_seeders.Any(s => s.DbCreationType == DbCreationType.CreateIfNotExists))
//                {
//                    db = new CreateDatabaseIfNotExistsWithSeed<TContext>()
//                    {
//                        Seeders = _seeders
//                    };
//                }

//                Database.SetInitializer(db);
//            }
//        }

//        private class CreateDatabaseIfNotExistsWithSeed<TContext> : CreateDatabaseIfNotExists<TContext>
//            where TContext : DbContext
//        {
//            internal List<IDbSeeder> Seeders;

//            protected override void Seed(TContext context)
//            {
//                foreach (var seeder in this.Seeders)
//                {
//                    seeder.Seed(context);
//                }
//            }
//        }

//        private class CreateDatabaseIfModelChangedWithSeed<TContext> : DropCreateDatabaseIfModelChanges<TContext>
//            where TContext : DbContext
//        {
//            internal List<IDbSeeder> Seeders;

//            protected override void Seed(TContext context)
//            {
//                foreach (var seeder in this.Seeders)
//                {
//                    seeder.Seed(context);
//                }
//            }
//        }

//        #endregion

//        #region 动态生成类型

//        /// <summary>
//        /// 构造 _DbContextBase 的子类型。
//        /// </summary>
//        /// <param name="dbName"></param>
//        /// <returns></returns>
//        private static Type EmitContextType(string dbName)
//        {
//            //目标格式
//            //internal class DbContextBase_Demo : DbContextBase
//            //{
//            //    public DbContextBase_Demo() : base("xxx") { }
//            //}

//            var baseType = typeof(_DbContextBase);
//            string className = baseType.FullName + "_" + dbName;

//            var module = EmitContext.Instance.GetDynamicModule();

//            //生成：
//            //internal class DbContextBase_Demo : DbContextBase
//            var newType = module.DefineType(className,
//                TypeAttributes.NotPublic | TypeAttributes.Class,
//                baseType
//                );

//            //生成：
//            //public DbContextBase_Demo() : base("xxx") { }
//            var ctor = newType.DefineConstructor(
//                MethodAttributes.Public, CallingConventions.Standard,
//                Type.EmptyTypes
//                );
//            var ctorIL = ctor.GetILGenerator();
//            ctorIL.Emit(OpCodes.Ldarg_0);
//            ctorIL.Emit(OpCodes.Ldstr, dbName);
//            var bastCtor = baseType.GetConstructor(new Type[] { typeof(string) });
//            ctorIL.Emit(OpCodes.Call, bastCtor);
//            ctorIL.Emit(OpCodes.Ret);

//            //生成：
//            //public DbContextBase_Demo(ConnectionManager connectionManager) : base(connectionManager) { }
//            var ctor2 = newType.DefineConstructor(
//                MethodAttributes.Public, CallingConventions.Standard,
//                new Type[] { typeof(ConnectionManager) }
//                );
//            var ctorIL2 = ctor2.GetILGenerator();
//            ctorIL2.Emit(OpCodes.Ldarg_0);
//            ctorIL2.Emit(OpCodes.Ldarg_1);
//            var bastCtor2 = baseType.GetConstructor(new Type[] { typeof(ConnectionManager) });
//            ctorIL2.Emit(OpCodes.Call, bastCtor2);
//            ctorIL2.Emit(OpCodes.Ret);

//            return newType.CreateType();
//        }

//        #endregion
//    }
//}