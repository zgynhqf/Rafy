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
        #region 构造函数

        public TestRole() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TestRole(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public bool IsLock { get; set; }

        public static readonly IRefIdProperty TestUserIdProperty =
            P<TestRole>.RegisterRefId(e => e.TestUserId, new RegisterRefIdArgs<int>
            {
                ReferenceType = ReferenceType.Parent,
                PropertyChangingCallBack = (o, e) => (o as TestRole).OnTestUserIdChanging(e),
                PropertyChangedCallBack = (o, e) => (o as TestRole).OnTestUserIdChanged(e)
            });
        public int TestUserId
        {
            get { return this.GetRefId(TestUserIdProperty); }
            set { this.SetRefId(TestUserIdProperty, value); }
        }
        public static readonly RefEntityProperty<TestUser> TestUserProperty =
            P<TestRole>.RegisterRef(e => e.TestUser, new RegisterRefArgs
            {
                RefIdProperty = TestUserIdProperty,
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
            get { return GetProperty(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }

        public static readonly Property<RoleType> RoleTypeProperty = P<TestRole>.Register(e => e.RoleType, RoleType.Normal);
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

    [Serializable]
    public partial class TestRoleList : UnitTestEntityList { }

    public partial class TestRoleRepository : UnitTestEntityRepository
    {
        public TestRoleList GetByUserId(int userId)
        {
            return this.FetchList(new GetByUserIdCriteria()
            {
                UserId = userId
            });
        }

        public EntityList GetByRawSql(string rawSql, object[] parameters, PagingInfo pi)
        {
            return this.FetchList(new GetByRawSqlCriteria
            {
                FormatSql = rawSql,
                Parameters = parameters,
                PagingInfo = pi
            });
        }

        protected EntityList FetchBy(GetByUserIdCriteria criteria)
        {
            return this.QueryList(q => q.Constrain(TestRole.TestUserIdProperty).Equal(criteria.UserId));
        }

        protected EntityList FetchBy(GetByRawSqlCriteria criteria)
        {
            return (this.DataQueryer as RdbDataQueryer).QueryList(new SqlQueryArgs
            {
                FormattedSql = criteria.FormatSql,
                Parameters = criteria.Parameters,
                PagingInfo = criteria.PagingInfo
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
    public partial class GetByUserIdCriteria
    {
        public int UserId { get; set; }
    }

    [Serializable]
    public partial class GetByRawSqlCriteria
    {
        public string FormatSql { get; set; }
        public object[] Parameters { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}