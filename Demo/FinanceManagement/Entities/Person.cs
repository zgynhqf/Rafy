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
    public class Person : FMEntity
    {
        public static readonly Property<string> NameProperty = P<Person>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<Person>.Register(e => e.IsDefault);
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }
    }

    [Serializable]
    public class PersonList : FMEntityList { }

    public class PersonRepository : EntityRepository
    {
        protected PersonRepository() { }
    }

    internal class PersonConfig : EntityConfig<Person>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();

            Meta.EnableCache();
        }

        protected override void ConfigView()
        {
            View.DomainName("相关人").HasDelegate(Person.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(Person.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(Person.IsDefaultProperty).HasLabel("默认").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}