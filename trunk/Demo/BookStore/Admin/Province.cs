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
    [RootEntity, Serializable]
    public class Province : DemoEntity
    {
        public static readonly Property<string> NameProperty = P<Province>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<CityList> CityListProperty = P<Province>.Register(e => e.CityList);
        [Association]
        public CityList CityList
        {
            get { return this.GetLazyChildren(CityListProperty); }
        }
    }

    [Serializable]
    public class ProvinceList : DemoEntityList { }

    public class ProvinceRepository : EntityRepository
    {
        protected ProvinceRepository() { }
    }

    internal class ProvinceConfig : EntityConfig<Province>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                Province.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("省").HasTitle(Province.NameProperty);

            View.Property(Province.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}