using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace Demo
{
    [ChildEntity, Serializable]
    public class Country : DemoEntity
    {
        public static readonly RefProperty<City> CityRefProperty =
            P<Country>.RegisterRef(e => e.City, ReferenceType.Parent);
        public int CityId
        {
            get { return this.GetRefId(CityRefProperty); }
            set { this.SetRefId(CityRefProperty, value); }
        }
        public City City
        {
            get { return this.GetRefEntity(CityRefProperty); }
            set { this.SetRefEntity(CityRefProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Country>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class CountryList : DemoEntityList { }

    public class CountryRepository : EntityRepository
    {
        protected CountryRepository() { }
    }

    internal class CountryConfig : EntityConfig<Country>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                Country.CityRefProperty,
                Country.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("县").HasDelegate(Country.NameProperty);

            View.Property(Country.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}