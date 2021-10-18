///*******************************************************
// * 
// * 作者：王国超
// * 创建日期：20180126
// * 运行环境：.NET 4.5
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 王国超 20180126 16:22
// * 
//*******************************************************/

//using Rafy;
//using Rafy.ComponentModel;
//using Rafy.Domain;
//using Rafy.Domain.ORM;
//using Rafy.MetaModel;
//using Rafy.MetaModel.Attributes;
//using Rafy.MultiTenancy;
//using Rafy.MultiTenancy.ShardMap;
//using MultiTenancyTestDomain;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Security.Permissions;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Rafy.Domain.ORM.DbMigration;
//using Rafy.DbMigration;

//namespace RafyUnitTest
//{
//    [TestClass]
//    public class MultiTenancyPluginTest
//    {
//        [TestMethod]
//        public void MigrationDb_AutoUpdate()
//        {
//            ShardMapConfigManager.Initialize();
//            new MultiTenancyPluginTestApp(true).Startup();

//            var count1 = 0;
//            var count2 = 0;
//            var count3 = 0;

//            var repo = RF.ResolveInstance<OrderRepository>();

//            using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, "Test_RafyMultiTenancy01"))
//            {
//                var list = repo.GetAll();
//                count1 = list.Count;
//            }

//            using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, "Test_RafyMultiTenancy02"))
//            {
//                var list = repo.GetAll();
//                count2 = list.Count;
//            }

//            using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, "Test_RafyMultiTenancy03"))
//            {
//                var list = repo.GetAll();
//                count3 = list.Count;
//            }

//            Assert.IsTrue(count1 >= 0);
//            Assert.IsTrue(count2 >= 0);
//            Assert.IsTrue(count3 >= 0);
//        }

//        [TestMethod]
//        public void ShardMap_DbSettingName()
//        {
//            ShardMapConfigManager.Initialize();
//            var mapList = ShardMapConfigManager.ShardMapList;

//            Assert.AreEqual("Test_RafyMultiTenancy01", mapList[0].DbSettingName);
//            Assert.AreEqual("Test_RafyMultiTenancy02", mapList[1].DbSettingName);
//            Assert.AreEqual("Test_RafyMultiTenancy03", mapList[2].DbSettingName);
//        }

//        [TestMethod]
//        public void ShardMap_TenantIdRange()
//        {
//            ShardMapConfigManager.Initialize();
//            var chardMap = ShardMapConfigManager.ShardMapList;

//            Assert.AreEqual(1, chardMap[0].TenantIdRange[0, 0]);
//            Assert.AreEqual(2000000, chardMap[0].TenantIdRange[0, 1]);

//            Assert.AreEqual(2000001, chardMap[1].TenantIdRange[0, 0]);
//            Assert.AreEqual(4000000, chardMap[1].TenantIdRange[0, 1]);

//            Assert.AreEqual(4000001, chardMap[2].TenantIdRange[0, 0]);
//            Assert.AreEqual(long.MaxValue, chardMap[2].TenantIdRange[0, 1]);
//        }

//        [TestMethod]
//        public void ShardMap_GetDbSettingName()
//        {
//            ShardMapConfigManager.Initialize();

//            var dbsetname = MultiTenancyUtility.GetDbSettingName(1);
//            Assert.AreEqual("Test_RafyMultiTenancy01", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(1000000);
//            Assert.AreEqual("Test_RafyMultiTenancy01", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(2000000);
//            Assert.AreEqual("Test_RafyMultiTenancy01", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(2000001);
//            Assert.AreEqual("Test_RafyMultiTenancy02", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(4000000);
//            Assert.AreEqual("Test_RafyMultiTenancy02", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(4000001);
//            Assert.AreEqual("Test_RafyMultiTenancy03", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(5000001);
//            Assert.AreEqual("Test_RafyMultiTenancy03", dbsetname);

//            dbsetname = MultiTenancyUtility.GetDbSettingName(10000001);
//            Assert.AreEqual("Test_RafyMultiTenancy03", dbsetname);
//        }

//        [TestMethod]
//        public void DataShard_Inserting()
//        {
//            ShardMapConfigManager.Initialize();
//            new MultiTenancyPluginTestApp(true).Startup();

//            var repo = RF.ResolveInstance<OrderRepository>();

