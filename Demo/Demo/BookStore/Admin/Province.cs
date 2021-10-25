using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Demo
{
    [RootEntity, Serializable]
    public partial class Province : DemoEntity
    {
        public static readonly Property<string> NameProperty = P<Province>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly ListProperty<CityList> CityListProperty = P<Province>.RegisterList(e => e.CityList);
        public CityList CityList
        {
            get { return this.GetLazyList(CityListProperty); }
        }
    }

    [Serializable]
    public partial class ProvinceList : DemoEntityList { }

    public partial class ProvinceRepository : DemoEntityRepository
    {
        protected ProvinceRepository() { }
    }

    internal class ProvinceConfig : DemoEntityConfig<Province>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                Province.NameProperty
                );
        }
    }

    internal class ProvinceWPFViewConfig : DemoEntityWPFViewConfig<Province>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("省").HasDelegate(Province.NameProperty);

            View.UseDefaultCommands();

            View.Property(Province.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}