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

namespace MP
{
    /// <summary>
    /// 每周说明
    /// </summary>
    [ChildEntity, Serializable]
    public partial class WeekNote : MPEntity
    {
        #region 构造函数

        public WeekNote() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected WeekNote(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty MonthPlanIdProperty =
            P<WeekNote>.RegisterRefId(e => e.MonthPlanId, ReferenceType.Parent);
        public int MonthPlanId
        {
            get { return (int)this.GetRefId(MonthPlanIdProperty); }
            set { this.SetRefId(MonthPlanIdProperty, value); }
        }
        public static readonly RefEntityProperty<MonthPlan> MonthPlanProperty =
            P<WeekNote>.RegisterRef(e => e.MonthPlan, MonthPlanIdProperty);
        public MonthPlan MonthPlan
        {
            get { return this.GetRefEntity(MonthPlanProperty); }
            set { this.SetRefEntity(MonthPlanProperty, value); }
        }

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<int> IndexProperty = P<WeekNote>.Register(e => e.Index);
        /// <summary>
        /// 从一开始的索引
        /// </summary>
        public int Index
        {
            get { return this.GetProperty(IndexProperty); }
            set { this.SetProperty(IndexProperty, value); }
        }

        public static readonly Property<string> NoteProperty = P<WeekNote>.Register(e => e.Note);
        public string Note
        {
            get { return this.GetProperty(NoteProperty); }
            set { this.SetProperty(NoteProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> NameROProperty = P<WeekNote>.RegisterReadOnly(
            e => e.NameRO, e => e.GetNameRO(), IndexProperty);
        public string NameRO
        {
            get { return this.GetProperty(NameROProperty); }
        }
        private string GetNameRO()
        {
            return string.Format("第 {0} 周", this.Index);
        }

        public static readonly Property<bool> UnSummarizingROProperty = P<WeekNote>.RegisterReadOnly(
            e => e.UnSummarizingRO, e => e.GetUnSummarizingRO(), MonthPlanProperty);
        public bool UnSummarizingRO
        {
            get { return this.GetProperty(UnSummarizingROProperty); }
        }
        private bool GetUnSummarizingRO()
        {
            return this.MonthPlan.PlanStatus != MonthPlanStatus.Summarizing;
        }

        public static readonly Property<bool> IsCurrentWeekROProperty = P<WeekNote>.RegisterReadOnly(
            e => e.IsCurrentWeekRO, e => e.GetIsCurrentWeekRO());
        public bool IsCurrentWeekRO
        {
            get { return this.GetProperty(IsCurrentWeekROProperty); }
        }
        private bool GetIsCurrentWeekRO()
        {
            return this.MonthPlan.IsCurrentRunningWeek(this.Index - 1);
        }

        #endregion
    }

    [Serializable]
    public partial class WeekNoteList : MPEntityList { }

    public partial class WeekNoteRepository : MPEntityRepository
    {
        protected WeekNoteRepository() { }
    }

    internal class WeekNoteConfig : MPEntityConfig<WeekNote>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }

    internal class WeekNoteWPFConfig : WPFViewConfig<WeekNote>
    {
        protected override void ConfigView()
        {
            View.DomainName("每周说明").HasDelegate(WeekNote.NameROProperty);

            using (View.OrderProperties())
            {
                View.Property(WeekNote.NameROProperty).HasLabel("周次").ShowIn(ShowInWhere.All);
                View.Property(WeekNote.NoteProperty).HasLabel("说明").ShowIn(ShowInWhere.All)
                    .UseMemoEditor().ShowInList(gridWidth: 400)
                    .Readonly(WeekNote.UnSummarizingROProperty);
            }
        }
    }
}