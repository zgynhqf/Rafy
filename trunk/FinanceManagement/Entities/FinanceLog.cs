using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using FM.Commands;

namespace FM
{
    [RootEntity, Serializable]
    public class FinanceLog : FMEntity
    {
        public static readonly Property<string> UsersProperty = P<FinanceLog>.Register(e => e.Users);
        public string Users
        {
            get { return this.GetProperty(UsersProperty); }
            set { this.SetProperty(UsersProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<FinanceLog>.Register(e => e.Date);
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

        public static readonly Property<string> TagsProperty = P<FinanceLog>.Register(e => e.Tags);
        public string Tags
        {
            get { return this.GetProperty(TagsProperty); }
            set { this.SetProperty(TagsProperty, value); }
        }

        public static readonly Property<string> ReasonProperty = P<FinanceLog>.Register(e => e.Reason);
        public string Reason
        {
            get { return this.GetProperty(ReasonProperty); }
            set { this.SetProperty(ReasonProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<FinanceLog>.RegisterReadOnly(e => e.Name, e => (e as FinanceLog).GetName(), null);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
        }
        private string GetName()
        {
            return string.Format("{0} {1} {2}",
                this.Users,
                this.IsOut ? "支出" : "收入",
                this.Amount
                );
        }

        public static readonly Property<string> View_IsOutProperty = P<FinanceLog>.RegisterReadOnly(e => e.View_IsOut, e => (e as FinanceLog).GetView_IsOut(), null);
        public string View_IsOut
        {
            get { return this.GetProperty(View_IsOutProperty); }
        }
        private string GetView_IsOut()
        {
            return this.IsOut ? "支出" : "收入";
        }

        protected override void AddValidations()
        {
            base.AddValidations();

            var rules = this.ValidationRules;
            rules.AddRule(UsersProperty, CommonRules.Required);
            rules.AddRule(AmountProperty, CommonRules.Positive);
        }
    }

    [Serializable]
    public class FinanceLogList : FMEntityList { }

    public class FinanceLogRepository : EntityRepository
    {
        protected FinanceLogRepository() { }
    }

    internal class FinanceLogConfig : EntityConfig<FinanceLog>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();

            Meta.DataOrderBy(FinanceLog.DateProperty, false);
        }

        protected override void ConfigView()
        {
            View.DomainName("经费记录").HasDelegate(FinanceLog.NameProperty);

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                    WPFCommandNames.Edit,
                    WPFCommandNames.Delete,
                    WPFCommandNames.SaveList,
                    WPFCommandNames.Cancel,
                    WPFCommandNames.Refresh,
                    WPFCommandNames.ExportToExcel
                );

            using (View.OrderProperties())
            {
                View.Property(FinanceLog.NameProperty).HasLabel("简述").ShowIn(ShowInWhere.ListDropDown);
                View.Property(FinanceLog.UsersProperty).HasLabel("相关人").ShowIn(ShowInWhere.ListDetail);
                View.Property(FinanceLog.DateProperty).HasLabel("日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(FinanceLog.AmountProperty).HasLabel("数量").ShowIn(ShowInWhere.ListDetail);
                View.Property(FinanceLog.TagsProperty).HasLabel("标签").ShowIn(ShowInWhere.ListDetail);
                View.Property(FinanceLog.IsOutProperty).HasLabel("支出").ShowIn(ShowInWhere.Detail);
                View.Property(FinanceLog.View_IsOutProperty).HasLabel("支出").ShowIn(ShowInWhere.List);
                View.Property(FinanceLog.ReasonProperty).HasLabel("原因").ShowIn(ShowInWhere.ListDetail)
                    .ShowInDetail(columnSpan: 2, height: 100).UseEditor(WPFEditorNames.Memo);
            }

            //两个多选的视图设置
            View.Property(FinanceLog.UsersProperty).UseEditor(WPFEditorNames.LookupDropDown)
                .ReferenceViewInfo = new ReferenceViewInfo
                {
                    SelectedValuePath = Person.NameProperty.Name,
                    SelectionMode = ReferenceSelectionMode.Multiple,
                    RefType = typeof(Person),
                };
            View.Property(FinanceLog.TagsProperty).UseEditor(WPFEditorNames.LookupDropDown)
                .ReferenceViewInfo = new ReferenceViewInfo
                {
                    SelectedValuePath = Tag.NameProperty.Name,
                    SelectionMode = ReferenceSelectionMode.Multiple,
                    RefType = typeof(Tag),
                };
        }
    }
}