using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OEA;
using OEA.Library;
using OEA.Library._Test;
using OEA.Reflection;
using OEA.ORM.DbMigration;
using OEA.Utils;
using System.Reflection;
using System.ComponentModel;

namespace OEAUnitTest
{
    [TestClass]
    public class EntityTest : TestBase
    {
        [ClassInitialize]
        public static void EntityTest_ClassInitialize(TestContext context)
        {
            ClassInitialize(context, true);

            using (new OEADbMigrationContext(UnitTestEntity.DbSetting).AutoMigrate()) { }
        }

        [TestMethod]
        public void EntityTest_RoutedEvent()
        {
            //创建对象
            var user = new TestUser();
            var list = user.TestTreeTaskList;
            var taskRoot = list.AddNew().CastTo<TestTreeTask>();
            var task1 = list.AddNew().CastTo<TestTreeTask>();
            var task11 = list.AddNew().CastTo<TestTreeTask>();
            var task111 = list.AddNew().CastTo<TestTreeTask>();
            var task112 = list.AddNew().CastTo<TestTreeTask>();
            var task12 = list.AddNew().CastTo<TestTreeTask>();
            var task2 = list.AddNew().CastTo<TestTreeTask>();
            var taskRoot2 = list.AddNew().CastTo<TestTreeTask>();

            //关系
            task1.TreeParent = taskRoot;
            task11.TreeParent = task1;
            task111.TreeParent = task11;
            task112.TreeParent = task11;
            task12.TreeParent = task1;
            task2.TreeParent = taskRoot;

            Assert.AreEqual(taskRoot.AllTime, 0);

            task111.AllTime += 1;
            Assert.AreEqual(task11.AllTime, 1);
            Assert.AreEqual(task1.AllTime, 1);
            Assert.AreEqual(taskRoot.AllTime, 1);
            Assert.AreEqual(user.TasksTime, 1);

            task12.AllTime += 1;
            Assert.AreEqual(task1.AllTime, 2);
            Assert.AreEqual(taskRoot.AllTime, 2);
            Assert.AreEqual(user.TasksTime, 2);

            task2.AllTime += 1;
            Assert.AreEqual(task1.AllTime, 2);
            Assert.AreEqual(taskRoot.AllTime, 3);
            Assert.AreEqual(user.TasksTime, 3);

            taskRoot2.AllTime += 1;
            Assert.AreEqual(user.TasksTime, 4);

            task111.AllTime -= 1;
            Assert.AreEqual(task11.AllTime, 0);
            Assert.AreEqual(task1.AllTime, 1);
            Assert.AreEqual(taskRoot.AllTime, 2);
            Assert.AreEqual(user.TasksTime, 3);
        }

        [TestMethod]
        public void EntityTest_AutoCollect()
        {
            //创建对象
            var user = new TestUser();
            var list = user.TestTreeTaskList;
            var taskRoot = list.AddNew().CastTo<TestTreeTask>();
            var task1 = list.AddNew().CastTo<TestTreeTask>();
            var task11 = list.AddNew().CastTo<TestTreeTask>();
            var task111 = list.AddNew().CastTo<TestTreeTask>();
            var task112 = list.AddNew().CastTo<TestTreeTask>();
            var task12 = list.AddNew().CastTo<TestTreeTask>();
            var task2 = list.AddNew().CastTo<TestTreeTask>();
            var taskRoot2 = list.AddNew().CastTo<TestTreeTask>();

            //关系
            task1.TreeParent = taskRoot;
            task11.TreeParent = task1;
            task111.TreeParent = task11;
            task112.TreeParent = task11;
            task12.TreeParent = task1;
            task2.TreeParent = taskRoot;

            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 0);

