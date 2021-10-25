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
using Rafy.MetaModel.View;

namespace Demo
{
    [ChildEntity, Serializable]
    public partial class City : DemoEntity
    {
        public static readonly IRefIdProperty ProvinceIdProperty =
            P<City>.RegisterRefId(e => e.ProvinceId, ReferenceType.Parent);
        public int ProvinceId
        {
            get { return (int)this.GetRefId(ProvinceIdProperty); }
            set { this.SetRefId(ProvinceIdProperty, value); }
        }
        public static readonly RefEntityProperty<Province> ProvinceProperty =
            P<City>.RegisterRef(e => e.Province, ProvinceIdProperty);
        public Province Province
        {
            get { return this.GetRefEntity(ProvinceProperty); }
            set { this.SetRefEntity(ProvinceProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<City>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly ListProperty<CountryList> CountryListProperty = P<City>.RegisterList(e => e.CountryList);
        public CountryList CountryList
        {
            get { return this.GetLazyList(CountryListProperty); }
        }
    }

    [Serializable]
    public partial class CityList : DemoEntityList { }

    public partial class CityRepository : DemoEntityRepository
    {
        protected CityRepository() { }
    }

    internal class CityConfig : DemoEntityConfig<City>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                City.ProvinceIdProperty,
                City.NameProperty
                );
        }
    }

    internal class CityWPFConfig : DemoEntityWPFViewConfig<City>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("市").HasDelegate(City.NameProperty);

            View.UseDefaultCommands();

            View.Property(City.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}