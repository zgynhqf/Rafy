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
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using MP.WPF.Commands;
using System.ComponentModel;

namespace MP
{
    /// <summary>
    /// 每个月内的任务项
    /// </summary>
    [ChildEntity, Serializable]
    public partial class TaskOrCategory : MPEntity
    {
        #region 构造函数

        public TaskOrCategory() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected TaskOrCategory(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty MonthPlanIdProperty =
            P<TaskOrCategory>.RegisterRefId(e => e.MonthPlanId, ReferenceType.Parent);
        public int MonthPlanId
        {
            get { return (int)this.GetRefId(MonthPlanIdProperty); }
            set { this.SetRefId(MonthPlanIdProperty, value); }
        }
        public static readonly RefEntityProperty<MonthPlan> MonthPlanProperty =
            P<TaskOrCategory>.RegisterRef(e => e.MonthPlan, MonthPlanIdProperty);
        public MonthPlan MonthPlan
        {
            get { return this.GetRefEntity(MonthPlanProperty); }
            set { this.SetRefEntity(MonthPlanProperty, value); }
        }

        #endregion

        #region 子属性

        public static readonly ListProperty<WeekCompletionList> WeekCompletionListProperty = P<TaskOrCategory>.RegisterList(e => e.WeekCompletionList);
        public WeekCompletionList WeekCompletionList
        {
            get { return this.GetLazyList(WeekCompletionListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<TaskOrCategory>.Register(e => e.Name);
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> ContentProperty = P<TaskOrCategory>.Register(e => e.Content);
        /// <summary>
        /// 说明
        /// </summary>
        public string Content
        {
            get { return this.GetProperty(ContentProperty); }
            set { this.SetProperty(ContentProperty, value); }
        }

        public static readonly Property<string> ObjectiveNameProperty = P<TaskOrCategory>.Register(e => e.ObjectiveName);
        /// <summary>
        /// 指标名称 / 指标单位
        /// </summary>
        public string ObjectiveName
        {
            get { return this.GetProperty(ObjectiveNameProperty); }
            set { this.SetProperty(ObjectiveNameProperty, value); }
        }

        public static readonly Property<int?> ObjectiveNumProperty = P<TaskOrCategory>.Register(e => e.ObjectiveNum, new PropertyMetadata<int?>
        {
            PropertyChangingCallBack = (o, e) => (o as TaskOrCategory).OnObjectiveNumChanging(e),
            PropertyChangedCallBack = (o, e) => (o as TaskOrCategory).OnObjectiveNumChanged(e)
        });
        /// <summary>
        /// 指标值
        /// </summary>
        public int? ObjectiveNum
        {
            get { return this.GetProperty(ObjectiveNumProperty); }
            set { this.SetProperty(ObjectiveNumProperty, value); }
        }
        protected virtual void OnObjectiveNumChanging(ManagedPropertyChangingEventArgs<int?> e)
        {
            if (RafyPropertyDescriptor.IsOperating)
            {
                if (this.IsTaskRO && !e.Value.HasValue || e.Value <= 0) { e.Cancel = true; }
            }
        }
        protected virtual void OnObjectiveNumChanged(ManagedPropertyChangedEventArgs e)
        {
            this.DivideObjectiveNum();
        }

        public static readonly Property<int?> WeightInCategoryProperty = P<TaskOrCategory>.Register(e => e.WeightInCategory, new PropertyMetadata<int?>
        {
            PropertyChangingCallBack = (o, e) => (o as TaskOrCategory).OnWeightInCategoryChanging(e)
        });
        /// <summary>
        /// 任务权重
        /// </summary>
        public int? WeightInCategory
        {
            get { return this.GetProperty(WeightInCategoryProperty); }
            set { this.SetProperty(WeightInCategoryProperty, value); }
        }
        protected virtual void OnWeightInCategoryChanging(ManagedPropertyChangingEventArgs<int?> e)
        {
            if (RafyPropertyDescriptor.IsOperating)
            {
                if (this.IsTaskRO && !e.Value.HasValue || e.Value <= 0) { e.Cancel = true; }
            }
        }

        public static readonly Property<int?> MonthPercentProperty = P<TaskOrCategory>.Register(e => e.MonthPercent, new PropertyMetadata<int?>
        {
            PropertyChangingCallBack = (o, e) => (o as TaskOrCategory).OnMonthPercentChanging(e)
        });
        /// <summary>
        /// 类别百分比
        /// </summary>
        public int? MonthPercent
        {
            get { return this.GetProperty(MonthPercentProperty); }
            set { this.SetProperty(MonthPercentProperty, value); }
        }
        protected virtual void OnMonthPercentChanging(ManagedPropertyChangingEventArgs<int?> e)
        {
            if (RafyPropertyDescriptor.IsOperating)
            {
                if (this.IsCategoryRO && !e.Value.HasValue || e.Value <= 0) { e.Cancel = true; }
            }
        }

        public static readonly Property<double?> ScoreProperty = P<TaskOrCategory>.Register(e => e.Score);
        /// <summary>
        /// 任务得分
        /// </summary>
        public double? Score
        {
            get { return this.GetProperty(ScoreProperty); }
            set { this.SetProperty(ScoreProperty, value); }
        }

        public static readonly Property<double?> MonthScoreProperty = P<TaskOrCategory>.Register(e => e.MonthScore, new PropertyMetadata<double?>
        {
            PropertyChangedCallBack = (o, e) => (o as TaskOrCategory).OnMonthScoreChanged(e)
        });
        /// <summary>
        /// 类别得分
        /// </summary>
        public double? MonthScore
        {
            get { return this.GetProperty(MonthScoreProperty); }
            set { this.SetProperty(MonthScoreProperty, value); }
        }
        protected virtual void OnMonthScoreChanged(ManagedPropertyChangedEventArgs e)
        {
            this.RaiseRoutedEvent(MonthScoreChangedEvent, e);
        }

        #endregion

        #region 只读属性

        public static readonly Property<bool> IsCategoryROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.IsCategoryRO, e => e.GetIsCategoryRO(), TreePIdProperty);
        /// <summary>
        /// 是否为类型。
        /// </summary>
        public bool IsCategoryRO
        {
            get { return this.GetProperty(IsCategoryROProperty); }
        }
        private bool GetIsCategoryRO()
        {
            return !this.TreePId.HasValue;
        }

        public static readonly Property<bool> IsTaskROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.IsTaskRO, e => e.GetIsTaskRO(), TreePIdProperty);
        /// <summary>
        /// 是否为任务
        /// </summary>
        public bool IsTaskRO
        {
            get { return this.GetProperty(IsTaskROProperty); }
        }
        private bool GetIsTaskRO()
        {
            return this.TreePId.HasValue;
        }

        public static readonly Property<int> SumCompletedROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.SumCompletedRO, e => e.GetSumCompletedRO());
        /// <summary>
        /// 如果是任务，则返回当前的完成量。
        /// </summary>
        public int SumCompletedRO
        {
            get { return this.GetProperty(SumCompletedROProperty); }
        }
        private int GetSumCompletedRO()
        {
            if (this.IsCategoryRO) return 0;

            return this.WeekCompletionList.Cast<WeekCompletion>().Sum(c => c.NumCompleted);
        }

        public static readonly Property<bool> IsTaskCompletedROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.IsTaskCompletedRO, e => e.GetIsTaskCompletedRO(), SumCompletedROProperty);
        /// <summary>
        /// 是否任务已经完成
        /// </summary>
        public bool IsTaskCompletedRO
        {
            get { return this.GetProperty(IsTaskCompletedROProperty); }
        }
        private bool GetIsTaskCompletedRO()
        {
            return this.IsTaskRO && this.SumCompletedRO >= this.ObjectiveNum;
        }

