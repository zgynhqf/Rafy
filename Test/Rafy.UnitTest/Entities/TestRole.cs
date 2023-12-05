using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain.ORM;

namespace UT
{
    [Serializable]
    [RootEntity]
    [Label("单元测试 - 角色")]
    public partial class TestRole : UnitTestEntity
    {
        public bool IsLock { get; set; }

        public static readonly Property<int> TestUserIdProperty =
            P<TestRole>.Register(e => e.TestUserId, new PropertyMetadata<int>
            {
                PropertyChangingCallBack = (o, e) => (o as TestRole).OnTestUserIdChanging(e),
                PropertyChangedCallBack = (o, e) => (o as TestRole).OnTestUserIdChanged(e)
            });
        public int TestUserId
        {
            get { return this.GetProperty(TestUserIdProperty); }
            set { this.SetProperty(TestUserIdProperty, value); }
        }
        public static readonly RefEntityProperty<TestUser> TestUserProperty =
            P<TestRole>.RegisterRef(e => e.TestUser, new RegisterRefArgs
            {
                ReferenceType = ReferenceType.Parent,
                RefKeyProperty = TestUserIdProperty,
                PropertyChangingCallBack = (o, e) => (o as TestRole).OnTestUserChanging(e),
                PropertyChangedCallBack = (o, e) => (o as TestRole).OnTestUserChanged(e)
            });
        public TestUser TestUser
        {
            get { return this.GetRefEntity(TestUserProperty); }
            set { this.SetRefEntity(TestUserProperty, value); }
        }
        protected virtual void OnTestUserIdChanging(ManagedPropertyChangingEventArgs<int> e)
        {
            e.Cancel = this.IsLock;
        }
        protected virtual void OnTestUserIdChanged(ManagedPropertyChangedEventArgs e)
        {
            this.TestUserIdChangedInternal = (int)e.NewValue;
        }
        protected virtual void OnTestUserChanging(ManagedPropertyChangingEventArgs<Entity> e)
        {
            e.Cancel = this.IsLock;
        }
        protected virtual void OnTestUserChanged(ManagedPropertyChangedEventArgs e)
        {
            this.TestUserChangedInternal = e.NewValue as TestUser;
        }
        public int TestUserIdChangedInternal { get; private set; }
        public Entity TestUserChangedInternal { get; private set; }

        public static Property<string> NameProperty = P<TestRole>.Register(e => e.Name);
        public string Name
        {
            get { return GetProperty<string>(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public static readonly Property<RoleType> RoleTypeProperty = P<TestRole>.Register(e => e.RoleType, RoleType.Normal);
        /// <summary>
        /// 角色的类型
        /// </summary>
        public RoleType RoleType
        {
            get { return this.GetProperty(RoleTypeProperty); }
            set { this.SetProperty(RoleTypeProperty, value); }
        }

        public static readonly Property<RoleType?> RoleType2Property = P<TestRole>.Register(e => e.RoleType2);
        public RoleType? RoleType2
        {
            get { return this.GetProperty(RoleType2Property); }
            set { this.SetProperty(RoleType2Property, value); }
        }
    }

    public partial class TestRoleList : UnitTestEntityList { }

    public partial class TestRoleRepository : UnitTestEntityRepository
    {
        public TestRoleList GetByUserId(int userId)
        {
            return this.GetBy(new CommonQueryCriteria
            {
                new PropertyMatch(TestRole.TestUserIdProperty, userId),
            });
        }

        [RepositoryQuery]
        public virtual TestRoleList GetByRawSql(string rawSql, object[] parameters, PagingInfo pi)
        {
            return (TestRoleList)(this.DataQueryer as RdbDataQueryer).QueryData(new SqlQueryArgs
            {
                FormattedSql = rawSql,
                Parameters = parameters,
                PagingInfo = pi
            });
        }
    }

    internal class TestRoleConfig : UnitTestEntityConfig<TestRole>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable("Roles");

            Meta.Property(TestRole.TestUserIdProperty).MapColumn().HasColumnName("UserId");
            Meta.Property(TestRole.NameProperty).MapColumn();
            Meta.Property(TestRole.RoleTypeProperty).MapColumn();
            Meta.Property(TestRole.RoleType2Property).MapColumn();
        }
    }

    [Serializable]
    public partial class GetByRawSqlCriteria
    {
        public string FormatSql { get; set; }
        public object[] Parameters { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}