using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.MetaModel.Attributes;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Audit;
using OEA.ManagedProperty;
using OEA.RBAC;

namespace OEA.Library.Audit
{
    [Serializable, QueryEntity]
    public partial class AuditItemConditionCriteria : Criteria
    {
        public AuditItemConditionCriteria()
        {
            //默认的查询起始时间和终止时间
            this.StartTime = DateTime.Today.AddDays(-3);
            this.EndTime = DateTime.Today.AddDays(0);
        }

        #region Properties

        /// <summary>
        /// 标题关键字
        /// </summary>
        public static readonly Property<string> TitleKeyWordsProperty =
            P<AuditItemConditionCriteria>.Register(e => e.TitleKeyWords);
        public string TitleKeyWords
        {
            get { return GetProperty(TitleKeyWordsProperty); }
            set { SetProperty(TitleKeyWordsProperty, value); }
        }

        /// <summary>
        /// 内容关键字
        /// </summary>
        public static readonly Property<string> ContentKeyWordsProperty =
            P<AuditItemConditionCriteria>.Register(e => e.ContentKeyWords);
        public string ContentKeyWords
        {
            get { return GetProperty(ContentKeyWordsProperty); }
            set { SetProperty(ContentKeyWordsProperty, value); }
        }

        /// <summary>
        /// 用户名关键字
        /// </summary>
        public static readonly Property<string> UserKeyWordsProperty =
            P<AuditItemConditionCriteria>.Register(e => e.UserKeyWords);
        public string UserKeyWords
        {
            get { return GetProperty(UserKeyWordsProperty); }
            set { SetProperty(UserKeyWordsProperty, value); }
        }

        /// <summary>
        /// 机器名关键字
        /// </summary>
        public static readonly Property<string> MachineKeyWordsProperty =
            P<AuditItemConditionCriteria>.Register(e => e.MachineKeyWords);
        public string MachineKeyWords
        {
            get { return GetProperty(MachineKeyWordsProperty); }
            set { SetProperty(MachineKeyWordsProperty, value); }
        }

        /// <summary>
        /// 模块名
        /// </summary>
        public static readonly RefProperty<ModuleAC> ModuleACRefProperty =
            P<AuditItemConditionCriteria>.RegisterRef(e => e.ModuleAC, new RefPropertyMeta
            {
                SerializeEntity = false,
                RefEntityChangedCallBack = (o, e) => (o as AuditItemConditionCriteria).OnModuleACChanged(e),
            });
        public int? ModuleACId
        {
            get { return this.GetRefNullableId(ModuleACRefProperty); }
            set { this.SetRefNullableId(ModuleACRefProperty, value); }
        }
        public ModuleAC ModuleAC
        {
            get { return this.GetRefEntity(ModuleACRefProperty); }
            set { this.SetRefEntity(ModuleACRefProperty, value); }
        }
        private void OnModuleACChanged(RefEntityChangedEventArgs e)
        {
            var value = e.NewEntity as ModuleAC;

            this.ModuleName = value != null ? value.KeyLabel : null;
        }
        /// <summary>
        /// 模块名（实际用于传输的数据）
        /// </summary>
        public string ModuleName { get; private set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public static readonly Property<AuditLogType> AuditLogTypeProperty =
            P<AuditItemConditionCriteria>.Register(e => e.AuditLogType);
        public AuditLogType AuditLogType
        {
            get { return GetProperty(AuditLogTypeProperty); }
            set { SetProperty(AuditLogTypeProperty, value); }
        }

        /// <summary>
        /// 起始时间
        /// </summary>
        public static readonly Property<DateTime> StartTimeProperty =
            P<AuditItemConditionCriteria>.Register(e => e.StartTime, new PropertyMetadata<DateTime>
            {
                PropertyChangingCallBack = (o, e) => (o as AuditItemConditionCriteria).OnStartTimeChanging(e)
            });
        public DateTime StartTime
        {
            get { return GetProperty(StartTimeProperty); }
            set { SetProperty(StartTimeProperty, value); }
        }
        private void OnStartTimeChanging(ManagedPropertyChangingEventArgs<DateTime> e)
        {
            if (e.Value == DateTime.MinValue) e.Cancel = true;
        }

        /// <summary>
        /// 终止时间
        /// </summary>
        public static readonly Property<DateTime> EndTimeProperty =
            P<AuditItemConditionCriteria>.Register(e => e.EndTime, new PropertyMetadata<DateTime>
            {
                PropertyChangingCallBack = (o, e) => (o as AuditItemConditionCriteria).OnEndTimeChanging(e)
            });
        public DateTime EndTime
        {
            get { return GetProperty(EndTimeProperty); }
            set { SetProperty(EndTimeProperty, value); }
        }
        private void OnEndTimeChanging(ManagedPropertyChangingEventArgs<DateTime> e)
        {
            if (e.Value == DateTime.MinValue) e.Cancel = true;
        }

        #endregion
    }

    internal class AuditItemConditionCriteriaConfig : EntityConfig<AuditItemConditionCriteria>
    {
        protected override void ConfigView()
        {
            View.ColumnsCountShowInDetail = 1;

            using (View.OrderProperties())
            {
                View.Property(AuditItemConditionCriteria.StartTimeProperty).ShowIn(ShowInWhere.Detail).HasLabel("起始时间");
                View.Property(AuditItemConditionCriteria.EndTimeProperty).ShowIn(ShowInWhere.Detail).HasLabel("终止时间");
                View.Property(AuditItemConditionCriteria.ModuleACRefProperty).ShowIn(ShowInWhere.Detail).HasLabel("模块名");
                View.Property(AuditItemConditionCriteria.AuditLogTypeProperty).ShowIn(ShowInWhere.Detail).HasLabel("日志类型");
                View.Property(AuditItemConditionCriteria.TitleKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("标题关键字");
                View.Property(AuditItemConditionCriteria.ContentKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("内容关键字");
                View.Property(AuditItemConditionCriteria.UserKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("用户名关键字");
                View.Property(AuditItemConditionCriteria.MachineKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("机器名关键字");
            }
        }
    }
}