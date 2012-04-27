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

        public static readonly Property<DateTime> LastUsedTimeProperty = P<Tag>.Register(e => e.LastUsedTime);
        public DateTime LastUsedTime
        {
            get { return this.GetProperty(LastUsedTimeProperty); }
            set { this.SetProperty(LastUsedTimeProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<Tag>.Register(e => e.IsDefault);
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }
    }

    [Serializable]
    public class TagList : FMEntityList { }

    public class TagRepository : EntityRepository
    {
        protected TagRepository() { }
    }

    internal class TagConfig : EntityConfig<Tag>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();

            Meta.DataOrderBy(Tag.LastUsedTimeProperty, false);

            Meta.EnableCache();
        }

        protected override void ConfigView()
        {
            View.DomainName("常用标签").HasDelegate(Tag.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(Tag.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(Tag.IsDefaultProperty).HasLabel("默认").ShowIn(ShowInWhere.ListDetail);
                //View.Property(Tag.OrderNoProperty).HasLabel("排序号").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}