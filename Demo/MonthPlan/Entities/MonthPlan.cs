/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121102 16:03
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121102 16:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using MP.WPF.Commands;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Utils;

namespace MP
{
    /// <summary>
    /// 月度计划
    /// </summary>
    [RootEntity, Serializable]
    public partial class MonthPlan : MPEntity
    {
        #region 构造函数

        public MonthPlan() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected MonthPlan(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 子属性

        public static readonly ListProperty<TaskOrCategoryList> TaskListProperty = P<MonthPlan>.RegisterList(e => e.TaskOrCategoryList);
        public TaskOrCategoryList TaskOrCategoryList
        {
            get { return this.GetLazyList(TaskListProperty); }
        }

        public static readonly ListProperty<WeekNoteList> WeekNoteListProperty = P<MonthPlan>.RegisterList(e => e.WeekNoteList);
        public WeekNoteList WeekNoteList
        {
            get { return this.GetLazyList(WeekNoteListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<DateTime> MonthProperty = P<MonthPlan>.Register(e => e.Month, new PropertyMetadata<DateTime>
        {
            DateTimePart = DateTimePart.Date
        });
        public DateTime Month
        {
            get { return this.GetProperty(MonthProperty); }
            set { this.SetProperty(MonthProperty, value); }
        }

        public static readonly Property<MonthPlanStatus> PlanStatusProperty = P<MonthPlan>.Register(e => e.PlanStatus, MonthPlanStatus.Planning);
        public MonthPlanStatus PlanStatus
        {
            get { return this.GetProperty(PlanStatusProperty); }
            set { this.SetProperty(PlanStatusProperty, value); }
        }

        public static readonly Property<double> FianlScoreProperty = P<MonthPlan>.Register(e => e.FinalScore);
        public double FinalScore
        {
            get { return this.GetProperty(FianlScoreProperty); }
            set { this.SetProperty(FianlScoreProperty, value); }
        }

        public static readonly Property<DateTime> CreateTimeProperty = P<MonthPlan>.Register(e => e.CreateTime);
        public DateTime CreateTime
        {
            get { return this.GetProperty(CreateTimeProperty); }
            set { this.SetProperty(CreateTimeProperty, value); }
        }

        public static readonly Property<string> CreateNoteProperty = P<MonthPlan>.Register(e => e.CreateNote);
        public string CreateNote
        {
            get { return this.GetProperty(CreateNoteProperty); }
            set { this.SetProperty(CreateNoteProperty, value); }
        }

        public static readonly Property<DateTime?> CompleteTimeProperty = P<MonthPlan>.Register(e => e.CompleteTime);
        public DateTime? CompleteTime
        {
            get { return this.GetProperty(CompleteTimeProperty); }
            set { this.SetProperty(CompleteTimeProperty, value); }
        }

        public static readonly Property<string> CompleteNoteProperty = P<MonthPlan>.Register(e => e.CompleteNote);
        public string CompleteNote
        {
            get { return this.GetProperty(CompleteNoteProperty); }
            set { this.SetProperty(CompleteNoteProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> MonthNameROProperty = P<MonthPlan>.RegisterReadOnly(
            e => e.MonthNameRO, e => e.GetMonthNameRO(), MonthProperty);
        public string MonthNameRO
        {
            get { return this.GetProperty(MonthNameROProperty); }
        }
        private string GetMonthNameRO()
        {
            return this.Month.ToString("yyyy-MM");
        }

        public static readonly Property<int> WeeksCountROProperty = P<MonthPlan>.RegisterReadOnly(
            e => e.WeeksCountRO, e => (e as MonthPlan).GetWeeksCountRO(), MonthProperty);
        public int WeeksCountRO
        {
            get { return this.GetProperty(WeeksCountROProperty); }
        }
        private int GetWeeksCountRO()
        {
            return TimeHelper.CountWeeksInMonth(this.Month);
        }

        public static readonly Property<bool> PlanningFieldsROProperty = P<MonthPlan>.RegisterReadOnly(
            e => e.PlanningFieldsRO, e => e.GetPlanningFieldsRO(), PlanStatusProperty);
        public bool PlanningFieldsRO
        {
            get { return this.GetProperty(PlanningFieldsROProperty); }
        }
        private bool GetPlanningFieldsRO()
        {
            return this.PlanStatus != MonthPlanStatus.Planning;
        }

        public static readonly Property<bool> SummarizingFieldsROProperty = P<MonthPlan>.RegisterReadOnly(
            e => e.SummarizingFieldsRO, e => e.GetSummarizingFieldsRO(), PlanStatusProperty);
        public bool SummarizingFieldsRO
        {
            get { return this.GetProperty(SummarizingFieldsROProperty); }
        }
        private bool GetSummarizingFieldsRO()
        {
            return this.PlanStatus != MonthPlanStatus.Summarizing;
        }

        #endregion

        #region 业务逻辑

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == TaskOrCategory.MonthScoreChangedEvent)
            {
                this.RefreshTotalScore();
                e.Handled = true;
            }

            base.OnRoutedEvent(sender, e);
        }

        private void RefreshTotalScore()
        {
            var value = this.TaskOrCategoryList.Cast<TaskOrCategory>()
                .Sum(category => category.MonthScore.GetValueOrDefault() * category.MonthPercent.GetValueOrDefault() / 100);
            this.FinalScore = Math.Round(value, 2);
        }

        public bool IsCurrentRunningWeek(int weekIndex)
        {
            if (this.PlanStatus == MonthPlanStatus.Summarizing)
            {
                var month = this.Month;
                var today = DateTime.Today;
                if (month.Year == today.Year && month.Month == today.Month)
                {
                    if (weekIndex == TimeHelper.CurrentWeekIndex)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion
    }

    [Serializable]
    public partial class MonthPlanList : MPEntityList { }

    public partial class MonthPlanRepository : MPEntityRepository
    {
        protected MonthPlanRepository() { }

        [DataProviderFor(typeof(MonthPlanRepository))]
        private class MonthPlanRepositoryDataProvider : RdbDataProvider
        {
            protected override void OnQuerying(EntityQueryArgs args)
            {
                var query = args.Query;
                query.OrderBy.Add(query.MainTable.Column(MonthPlan.MonthProperty), OrderDirection.Descending);

                base.OnQuerying(args);
            }
        }
    }

    [DataProviderFor(typeof(MonthPlanRepository))]
    public partial class MonthPlanDataProvider : RdbDataProvider
    {
        public MonthPlanDataProvider()
        {
            this.DataSaver.EnableDeletingChildrenInMemory = true;
        }
    }

    internal class MonthPlanConfig : MPEntityConfig<MonthPlan>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }

    internal class MonthPlanWPFViewConfig : WPFViewConfig<MonthPlan>
    {
        protected override void ConfigView()
        {
            View.DomainName("月度计划").HasDelegate(MonthPlan.MonthNameROProperty);

            if (RafyEnvironment.IsDebuggingEnabled)
            {
                View.UseCommands(
                    typeof(SetPlanStatus_Planning), typeof(SetPlanStatus_Summarizing)
                    );
            }

            View.UseCommands(
                typeof(LockPlan), typeof(ShowWeekSummary), typeof(CompletePlan),
                WPFCommandNames.PopupAdd, typeof(DeletePlan),
                typeof(SaveMonthPlan), WPFCommandNames.Cancel, WPFCommandNames.Refresh,
                typeof(About)
                );

            using (View.OrderProperties())
            {
                View.Property(MonthPlan.MonthNameROProperty).HasLabel("月份").ShowIn(ShowInWhere.List);
                View.Property(MonthPlan.MonthProperty).HasLabel("月份").ShowIn(ShowInWhere.Detail);
                View.Property(MonthPlan.PlanStatusProperty).HasLabel("状态").ShowIn(ShowInWhere.List).Readonly();
                View.Property(MonthPlan.FianlScoreProperty).HasLabel("得分").ShowIn(ShowInWhere.List).Readonly();
                View.Property(MonthPlan.CreateTimeProperty).HasLabel("计划时间").ShowIn(ShowInWhere.List).Readonly();
                View.Property(MonthPlan.CreateNoteProperty).HasLabel("计划说明").ShowIn(ShowInWhere.All)
                    .UseMemoEditor();
                //.ShowMemoInDetail();
                View.Property(MonthPlan.CompleteTimeProperty).HasLabel("总结时间").ShowIn(ShowInWhere.List)
                    .Readonly();
                View.Property(MonthPlan.CompleteNoteProperty).HasLabel("总结说明").ShowIn(ShowInWhere.List)
                    .UseMemoEditor();
            }
        }
    }

    public enum MonthPlanStatus
    {
        [Label("计划中")]
        Planning,
        [Label("进行中")]
        Summarizing,
        [Label("完成")]
        Completed,
    }
}