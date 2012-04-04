using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OEA.ManagedProperty;
using OEA.MetaModel.Attributes;
using OEA.ORM;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Library.Validation;

namespace OEA.Library._Test
{
    [Serializable]
    [RootEntity, Label("单元测试 - 用户")]
    [Table("User")]
    public class TestUser : UnitTestEntity
    {
        public static ManagedProperty<string> NameProperty = P<TestUser>.Register(e => e.Name, new PropertyMetadata<string>
        {
            DefaultValue = "DefaultName",
            CoerceGetValueCallBack = (s, v) => (s as TestUser).CoerceGetName(v)
        });
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
        private string CoerceGetName(string value)
        {
            if (value.Length > 20) return value.Substring(0, 20);
            return value;
        }

        public static ManagedProperty<int> AgeProperty = P<TestUser>.Register(e => e.Age, new PropertyMetadata<int>
        {
            DefaultValue = 10,
            PropertyChangingCallBack = (o, e) => (o as TestUser).OnAgeChanging(e),
            PropertyChangedCallBack = (o, e) => (o as TestUser).OnAgeChanged(e),
        });
        [Column]
        public int Age
        {
            get { return this.GetProperty(AgeProperty); }
            set { this.SetProperty(AgeProperty, value); }
        }
        protected virtual void OnAgeChanging(ManagedPropertyChangingEventArgs<int> e)
        {
            var value = e.Value;
            if (value > 100) e.CoercedValue = 100;

            e.Cancel = e.Value < 0;
        }
        protected virtual void OnAgeChanged(ManagedPropertyChangedEventArgs<int> e)
        {
            if (e.Source == ManagedPropertyChangedSource.FromProperty)
            {
                this.AgeChangedInternally_Property = e.NewValue;
            }

            this._ageSerailizable = e.NewValue;
            this._ageNonserailizable = e.NewValue;
        }
        public int AgeChangedInternally_Property { get; private set; }
        public int AgeChangedInternally_Persistence { get; private set; }

        public static ManagedProperty<string> NotEmptyCodeProperty = P<TestUser>.Register(e => e.NotEmptyCode, OnNotEmptyCodeChanged, null);
        public string NotEmptyCode
        {
            get { return this.GetProperty(NotEmptyCodeProperty); }
            set { this.SetProperty(NotEmptyCodeProperty, value); }
        }
        private static void OnNotEmptyCodeChanged(ManagedPropertyObject sender, ManagedPropertyChangedEventArgs<string> e)
        {
            //var entity = sender as TestUser;
        }

        public static ManagedProperty<TestRoleList> TestRoleListProperty = P<TestUser>.Register(e => e.TestRoleList);
        [Association]
        public TestRoleList TestRoleList
        {
            get { return this.GetLazyChildren(TestRoleListProperty); }
        }

        public static ManagedProperty<string> ReadOnlyNameAgeProperty = P<TestUser>.RegisterReadOnly(e => e.ReadOnlyNameAge, ReadOnlyNameAgeProperty_GetValue, null, NameProperty, AgeProperty);
        public string ReadOnlyNameAge
        {
            get { return this.GetProperty(ReadOnlyNameAgeProperty); }
        }
        private static string ReadOnlyNameAgeProperty_GetValue(TestUser user)
        {
            return string.Format("{0} 的年龄是 {1}", user.Name, user.Age);
        }

        protected override void OnPropertyChanged(IManagedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == AgeProperty)
            {
                if (e.Source == ManagedPropertyChangedSource.FromPersistence)
                {
                    this.AgeChangedInternally_Persistence = this.Age;
                }
            }
        }

        protected override void AddValidations()
        {
            this.ValidationRules.AddRule(NotEmptyCodeProperty, CommonRules.StringRequired);
        }

        #region TestTreeTaskList 的路由事件

        public static Property<TestTreeTaskList> TestTreeTaskListProperty = P<TestUser>.Register(e => e.TestTreeTaskList);
        [Association]
        public TestTreeTaskList TestTreeTaskList
        {
            get { return this.GetLazyChildren(TestTreeTaskListProperty); }
        }

        public static Property<int> TasksTimeProperty = P<TestUser>.Register(e => e.TasksTime);
        [EntityProperty]
        [Column]
        public int TasksTime
        {
            get { return this.GetProperty(TasksTimeProperty); }
            set { this.SetProperty(TasksTimeProperty, value); }
        }

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == TestTreeTask.AllTimeChangedParentRoutedEvent)
            {
                var args = e.Args.CastTo<ManagedPropertyChangedEventArgs<int>>();
                this.TasksTime += (args.NewValue - args.OldValue);
                e.Handled = true;
            }

            base.OnRoutedEvent(sender, e);
        }

        #endregion

        public static Property<int> TasksTimeByAutoCollectProperty = P<TestUser>.Register(e => e.TasksTimeByAutoCollect);
        public int TasksTimeByAutoCollect
        {
            get { return this.GetProperty(TasksTimeByAutoCollectProperty); }
        }

        [NonSerialized]
        public int _ageNonserailizable;
        public int _ageSerailizable;
        public object _mySelfReference;
        public DateTime _now = DateTime.Today;
    }

    [Serializable]
    public class TestUserList : EntityList
    {
        protected void QueryBy(TestUserGetByNameCriteria criteria)
        {
            this.QueryDb(q => q.Constrain(TestUser.NameProperty).Equal(criteria.UserName));
        }
    }

    public class TestUserRepository : EntityRepository
    {
        public TestUser GetByName(string name)
        {
            var list = this.FetchList(new TestUserGetByNameCriteria()
            {
                UserName = name
            });

            return list.Count == 1 ? list[0].CastTo<TestUser>() : null;
        }
    }

    [Serializable]
    public class TestUserGetByNameCriteria
    {
        public string UserName { get; set; }
    }

    internal class TestUserConfig : EntityConfig<TestUser>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.Property(TestUser.NameProperty).MapColumn().HasColumnName("UserName");
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.Property(TestUser.NameProperty).HasLabel("姓名").ShowIn(ShowInWhere.All);
            View.Property(TestUser.AgeProperty).HasLabel("年龄").ShowIn(ShowInWhere.List | ShowInWhere.Detail);
            View.Property(TestUser.NotEmptyCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.List | ShowInWhere.Detail);
            View.Property(TestUser.TasksTimeProperty).HasLabel("任务时间").ShowIn(ShowInWhere.All);
        }
    }
}