            task111.AllTimeByAutoCollect += 1;
            Assert.AreEqual(task11.AllTimeByAutoCollect, 1);
            Assert.AreEqual(task1.AllTimeByAutoCollect, 1);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 1);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 1);

            task12.AllTimeByAutoCollect += 1;
            Assert.AreEqual(task1.AllTimeByAutoCollect, 2);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 2);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 2);

            task2.AllTimeByAutoCollect += 1;
            Assert.AreEqual(task1.AllTimeByAutoCollect, 2);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 3);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 3);

            taskRoot2.AllTimeByAutoCollect += 1;
            Assert.AreEqual(user.TasksTimeByAutoCollect, 4);

            task111.AllTimeByAutoCollect -= 1;
            Assert.AreEqual(task11.AllTimeByAutoCollect, 0);
            Assert.AreEqual(task1.AllTimeByAutoCollect, 1);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 2);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 3);
        }

        [TestMethod]
        public void EntityTest_LazyRef_Serialization_OnServer()
        {
            EnterLocation(OEALocation.WPFServer, () =>
            {
                var role = new TestRole
                {
                    TestUser = new TestUser
                    {
                        Id = 1,
                        Name = "TestUser"
                    }
                };

                var lazyRef = role.GetLazyRef(TestRole.TestUserRefProperty);
                Assert.IsTrue(lazyRef.GetSerializeEntityValue(), "默认在服务端，应该是可以序列化实体的。");

                var roleCloned = ObjectCloner.Clone(role);
                var lazyRefCloned = roleCloned.GetLazyRef(TestRole.TestUserRefProperty);
                Assert.IsTrue(lazyRefCloned.LoadedOrAssigned, "服务端到客户端，需要序列化实体。");
            });
        }

        [TestMethod]
        public void EntityTest_LazyRef_Serialization_OnClient()
        {
            EnterLocation(OEALocation.Client, () =>
            {
                var role = new TestRole
                {
                    TestUser = new TestUser
                    {
                        Id = 1,
                        Name = "TestUser"
                    }
                };

                var lazyRef = role.GetLazyRef(TestRole.TestUserRefProperty);
                Assert.IsFalse(lazyRef.GetSerializeEntityValue(), "默认在服务端，应该是不可以序列化实体的。");

                var roleCloned = ObjectCloner.Clone(role);
                var lazyRefCloned = roleCloned.GetLazyRef(TestRole.TestUserRefProperty);
                Assert.IsFalse(lazyRefCloned.LoadedOrAssigned, "服务端到客户端，不需要序列化实体。");
            });
        }

        [TestMethod]
        public void EntityTest_LazyRef_Serialization_Manual()
        {
            var role = new TestRole
            {
                TestUser = new TestUser
                {
                    Id = 1,
                    Name = "TestUser"
                }
            };

            var lazyRef = role.GetLazyRef(TestRole.TestUserRefProperty);

            lazyRef.SerializeEntity = true;
            var roleCloned = ObjectCloner.Clone(role);
            var lazyRefCloned = roleCloned.GetLazyRef(TestRole.TestUserRefProperty);
            Assert.IsTrue(lazyRefCloned.LoadedOrAssigned, "需要序列化实体。");

            lazyRef.SerializeEntity = false;
            roleCloned = ObjectCloner.Clone(role);
            lazyRefCloned = roleCloned.GetLazyRef(TestRole.TestUserRefProperty);
            Assert.IsFalse(lazyRefCloned.LoadedOrAssigned, "不需要序列化实体。");
        }

        [TestMethod]
        public void EntityTest_ORM_EntityContext_Query()
        {
            var repo = RF.Concreate<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = new TestUser();
                repo.Save(user);

                var user2 = repo.GetById(user.Id);
                Assert.IsTrue(user != user2, "实体上下文块外，二者应该是不同的对象。");

                using (RF.EnterEntityContext())
                {
                    var user3 = repo.GetById(user.Id);
                    var user4 = repo.GetById(user.Id);
                    Assert.IsTrue(user3 == user4, "实体上下文块内，二者应该是同一个对象。");
                }
            }
        }

        [TestMethod]
        public void EntityTest_ORM_EntityContext_Insert()
        {
            var repo = RF.Concreate<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                using (RF.EnterEntityContext())
                {
                    var user = new TestUser();
                    repo.Save(user);

                    var user2 = repo.GetById(user.Id);
                    Assert.IsTrue(user == user2, "实体上下文块内，二者应该是同一个对象。");
                }
            }
        }

        [TestMethod]
        public void EntityTest_ORM_EntityContext_Update()
        {
            var repo = RF.Concreate<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = new TestUser();
                repo.Save(user);

                using (RF.EnterEntityContext())
                {
                    user.MarkModified();
                    repo.Save(user);

                    var user2 = repo.GetById(user.Id);
                    Assert.IsTrue(user == user2, "实体上下文块内，二者应该是同一个对象。");
                }
            }
        }

        [TestMethod]
        public void EntityTest_Property_EnumForUI()
        {
            var entity = new TestRole
            {
                RoleType = RoleType.Administrator
            };
            var properties = (entity as ICustomTypeDescriptor).GetProperties();
            var enumProperty = properties.Find("RoleType", false);
            Assert.IsNotNull(enumProperty, "TestRole 类型上没有找到 RoleType 枚举属性。");

            Assert.IsTrue(enumProperty.PropertyType == typeof(string), "枚举属性在界面的返回值类型应该是字符串。");

            var value = enumProperty.GetValue(entity) as string;
            Assert.IsTrue(value != null && value == "管理员", "枚举属性在界面的返回值应该是枚举在 Label 标签中定义的字符串。");

            enumProperty.SetValue(entity, "一般");
            Assert.IsTrue(entity.RoleType == RoleType.Normal, "枚举属性被界面设置字符串的值时，应该转换为相应的枚举值。");
        }

        //[TestMethod]
        //public void EntityTest_Repository()
        //{
        //    var pbsRepository = RF.Create<PBS>();
        //    var pbss = pbsRepository.GetAll();
        //    if (pbss.Count > 0)
        //    {
        //        var pbsOriginal = pbss[0] as PBS;
        //        var pbs = pbsRepository.GetById(pbsOriginal.Id) as PBS;
        //        Assert.IsNotNull(pbs);
        //        Assert.AreEqual(pbs.Id, pbsOriginal.Id);
        //        Assert.AreEqual(pbs.PBSTypeId, pbsOriginal.PBSTypeId);
        //        Assert.AreEqual(pbs.Name, pbsOriginal.Name);
        //    }
        //}

        //[TestMethod]
        //public void EntityTest_Repository_Override()
        //{
        //    RF.OverrideRepository<ContractBudget, RealContractBudget>();
        //    var r = RF.Concreate<ContractBudgetRepository>();
        //    Assert.AreEqual(r.GetType().Name, "RealContractBudgetRepository");
        //    Assert.AreEqual(r.New().GetType(), typeof(RealContractBudget));

        //    var r2 = RF.Create<ContractBudget>();
        //    Assert.AreEqual(r2.New().GetType(), typeof(RealContractBudget));
        //}

        //[TestMethod]
        //public void EntityTest_AggregateSQL_LoadReferenceEntities()
        //{
        //    var api = AggregateSQL.Instance;

        //    var loadOptions = api
        //        .BeginLoadOptions<ProjectPBS>()
        //        .LoadChildren(pp => pp.ProjectPBSPropertyValues)
        //        .Order<ProjectPBSPropertyValue>().By(v => (v as ProjectPBSPropertyValue).PBSProperty.OrderNo)
        //        .LoadFK(v => v.PBSProperty)
        //        .LoadChildren(p => p.PBSPropertyOptionalValues);

        //    var projectId = new Guid("93369894-5182-4E5A-BC49-91198CE1F092");//江南四期1、4区项目
        //    var sql = api.GenerateQuerySQL(loadOptions, projectId);

        //    //聚合加载整个对象树。
        //    var entities = api.LoadEntities(sql, loadOptions);

        //    //以下使用时间进行测试，如果整个遍历用不了0.1秒，则表示没有多余的数据库操作，聚合加载成功。
        //    var watch = new System.Diagnostics.Stopwatch();
        //    watch.Start();
        //    foreach (ProjectPBS pbs in entities)
        //    {
        //        foreach (ProjectPBSPropertyValue p in pbs.ProjectPBSPropertyValues)
        //        {
        //            foreach (var v in p.PBSProperty.PBSPropertyOptionalValues) ;
        //        }
        //    }
        //    watch.Stop();
        //    Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 100);
        //}

        //[TestMethod]
        //public void EntityTest_AggregateSQL_LoadEntities()
        //{
        //    var api = AggregateSQL.Instance;

        //    var loadOptions = api
        //        .BeginLoadOptions<PBS>().LoadChildren(pbs => pbs.PBSPropertys)
        //        .Order<PBSProperty>().By(p => p.Name)
        //        //.Continue<PBSProperty>()
        //        .LoadChildren(p => p.PBSPropertyOptionalValues)
        //        .Continue<PBSPropertyOptionalValue>().LoadChildren(p => p.PBSPropertyOptionalValueFilters);

        //    var id = new Guid("3C23DF98-83AE-4802-95F4-E91A8ADAC8BF");//广州城建组团项目PBS结构模板(住宅)
        //    var sql = api.GenerateQuerySQL(loadOptions, id);
        //    var sql2 = api.GenerateQuerySQL(loadOptions, string.Format("PBS.PBSTypeId = '{0}'", id));

        //    Assert.AreEqual(sql, sql2);

        //    //聚合加载整个对象树。
        //    var entities = api.LoadEntities(sql, loadOptions);

        //    //以下使用时间进行测试，如果整个遍历用不了0.1秒，则表示没有多余的数据库操作，聚合加载成功。
        //    var watch = new System.Diagnostics.Stopwatch();
        //    watch.Start();
        //    foreach (PBS pbs in entities)
        //    {
        //        foreach (PBSProperty p in pbs.PBSPropertys)
        //        {
        //            foreach (PBSPropertyOptionalValue v in p.PBSPropertyOptionalValues)
        //            {
        //                foreach (PBSPropertyOptionalValueFilter f in v.PBSPropertyOptionalValueFilters) ;
        //            }
        //        }
        //    }
        //    watch.Stop();
        //    Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 100);
        //}

        //[TestMethod]
        //public void EntityTest_AggregateSQL_LoadSingleChild()
        //{
        //    var pbsTypeId = Guid.NewGuid();
        //    var sqlSimple = AggregateSQL.Instance.GenerateQuerySQL<PBS>(
        //        option => option.LoadChildren(pbs => pbs.PBSBQItems),
        //        pbsTypeId
        //        );
        //    var pbsList = AggregateSQL.Instance.LoadEntities<PBS>(
        //        option => option.LoadChildren(pbs => pbs.PBSBQItems),
        //        pbsTypeId
        //        );
        //}

        //[TestMethod]
        //public void EntityTest_AggregateSQL_JoinWhere()
        //{
        //    var sqlSimple = AggregateSQL.Instance.GenerateQuerySQL<PBS>(
        //        option => option.LoadChildren(pbs => pbs.PBSBQItems),
        //        "pp.ProjectId = '984048A9-594E-4C3B-BE9A-69C0CF5A9BA2'",
        //        "JOIN ProjectPBS as pp on PBS.Id = pp.PBSId"
        //        );
        //}

        //[TestMethod]
        //public void EntityTest_AddBatch()
        //{
        //    //以下测试是无法直接通过的，因为无法插入相同的数据

        //    //var pbsTypes = RF.Create<PBSType>();
        //    //var pbsType1 = pbsTypes.GetAll()[3] as PBSType;
        //    //pbsType1.PBSBQItemsLoader.WaitForLoading();
        //    //pbsType1.PBSNormItemsLoader.WaitForLoading();
        //    //pbsType1.PBSPropertiesLoader.WaitForLoading();
        //    //var reader = new EntityChldrenBatchReader(pbsType1);
        //    //var dic = reader.Read();

        //    //foreach (var dicItem in dic)
        //    //{
        //    //    var repository = RF.Create(dicItem.Key);
        //    //    repository.AddBatch(dicItem.Value);
        //    //}
        //}

        private static TEntity Get<TEntity>()
            where TEntity : Entity, new()
        {
            var e = new TEntity();
            e.MarkUnchanged();
            return e;
        }
    }
}