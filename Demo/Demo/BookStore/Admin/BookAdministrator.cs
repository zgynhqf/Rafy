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
    [RootEntity]
    public partial class BookAdministrator : DemoEntity
    {
        public static readonly Property<string> UserNameProperty = P<BookAdministrator>.Register(e => e.UserName);
        public string UserName
        {
            get { return this.GetProperty(UserNameProperty); }
            set { this.SetProperty(UserNameProperty, value); }
        }

        public static readonly Property<int> ProvinceIdProperty =
            P<BookAdministrator>.Register(e => e.ProvinceId);
        public int? ProvinceId
        {
            get { return (int?)this.GetRefNullableId(ProvinceIdProperty); }
            set { this.SetRefNullableId(ProvinceIdProperty, value); }
        }
        public static readonly RefEntityProperty<Province> ProvinceProperty =
            P<BookAdministrator>.RegisterRef(e => e.Province, new RegisterRefArgs
            {
                RefIdProperty = ProvinceIdProperty,
                PropertyChangedCallBack = (o, e) => (o as BookAdministrator).OnProvinceChanged(e)
            });
        public Province Province
        {
            get { return this.GetRefEntity(ProvinceProperty); }
            set { this.SetRefEntity(ProvinceProperty, value); }
        }
        protected virtual void OnProvinceChanged(ManagedPropertyChangedEventArgs e)
        {
            this.City = null;
        }

        public static readonly Property<string> ProvinceNameProperty =
            P<BookAdministrator>.RegisterReadOnly(e => e.ProvinceName, e => (e as BookAdministrator).GetProvinceName());
        public string ProvinceName
        {
            get { return this.GetProperty(ProvinceNameProperty); }
        }
        private string GetProvinceName()
        {
            return this.Province != null ? this.Province.Name : string.Empty;
        }

        public static readonly Property<int> CityIdProperty =
            P<BookAdministrator>.Register(e => e.CityId);
        public int? CityId
        {
            get { return (int?)this.GetRefNullableId(CityIdProperty); }
            set { this.SetRefNullableId(CityIdProperty, value); }
        }
        public static readonly RefEntityProperty<City> CityProperty =
            P<BookAdministrator>.RegisterRef(e => e.City, new RegisterRefArgs
            {
                RefIdProperty = CityIdProperty,
                PropertyChangedCallBack = (o, e) => (o as BookAdministrator).OnCityChanged(e)
            });
        public City City
        {
            get { return this.GetRefEntity(CityProperty); }
            set { this.SetRefEntity(CityProperty, value); }
        }
        protected virtual void OnCityChanged(ManagedPropertyChangedEventArgs e)
        {
            this.Country = null;
        }
        public static readonly Property<CityList> CityDataSourceProperty = P<BookAdministrator>.RegisterReadOnly(e => e.CityDataSource, e => (e as BookAdministrator).GetCityDataSource());
        public CityList CityDataSource
        {
            get { return this.GetProperty(CityDataSourceProperty); }
        }
        private CityList GetCityDataSource()
        {
            return this.Province.CityList;
        }

        public static readonly Property<int> CountryIdProperty =
            P<BookAdministrator>.Register(e => e.CountryId);
        public int? CountryId
        {
            get { return (int?)this.GetRefNullableId(CountryIdProperty); }
            set { this.SetRefNullableId(CountryIdProperty, value); }
        }
        public static readonly RefEntityProperty<Country> CountryProperty =
            P<BookAdministrator>.RegisterRef(e => e.Country, CountryIdProperty);
        public Country Country
        {
            get { return this.GetRefEntity(CountryProperty); }
            set { this.SetRefEntity(CountryProperty, value); }
        }
        public static readonly Property<CountryList> CountryDataSourceProperty = P<BookAdministrator>.RegisterReadOnly(e => e.CountryDataSource, e => (e as BookAdministrator).GetCountryDataSource());
        public CountryList CountryDataSource
        {
            get { return this.GetProperty(CountryDataSourceProperty); }
        }
        private CountryList GetCountryDataSource()
        {
            return this.City.CountryList;
        }
    }

    public partial class BookAdministratorList : DemoEntityList { }

    public partial class BookAdministratorRepository : DemoEntityRepository
    {
        protected BookAdministratorRepository() { }
    }

    internal class BookAdministratorConfig : DemoEntityConfig<BookAdministrator>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                BookAdministrator.ProvinceIdProperty,
                BookAdministrator.CityIdProperty,
                BookAdministrator.CountryIdProperty,
                BookAdministrator.UserNameProperty
                );
        }
    }

    internal class BookAdministratorWPFConfig : DemoEntityWPFViewConfig<BookAdministrator>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("管理员").HasDelegate(BookAdministrator.UserNameProperty);

            View.UseDefaultCommands();

            View.Property(BookAdministrator.UserNameProperty).HasLabel("姓名").ShowIn(ShowInWhere.All);

            //三级联动示例
            View.Property(BookAdministrator.ProvinceProperty).HasLabel("住址：省").ShowIn(ShowInWhere.All)
                .UseEditor(WPFEditorNames.EntitySelection_Popup);
            View.Property(BookAdministrator.CityProperty).HasLabel("住址：市").ShowIn(ShowInWhere.All)
                .UseDataSource(BookAdministrator.CityDataSourceProperty);
            View.Property(BookAdministrator.CountryProperty).HasLabel("住址：县").ShowIn(ShowInWhere.All)
                .UseDataSource(BookAdministrator.CountryDataSourceProperty);
        }
    }
}