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
    public partial class Country : DemoEntity
    {
        #region 构造函数

        public Country() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Country(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty CityIdProperty =
            P<Country>.RegisterRefId(e => e.CityId, ReferenceType.Parent);
        public int CityId
        {
            get { return (int)this.GetRefId(CityIdProperty); }
            set { this.SetRefId(CityIdProperty, value); }
        }
        public static readonly RefEntityProperty<City> CityProperty =
            P<Country>.RegisterRef(e => e.City, CityIdProperty);
        public City City
        {
            get { return this.GetRefEntity(CityProperty); }
            set { this.SetRefEntity(CityProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Country>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public partial class CountryList : DemoEntityList { }

    public partial class CountryRepository : DemoEntityRepository
    {
        protected CountryRepository() { }
    }

    internal class CountryConfig : DemoEntityConfig<Country>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                Country.CityIdProperty,
                Country.NameProperty
                );
        }
    }

    internal class CountryWPFConfig : DemoEntityWPFViewConfig<Country>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("县").HasDelegate(Country.NameProperty);

            View.UseDefaultCommands();

            View.Property(Country.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}