//            using (TenantContext.TenantId.UseScopeValue("1"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var entity = new Order
//                    {
//                        OrderName = "database1-order1",
//                        CreateTime = DateTime.Now
//                    };

//                    repo.Save(entity);
//                }
//            }

//            using (TenantContext.TenantId.UseScopeValue("1000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var entity = new Order
//                    {
//                        OrderName = "database1-order1",
//                        CreateTime = DateTime.Now
//                    };

//                    repo.Save(entity);
//                }
//            }

//            using (TenantContext.TenantId.UseScopeValue("2000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var entity = new Order
//                    {
//                        OrderName = "database1-order1",
//                        CreateTime = DateTime.Now
//                    };

//                    repo.Save(entity);
//                }
//            }

//            using (TenantContext.TenantId.UseScopeValue("2000001"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var entity = new Order
//                    {
//                        OrderName = "database2-order2",
//                        CreateTime = DateTime.Now
//                    };

//                    repo.Save(entity);
//                }
//            }

//            using (TenantContext.TenantId.UseScopeValue("4000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var entity = new Order
//                    {
//                        OrderName = "database2-order2",
//                        CreateTime = DateTime.Now
//                    };

//                    repo.Save(entity);
//                }
//            }

//            using (TenantContext.TenantId.UseScopeValue("10000001"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var entity = new Order
//                    {
//                        OrderName = "database3-order3",
//                        CreateTime = DateTime.Now
//                    };

//                    repo.Save(entity);
//                }
//            }

//            var count1 = 0;
//            using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, "Test_RafyMultiTenancy01"))
//            {
//                var list = repo.GetBy(new CommonQueryCriteria()
//                {
//                    new PropertyMatchGroup
//                    {
//                        new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database1-order1")
//                    }
//                });
//                count1 = list.Count;
//            }

//            var count2 = 0;
//            using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, "Test_RafyMultiTenancy02"))
//            {
//                var list = repo.GetBy(new CommonQueryCriteria()
//                {
//                    new PropertyMatchGroup
//                    {
//                        new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database2-order2")
//                    }
//                });
//                count2 = list.Count;
//            }

//            var count3 = 0;
//            using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, "Test_RafyMultiTenancy03"))
//            {
//                var list = repo.GetBy(new CommonQueryCriteria()
//                {
//                    new PropertyMatchGroup
//                    {
//                        new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database3-order3")
//                    }
//                });
//                count3 = list.Count;
//            }

//            Assert.IsTrue(Math.Ceiling((double)count1 / 3) >= 1);
//            Assert.IsTrue(count2 >= 1);
//            Assert.IsTrue(count3 >= 1);
//        }

//        [TestMethod]
//        public void DataShard_Query()
//        {
//            ShardMapConfigManager.Initialize();
//            new MultiTenancyPluginTestApp(true).Startup();

//            var repo = RF.ResolveInstance<OrderRepository>();

//            var count1 = 0;
//            using (TenantContext.TenantId.UseScopeValue("1000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var list = repo.GetBy(new CommonQueryCriteria()
//                    {
//                        new PropertyMatchGroup
//                        {
//                            new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database1-order1")
//                        }
//                    });
//                    count1 = list.Count;
//                }
//            }

//            var count1_1 = 0;
//            using (TenantContext.TenantId.UseScopeValue("2000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var list = repo.GetBy(new CommonQueryCriteria()
//                    {
//                        new PropertyMatchGroup
//                        {
//                            new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database1-order1")
//                        }
//                    });
//                    count1_1 = list.Count;
//                }
//            }

//            var count2 = 0;
//            using (TenantContext.TenantId.UseScopeValue("4000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var list = repo.GetBy(new CommonQueryCriteria()
//                    {
//                        new PropertyMatchGroup
//                        {
//                            new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database2-order2")
//                        }
//                    });
//                    count2 = list.Count;
//                }
//            }

//            var count2_1 = 0;
//            using (TenantContext.TenantId.UseScopeValue("3000000"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var list = repo.GetBy(new CommonQueryCriteria()
//                    {
//                        new PropertyMatchGroup
//                        {
//                            new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database2-order2")
//                        }
//                    });
//                    count2_1 = list.Count;
//                }
//            }