        public static readonly Property<bool> PlanningFieldsROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.PlanningFieldsRO, e => e.GetPlanningFieldsRO(), MonthPlanProperty);
        public bool PlanningFieldsRO
        {
            get { return this.GetProperty(PlanningFieldsROProperty); }
        }
        private bool GetPlanningFieldsRO()
        {
            return this.MonthPlan.PlanStatus != MonthPlanStatus.Planning;
        }

        public static readonly Property<bool> SummarizingFieldsROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.SummarizingFieldsRO, e => e.GetSummarizingFieldsRO(), MonthPlanProperty);
        public bool SummarizingFieldsRO
        {
            get { return this.GetProperty(SummarizingFieldsROProperty); }
        }
        private bool GetSummarizingFieldsRO()
        {
            return this.MonthPlan.PlanStatus != MonthPlanStatus.Summarizing;
        }

        public static readonly Property<bool> CategoryFieldsROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.CategoryFieldsRO, e => e.GetCategoryFieldsRO(), IsCategoryROProperty, MonthPlanProperty);
        public bool CategoryFieldsRO
        {
            get { return this.GetProperty(CategoryFieldsROProperty); }
        }
        private bool GetCategoryFieldsRO()
        {
            return !this.IsCategoryRO || this.PlanningFieldsRO;
        }

        public static readonly Property<bool> TaskPlanningFieldsROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.TaskPlanningFieldsRO, e => e.GetTaskPlanningFieldsRO(), IsTaskROProperty, MonthPlanProperty);
        public bool TaskPlanningFieldsRO
        {
            get { return this.GetProperty(TaskPlanningFieldsROProperty); }
        }
        private bool GetTaskPlanningFieldsRO()
        {
            return !this.IsTaskRO || this.PlanningFieldsRO;
        }

        public static readonly Property<bool> TaskSummarizingFieldsROProperty = P<TaskOrCategory>.RegisterReadOnly(
            e => e.TaskSummarizingFieldsRO, e => e.GetTaskSummarizingFieldsRO(), IsTaskROProperty, MonthPlanProperty);
        public bool TaskSummarizingFieldsRO
        {
            get { return this.GetProperty(TaskSummarizingFieldsROProperty); }
        }
        private bool GetTaskSummarizingFieldsRO()
        {
            return !this.IsTaskRO || this.SummarizingFieldsRO;
        }

        #endregion

        #region 业务逻辑

        #region 由下向上统计分数

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (this.IsTaskRO)
            {
                if (e.Event == WeekCompletion.NumCompletedChangedEvent)
                {
                    this.CalcTaskScore();

                    this.NotifyPropertyChanged(SumCompletedROProperty);

                    e.Handled = true;
                }
            }

            base.OnRoutedEvent(sender, e);
        }

        private void CalcTaskScore()
        {
            //完成量占总量的百分比，即是所得分数。
            double completed = this.SumCompletedRO;
            double total = this.ObjectiveNum.GetValueOrDefault();
            this.Score = Math.Round(Math.Min(completed, total) / total * 100, 2);

            //变更类别的
            var category = this.TreeParent as TaskOrCategory;
            category.CalcCategoryScore();
        }

        private void CalcCategoryScore()
        {
            var tasks = this.TreeChildren.Cast<TaskOrCategory>();

            double totalWeight = tasks.Sum(t => t.WeightInCategory).Value;

            //任务分数 * 任务权重百分比，即是类别的分数。
            var categoryScore = tasks.Sum(task =>
            {
                var weightPercent = task.WeightInCategory.Value / totalWeight;
                return task.Score.Value * weightPercent;
            });

            this.MonthScore = Math.Round(categoryScore, 2);
        }

        public static readonly EntityRoutedEvent MonthScoreChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        #endregion

        /// <summary>
        /// 计划阶段，指标值变化时，把值平均分配到前四周上。
        /// </summary>
        private void DivideObjectiveNum()
        {
            var monthNum = this.ObjectiveNum.GetValueOrDefault();
            if (monthNum <= 0) return;

            var weeks = this.WeekCompletionList;
            if (weeks.Count == 0) return;

            foreach (WeekCompletion week in weeks)
            {
                week.ObjectiveNum = 0;
            }

            //只分配到最后一周的前几周。
            var weeksCount = this.MonthPlan.WeeksCountRO - 1;

            //尽量平均地分配到每一周上。
            for (int i = 0; i < weeksCount && monthNum > 0; i++)
            {
                var week = weeks[i] as WeekCompletion;
                week.ObjectiveNum++;
                monthNum--;

                //如果是最后一个，则重新循环。
                if (i == weeksCount - 1) { i = -1; }
            }
        }

        #endregion
    }

    [Serializable]
    public partial class TaskOrCategoryList : MPEntityList { }

    public partial class TaskOrCategoryRepository : MPEntityRepository
    {
        protected TaskOrCategoryRepository() { }
    }

    internal class TaskOrCategoryConfig : MPEntityConfig<TaskOrCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.SupportTree();
        }
    }

    internal class TaskOrCategoryWPFViewConfig : WPFViewConfig<TaskOrCategory>
    {
        protected override void ConfigView()
        {
            View.DomainName("月内任务项").HasDelegate(TaskOrCategory.NameProperty);

            View.UseCommands(
                typeof(ExpandTask), typeof(AddCategory), typeof(AddTask), typeof(DeleteTask),
                typeof(MoveUpTask), typeof(MoveDownTask),
                typeof(FilterCompleted)
                );

            using (View.OrderProperties())
            {
                View.Property(TaskOrCategory.TreeIndexProperty).HasLabel("编码").ShowIn(ShowInWhere.All).Readonly();
                View.Property(TaskOrCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All)
                    .Readonly(TaskOrCategory.PlanningFieldsROProperty);
                View.Property(TaskOrCategory.ContentProperty).HasLabel("说明").ShowIn(ShowInWhere.All)
                    .Readonly(TaskOrCategory.PlanningFieldsROProperty)
                    .UseMemoEditor();
                View.Property(TaskOrCategory.ObjectiveNumProperty).HasLabel("指标值").ShowIn(ShowInWhere.All)
                    .Readonly(TaskOrCategory.TaskPlanningFieldsROProperty);
                View.Property(TaskOrCategory.ObjectiveNameProperty).HasLabel("指标单位").ShowIn(ShowInWhere.All)
                    .Readonly(TaskOrCategory.TaskPlanningFieldsROProperty);
                View.Property(TaskOrCategory.WeightInCategoryProperty).HasLabel("任务权重").ShowIn(ShowInWhere.All)
                    .Readonly(TaskOrCategory.TaskPlanningFieldsROProperty);
                View.Property(TaskOrCategory.MonthPercentProperty).HasLabel("类别百分比").ShowIn(ShowInWhere.All)
                    .Readonly(TaskOrCategory.CategoryFieldsROProperty);
                View.Property(TaskOrCategory.ScoreProperty).HasLabel("任务得分").ShowIn(ShowInWhere.All)
                    .Readonly();
                View.Property(TaskOrCategory.MonthScoreProperty).HasLabel("类别得分").ShowIn(ShowInWhere.All).Readonly();
            }
        }
    }
}