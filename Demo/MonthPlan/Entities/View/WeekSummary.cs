/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121106 21:13
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121106 21:13
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
    /// 以本周为视角的计划与总结对象
    /// </summary>
    [RootEntity]
    public partial class WeekSummary : MPEntity
    {
        internal WeekCompletion WeekCompletion;

        #region 一般属性

        public static readonly Property<string> NameProperty = P<WeekSummary>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> ContentProperty = P<WeekSummary>.Register(e => e.Content);
        public string Content
        {
            get { return this.GetProperty(ContentProperty); }
            set { this.SetProperty(ContentProperty, value); }
        }

        public static readonly Property<int?> MonthObjectiveNumProperty = P<WeekSummary>.Register(e => e.MonthObjectiveNum);
        /// <summary>
        /// 月目标
        /// </summary>
        public int? MonthObjectiveNum
        {
            get { return this.GetProperty(MonthObjectiveNumProperty); }
            set { this.SetProperty(MonthObjectiveNumProperty, value); }
        }

        public static readonly Property<int?> MonthCompletedNumProperty = P<WeekSummary>.Register(e => e.MonthCompletedNum);
        /// <summary>
        /// 月完成量
        /// </summary>
        public int? MonthCompletedNum
        {
            get { return this.GetProperty(MonthCompletedNumProperty); }
            set { this.SetProperty(MonthCompletedNumProperty, value); }
        }

        public static readonly Property<string> ObjectiveNameProperty = P<WeekSummary>.Register(e => e.ObjectiveName);
        public string ObjectiveName
        {
            get { return this.GetProperty(ObjectiveNameProperty); }
            set { this.SetProperty(ObjectiveNameProperty, value); }
        }

        public static readonly Property<int?> ObjectiveNumProperty = P<WeekSummary>.Register(e => e.ObjectiveNum);
        public int? ObjectiveNum
        {
            get { return this.GetProperty(ObjectiveNumProperty); }
            set { this.SetProperty(ObjectiveNumProperty, value); }
        }

        public static readonly Property<int> NumCompletedProperty = P<WeekSummary>.Register(e => e.NumCompleted);
        public int NumCompleted
        {
            get { return this.GetProperty(NumCompletedProperty); }
            set { this.SetProperty(NumCompletedProperty, value); }
        }

        public static readonly Property<string> WeekSummaryNoteProperty = P<WeekSummary>.Register(e => e.WeekSummaryNote);
        public string WeekSummaryNote
        {
            get { return this.GetProperty(WeekSummaryNoteProperty); }
            set { this.SetProperty(WeekSummaryNoteProperty, value); }
        }

        public static readonly Property<bool> IsCategoryProperty = P<WeekSummary>.Register(e => e.IsCategory);
        public bool IsCategory
        {
            get { return this.GetProperty(IsCategoryProperty); }
            set { this.SetProperty(IsCategoryProperty, value); }
        }

        public static readonly Property<bool> IsTaskProperty = P<WeekSummary>.Register(e => e.IsTask);
        public bool IsTask
        {
            get { return this.GetProperty(IsTaskProperty); }
            set { this.SetProperty(IsTaskProperty, value); }
        }

        public static readonly Property<bool> IsCompletedROProperty = P<WeekSummary>.RegisterReadOnly(
            e => e.IsCompletedRO, e => e.GetIsCompletedRO(), NumCompletedProperty);
        /// <summary>
        /// 本周计划是否已经完成
        /// </summary>
        public bool IsCompletedRO
        {
            get { return this.GetProperty(IsCompletedROProperty); }
        }
        private bool GetIsCompletedRO()
        {
            return this.NumCompleted >= this.ObjectiveNum;
        }

        #endregion
    }

    public partial class WeekSummaryList : MPEntityList { }

    public partial class WeekSummaryRepository : MPEntityRepository
    {
        protected WeekSummaryRepository() { }
    }

    internal class WeekSummaryConfig : MPEntityConfig<WeekSummary>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();
        }
    }

    internal class WeekSummaryWPFViewConfig : WPFViewConfig<WeekSummary>
    {
        protected override void ConfigView()
        {
            View.DomainName("以本周为视角的计划与总结对象").HasDelegate(WeekSummary.NameProperty);

            View.UseCommands(typeof(FilterCompleted));

            using (View.OrderProperties())
            {
                View.Property(WeekSummary.TreeIndexProperty).HasLabel("编码").ShowIn(ShowInWhere.List).Readonly();
                View.Property(WeekSummary.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.List).Readonly();
                View.Property(WeekSummary.ObjectiveNumProperty).HasLabel("本周目标").ShowIn(ShowInWhere.List).Readonly();
                View.Property(WeekSummary.NumCompletedProperty).HasLabel("本周执行").ShowIn(ShowInWhere.List)
                    .Readonly(WeekSummary.IsCategoryProperty);
                View.Property(WeekSummary.ObjectiveNameProperty).HasLabel("指标单位").ShowIn(ShowInWhere.List).Readonly();
                View.Property(WeekSummary.MonthObjectiveNumProperty).HasLabel("月目标").ShowIn(ShowInWhere.List).Readonly();
                View.Property(WeekSummary.MonthCompletedNumProperty).HasLabel("月完成").ShowIn(ShowInWhere.List).Readonly();
                View.Property(WeekSummary.WeekSummaryNoteProperty).HasLabel("说明").ShowIn(ShowInWhere.List)
                    .Readonly(WeekSummary.IsCategoryProperty)
                    .UseMemoEditor();
            }
        }
    }
}