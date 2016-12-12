/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110414
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110414
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.RBAC.Old.Audit;
using Rafy.MetaModel.View;
using Rafy.RBAC.Old;
using Rafy.Utils;

namespace Rafy.RBAC.Old.Audit
{
    [QueryEntity, Serializable]
    public partial class AuditItemConditionCriteria : Criteria
    {
        #region 构造函数

        public AuditItemConditionCriteria()
        {
            //默认的查询起始时间和终止时间
            this.StartTime = DateTime.Today.AddDays(-3);
            this.EndTime = DateTime.Today.AddDays(0);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected AuditItemConditionCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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
        public static readonly IRefIdProperty ModuleACIdProperty =
            P<AuditItemConditionCriteria>.RegisterRefId(e => e.ModuleACId, ReferenceType.Normal);
        public int? ModuleACId
        {
            get { return (int?)this.GetRefNullableId(ModuleACIdProperty); }
            set { this.SetRefNullableId(ModuleACIdProperty, value); }
        }
        public static readonly RefEntityProperty<ModuleAC> ModuleACProperty =
            P<AuditItemConditionCriteria>.RegisterRef(e => e.ModuleAC, new RegisterRefArgs
            {
                RefIdProperty = ModuleACIdProperty,
                Serializable = false,
                PropertyChangedCallBack = (o, e) => (o as AuditItemConditionCriteria).OnModuleACChanged(e)
            });
        public ModuleAC ModuleAC
        {
            get { return this.GetRefEntity(ModuleACProperty); }
            set { this.SetRefEntity(ModuleACProperty, value); }
        }
        protected virtual void OnModuleACChanged(ManagedPropertyChangedEventArgs e)
        {
            var value = e.NewValue as ModuleAC;

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
                DateTimePart = DateTimePart.Date,
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
                DateTimePart = DateTimePart.Date,
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
}