//            var count3 = 0;
//            using (TenantContext.TenantId.UseScopeValue("4000001"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var list = repo.GetBy(new CommonQueryCriteria()
//                    {
//                        new PropertyMatchGroup
//                        {
//                            new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database3-order3")
//                        }
//                    });
//                    count3 = list.Count;
//                }
//            }

//            var count3_1 = 0;
//            using (TenantContext.TenantId.UseScopeValue("78989000001"))
//            {
//                using (RdbDataProvider.RedirectDbSetting(MultiTenancyTestDomainPlugin.DbSettingName, MultiTenancyUtility.GetDbSettingName()))
//                {
//                    var list = repo.GetBy(new CommonQueryCriteria()
//                    {
//                        new PropertyMatchGroup
//                        {
//                            new PropertyMatch(Order.OrderNameProperty, PropertyOperator.Equal, "database3-order3")
//                        }
//                    });
//                    count3_1 = list.Count;
//                }
//            }

//            Assert.IsTrue(count1 == count1_1);
//            Assert.IsTrue(count2 == count2_1);
//            Assert.IsTrue(count3 == count3_1);
//        }
//    }

//    public class MultiTenancyPluginTestApp : DomainApp
//    {
//        public MultiTenancyPluginTestApp()
//        {
//            AutoUpdateDb = false;
//        }

//        public MultiTenancyPluginTestApp(bool autoUpdateDb)
//        {
//            AutoUpdateDb = autoUpdateDb;
//        }

//        public bool AutoUpdateDb { get; set; }

//        protected override void InitEnvironment()
//        {
//            InitMultiTenancyType();
//            RafyEnvironment.DomainPlugins.Add(new MultiTenancyTestDomainPlugin());
//            base.InitEnvironment();
//        }

//        private void InitMultiTenancyType()
//        {
//            MultiTenancyPlugin.Configuration.EnableMultiTenancy(
//              typeof(Order));
//        }

//        protected override void OnRuntimeStarting()
//        {
//            base.OnRuntimeStarting();

//            if (!AutoUpdateDb) return;

//            var dbSettingNames = MultiTenancyUtility.GetDbSettingNames();

//            foreach (var item in dbSettingNames)
//            {
//                MultiTenancyTestDomainPlugin.DbSettingName = item;

//                var svc = ServiceFactory.Create<MigrateService>();

//                svc.Options = new MigratingOptions
//                {
//                    //ReserveHistory = true,//ReserveHistory 表示是否需要保存所有数据库升级的历史记录
//                    RunDataLossOperation = DataLossOperation.All,//要禁止数据库表、字段的删除操作，请使用 DataLossOperation.None 值。
//                    Databases = new string[] { item }
//                };

//                svc.Invoke();
//            }
//        }

//        protected override void OnStartupCompleted()
//        {
//            base.OnStartupCompleted();
//        }
//    }
//}

//namespace MultiTenancyTestDomain
//{
//    [Serializable]
//    public abstract class MultiTenancyTestDomainEntity : LongEntity
//    {
//        #region 构造函数

//        protected MultiTenancyTestDomainEntity() { }

//        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
//        protected MultiTenancyTestDomainEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

//        #endregion

//        #region 一般属性

//        public static readonly Property<DateTime> CreateTimeProperty = P<MultiTenancyTestDomainEntity>.Register(e => e.CreateTime);
//        /// <summary>
//        /// 创建时间
//        /// </summary>
//        public DateTime CreateTime
//        {
//            get { return this.GetProperty(CreateTimeProperty); }
//            set { this.SetProperty(CreateTimeProperty, value); }
//        }

//        #endregion
//    }

//    [Serializable]
//    public abstract class MultiTenancyTestDomainEntityList : EntityList { }

//    public abstract class MultiTenancyTestDomainEntityRepository : EntityRepository
//    {
//        protected MultiTenancyTestDomainEntityRepository() { }
//    }

//    [DataProviderFor(typeof(MultiTenancyTestDomainEntityRepository))]
//    public class MultiTenancyTestDomainEntityRepositoryDataProvider : RdbDataProvider
//    {
//        internal protected override string ConnectionStringSettingName
//        {
//            get { return MultiTenancyTestDomainPlugin.DbSettingName; }
//        }
//    }

//    public abstract class MultiTenancyTestDomainEntityConfig<TEntity> : EntityConfig<TEntity> { }


