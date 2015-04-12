using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using FM.Commands;
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

namespace FM
{
    [RootEntity, Serializable]
    public partial class FinanceLog : FMEntity
    {
        #region 构造函数

        public FinanceLog() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FinanceLog(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> UsersProperty = P<FinanceLog>.Register(e => e.Users, new PropertyMetadata<string>
        {
            PropertyChangingCallBack = (o, e) => (o as FinanceLog).OnUsersTagsChanging(e)
        });
        public string Users
        {
            get { return this.GetProperty(UsersProperty); }
            set { this.SetProperty(UsersProperty, value); }
        }

        protected virtual void OnUsersTagsChanging(ManagedPropertyChangingEventArgs<string> e)
        {
            var value = e.Value;
            if (value.Contains("，"))
            {
                e.CoercedValue = value.Replace("，", ",");
            }
        }

        public static readonly Property<string> TagsProperty = P<FinanceLog>.Register(e => e.Tags, new PropertyMetadata<string>
        {
            PropertyChangingCallBack = (o, e) => (o as FinanceLog).OnUsersTagsChanging(e)
        });
        public string Tags
        {
            get { return this.GetProperty(TagsProperty); }
            set { this.SetProperty(TagsProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<FinanceLog>.Register(e => e.Date, new PropertyMetadata<DateTime>
        {
            DateTimePart = DateTimePart.Date
        });
        public DateTime Date
        {
            get { return this.GetProperty(DateProperty); }
            set { this.SetProperty(DateProperty, value); }
        }

        public static readonly Property<double> AmountProperty = P<FinanceLog>.Register(e => e.Amount);
        public double Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<bool> IsOutProperty = P<FinanceLog>.Register(e => e.IsOut, true);
        public bool IsOut
        {
            get { return this.GetProperty(IsOutProperty); }
            set { this.SetProperty(IsOutProperty, value); }
        }

        public static readonly Property<string> ReasonProperty = P<FinanceLog>.Register(e => e.Reason);
        public string Reason
        {
            get { return this.GetProperty(ReasonProperty); }
            set { this.SetProperty(ReasonProperty, value); }
        }

        public static readonly Property<string> CommentProperty = P<FinanceLog>.Register(e => e.Comment);
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        #region 视图属性

        public static readonly Property<string> DescriptionProperty = P<FinanceLog>.RegisterReadOnly(e => e.Description, e => (e as FinanceLog).GetDescription());
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
        }
        private string GetDescription()
        {
            return string.Format("{0} {1} {2}，事由：{3}",
                this.Users, this.IsOut ? "支出" : "收入", this.Amount, this.Reason
                );
        }

        public static readonly Property<string> View_IsOutProperty = P<FinanceLog>.RegisterReadOnly(e => e.View_IsOut, e => (e as FinanceLog).GetView_IsOut());
        public string View_IsOut
        {
            get { return this.GetProperty(View_IsOutProperty); }
        }
        private string GetView_IsOut()
        {
            return this.IsOut ? "支出" : "收入";
        }

        public static readonly Property<TagList> TagDataSourceProperty = P<FinanceLog>.RegisterReadOnly(e => e.TagDataSource, e => (e as FinanceLog).GetTagDataSource());
        public TagList TagDataSource
        {
            get { return this.GetProperty(TagDataSourceProperty); }
        }
        private TagList GetTagDataSource()
        {
            return RF.Concrete<TagRepository>().GetValidList();
        }

        #endregion
    }

    [Serializable]
    public partial class FinanceLogList : FMEntityList { }

    public partial class FinanceLogRepository : FMEntityRepository
    {
        protected FinanceLogRepository() { }

        [DataProviderFor(typeof(FinanceLogRepository))]
        private class FinanceLogRepositoryDataProvider : RdbDataProvider
        {
            protected override void OnQuerying(EntityQueryArgs args)
            {
                var query = args.Query;
                query.OrderBy.Add(query.MainTable.Column(FinanceLog.DateProperty), OrderDirection.Descending);

                base.OnQuerying(args);
            }
        }
    }

    internal class FinanceLogConfig : FMEntityConfig<FinanceLog>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(FinanceLog.ReasonProperty, new RequiredRule());
            rules.AddRule(FinanceLog.AmountProperty, new PositiveNumberRule());
            rules.AddRule(FinanceLog.UsersProperty, new RequiredRule());
        }

        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }

    internal class FinanceLogWPFViewConfig : WPFViewConfig<FinanceLog>
    {
        protected override void ConfigView()
        {
            View.DomainName("经费记录").HasDelegate(FinanceLog.DescriptionProperty);

            View.UseCommands(
                WPFCommandNames.Edit,
                WPFCommandNames.Delete,
                WPFCommandNames.SaveList,
                WPFCommandNames.Cancel,
                WPFCommandNames.Refresh,
                WPFCommandNames.ExportToExcel
                );

            using (View.OrderProperties())
            {
                View.UseDetailLayoutMode(DetailLayoutMode.AutoGrid).HasDetailColumnsCount(2);

                using (View.DeclareGroup("基本信息"))
                {
                    View.Property(FinanceLog.DescriptionProperty).HasLabel("简述").ShowIn(ShowInWhere.ListDropDown);
                    View.Property(FinanceLog.ReasonProperty).HasLabel("事由").ShowIn(ShowInWhere.ListDetail);
                    View.Property(FinanceLog.AmountProperty).HasLabel("数量").ShowIn(ShowInWhere.ListDetail);
                    View.Property(FinanceLog.UsersProperty).HasLabel("相关人").ShowIn(ShowInWhere.ListDetail);
                    View.Property(FinanceLog.TagsProperty).HasLabel("标签").ShowIn(ShowInWhere.ListDetail);
                    View.Property(FinanceLog.DateProperty).HasLabel("日期").ShowIn(ShowInWhere.ListDetail);
                    View.Property(FinanceLog.IsOutProperty).HasLabel("支出").ShowIn(ShowInWhere.Detail);
                    View.Property(FinanceLog.View_IsOutProperty).HasLabel("支出").ShowIn(ShowInWhere.List);
                }
                using (View.DeclareGroup("事由"))
                {
                    View.Property(FinanceLog.CommentProperty).HasLabel("原因").ShowIn(ShowInWhere.ListDetail)
                        .ShowInDetail(columnSpan: 2, height: 100).UseEditor(WPFEditorNames.Memo);
                }
            }

            //两个多选的视图设置
            View.Property(FinanceLog.UsersProperty).UseEditor(WPFEditorNames.EntitySelection_DropDown)
                .SelectionViewMeta = new SelectionViewMeta
                {
                    SelectionEntityType = typeof(Person),
                    SelectedValuePath = Person.NameProperty,
                    SelectionMode = EntitySelectionMode.Multiple,
                };
            View.Property(FinanceLog.TagsProperty).UseEditor(WPFEditorNames.EntitySelection_DropDown)
                .SelectionViewMeta = new SelectionViewMeta
                {
                    SelectionEntityType = typeof(Tag),
                    DataSourceProperty = FinanceLog.TagDataSourceProperty,
                    SelectedValuePath = Tag.NameProperty,
                    SelectionMode = EntitySelectionMode.Multiple,
                };
        }
    }
}