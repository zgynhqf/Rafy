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
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.RBAC.Old.Audit;

namespace Rafy.RBAC.Old.Audit
{
    /// <summary>
    /// 审查日志项
    /// </summary>
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(AuditItemConditionCriteria))]
    public partial class AuditItem : IntEntity
    {
        public static readonly Property<string> TitleProperty = P<AuditItem>.Register(e => e.Title);
        public string Title
        {
            get { return this.GetProperty(TitleProperty); }
            set { this.SetProperty(TitleProperty, value); }
        }

        public static readonly Property<string> ContentProperty = P<AuditItem>.Register(e => e.Content);
        public string Content
        {
            get { return this.GetProperty(ContentProperty); }
            set { this.SetProperty(ContentProperty, value); }
        }

        public static readonly Property<string> PrivateContentProperty = P<AuditItem>.Register(e => e.PrivateContent);
        public string PrivateContent
        {
            get { return this.GetProperty(PrivateContentProperty); }
            set { this.SetProperty(PrivateContentProperty, value); }
        }

        public static readonly Property<string> UserProperty = P<AuditItem>.Register(e => e.User);
        public string User
        {
            get { return this.GetProperty(UserProperty); }
            set { this.SetProperty(UserProperty, value); }
        }

        public static readonly Property<string> MachineNameProperty = P<AuditItem>.Register(e => e.MachineName);
        public string MachineName
        {
            get { return this.GetProperty(MachineNameProperty); }
            set { this.SetProperty(MachineNameProperty, value); }
        }

        public static readonly Property<string> ModuleNameProperty = P<AuditItem>.Register(a => a.ModuleName);
        public string ModuleName
        {
            get { return this.GetProperty(ModuleNameProperty); }
            set { this.SetProperty(ModuleNameProperty, value); }
        }

        public static readonly Property<AuditLogType> TypeProperty = P<AuditItem>.Register(a => a.Type, AuditLogType.None);
        public AuditLogType Type
        {
            get { return this.GetProperty(TypeProperty); }
            set { this.SetProperty(TypeProperty, value); }
        }

        public static readonly Property<DateTime> LogTimeProperty = P<AuditItem>.Register(a => a.LogTime);
        public DateTime LogTime
        {
            get { return this.GetProperty(LogTimeProperty); }
            set { this.SetProperty(LogTimeProperty, value); }
        }

        /// <summary>
        /// 当前操作的实体对象的ID
        /// </summary>
        public static readonly Property<int?> EntityIdProperty = P<AuditItem>.Register(a => a.EntityId);
        public int? EntityId
        {
            get { return this.GetProperty(EntityIdProperty); }
            set { this.SetProperty(EntityIdProperty, value); }
        }

        #region 性能测试代码

        //static AuditItem()
        //{
        //    var ran = new Random();

        //    for (int i = 0; i < 100; i++)
        //    {
        //        if (i % 2 == 0)
        //        {
        //            P<AuditItem>.RegisterExtension("Name" + i, typeof(AuditItem), "默认数据");
        //        }
        //        else
        //        {
        //            P<AuditItem>.RegisterExtension("Name" + i, typeof(AuditItem), ran.Next());
        //        }
        //    }
        //}

        //        for (int i = 0; i < 100; i++)
        //        {
        //            View.Property("Name" + i).ShowIn(ShowInWhere.List);
        //        }

        #endregion
    }

    [Serializable]
    public partial class AuditItemList : EntityList { }

    public partial class AuditItemRepository : EntityRepository
    {
        protected AuditItemRepository() { }

        /// <summary>
        /// 条件面板查询
        /// </summary>
        /// <param name="criteria"></param>
        [RepositoryQuery]
        public virtual AuditItemList GetBy(AuditItemConditionCriteria criteria)
        {
            //起始时间和终止时间只精确到日。终止时间往后推一天
            var startDate = criteria.StartTime.Date;
            var endDate = criteria.EndTime.AddDays(1d).Date;

            var f = QueryFactory.Instance;
            var q = f.Query(this);


            //拼装查询条件
            q.AddConstraint(AuditItem.LogTimeProperty, PropertyOperator.GreaterEqual, startDate);
            q.AddConstraint(AuditItem.LogTimeProperty, PropertyOperator.LessEqual, endDate);

            q.AddConstraintIf(AuditItem.ContentProperty, PropertyOperator.Contains, criteria.ContentKeyWords);
            q.AddConstraintIf(AuditItem.UserProperty, PropertyOperator.Contains, criteria.UserKeyWords);
            q.AddConstraintIf(AuditItem.MachineNameProperty, PropertyOperator.Contains, criteria.MachineKeyWords);
            q.AddConstraintIf(AuditItem.TitleProperty, PropertyOperator.Contains, criteria.TitleKeyWords);

            if (RafyEnvironment.Location.IsWebUI)
            {
                if (criteria.ModuleACId.HasValue)
                {
                    q.AddConstraintIf(AuditItem.ModuleNameProperty, PropertyOperator.Equal, criteria.ModuleAC.KeyLabel);
                }
            }
            else
            {
                q.AddConstraintIf(AuditItem.ModuleNameProperty, PropertyOperator.Equal, criteria.ModuleName);
            }

            if (criteria.AuditLogType != AuditLogType.None)
            {
                q.AddConstraintIf(AuditItem.TypeProperty, PropertyOperator.Equal, criteria.AuditLogType);
            }

            return (AuditItemList)this.QueryData(q);
        }
    }

    [DataProviderFor(typeof(AuditItemRepository))]
    public partial class AuditItemRepositoryDataProvider : RdbDataProvider
    {
        public AuditItemRepositoryDataProvider()
        {
            this.DataQueryer = new AuditItemRepositoryQueryer();
        }

        private class AuditItemRepositoryQueryer : RdbDataQueryer
        {
            protected override void OnQuerying(ORMQueryArgs args)
            {
                var query = args.Query;
                query.OrderBy.Add(query.MainTable.Column(AuditItem.LogTimeProperty), OrderDirection.Descending);

                base.OnQuerying(args);
            }
        }
    }


    internal class AuditItemConfig : EntityConfig<AuditItem>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                AuditItem.TitleProperty,
                AuditItem.ContentProperty,
                AuditItem.PrivateContentProperty,
                AuditItem.UserProperty,
                AuditItem.MachineNameProperty,
                AuditItem.ModuleNameProperty,
                AuditItem.TypeProperty,
                AuditItem.LogTimeProperty,
                AuditItem.EntityIdProperty
                );
        }
    }
}