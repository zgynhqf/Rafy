using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace FM
{
    [RootEntity, Serializable]
    public partial class Tag : FMEntity
    {
        public static readonly Property<string> NameProperty = P<Tag>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<int> OrderNoProperty = P<Tag>.Register(e => e.OrderNo);
        public int OrderNo
        {
            get { return this.GetProperty(OrderNoProperty); }
            set { this.SetProperty(OrderNoProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<Tag>.Register(e => e.IsDefault);
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }

        public static readonly Property<bool> NotUsedProperty = P<Tag>.Register(e => e.NotUsed);
        public bool NotUsed
        {
            get { return this.GetProperty(NotUsedProperty); }
            set { this.SetProperty(NotUsedProperty, value); }
        }
    }

    [Serializable]
    public partial class TagList : FMEntityList { }

    public partial class TagRepository : FMEntityRepository
    {
        protected TagRepository() { }

        public TagList GetValidList()
        {
            var all = this.CacheAll();
            return this.CreateList(all.Where(e => !e.CastTo<Tag>().NotUsed)) as TagList;
        }

        [DataProviderFor(typeof(TagRepository))]
        private class TagRepositoryDataProvider : RdbDataProvider
        {
            protected override void OnQuerying(ORMQueryArgs args)
            {
                var query = args.Query;
                query.OrderBy.Add(query.MainTable.Column(Tag.OrderNoProperty));
                query.OrderBy.Add(query.MainTable.Column(Tag.IdProperty));

                base.OnQuerying(args);
            }
        }
    }

    internal class TagConfig : FMEntityConfig<Tag>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache();
        }
    }

    internal class TagWPFViewConfig : WPFViewConfig<Tag>
    {
        protected override void ConfigView()
        {
            View.DomainName("常用标签").HasDelegate(Tag.NameProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(Tag.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(Tag.IsDefaultProperty).HasLabel("默认").ShowIn(ShowInWhere.ListDetail);
                View.Property(Tag.NotUsedProperty).HasLabel("禁用").ShowIn(ShowInWhere.ListDetail);
                View.Property(Tag.OrderNoProperty).HasLabel("排序号").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}