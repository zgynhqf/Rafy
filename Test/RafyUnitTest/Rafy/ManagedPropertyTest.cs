using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM.DbMigration;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Serialization;
using Rafy.Serialization.Mobile;
using Rafy.Utils;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class ManagedPropertyTest
    {
        [ClassInitialize]
        public static void AutoUITest_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region MPT Core

        [TestMethod]
        public void MPT_DefaultValue()
        {
            var user = Get<TestUser>();
            Assert.AreEqual(user.Name, "DefaultName");
            Assert.AreEqual(user.Age, 10);
        }

        [TestMethod]
        public void MPT_CoerceGetValue()
        {
            var user = Get<TestUser>();

            Assert.AreEqual(user.Name, "DefaultName");

            user.Name = "123456789012345678901234567890";//30位

            Assert.AreEqual(user.Name.Length, 20);
        }

        [TestMethod]
        public void MPT_PropertyChanging_Cancel_RefEntity()
        {
            //当 IsLock 时，TestUser 属性无效。
            var role = Get<TestRole>();
            role.IsLock = true;

            var user = Get<TestUser>();
            role.TestUser = user;
            Assert.AreEqual(role.TestUserId, default(int));
            Assert.IsNull(role.TestUser);

            role.TestUserId = user.Id;
            Assert.AreEqual(role.TestUserId, default(int));
            Assert.IsNull(role.TestUser);
        }

        [TestMethod]
        public void MPT_PropertyChanging_Cancel()
        {
            var user = Get<TestUser>();

            Assert.AreEqual(user.Age, 10);

            //小于0时，无效
            user.Age = -1;
            Assert.AreEqual(user.Age, 10);
        }

        [TestMethod]
        public void MPT_PropertyChanging_CoercedValue()
        {
            var user = Get<TestUser>();

            Assert.AreEqual(user.Age, 10);

            //大于 100 时，值应该是100
            user.Age = 101;
            Assert.AreEqual(user.Age, 100);

            user.Age = 99;
            Assert.AreEqual(user.Age, 99);
        }

        [TestMethod]
        public void MPT_PropertyChanged_StaticEvent()
        {
            var user = Get<TestUser>();
            Assert.AreEqual(user.AgeChangedInternally_Property, 0);
            Assert.AreEqual(user.AgeChangedInternally_UIOperating, 0);

            user.Age = 1;
            Assert.AreEqual(user.AgeChangedInternally_Property, 1);
            Assert.AreEqual(user.AgeChangedInternally_UIOperating, 0);

            user.SetProperty(TestUser.AgeProperty, 2, ManagedPropertyChangedSource.FromUIOperating);
            Assert.AreEqual(user.AgeChangedInternally_Property, 1);
            Assert.AreEqual(user.AgeChangedInternally_UIOperating, 2);
        }

        [TestMethod]
        public void MPT_PropertyChanged_StaticEvent_RefEntityProperty()
        {
            var role = Get<TestRole>();
            Assert.AreEqual(role.TestUserIdChangedInternal, default(int));
            Assert.AreEqual(role.TestUserChangedInternal, null);

            var user = Get<TestUser>();
            user.Id = RafyEnvironment.NewLocalId();
            role.TestUser = user;

            Assert.AreEqual(role.TestUserIdChangedInternal, user.Id);
            Assert.AreEqual(role.TestUserChangedInternal, user);
        }

        [TestMethod]
        public void MPT_PropertyChanged_InstanceEvent()
        {
            var role = Get<TestRole>();

            var properties = new List<string>();
            role.PropertyChanged += (o, e) => { properties.Add(e.PropertyName); };

            //一般属性
            role.Name = "Name";
            Assert.AreEqual(role.Name, "Name");
            Assert.IsTrue(properties.Contains("Name"));

            //引用实体属性
            var user = Get<TestUser>();
            user.Id = RafyEnvironment.NewLocalId();
            role.TestUser = user;
            Assert.IsTrue(properties.Contains("TestUser"));
            Assert.IsTrue(properties.Contains("TestUserId"));
        }

        [TestMethod]
        public void MPT_PropertyChanged_CallDirectly()
        {
            var role = Get<TestRole>();

            var properties = new List<string>();
            role.PropertyChanged += (o, e) => { properties.Add(e.PropertyName); };

            role.NotifyPropertyChanged(TestRole.NameProperty);
            Assert.IsTrue(properties.Contains("Name"));

            role.NotifyPropertyChanged(TestRole.TestUserProperty);
            Assert.IsTrue(properties.Contains("TestUser"));
            Assert.IsTrue(!properties.Contains("TestUserId"));

            role.NotifyPropertyChanged(TestRole.TestUserIdProperty);
            Assert.IsTrue(properties.Contains("TestUserId"));
        }

        [TestMethod]
        public void MPT_PropertyChanged_CallDirectly_All()
        {
            var role = Get<TestRole>();

            var properties = new List<string>();
            role.PropertyChanged += (o, e) => { properties.Add(e.PropertyName); };

            role.NotifyAllPropertiesChanged();
            Assert.IsTrue(properties.Contains("Name"));
            Assert.IsTrue(properties.Contains("TestUser"));
            Assert.IsTrue(properties.Contains("TestUserId"));
        }

        [TestMethod]
        public void MPT_ExtensionProperties()
        {
            var user = Get<TestUser>();
            Assert.AreEqual(TestUserExt.GetUserCode(user), "DefaultUserCode");

            TestUserExt.SetUserCode(user, "NewCode");
            Assert.AreEqual(TestUserExt.GetUserCode(user), "NewCode");

            var properties = TypeDescriptor.GetProperties(user);
            var property = properties.Find("TestUserExt_UserCode", false);
            Assert.IsNotNull(property);
            Assert.IsFalse(property.IsReadOnly);
        }

        [TestMethod]
        public void ___MPT_ExtensionProperties_CreateListControl()
        {
            //var grid = AutoUI.BlockUIFactory.CreateTreeListControl(
            //    AppModel.EntityViews.FindViewMeta(typeof(TestUser)), ShowInWhere.List
            //    );
            //var grid2 = AutoUI.BlockUIFactory.CreateTreeListControl(
            //    AppModel.EntityViews.FindViewMeta(typeof(TestAdministrator)), ShowInWhere.List
            //    );

            //if (RafyEnvironment.Location.IsOnClient())
            //{
            //    Assert.AreEqual(grid.Columns.Count(), 5);
            //    Assert.AreEqual(grid2.Columns.Count(), 6);
            //}
            //else
            //{
            //    Assert.AreEqual(grid.Columns.Count(), 4);
            //    Assert.AreEqual(grid2.Columns.Count(), 4);
            //}
        }

        [TestMethod]
        public void MPT_ReadOnlyProperties()
        {
            var user = Get<TestUser>();
            Assert.AreEqual(user.ReadOnlyNameAge, "DefaultName 的年龄是 10");

            var properties = new List<string>();
            user.PropertyChanged += (o, e) => { properties.Add(e.PropertyName); };

            user.Name = "huqf";
            Assert.IsTrue(properties.Contains("Name"));
            Assert.IsTrue(properties.Contains("ReadOnlyNameAge"));
            Assert.AreEqual(user.ReadOnlyNameAge, "huqf 的年龄是 10");

            properties.Clear();
            user.Age = 25;
            Assert.IsTrue(properties.Contains("Age"));
            Assert.IsTrue(properties.Contains("ReadOnlyNameAge"));
            Assert.AreEqual(user.ReadOnlyNameAge, "huqf 的年龄是 25");

            var typeProperties = TypeDescriptor.GetProperties(user);
            var property = typeProperties.Find("ReadOnlyNameAge", false);
            Assert.IsNotNull(property);
            Assert.IsTrue(property.IsReadOnly);
        }

        [TestMethod]
        public void MPT_ReadOnlyExtensionProperties()
        {
            var user = Get<TestUser>();

            var properties = new List<string>();
            user.PropertyChanged += (o, e) => { properties.Add(e.PropertyName); };

            properties.Clear();
            TestUserExt.SetUserCode(user, "NewCode");
            Assert.IsTrue(properties.Contains("TestUserExt_UserCode"));
            Assert.IsTrue(properties.Contains("TestUserExt_ReadOnlyUserCode"));
            Assert.IsTrue(properties.Contains("TestUserExt_ReadOnlyUserCodeShadow"));
            Assert.AreEqual(TestUserExt.GetUserCode(user), "NewCode");
            Assert.AreEqual(TestUserExt.GetReadOnlyUserCode(user), "NewCode ReadOnly!");
            Assert.AreEqual(TestUserExt.GetReadOnlyUserCodeShadow(user), "NewCode ReadOnly!");

            var typeProperties = TypeDescriptor.GetProperties(user);
            var property = typeProperties.Find("TestUserExt_ReadOnlyUserCode", false);
            Assert.IsNotNull(property);
            Assert.IsTrue(property.IsReadOnly);
            property = typeProperties.Find("TestUserExt_ReadOnlyUserCodeShadow", false);
            Assert.IsNotNull(property);
            Assert.IsTrue(property.IsReadOnly);
        }

        [TestMethod]
        public void MPT_DynamicProperties()
        {
            TestDynamicPropertiesCore(Get<TestUser>());
        }

        [TestMethod]
        public void MPT_DynamicProperties_Inheritance()
        {
            TestDynamicPropertiesCore(Get<TestAdministrator>());
        }

        private static void TestDynamicPropertiesCore(TestUser admin)
        {
            var container = admin.PropertiesContainer;
            var count1 = container.GetAvailableProperties().Count;

            //为 TestUser 注册属性
            var DynamicNameProperty = P<TestUser>.RegisterExtension<string>("DynamicName", typeof(ManagedPropertyTest));
            var DynamicAgeProperty = P<TestUser>.RegisterExtension("DynamicAge", typeof(ManagedPropertyTest), 10);

            var count2 = container.GetAvailableProperties().Count;
            Assert.AreEqual(count2, count1 + 2);

            Assert.AreEqual(admin.GetProperty(DynamicNameProperty), string.Empty);
            Assert.AreEqual(admin.GetProperty(DynamicAgeProperty), 10);

            admin.SetProperty(DynamicNameProperty, "Haha");
            admin.SetProperty(DynamicAgeProperty, 12);
            Assert.AreEqual(admin.GetProperty(DynamicNameProperty), "Haha");
            Assert.AreEqual(admin.GetProperty(DynamicAgeProperty), 12);

            P.UnRegister(DynamicAgeProperty, DynamicNameProperty);

            var count3 = container.GetAvailableProperties().Count;

            Assert.AreEqual(count1, count3);
        }

        [TestMethod]
        public void MPT_Serialization()
        {
            var e1 = Get<TestUser>();
            e1.Age = 15;
            e1._mySelfReference = e1;
            TestUserExt.SetUserCode(e1, "TestUserExt_UserCode");

            Assert.AreEqual(e1.Validate().Count, 1);

            //在这里可以查看序列化后传输的字符串
            var serializedString = MobileFormatter.SerializeToString(e1);
            Assert.IsNotNull(serializedString);
            var serializedXml = MobileFormatter.SerializeToXml(e1);
            Assert.IsNotNull(serializedXml);

            #region 复制对象（序列化+反序列化）

            var e2 = ObjectCloner.Clone(e1).CastTo<TestUser>();

            //实体直接定义的字段
            Assert.AreEqual(e1._ageNonserailizable, 15);
            Assert.AreEqual(e1._ageSerailizable, 15);
            Assert.AreEqual(e2._ageSerailizable, 15);
            Assert.AreEqual(e2._ageNonserailizable, 0);
            Assert.AreEqual(e2._mySelfReference, e2);
            Assert.AreEqual(e2._now, DateTime.Today);

            //Rafy属性
            Assert.IsTrue(e2.IsDirty);
            Assert.IsFalse(e2.IsNew);
            Assert.IsFalse(e2.IsDeleted);

            //一般属性
            Assert.AreEqual(e2.Age, 15);
            Assert.AreEqual(e1.Age, 15);

            //默认属性
            Assert.AreEqual(e2.Id, e1.Id);
            Assert.AreEqual(e2.Name, "DefaultName");
            Assert.AreEqual(e1.Name, "DefaultName");

            #endregion

            #region 检测具体的序列化的值

            var si = new SerializationContainerContext(new SerializationInfoContainer(0), null);
            si.IsProcessingState = true;
            e1.CastTo<IMobileObject>().SerializeState(si);

            var list = si.States.Keys.ToArray();

            Assert.IsTrue(list.Contains("Age"), "Age 是属性值，需要序列化");
            Assert.IsTrue(list.Contains("TestUserExt_UserCode"), "UserCode 是扩展属性值，需要序列化");
            Assert.IsTrue(!list.Contains("Name"), "Name 是默认值，不需要序列化");
            Assert.IsTrue(!list.Contains("Id"), "Id 是默认值，不需要序列化");

            #endregion
        }

        [TestMethod]
        public void MPT_WPFBinding()
        {
            var userList = RF.Find<TestUser>().NewList();
            var newUser = Get<TestUser>();
            newUser.Name = "1";
            userList.Add(newUser);

            //创建 binding view
            var view = CollectionViewSource.GetDefaultView(userList) as ListCollectionView;
            Assert.IsNotNull(view);
            var userListView = view.SourceCollection as TestUserList;
            Assert.AreEqual(userListView, userList);

            //list change 事件
            NotifyCollectionChangedEventArgs eventArgs = null;
            userListView.CollectionChanged += (oo, ee) => { eventArgs = ee; };

            newUser = Get<TestUser>();
            newUser.Name = "2";
            userList.Add(newUser);

            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(eventArgs.Action, NotifyCollectionChangedAction.Add);
            Assert.AreEqual(userListView.Count, userList.Count);

            //测试 ICustomTypeDescriptor 的实现
            var newUserView = userListView[1];
            var properties = (newUserView as ICustomTypeDescriptor).GetProperties();
            var managedProperties = newUser.PropertiesContainer.GetAvailableProperties();
            Assert.IsTrue(managedProperties.All(mp => properties.Cast<PropertyDescriptor>().Any(p => p.Name == mp.Name)));
            var clrProperties = typeof(TestUser).GetProperties();
            Assert.IsTrue(clrProperties.All(clr => properties.Cast<PropertyDescriptor>().Any(p => p.Name == clr.Name)));

            //view model 的属性更改事件
            var namePropertyChanged = false;
            newUserView.PropertyChanged += (oo, ee) =>
            {
                if (ee.PropertyName == "Name") { namePropertyChanged = true; }
            };
            newUser.Name += "_Modified";
            Assert.IsTrue(namePropertyChanged);
        }

        [TestMethod]
        public void MPT_Override_Merge()
        {
            var house = new House();
            house.Id = "changed";
            Assert.IsTrue(house.IdChanged, "重写元数据后，不能影响基类的元数据。");
        }

        #endregion

        #region MPT ORM

        [TestMethod]
        public void MPT_ORM_PropertiesMapping()
        {
            var repo = RF.Concrete<TestUserRepository>();

            //清空历史数据
            var user = repo.GetByName("huqf");
            if (user != null)
            {
                user.MarkDeleted();
                repo.Save(user);
            }

            //新建用户并设置一些值。
            var user1 = repo.New().CastTo<TestUser>();
            user1.Name = "huqf";
            TestUserExt.SetUserCode(user1, "NewUserCode");

            //为用户添加一个角色
            var userRoles = user1.TestRoleList;
            var role = new TestRole();
            userRoles.Add(role);
            role.Name = "admin";
            Assert.AreEqual(user1.Id, role.TestUserId);

            //保存新建用户
            repo.Save(user1);

            //重新获取保存的用户
            var user2 = repo.GetByName("huqf");
            Assert.IsNotNull(user2);
            Assert.AreEqual(user1.Name, user2.Name);
            Assert.AreEqual(TestUserExt.GetUserCode(user2), "NewUserCode");
            Assert.AreEqual(user1.TestRoleList.Count, 1);
            Assert.AreEqual(user1.TestRoleList[0].CastTo<TestRole>().Name, "admin");

            //获取 Role
            var role2 = RF.Find<TestRole>().GetById(role.Id).CastTo<TestRole>();
            Assert.AreEqual(role.Name, "admin");
            Assert.AreEqual(role2.Name, "admin");
            Assert.AreEqual(user1.Id, role2.TestUserId);
            Assert.IsNotNull(role2.TestUser);

            //删除用户
            user1.MarkDeleted();
            repo.Save(user1);
            var users = repo.GetAll();
            Assert.IsTrue(!users.Cast<TestUser>().Any(u => u.Name == "huqf"));
        }

        [TestMethod]
        public void MPT_ORM_PropertiesMapping_Inheritance()
        {
            var repo = RF.Find<TestAdministrator>() as TestUserRepository;

            //清空历史数据
            var user = repo.GetByName("huqf");
            if (user != null)
            {
                user.MarkDeleted();
                repo.Save(user);
            }

            //新建用户并设置一些值。
            var user1 = repo.New().CastTo<TestAdministrator>();
            user1.Name = "huqf";
            user1.Level = 1;
            TestUserExt.SetUserCode(user1, "NewUserCode");

            //为用户添加一个角色
            var userRoles = user1.TestRoleList;
            var role = new TestRole();
            userRoles.Add(role);
            role.Name = "admin";
            Assert.AreEqual(user1.Id, role.TestUserId);

            //保存新建用户
            repo.Save(user1);

            //重新获取保存的用户
            var user2 = repo.GetByName("huqf").CastTo<TestAdministrator>();
            Assert.IsNotNull(user2);
            Assert.AreEqual(user1.Name, user2.Name);
            Assert.AreEqual(user2.Level, 1);
            Assert.AreEqual(TestUserExt.GetUserCode(user2), "NewUserCode");
            Assert.AreEqual(user1.TestRoleList.Count, 1);
            Assert.AreEqual(user1.TestRoleList[0].CastTo<TestRole>().Name, "admin");

            //获取 Role
            var role2 = RF.Find<TestRole>().GetById(role.Id).CastTo<TestRole>();
            Assert.AreEqual(role.Name, "admin");
            Assert.AreEqual(role2.Name, "admin");
            Assert.AreEqual(user1.Id, role2.TestUserId);
            Assert.IsNotNull(role2.TestUser);

            //删除用户
            user1.MarkDeleted();
            repo.Save(user1);
            var users = repo.GetAll();
            Assert.IsTrue(!users.Cast<TestUser>().Any(u => u.Name == "huqf"));
        }

        [TestMethod]
        public void MPT_ORM_IsSelfDirty()
        {
            var repo = RF.Concrete<TestUserRepository>();

            //clear
            var e = repo.GetByName("huqf");
            if (e != null)
            {
                e.MarkDeleted();
                repo.Save(e);
            }

            var user = repo.New().CastTo<TestUser>();
            user.Name = "huqf";
            user.NotEmptyCode = "NotEmptyCode";

            Assert.IsTrue(user.IsNew);
            Assert.IsTrue(user.IsDirty);

            repo.Save(user);
            Assert.IsTrue(!user.IsNew);
            Assert.IsTrue(!user.IsDirty);

            user.MarkDeleted();
            Assert.IsTrue(user.IsDeleted);

            repo.Save(user);
            Assert.IsTrue(!user.IsDeleted);
            Assert.IsTrue(!user.IsDirty);
        }

        [TestMethod]
        public void MPT_ORM_ForeignKey()
        {
            var repo = RF.Concrete<TestUserRepository>();

            //clear
            var e = repo.GetByName("huqf");
            if (e != null)
            {
                e.MarkDeleted();
                repo.Save(e);
            }

            var user = repo.New().CastTo<TestUser>();
            user.Name = "huqf";

            //为用户添加一个角色
            var userRoles = user.TestRoleList;
            var role = new TestRole();
            userRoles.Add(role);
            role.Name = "admin";
            Assert.IsTrue(user.Id == role.TestUserId);
            Assert.IsTrue(!user.HasId);

            //保存新建用户
            repo.Save(user);
            Assert.AreEqual(user.Id, role.TestUserId);

            var roles = RF.Concrete<TestRoleRepository>().GetByUserId(user.Id);

            Assert.AreEqual(roles.Count, 1);
            Assert.AreEqual(userRoles.Count, 1);
            Assert.AreEqual(roles[0].CastTo<TestRole>().Name, role.Name);

            //clear
            user.MarkDeleted();
            repo.Save(user);
        }

        #endregion

        #region MPT Redundancy

        [TestMethod]
        public void MPT_Redundancy_SetB()
        {
            var a = new A { Id = 1, Name = "AName" };

            var b = new B { Id = 2, A = a };

            Assert.AreEqual(a.Name, b.AName);
        }

        [TestMethod]
        public void MPT_Redundancy_SetC()
        {
            var a = new A { Id = 1, Name = "AName" };

            var b = new B { Id = 2, A = a };

            var c = new C { Id = 3, B = b };

            Assert.AreEqual(a.Name, c.AName);
        }

        [TestMethod]
        public void MPT_Redundancy_SetBOfC()
        {
            var a1 = new A { Name = "a1" };
            var a2 = new A { Name = "a2" };

            var b1 = new B { A = a1, Id = 1 };
            var b2 = new B { A = a2, Id = 2 };

            var c = new C { B = b1 };
            Assert.AreEqual("a1", c.AName);
            c.B = b2;
            Assert.AreEqual("a2", c.AName);
        }

        [TestMethod]
        public void MPT_Redundancy_UpdateB()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var a = new A { Name = "AName" };
                Save(a);

                var b = new B { A = a };
                Save(b);

                a.Name = "New Name";
                Save(a);

                var b2 = RF.Concrete<BRepository>().GetById(b.Id) as B;
                Assert.AreEqual("New Name", b2.AName);
            }
        }

        [TestMethod]
        public void MPT_Redundancy_UpdateC()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var a = new A { Name = "AName" };
                Save(a);

                var b = new B { A = a };
                Save(b);

                var c = new C { B = b };
                Save(c);

                a.Name = "New Name";
                Save(a);

                var b2 = RF.Concrete<BRepository>().GetById(b.Id) as B;
                Assert.AreEqual("New Name", b2.AName);

                var c2 = RF.Concrete<CRepository>().GetById(c.Id) as C;
                Assert.AreEqual("New Name", c2.AName);
            }
        }

        [TestMethod]
        public void MPT_Redundancy_UpdateCByRefChanged()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var a1 = new A { Name = "A1" };
                var a2 = new A { Name = "A2" };
                Save(a1, a2);

                var b = new B { A = a1 };
                Save(b);

                var c = new C { B = b };
                Save(c);

                Assert.AreEqual(c.AName, "A1");

                b.A = a2;
                Save(b);

                var cInDb = RF.Concrete<CRepository>().GetById(c.Id) as C;
                Assert.AreEqual(cInDb.AName, "A2");
            }
        }

        [TestMethod]
        public void MPT_Redundancy_UpdateDEByRefChanged()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var a1 = new A { Name = "A1" };
                var a2 = new A { Name = "A2" };
                Save(a1, a2);

                var b = new B { A = a1 };
                Save(b);

                var c = new C { B = b };
                Save(c);

                var d = new D { C = c };
                Save(d);

                var e = new E { D = d, C = c };
                Save(e);

                Assert.AreEqual(d.AName, "A1");
                Assert.AreEqual(e.ANameFromDCBA, "A1");
                Assert.AreEqual(e.ANameFromCBA, "A1");

                b.A = a2;
                Save(b);

                var dInDb = RF.Concrete<DRepository>().GetById(d.Id) as D;
                Assert.AreEqual(dInDb.AName, "A2");

                var eInDb = RF.Concrete<ERepository>().GetById(e.Id) as E;
                Assert.AreEqual(eInDb.ANameFromDCBA, "A2");
                Assert.AreEqual(eInDb.ANameFromCBA, "A2");
            }
        }

        [TestMethod]
        public void MPT_Redundancy_Id()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var a1 = new A { Name = "A1" };
                var a2 = new A { Name = "A2" };
                Save(a1, a2);

                var b = new B { A = a1 };
                Save(b);

                var c = new C { B = b };
                Save(c);

                Assert.AreEqual(c.AId, b.AId);
                Assert.AreEqual(b.AId, a1.Id);

                b.A = a2;
                Save(b);

                var cInDb = RF.Concrete<CRepository>().GetById(c.Id) as C;
                Assert.AreEqual(cInDb.AId, a2.Id);
            }
        }

        [TestMethod]
        public void MPT_Redundancy_RefId()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var a1 = new A { Name = "A1" };
                var a2 = new A { Name = "A2" };
                Save(a1, a2);

                var b = new B { A = a1 };
                Save(b);

                var c = new C { B = b };
                Save(c);

                Assert.AreEqual(c.AIdOfB, b.AId);
                Assert.AreEqual(b.AId, a1.Id);

                b.A = a2;
                Save(b);

                var cInDb = RF.Concrete<CRepository>().GetById(c.Id) as C;
                Assert.AreEqual(cInDb.AIdOfB, a2.Id);
            }
        }

        #endregion

        #region Id\RefId Field Type

        /// <summary>
        /// 对 Id 属性设置一个数值类型时，内部应该转换为一个 int 型。
        /// </summary>
        [TestMethod]
        public void MPT_Id_CoerceType()
        {
            var user = Get<TestUser>();

            user.SetProperty(Entity.IdProperty, 100D);
            var value = user.GetProperty(Entity.IdProperty);
            Assert.AreEqual(value.GetType(), typeof(int));

            user.SetProperty(Entity.IdProperty, 100L);
            value = user.GetProperty(Entity.IdProperty);
            Assert.AreEqual(value.GetType(), typeof(int));
        }

        /// <summary>
        /// 对引用 Id 属性设置一个数值类型时，内部应该转换为一个 int 型。
        /// </summary>
        [TestMethod]
        public void MPT_RefId_CoerceType()
        {
            var model = Get<TestTreeTask>();
            model.SetRefNullableId(TestTreeTask.TestUserIdProperty, 100L);
            var value = model.GetProperty(TestTreeTask.TestUserIdProperty);
            Assert.AreEqual(value.GetType(), typeof(int));
        }

        [TestMethod]
        public void MPT_RefId_FieldType()
        {
            var model = Get<Book>();

            model.SetRefNullableId(Book.BookCategoryIdProperty, null);
            var value = model.GetProperty(Book.BookCategoryIdProperty);
            Assert.AreEqual(value, 0);

            model.SetRefId(Book.BookCategoryIdProperty, null);
            value = model.GetProperty(Book.BookCategoryIdProperty);
            Assert.AreEqual(value, 0);
        }

        #endregion

        #region Helpers

        private static TEntity Get<TEntity>()
            where TEntity : Entity, new()
        {
            var e = new TEntity();
            e.MarkUnchanged();
            return e;
        }

        private static void Delete(params Entity[] entities)
        {
            foreach (var item in entities)
            {
                item.MarkDeleted();
                RF.Save(item);
            }
        }

        private static void Save(params Entity[] entities)
        {
            foreach (var item in entities)
            {
                RF.Save(item);
            }
        }

        #endregion
    }
}