using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy;
using System.Data;

namespace UT
{
    [Serializable]
    [RootEntity, Label("单元测试 - 任务")]
    public partial class TestTreeTask : UnitTestEntity
    {
        public static readonly IRefIdProperty TestUserIdProperty =
            P<TestTreeTask>.RegisterRefId(e => e.TestUserId, ReferenceType.Parent);
        public int TestUserId
        {
            get { return this.GetRefId(TestUserIdProperty); }
            set { this.SetRefId(TestUserIdProperty, value); }
        }
        public static readonly RefEntityProperty<TestUser> TestUserProperty =
            P<TestTreeTask>.RegisterRef(e => e.TestUser, TestUserIdProperty);
        public TestUser TestUser
        {
            get { return this.GetRefEntity(TestUserProperty); }
            set { this.SetRefEntity(TestUserProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<TestTreeTask>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #region 汇总 AllTime

        public static Property<int> AllTimeProperty = P<TestTreeTask>.Register(e => e.AllTime, (o, e) => (o as TestTreeTask).OnAllTimeChanged(e));
        public int AllTime
        {
            get { return this.GetProperty(AllTimeProperty); }
            set { this.SetProperty(AllTimeProperty, value); }
        }
        protected virtual void OnAllTimeChanged(ManagedPropertyChangedEventArgs e)
        {
            var parentTree = this.TreeParent;
            if (parentTree != null)
            {
                this.RaiseRoutedEvent(AllTimeChangedTreeRoutedEvent, e);
            }
            else
            {
                this.RaiseRoutedEvent(AllTimeChangedParentRoutedEvent, e);
            }
        }

        public static EntityRoutedEvent AllTimeChangedParentRoutedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);
        public static EntityRoutedEvent AllTimeChangedTreeRoutedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToTreeParent);
        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == AllTimeChangedTreeRoutedEvent)
            {
                var args = (ManagedPropertyChangedEventArgs)e.Args;
                this.AllTime += ((int)args.NewValue - (int)args.OldValue);
                e.Handled = true;
            }

            base.OnRoutedEvent(sender, e);
        }

        #endregion

        #region 自动汇总 AllTimeByAutoCollect

        public static Property<int> AllTimeByAutoCollectProperty = P<TestTreeTask>.Register(e => e.AllTimeByAutoCollect, (o, e) => (o as TestTreeTask).OnAllTimeByAutoCollectChanged(e));
        public int AllTimeByAutoCollect
        {
            get { return this.GetProperty(AllTimeByAutoCollectProperty); }
            set { this.SetProperty(AllTimeByAutoCollectProperty, value); }
        }
        protected virtual void OnAllTimeByAutoCollectChanged(ManagedPropertyChangedEventArgs e)
        {
            AutoCollectHelper.AutoCollectAsChanged(this, e, true, TestUser.TasksTimeByAutoCollectProperty);
        }

        #endregion

        public static readonly Property<string> XmlContentProperty = P<TestTreeTask>.Register(e => e.XmlContent);
        public string XmlContent
        {
            get { return this.GetProperty(XmlContentProperty); }
            set { this.SetProperty(XmlContentProperty, value); }
        }
    }

    [Serializable]
    public partial class TestTreeTaskList : UnitTestEntityList
    {
        public TestTreeTask AddNew()
        {
            var item = new TestTreeTask();
            this.Add(item);
            return item;
        }
    }

    public partial class TestTreeTaskRepository : UnitTestEntityRepository
    {
        protected TestTreeTaskRepository() { }

        [RepositoryQuery]
        public virtual TestTreeTaskList GetAndOrderByIdDesc2()
        {
            var source = f.Table(this);
            var q = f.Query(source);
            q.OrderBy.Add(source.Column(TestTreeTask.IdProperty), OrderDirection.Descending);

            return (TestTreeTaskList)this.QueryData(q);
        }

        //[RepositoryQuery]
        //public virtual TestTreeTaskList GetAndOrderByIdDesc()
        //{
        //    return (TestTreeTaskList)this.QueryList(q =>
        //    {
        //        q.OrderBy(TestTreeTask.IdProperty, OrderDirection.Descending);
        //    });
        //}
    }

    internal class TestTreeTaskConfig : UnitTestEntityConfig<TestTreeTask>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable("Task").MapProperties(
                TestTreeTask.TestUserIdProperty,
                TestTreeTask.AllTimeProperty,
                TestTreeTask.AllTimeByAutoCollectProperty,
                TestTreeTask.NameProperty,
                TestTreeTask.TreeIndexProperty,
                TestTreeTask.TreePIdProperty,
                TestTreeTask.XmlContentProperty
                );

            Meta.Property(TestTreeTask.XmlContentProperty).MapColumn().HasDbType(DbType.Xml);
        }
    }
}