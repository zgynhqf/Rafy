using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OEA;
using OEA.Library;
using OEA.Library._Test;
using OEA.Library.Caching;
using OEA.ORM.DbMigration;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.Serialization;
using OEA.Serialization.Mobile;
using OEA.Utils;

namespace OEAUnitTest
{
    [TestClass]
    public class ManagedPropertyTest : TestBase
    {
        [ClassInitialize]
        public static void AutoUITest_ClassInitialize(TestContext context)
        {
            ClassInitialize(context, true);

            using (new OEADbMigrationContext(UnitTestEntity.ConnectionString).AutoMigrate()) { }
        }

        [TestMethod]
        public void MPT_DefaultValue()
        {
            var user = Get<TestUser>();
            Assert.AreEqual(user.Name, "DefaultName");
            Assert.AreEqual(user.Age, 10);
        }

        [TestMethod]
        public void MPT_Validation()
        {
            var user = Get<TestUser>();
            var brokenRules = user.ValidationRules.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
            Assert.AreEqual(brokenRules[0].Description, "编码 并没有填写。");
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
            Assert.AreEqual(user.AgeChangedInternally_Persistence, 0);

            user.Age = 1;
            Assert.AreEqual(user.AgeChangedInternally_Property, 1);
            Assert.AreEqual(user.AgeChangedInternally_Persistence, 0);

            user.SetProperty(TestUser.AgeProperty, 2, ManagedPropertyChangedSource.FromPersistence);
            Assert.AreEqual(user.AgeChangedInternally_Property, 1);
            Assert.AreEqual(user.AgeChangedInternally_Persistence, 2);
        }

        [TestMethod]
        public void MPT_PropertyChanged_StaticEvent_RefEntityProperty()
        {
            var role = Get<TestRole>();
            Assert.AreEqual(role.TestUserIdChangedInternal, default(int));
            Assert.AreEqual(role.TestUserChangedInternal, null);

            var user = Get<TestUser>();
            user.Id = OEAEnvironment.NewLocalId();
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
            user.Id = OEAEnvironment.NewLocalId();
            role.TestUser = user;
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

            var property = TypeDescriptor.GetProperties(user).Find("UserCode", false);
            Assert.IsNotNull(property);
            Assert.IsFalse(property.IsReadOnly);
        }

        [TestMethod]
        public void MPT_ExtensionProperties_CreateListControl()
        {
            //var grid = AutoUI.BlockUIFactory.CreateTreeListControl(
            //    AppModel.EntityViews.FindViewMeta(typeof(TestUser)), ShowInWhere.List
            //    );
            //var grid2 = AutoUI.BlockUIFactory.CreateTreeListControl(
            //    AppModel.EntityViews.FindViewMeta(typeof(TestAdministrator)), ShowInWhere.List
            //    );

            //if (OEAEnvironment.Location.IsOnClient())
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
            Assert.IsTrue(properties.Contains("UserCode"));
            Assert.IsTrue(properties.Contains("ReadOnlyUserCode"));
            Assert.IsTrue(properties.Contains("ReadOnlyUserCodeShadow"));
            Assert.AreEqual(TestUserExt.GetUserCode(user), "NewCode");
            Assert.AreEqual(TestUserExt.GetReadOnlyUserCode(user), "NewCode ReadOnly!");
            Assert.AreEqual(TestUserExt.GetReadOnlyUserCodeShadow(user), "NewCode ReadOnly!");

            var typeProperties = TypeDescriptor.GetProperties(user);
            var property = typeProperties.Find("ReadOnlyUserCode", false);
            Assert.IsNotNull(property);
            Assert.IsTrue(property.IsReadOnly);
            property = typeProperties.Find("ReadOnlyUserCodeShadow", false);
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
            TestUserExt.SetUserCode(e1, "UserCode");

            Assert.AreEqual(e1.ValidationRules.Validate().Count, 1);

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

            //OEA属性
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

            //引用属性
            Assert.AreEqual(e2.ValidationRules.Target, e2);

            #endregion

            #region 检测具体的序列化的值

            var si = new SerializationContainerContext(new SerializationInfoContainer(0), null);
            si.IsProcessingState = true;
            e1.CastTo<IMobileObject>().SerializeState(si);

            var list = si.States.Keys.ToArray();

            Assert.IsTrue(list.Contains("Age"), "Age 是属性值，需要序列化");
            Assert.IsTrue(list.Contains("UserCode"), "UserCode 是扩展属性值，需要序列化");
            Assert.IsTrue(!list.Contains("Name"), "Name 是默认值，不需要序列化");
            Assert.IsTrue(!list.Contains("Id"), "Id 是默认值，不需要序列化");

            #endregion
        }

        [TestMethod]
        public void MPT_WPFBinding()
        {
            var userList = RF.Create<TestUser>().NewList();
            var newUser = Get<TestUser>();
            newUser.Name = "1";
            userList.Add(newUser);

            //创建 binding view
            var view = CollectionViewSource.GetDefaultView(userList) as BindingListCollectionView;
            Assert.IsNotNull(view);
            var userListView = view.SourceCollection as TestUserList;
            Assert.AreEqual(userListView, userList);

            //list change 事件
            ListChangedEventArgs eventArgs = null;
            userListView.ListChanged += (oo, ee) => { eventArgs = ee; };

            newUser = Get<TestUser>();
            newUser.Name = "2";
            userList.Add(newUser);

            Assert.IsNotNull(eventArgs);
            Assert.AreEqual(eventArgs.ListChangedType, ListChangedType.ItemAdded);
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
        public void MPT_ORM_PropertiesMapping()
        {
            var repo = RF.Concreate<TestUserRepository>();

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
            var role = userRoles.AddNew().CastTo<TestRole>();
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
            var role2 = RF.Create<TestRole>().GetById(role.Id).CastTo<TestRole>();
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
            var repo = RF.Create<TestAdministrator>() as TestUserRepository;

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
            var role = userRoles.AddNew().CastTo<TestRole>();
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
            var role2 = RF.Create<TestRole>().GetById(role.Id).CastTo<TestRole>();
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
            var repo = RF.Concreate<TestUserRepository>();

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
            var repo = RF.Concreate<TestUserRepository>();

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
            var role = userRoles.AddNew().CastTo<TestRole>();
            role.Name = "admin";
            Assert.IsTrue(user.Id == role.TestUserId);
            Assert.IsTrue(user.Id == -1);

            //保存新建用户
            repo.Save(user);
            Assert.AreEqual(user.Id, role.TestUserId);

            var roles = RF.Concreate<TestRoleRepository>().GetByUserId(user.Id);

            Assert.AreEqual(roles.Count, 1);
            Assert.AreEqual(userRoles.Count, 1);
            Assert.AreEqual(roles[0].CastTo<TestRole>().Name, role.Name);

            //clear
            user.MarkDeleted();
            repo.Save(user);
        }

        private static TEntity Get<TEntity>()
            where TEntity : Entity, new()
        {
            var e = new TEntity();
            e.Status = PersistenceStatus.Unchanged;
            return e;
        }
    }
}