//    /// <summary>
//    /// 订单
//    /// </summary>
//    [RootEntity, Serializable]
//    public partial class Order : MultiTenancyTestDomainEntity
//    {
//        #region 构造函数

//        public Order() { }

//        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
//        protected Order(SerializationInfo info, StreamingContext context) : base(info, context) { }

//        #endregion

//        #region 引用属性

//        #endregion

//        #region 组合子属性

//        #endregion

//        #region 一般属性

//        public static readonly Property<string> OrderNameProperty = P<Order>.Register(e => e.OrderName);
//        /// <summary>
//        /// 订单名称
//        /// </summary>
//        public string OrderName
//        {
//            get { return this.GetProperty(OrderNameProperty); }
//            set { this.SetProperty(OrderNameProperty, value); }
//        }

//        #endregion

//        #region 只读属性


//        #endregion

//        #region 冗余属性

//        #endregion
//    }

//    /// <summary>
//    /// 订单 列表类。
//    /// </summary>
//    [Serializable]
//    public partial class OrderList : MultiTenancyTestDomainEntityList { }

//    /// <summary>
//    /// 订单 仓库类。
//    /// 负责 订单 类的查询、保存。
//    /// </summary>
//    public partial class OrderRepository : MultiTenancyTestDomainEntityRepository
//    {
//        /// <summary>
//        /// 单例模式，外界不可以直接构造本对象。
//        /// </summary>
//        protected OrderRepository() { }

//        public virtual Order GetById(int id)
//        {
//            return this.GetById(id);
//        }
//    }

//    /// <summary>
//    /// 订单 配置类。
//    /// 负责 订单 类的实体元数据的配置。
//    /// </summary>
//    internal class OrderConfig : MultiTenancyTestDomainEntityConfig<Order>
//    {
//        /// <summary>
//        /// 配置实体的元数据
//        /// </summary>
//        internal protected override void ConfigMeta()
//        {
//            //配置实体的所有属性都映射到数据表中。
//            Meta.MapTable().MapAllProperties();
//        }
//    }

//    partial class OrderList
//    {
//        #region 强类型公有接口

//        /// <summary>
//        /// 获取或设置指定位置的实体。
//        /// </summary>
//        /// <param name="index"></param>
//        /// <returns></returns>
//        public new Order this[int index]
//        {
//            get
//            {
//                return base[index] as Order;
//            }
//            set
//            {
//                base[index] = value;
//            }
//        }

//        /// <summary>
//        /// 获取本实体列表的迭代器。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new IEnumerator<Order> GetEnumerator()
//        {
//            return new EntityListEnumerator<Order>(this);
//        }

//        /// <summary>
//        /// 返回子实体的强类型迭代接口，方便使用 Linq To Object 操作。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public IEnumerable<Order> Concrete()
//        {
//            return this.Cast<Order>();
//        }

//        /// <summary>
//        /// 添加指定的实体到集合中。
//        /// </summary>
//        [DebuggerStepThrough]
//        public void Add(Order entity)
//        {
//            base.Add(entity);
//        }

//        /// <summary>
//        /// 判断本集合是否包含指定的实体。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public bool Contains(Order entity)
//        {
//            return base.Contains(entity);
//        }

//        /// <summary>
//        /// 判断指定的实体在本集合中的索引号。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public int IndexOf(Order entity)
//        {
//            return base.IndexOf(entity);
//        }

//        /// <summary>
//        /// 在指定的位置插入实体。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public void Insert(int index, Order entity)
//        {
//            base.Insert(index, entity);
//        }

//        /// <summary>
//        /// 在集合中删除指定的实体。返回是否成功删除。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public bool Remove(Order entity)
//        {
//            return base.Remove(entity);
//        }

//        #endregion
//    }

//    partial class OrderRepository
//    {
//        #region 私有方法，本类内部使用

//        /// <summary>
//        /// 创建一个实体类的 Linq 查询器
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        private IQueryable<Order> CreateLinqQuery()
//        {
//            return base.CreateLinqQuery<Order>();
//        }

//        #endregion

//        #region 强类型公有接口

//        /// <summary>
//        /// 创建一个新的实体。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new Order New()
//        {
//            return base.New() as Order;
//        }

//        /// <summary>
//        /// 创建一个全新的列表
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList NewList()
//        {
//            return base.NewList() as OrderList;
//        }

