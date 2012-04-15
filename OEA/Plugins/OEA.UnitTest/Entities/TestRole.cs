using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OEA;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.ORM;

using OEA.MetaModel.View;

namespace OEA.Library._Test
{
    [Serializable]
    [RootEntity]
    [Label("单元测试 - 角色")]
    [Table("Role")]
    public class TestRole : UnitTestEntity
    {
        public bool IsLock { get; set; }
        public static readonly RefProperty<TestUser> TestUserRefProperty =
            P<TestRole>.RegisterRef(e => e.TestUser, new RefPropertyMeta
            {
                ReferenceType = ReferenceType.Parent,
                IdChangingCallBack = (o, e) => (o as TestRole).OnTestUserIdChanging(e),
                RefEntityChangingCallBack = (o, e) => (o as TestRole).OnTestUserChanging(e),
                IdChangedCallBack = (o, e) => (o as TestRole).OnTestUserIdChanged(e),
                RefEntityChangedCallBack = (o, e) => (o as TestRole).OnTestUserChanged(e),
            });
        [EntityProperty, Lookup("TestUser", ReferenceType.Parent)]
        public int TestUserId
        {
            get { return this.GetRefId(TestUserRefProperty); }
            set { this.SetRefId(TestUserRefProperty, value); }
        }
        public TestUser TestUser
        {
            get { return this.GetRefEntity(TestUserRefProperty); }
            set { this.SetRefEntity(TestUserRefProperty, value); }
        }
        protected virtual void OnTestUserIdChanging(RefIdChangingEventArgs e)
        {
            e.Cancel = this.IsLock;
        }
        protected virtual void OnTestUserIdChanged(RefIdChangedEventArgs e)
        {
            this.TestUserIdChangedInternal = e.NewId;
        }
        protected virtual void OnTestUserChanging(RefEntityChangingEventArgs e)
        {
            e.Cancel = this.IsLock;
        }
        protected virtual void OnTestUserChanged(RefEntityChangedEventArgs e)
        {
            this.TestUserChangedInternal = e.NewEntity as TestUser;
        }
        public int TestUserIdChangedInternal { get; private set; }
        public Entity TestUserChangedInternal { get; private set; }

        public static Property<string> NameProperty = P<TestRole>.Register(e => e.Name);
        [EntityProperty]
        public string Name
        {
            get { return GetProperty(NameProperty); }
            set { SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class TestRoleList : EntityList
    {
        protected void QueryBy(GetByUserIdCriteria criteria)
        {
            this.QueryDb(q => q.Constrain(TestRole.TestUserRefProperty).Equal(criteria.UserId));
        }
    }

    public class TestRoleRepository : EntityRepository
    {
        public TestRoleList GetByUserId(int userId)
        {
            return this.FetchListCast<TestRoleList>(new GetByUserIdCriteria()
            {
                UserId = userId
            });
        }
    }

    [Serializable]
    public class GetByUserIdCriteria
    {
        public int UserId { get; set; }
    }

    internal class TestRoleConfig : EntityConfig<TestRole>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.Property(TestRole.TestUserRefProperty).MapColumn().HasColumnName("UserId");
            Meta.Property(TestRole.NameProperty).MapColumn(true);
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasDelegate(TestRole.NameProperty);
            View.Property(TestRole.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            View.Property(TestRole.TestUserRefProperty).HasLabel("用户扩展编码").ShowIn(ShowInWhere.List);
        }
    }
}