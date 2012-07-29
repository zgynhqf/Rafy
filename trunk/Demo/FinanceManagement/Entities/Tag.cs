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
using OEA.ORM;

namespace FM
{
    [RootEntity, Serializable]
    public class Tag : FMEntity
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
    public class TagList : FMEntityList
    {
        protected override void OnQueryDbOrder(IQuery query)
        {
            query.Order(Tag.OrderNoProperty, true).Order(Tag.IdProperty, true);
        }
    }

    public class TagRepository : EntityRepository
    {
        protected TagRepository() { }

        public TagList GetValidList()
        {
            var all = this.GetAll();
            return this.CreateList(all.Where(e => !e.CastTo<Tag>().NotUsed)) as TagList;
        }
    }

    internal class TagConfig : EntityConfig<Tag>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();

            //Meta.DataOrderBy(Tag.OrderNoProperty, false);

            Meta.EnableCache();
        }

        protected override void ConfigView()
        {
            View.DomainName("常用标签").HasDelegate(Tag.NameProperty);

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