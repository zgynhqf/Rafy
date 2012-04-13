using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    [NavigationQueryType(typeof(ProductNavigationCriteria))]
    public class Product : JXCEntity
    {
        public static readonly Property<string> BianMaProperty = P<Product>.Register(e => e.BianMa);
        public string BianMa
        {
            get { return this.GetProperty(BianMaProperty); }
            set { this.SetProperty(BianMaProperty, value); }
        }

        public static readonly Property<string> MingChengProperty = P<Product>.Register(e => e.MingCheng);
        public string MingCheng
        {
            get { return this.GetProperty(MingChengProperty); }
            set { this.SetProperty(MingChengProperty, value); }
        }

        public static readonly RefProperty<ProductCategory> ProductCategoryRefProperty =
            P<Product>.RegisterRef(e => e.ProductCategory, ReferenceType.Normal);
        public int ProductCategoryId
        {
            get { return this.GetRefId(ProductCategoryRefProperty); }
            set { this.SetRefId(ProductCategoryRefProperty, value); }
        }
        public ProductCategory ProductCategory
        {
            get { return this.GetRefEntity(ProductCategoryRefProperty); }
            set { this.SetRefEntity(ProductCategoryRefProperty, value); }
        }

        public static readonly Property<string> GuiGeProperty = P<Product>.Register(e => e.GuiGe);
        public string GuiGe
        {
            get { return this.GetProperty(GuiGeProperty); }
            set { this.SetProperty(GuiGeProperty, value); }
        }

        public static readonly Property<string> PingPaiProperty = P<Product>.Register(e => e.PingPai);
        public string PingPai
        {
            get { return this.GetProperty(PingPaiProperty); }
            set { this.SetProperty(PingPaiProperty, value); }
        }

        public static readonly Property<double> CaiGouDanjiaProperty = P<Product>.Register(e => e.CaiGouDanjia);
        public double CaiGouDanjia
        {
            get { return this.GetProperty(CaiGouDanjiaProperty); }
            set { this.SetProperty(CaiGouDanjiaProperty, value); }
        }

        public static readonly Property<double> XiaoShouDanJiaProperty = P<Product>.Register(e => e.XiaoShouDanJia);
        public double XiaoShouDanJia
        {
            get { return this.GetProperty(XiaoShouDanJiaProperty); }
            set { this.SetProperty(XiaoShouDanJiaProperty, value); }
        }

        public static readonly RefProperty<ClientInfo> ClientInfoRefProperty =
            P<Product>.RegisterRef(e => e.ClientInfo, ReferenceType.Normal);
        public int ClientInfoId
        {
            get { return this.GetRefId(ClientInfoRefProperty); }
            set { this.SetRefId(ClientInfoRefProperty, value); }
        }
        public ClientInfo ClientInfo
        {
            get { return this.GetRefEntity(ClientInfoRefProperty); }
            set { this.SetRefEntity(ClientInfoRefProperty, value); }
        }

        public static readonly Property<double> XiaoShouJia_1Property = P<Product>.Register(e => e.XiaoShouJia_1);
        public double XiaoShouJia_1
        {
            get { return this.GetProperty(XiaoShouJia_1Property); }
            set { this.SetProperty(XiaoShouJia_1Property, value); }
        }

        public static readonly Property<double> XiaoShouJia_2Property = P<Product>.Register(e => e.XiaoShouJia_2);
        public double XiaoShouJia_2
        {
            get { return this.GetProperty(XiaoShouJia_2Property); }
            set { this.SetProperty(XiaoShouJia_2Property, value); }
        }

        public static readonly Property<double> XiaoShouJia_3Property = P<Product>.Register(e => e.XiaoShouJia_3);
        public double XiaoShouJia_3
        {
            get { return this.GetProperty(XiaoShouJia_3Property); }
            set { this.SetProperty(XiaoShouJia_3Property, value); }
        }

        public static readonly Property<string> BeiZhuProperty = P<Product>.Register(e => e.BeiZhu);
        public string BeiZhu
        {
            get { return this.GetProperty(BeiZhuProperty); }
            set { this.SetProperty(BeiZhuProperty, value); }
        }
    }

    [Serializable]
    public class ProductList : JXCEntityList
    {
        /// <summary>
        /// 导航面板查询
        /// </summary>
        /// <param name="criteria"></param>
        protected void QueryBy(ProductNavigationCriteria criteria)
        {
            this.QueryDb(q =>
            {
                q.Constrain(Product.ProductCategoryRefProperty).Equal(criteria.ProductCategoryId);
            });
        }
    }

    public class ProductRepository : EntityRepository
    {
        protected ProductRepository() { }
    }

    internal class ProductConfig : EntityConfig<Product>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                Product.BianMaProperty,
                Product.MingChengProperty,
                Product.ProductCategoryRefProperty,
                Product.GuiGeProperty,
                Product.PingPaiProperty,
                Product.CaiGouDanjiaProperty,
                Product.XiaoShouDanJiaProperty,
                Product.ClientInfoRefProperty,
                Product.BeiZhuProperty,
                Product.XiaoShouJia_1Property,
                Product.XiaoShouJia_2Property,
                Product.XiaoShouJia_3Property
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("商品").HasTitle(Product.MingChengProperty);

            View.Property(Product.BianMaProperty).HasLabel("编码").ShowIn(ShowInWhere.All)
                .ShowInDetail(columnSpan: 2, width: 0.7);
            View.Property(Product.MingChengProperty).HasLabel("名称").ShowIn(ShowInWhere.All)
                .ShowInDetail(columnSpan: 2, width: 600);
            View.Property(Product.ProductCategoryRefProperty).HasLabel("商品类别").ShowIn(ShowInWhere.All);
            View.Property(Product.GuiGeProperty).HasLabel("规格").ShowIn(ShowInWhere.All);
            View.Property(Product.PingPaiProperty).HasLabel("品牌").ShowIn(ShowInWhere.All)
                .ShowInDetail(columnSpan: 2);
            View.Property(Product.CaiGouDanjiaProperty).HasLabel("采购单价").ShowIn(ShowInWhere.All);
            View.Property(Product.XiaoShouDanJiaProperty).HasLabel("销售单价").ShowIn(ShowInWhere.All);
            View.Property(Product.ClientInfoRefProperty).HasLabel("销售商名称").ShowIn(ShowInWhere.All);
            View.Property(Product.XiaoShouJia_1Property).HasLabel("一级销售价").ShowIn(ShowInWhere.All);
            View.Property(Product.XiaoShouJia_2Property).HasLabel("二级销售价").ShowIn(ShowInWhere.All);
            View.Property(Product.XiaoShouJia_3Property).HasLabel("三级销售价").ShowIn(ShowInWhere.All);
            View.Property(Product.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.All)
                .ShowInDetail(columnSpan: 2, height: 200)
                .UseEditor(WPFEditorNames.Memo);
        }
    }
}