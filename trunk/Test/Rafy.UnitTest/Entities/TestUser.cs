using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;

namespace UT
{
    [Serializable]
    [RootEntity, Label("单元测试 - 用户")]
    public partial class TestUser : UnitTestEntity
    {
        #region 构造函数

        public TestUser() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TestUser(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static Property<string> NameProperty = P<TestUser>.Register(e => e.Name, new PropertyMetadata<string>
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

        public static Property<int> AgeProperty = P<TestUser>.Register(e => e.Age, new PropertyMetadata<int>
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
        protected virtual void OnAgeChanged(ManagedPropertyChangedEventArgs e)
        {
            var newValue = (int)e.NewValue;

            if (e.Source == ManagedPropertyChangedSource.FromProperty)
            {
                this.AgeChangedInternally_Property = newValue;
            }

            this._ageSerailizable = newValue;
            this._ageNonserailizable = newValue;
        }
        public int AgeChangedInternally_Property { get; private set; }
        public int AgeChangedInternally_UIOperating { get; private set; }

        public static Property<string> NotEmptyCodeProperty = P<TestUser>.Register(e => e.NotEmptyCode, OnNotEmptyCodeChanged, null);
        public string NotEmptyCode
        {
            get { return this.GetProperty(NotEmptyCodeProperty); }
            set { this.SetProperty(NotEmptyCodeProperty, value); }
        }
        private static void OnNotEmptyCodeChanged(ManagedPropertyObject sender, ManagedPropertyChangedEventArgs e)
        {
            //var entity = sender as TestUser;
        }

        public static ListProperty<TestRoleList> TestRoleListProperty = P<TestUser>.RegisterList(e => e.TestRoleList);
        public TestRoleList TestRoleList
        {
            get { return this.GetLazyList(TestRoleListProperty); }
        }

        public static Property<string> ReadOnlyNameAgeProperty = P<TestUser>.RegisterReadOnly(e => e.ReadOnlyNameAge, ReadOnlyNameAgeProperty_GetValue, NameProperty, AgeProperty);
        public string ReadOnlyNameAge
        {
            get { return this.GetProperty(ReadOnlyNameAgeProperty); }
        }
        private static string ReadOnlyNameAgeProperty_GetValue(TestUser user)
        {
            return string.Format("{0} 的年龄是 {1}", user.Name, user.Age);
        }

        protected override void OnPropertyChanged(ManagedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == AgeProperty)
            {
                if (e.Source == ManagedPropertyChangedSource.FromUIOperating)
                {
                    this.AgeChangedInternally_UIOperating = this.Age;
                }
            }
        }

        #region TestTreeTaskList 的路由事件

        public static ListProperty<TestTreeTaskList> TestTreeTaskListProperty = P<TestUser>.RegisterList(e => e.TestTreeTaskList);
        public TestTreeTaskList TestTreeTaskList
        {
            get { return this.GetLazyList(TestTreeTaskListProperty); }
        }

        public static Property<int> TasksTimeProperty = P<TestUser>.Register(e => e.TasksTime);
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
                var args = (ManagedPropertyChangedEventArgs)e.Args;
                this.TasksTime += ((int)args.NewValue - (int)args.OldValue);
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

        public static readonly Property<string> TemporaryNameProperty = P<TestUser>.Register(e => e.TemporaryName, new PropertyMetadata<string>
        {
            AffectStatus = false
        });
        public string TemporaryName
        {
            get { return this.GetProperty(TemporaryNameProperty); }
            set { this.SetProperty(TemporaryNameProperty, value); }
        }
    }

    [Serializable]
    public partial class TestUserList : UnitTestEntityList { }

    public partial class TestUserRepository : UnitTestEntityRepository
    {
        public TestUser GetByName(string name)
        {
            return this.FetchFirst(new TestUserGetByNameCriteria()
            {
                UserName = name
            });
        }

        public TestUserList GetByName_Expression(string name, PagingInfo pagingInfo)
        {
            return this.FetchList(r => r.FetchByCustom(name, pagingInfo));
        }

        public int CountByName_Expression(string name, PagingInfo pagingInfo)
        {
            return this.FetchCount(r => r.FetchByCustom(name, pagingInfo));
        }

        public TestUserList GetByEmptyArgument()
        {
            return this.FetchList();
        }

        public TestUserList GetByOrder(bool nameAscending)
        {
            return this.FetchList(nameAscending);
        }

        public TestUserList GetByNameAge(string name, int age)
        {
            return this.FetchList(new CommonQueryCriteria
            {
                new PropertyMatch(TestUser.NameProperty, name),
                new PropertyMatch(TestUser.AgeProperty, age),
            });
        }
        public TestUserList GetByNameAge_PropertyQuery(string name, int age)
        {
            return this.FetchList(r => r.DA_GetByNameAge_PropertyQuery(name, age));
        }
        private EntityList DA_GetByNameAge_PropertyQuery(string name, int age)
        {
            var q = this.CreatePropertyQuery();
            q.AddConstrain(TestUser.NameProperty).Equal(name);
            q.AddConstrain(TestUser.AgeProperty).Equal(age);
            return this.QueryList(q);
        }

        public TestUserList GetByNameOrAge(string name, int age)
        {
            return this.FetchList(new CommonQueryCriteria(BinaryOperator.Or)
            {
                new PropertyMatchGroup
                {
                    new PropertyMatch(TestUser.NameProperty, name)
                },
                new PropertyMatchGroup
                {
                    new PropertyMatch(TestUser.AgeProperty, age)
                }
            });
        }

        public TestUserList GetByNameAgeByMultiParameters(string name, int age)
        {
            return this.FetchList(name, age);
        }

        public TestUserList GetByNameAgeByMultiParameters(string name, int age, PagingInfo pagingInfo)
        {
            return this.FetchList(name, age, pagingInfo);
        }

        protected EntityList FetchBy()
        {
            return this.QueryList(null as Action<IPropertyQuery>);
        }

        protected EntityList FetchBy(bool nameAscending)
        {
            return this.QueryList(q =>
            {
                q.OrderBy(TestUser.NameProperty, nameAscending ? OrderDirection.Ascending : OrderDirection.Descending);
            });
        }

        protected EntityList FetchBy(TestUserGetByNameCriteria criteria)
        {
            return this.QueryList(q => q.Constrain(TestUser.NameProperty).Equal(criteria.UserName));
        }

        protected EntityList FetchBy(string name, int age)
        {
            return this.QueryList(q =>
            {
                q.AddConstrainEqualIf(TestUser.NameProperty, name);
                q.AddConstrain(TestUser.AgeProperty).Equal(age);
            });
        }

        protected EntityList FetchBy(string name, int age, PagingInfo pagingInfo)
        {
            return this.QueryList(q =>
            {
                q.Paging(pagingInfo);

                q.Constrain(TestUser.NameProperty).Equal(name);
                q.AddConstrain(TestUser.AgeProperty).Equal(age);
            });
        }

        private EntityList FetchByCustom(string name, PagingInfo pagingInfo)
        {
            return this.QueryList(q =>
            {
                q.Constrain(TestUser.NameProperty).Equal(name);
                q.Paging(pagingInfo);
            });
        }

        public TestUserList GetByOrder2(bool nameAscending)
        {
            return this.FetchList(r => r.DA_GetByOrder2(nameAscending));
        }
        private EntityList DA_GetByOrder2(bool nameAscending)
        {
            var f = QueryFactory.Instance;
            var source = f.Table(this);
            var q = f.Query(
                from: source,
                orderBy: new List<IOrderBy> {
                    f.OrderBy(source.Column(TestUser.NameProperty), nameAscending ? OrderDirection.Ascending : OrderDirection.Descending)
                }
            );
            return this.QueryList(q);
        }

        public TestUserList GetByEmptyArgument2()
        {
            return this.FetchList(r => r.DA_GetByEmptyArgument2());
        }
        private EntityList DA_GetByEmptyArgument2()
        {
            var q = QueryFactory.Instance.Query(this);
            return this.QueryList(q);
        }

        public TestUserList GetByNameAgeByMultiParameters2(string name, int age)
        {
            return this.FetchList(r => r.DA_GetByNameAgeByMultiParameters2(name, age));
        }
        private EntityList DA_GetByNameAgeByMultiParameters2(string name, int age)
        {
            var f = QueryFactory.Instance;
            var q = f.Query(this);
            q.AddConstraintIf(TestUser.NameProperty, PropertyOperator.Equal, name);
            q.AddConstraintIf(TestUser.AgeProperty, PropertyOperator.Equal, age);

            return this.QueryList(q);
        }

        public TestUserList GetByIds(int[] ids)
        {
            return this.FetchList(r => r.DA_GetByIds(ids));
        }
        private EntityList DA_GetByIds(int[] ids)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => ids.Contains(e.Id));
            return this.QueryList(q);
        }

        public TestUserList GetByIds2(string userName, params int[] ids)
        {
            return this.FetchList(r => r.DA_GetByIds2(userName, ids));
        }
        private EntityList DA_GetByIds2(string userName, params int[] ids)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => ids.Contains(e.Id) && e.Name.Contains(userName));
            return this.QueryList(q);
        }

        public TestUser GetWithTasks(int userId)
        {
            return this.FetchFirst(r => r.DA_GetWithTasks(userId));
        }
        private EntityList DA_GetWithTasks(int userId)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Id == userId);
            var args = new EntityQueryArgs
            {
                Query = this.ConvertToQuery(q),
            };
            args.EagerLoad(TestUser.TestTreeTaskListProperty);
            return this.QueryList(args);
        }
    }

    internal class TestUserConfig : UnitTestEntityConfig<TestUser>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(TestUser.NotEmptyCodeProperty, RequiredRule.Instance);
        }

        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable("Users");
            Meta.Property(TestUser.NameProperty).MapColumn().HasColumnName("UserName");
        }
    }

    [Serializable]
    public partial class TestUserGetByNameCriteria
    {
        public string UserName { get; set; }
    }
}