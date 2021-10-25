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

namespace MP
{
    /// <summary>
    /// 每周完成量
    /// </summary>
    [ChildEntity]
    public partial class WeekCompletion : MPEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty TaskOrCategoryIdProperty =
            P<WeekCompletion>.RegisterRefId(e => e.TaskOrCategoryId, ReferenceType.Parent);
        public int TaskOrCategoryId
        {
            get { return (int)this.GetRefId(TaskOrCategoryIdProperty); }
            set { this.SetRefId(TaskOrCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<TaskOrCategory> TaskOrCategoryProperty =
            P<WeekCompletion>.RegisterRef(e => e.TaskOrCategory, TaskOrCategoryIdProperty);
        public TaskOrCategory TaskOrCategory
        {
            get { return this.GetRefEntity(TaskOrCategoryProperty); }
            set { this.SetRefEntity(TaskOrCategoryProperty, value); }
        }

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<int> IndexProperty = P<WeekCompletion>.Register(e => e.Index);
        /// <summary>
        /// 从一开始的索引
        /// </summary>
        public int Index
        {
            get { return this.GetProperty(IndexProperty); }
            set { this.SetProperty(IndexProperty, value); }
        }

        public static readonly Property<int> ObjectiveNumProperty = P<WeekCompletion>.Register(e => e.ObjectiveNum);
        public int ObjectiveNum
        {
            get { return this.GetProperty(ObjectiveNumProperty); }
            set { this.SetProperty(ObjectiveNumProperty, value); }
        }

        public static readonly Property<int> NumCompletedProperty = P<WeekCompletion>.Register(e => e.NumCompleted, new PropertyMetadata<int>
        {
            PropertyChangingCallBack = (o, e) => (o as WeekCompletion).OnNumCompletedChanging(e),
            PropertyChangedCallBack = (o, e) => (o as WeekCompletion).OnNumCompletedChanged(e)
        });
        public int NumCompleted
        {
            get { return this.GetProperty(NumCompletedProperty); }
            set { this.SetProperty(NumCompletedProperty, value); }
        }
        protected virtual void OnNumCompletedChanging(ManagedPropertyChangingEventArgs<int> e)
        {
            if (e.Value < 0) e.Cancel = true;
        }
        protected virtual void OnNumCompletedChanged(ManagedPropertyChangedEventArgs e)
        {
            this.RaiseRoutedEvent(NumCompletedChangedEvent, e);
        }

        public static readonly Property<string> NoteProperty = P<WeekCompletion>.Register(e => e.Note);
        public string Note
        {
            get { return this.GetProperty(NoteProperty); }
            set { this.SetProperty(NoteProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> NameROProperty = P<WeekCompletion>.RegisterReadOnly(
            e => e.NameRO, e => e.GetNameRO(), IndexProperty);
        public string NameRO
        {
            get { return this.GetProperty(NameROProperty); }
        }
        private string GetNameRO()
        {
            return string.Format("第 {0} 周", this.Index);
        }

        public static readonly Property<bool> SummarizingFieldsROProperty = P<WeekCompletion>.RegisterReadOnly(
            e => e.SummarizingFieldsRO, e => e.GetSummarizingFieldsRO(), TaskOrCategoryProperty);
        public bool SummarizingFieldsRO
        {
            get { return this.GetProperty(SummarizingFieldsROProperty); }
        }
        private bool GetSummarizingFieldsRO()
        {
            return this.TaskOrCategory.MonthPlan.PlanStatus != MonthPlanStatus.Summarizing;
        }

        public static readonly Property<bool> PlanningFieldsROProperty = P<WeekCompletion>.RegisterReadOnly(
            e => e.PlanningFieldsRO, e => e.GetPlanningFieldsRO(), TaskOrCategoryProperty);
        public bool PlanningFieldsRO
        {
            get { return this.GetProperty(PlanningFieldsROProperty); }
        }
        private bool GetPlanningFieldsRO()
        {
            return this.TaskOrCategory.MonthPlan.PlanStatus != MonthPlanStatus.Planning;
        }

        public static readonly Property<bool> IsObjectiveNumReadOnlyROProperty = P<WeekCompletion>.RegisterReadOnly(
            e => e.IsObjectiveNumReadOnlyRO, e => e.GetIsObjectiveNumReadOnlyRO(), TaskOrCategoryProperty);
        public bool IsObjectiveNumReadOnlyRO
        {
            get { return this.GetProperty(IsObjectiveNumReadOnlyROProperty); }
        }
        private bool GetIsObjectiveNumReadOnlyRO()
        {
            return this.TaskOrCategory.MonthPlan.PlanStatus == MonthPlanStatus.Completed;
        }

        public static readonly Property<bool> IsCurrentWeekROProperty = P<WeekCompletion>.RegisterReadOnly(
            e => e.IsCurrentWeekRO, e => e.GetIsCurrentWeekRO());
        public bool IsCurrentWeekRO
        {
            get { return this.GetProperty(IsCurrentWeekROProperty); }
        }
        private bool GetIsCurrentWeekRO()
        {
            return this.TaskOrCategory.MonthPlan.IsCurrentRunningWeek(this.Index - 1);
        }

        #endregion

        #region 业务逻辑

        public static readonly EntityRoutedEvent NumCompletedChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        #endregion
    }

    public partial class WeekCompletionList : MPEntityList { }

    public partial class WeekCompletionRepository : MPEntityRepository
    {
        protected WeekCompletionRepository() { }
    }

    internal class WeekCompletionConfig : MPEntityConfig<WeekCompletion>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }

    internal class WeekCompletionWPFConfig : WPFViewConfig<WeekCompletion>
    {
        protected override void ConfigView()
        {
            View.DomainName("每周完成量").HasDelegate(WeekCompletion.NameROProperty);

            View.ShowSummaryRow()
                .UseCommands(typeof(ResignWeekSubjectiveNum));

            using (View.OrderProperties())
            {
                View.Property(WeekCompletion.NameROProperty).HasLabel("周次").ShowIn(ShowInWhere.All);
                View.Property(WeekCompletion.ObjectiveNumProperty).HasLabel("指导目标").ShowIn(ShowInWhere.All)
                    .Readonly(WeekCompletion.IsObjectiveNumReadOnlyROProperty);
                View.Property(WeekCompletion.NumCompletedProperty).HasLabel("指标完成数").ShowIn(ShowInWhere.All)
                    .Readonly(WeekCompletion.SummarizingFieldsROProperty);
                View.Property(WeekCompletion.NoteProperty).HasLabel("说明").ShowIn(ShowInWhere.All)
                    .UseMemoEditor().ShowInList(gridWidth: 200)
                    .Readonly(WeekCompletion.SummarizingFieldsROProperty);
            }
        }
    }
}