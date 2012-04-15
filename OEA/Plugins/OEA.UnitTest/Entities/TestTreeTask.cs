using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;
using OEA.MetaModel;

using OEA.MetaModel.View;

namespace OEA.Library._Test
{
    [Serializable]
    [RootEntity, Label("单元测试 - 任务")]
    [Table("Task")]
    public class TestTreeTask : UnitTestEntity
    {
        public static readonly RefProperty<TestUser> TestUserRefProperty =
            P<TestTreeTask>.RegisterRef(e => e.TestUser, ReferenceType.Parent);
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

        #region 汇总 AllTime

        public static Property<int> AllTimeProperty = P<TestTreeTask>.Register(e => e.AllTime, (o, e) => (o as TestTreeTask).OnAllTimeChanged(e));
        [EntityProperty]
        public int AllTime
        {
            get { return this.GetProperty(AllTimeProperty); }
            set { this.SetProperty(AllTimeProperty, value); }
        }
        protected virtual void OnAllTimeChanged(ManagedPropertyChangedEventArgs<int> e)
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
                var args = e.Args.CastTo<ManagedPropertyChangedEventArgs<int>>();
                this.AllTime += (args.NewValue - args.OldValue);
                e.Handled = true;
            }

            base.OnRoutedEvent(sender, e);
        }

        #endregion

        #region 自动汇总 AllTimeByAutoCollect

        public static Property<int> AllTimeByAutoCollectProperty = P<TestTreeTask>.Register(e => e.AllTimeByAutoCollect, (o, e) => (o as TestTreeTask).OnAllTimeByAutoCollectChanged(e));
        [EntityProperty]
        public int AllTimeByAutoCollect
        {
            get { return this.GetProperty(AllTimeByAutoCollectProperty); }
            set { this.SetProperty(AllTimeByAutoCollectProperty, value); }
        }
        protected virtual void OnAllTimeByAutoCollectChanged(ManagedPropertyChangedEventArgs<int> e)
        {
            this.AutoCollectAsChanged(e, true, TestUser.TasksTimeByAutoCollectProperty);
        }

        #endregion
    }

    [Serializable]
    public class TestTreeTaskList : EntityList { }

    internal class TestTreeTaskConfig : EntityConfig<TestTreeTask>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.HasColumns(
                TestTreeTask.TestUserRefProperty,
                TestTreeTask.AllTimeProperty,
                TestTreeTask.AllTimeByAutoCollectProperty,
                TestTreeTask.TreeCodeProperty,
                TestTreeTask.TreePIdProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.Property(TestTreeTask.TestUserRefProperty).HasLabel("负责人").ShowIn(ShowInWhere.ListDetail);
        }
    }
}