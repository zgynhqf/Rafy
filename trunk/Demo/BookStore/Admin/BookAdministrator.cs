/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120404
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120404
 * 
*******************************************************/

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
    public class BookAdministrator : DemoEntity
    {
        public static readonly Property<string> UserNameProperty = P<BookAdministrator>.Register(e => e.UserName);
        public string UserName
        {
            get { return this.GetProperty(UserNameProperty); }
            set { this.SetProperty(UserNameProperty, value); }
        }

        public static readonly RefProperty<Province> ProvinceRefProperty =
            P<BookAdministrator>.RegisterRef(e => e.Province, ReferenceType.Normal);
        public int? ProvinceId
        {
            get { return this.GetRefNullableId(ProvinceRefProperty); }
            set { this.SetRefNullableId(ProvinceRefProperty, value); }
        }
        public Province Province
        {
            get { return this.GetRefEntity(ProvinceRefProperty); }
            set { this.SetRefEntity(ProvinceRefProperty, value); }
        }

        public static readonly RefProperty<City> CityRefProperty =
            P<BookAdministrator>.RegisterRef(e => e.City, ReferenceType.Normal);
        public int? CityId
        {
            get { return this.GetRefNullableId(CityRefProperty); }
            set { this.SetRefNullableId(CityRefProperty, value); }
        }
        public City City
        {
            get { return this.GetRefEntity(CityRefProperty); }
            set { this.SetRefEntity(CityRefProperty, value); }
        }

        public static readonly RefProperty<Country> CountryRefProperty =
            P<BookAdministrator>.RegisterRef(e => e.Country, ReferenceType.Normal);
        public int? CountryId
        {
            get { return this.GetRefNullableId(CountryRefProperty); }
            set { this.SetRefNullableId(CountryRefProperty, value); }
        }
        public Country Country
        {
            get { return this.GetRefEntity(CountryRefProperty); }
            set { this.SetRefEntity(CountryRefProperty, value); }
        }
    }

    [Serializable]
    public class BookAdministratorList : DemoEntityList { }

    public class BookAdministratorRepository : EntityRepository
    {
        protected BookAdministratorRepository() { }
    }

    internal class BookAdministratorConfig : EntityConfig<BookAdministrator>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                BookAdministrator.ProvinceRefProperty,
                BookAdministrator.CityRefProperty,
                BookAdministrator.CountryRefProperty,
                BookAdministrator.UserNameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("管理员").HasTitle(BookAdministrator.UserNameProperty);

            View.Property(BookAdministrator.UserNameProperty).HasLabel("姓名").ShowIn(ShowInWhere.All);

            //三级联动示例
            View.Property(BookAdministrator.ProvinceRefProperty).HasLabel("住址：省").ShowIn(ShowInWhere.All);
            View.Property(BookAdministrator.CityRefProperty).HasLabel("住址：市").ShowIn(ShowInWhere.All)
                .ReferenceViewInfo.DataSourceProperty = "Province.CityList";
            View.Property(BookAdministrator.CountryRefProperty).HasLabel("住址：县").ShowIn(ShowInWhere.All)
                .ReferenceViewInfo.DataSourceProperty = "City.CountryList";
        }
    }
}