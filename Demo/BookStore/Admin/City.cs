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
    public class City : DemoEntity
    {
        public static readonly RefProperty<Province> ProvinceRefProperty =
            P<City>.RegisterRef(e => e.Province, ReferenceType.Parent);
        public int ProvinceId
        {
            get { return this.GetRefId(ProvinceRefProperty); }
            set { this.SetRefId(ProvinceRefProperty, value); }
        }
        public Province Province
        {
            get { return this.GetRefEntity(ProvinceRefProperty); }
            set { this.SetRefEntity(ProvinceRefProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<City>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<CountryList> CountryListProperty = P<City>.Register(e => e.CountryList);
        [Association]
        public CountryList CountryList
        {
            get { return this.GetLazyChildren(CountryListProperty); }
        }
    }

    [Serializable]
    public class CityList : DemoEntityList { }

    public class CityRepository : EntityRepository
    {
        protected CityRepository() { }
    }

    internal class CityConfig : EntityConfig<City>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                City.ProvinceRefProperty,
                City.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("市").HasTitle(City.NameProperty);

            View.Property(City.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}