//        /// <summary>
//        /// 优先使用缓存中的数据来通过 Id 获取指定的实体对象
//        /// 
//        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetById 并返回结果。
//        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
//        /// </summary>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new Order CacheById(object id)
//        {
//            return base.CacheById(id) as Order;
//        }

//        /// <summary>
//        /// 优先使用缓存中的数据来查询所有的实体类
//        /// 
//        /// 如果该实体的缓存没有打开，则本方法会直接调用 GetAll 并返回结果。
//        /// 如果缓存中没有这些数据，则本方法同时会把数据缓存起来。
//        /// </summary>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList CacheAll()
//        {
//            return base.CacheAll() as OrderList;
//        }

//        /// <summary>
//        /// 通过Id在数据层中查询指定的对象
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new Order GetById(object id, LoadOptions loadOptions = null)
//        {
//            return base.GetById(id, loadOptions) as Order;
//        }

//        /// <summary>
//        /// 查询第一个实体类
//        /// </summary>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new Order GetFirst(LoadOptions loadOptions = null)
//        {
//            return base.GetFirst(loadOptions) as Order;
//        }

//        /// <summary>
//        /// 分页查询所有的实体类
//        /// </summary>
//        /// <param name="paging"></param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetAll(PagingInfo paging = null, LoadOptions loadOptions = null)
//        {
//            return base.GetAll(paging, loadOptions) as OrderList;
//        }

//        /// <summary>
//        /// 获取指定 id 集合的实体列表。
//        /// </summary>
//        /// <param name="idList"></param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetByIdList(object[] idList, LoadOptions loadOptions = null)
//        {
//            return base.GetByIdList(idList, loadOptions) as OrderList;
//        }

//        /// <summary>
//        /// 通过组合父对象的 Id 列表，查找所有的组合子对象的集合。
//        /// </summary>
//        /// <param name="parentIdList"></param>
//        /// <param name="paging">分页信息。</param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetByParentIdList(object[] parentIdList, PagingInfo paging = null, LoadOptions loadOptions = null)
//        {
//            return base.GetByParentIdList(parentIdList, paging, loadOptions) as OrderList;
//        }

//        /// <summary>
//        /// 通过父对象 Id 分页查询子对象的集合。
//        /// </summary>
//        /// <param name="parentId"></param>
//        /// <param name="paging">分页信息。</param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetByParentId(object parentId, PagingInfo paging = null, LoadOptions loadOptions = null)
//        {
//            return base.GetByParentId(parentId, paging, loadOptions) as OrderList;
//        }

//        /// <summary>
//        /// 通过 CommonQueryCriteria 来查询实体列表。
//        /// </summary>
//        /// <param name="criteria">常用查询条件。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetBy(CommonQueryCriteria criteria)
//        {
//            return base.GetBy(criteria) as OrderList;
//        }

//        /// <summary>
//        /// 通过 CommonQueryCriteria 来查询单一实体。
//        /// </summary>
//        /// <param name="criteria">常用查询条件。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new Order GetFirstBy(CommonQueryCriteria criteria)
//        {
//            return base.GetFirstBy(criteria) as Order;
//        }

//        /// <summary>
//        /// 递归查找所有树型子
//        /// </summary>
//        /// <param name="treeIndex"></param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetByTreeParentIndex(string treeIndex, LoadOptions loadOptions = null)
//        {
//            return base.GetByTreeParentIndex(treeIndex, loadOptions) as OrderList;
//        }

//        /// <summary>
//        /// 查找指定树节点的直接子节点。
//        /// </summary>
//        /// <param name="treePId">需要查找的树节点的Id.</param>
//        /// <param name="loadOptions">数据加载时选项（贪婪加载等）。</param>
//        /// <returns></returns>
//        [DebuggerStepThrough]
//        public new OrderList GetByTreePId(object treePId, LoadOptions loadOptions = null)
//        {
//            return base.GetByTreePId(treePId, loadOptions) as OrderList;
//        }

//        #endregion
//    }

//    public class MultiTenancyTestDomainPlugin : DomainPlugin
//    {
//        public static string DbSettingName = "Test_RafyMultiTenancy01";

//        public override void Initialize(IApp app)
//        {
//        }
//    }
//}

