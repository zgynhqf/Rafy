/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120413
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
using OEA.RBAC;
using OEA.RBAC.Security;
using OEA;

namespace JXC
{
    [RootEntity, Serializable]
    [NavigationQueryType(typeof(ProductNavigationCriteria))]
    public class Product : JXCEntity
    {
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

        public static readonly RefProperty<ClientInfo> SupplierRefProperty =
            P<Product>.RegisterRef(e => e.Supplier, ReferenceType.Normal);
        public int SupplierId
        {
            get { return this.GetRefId(SupplierRefProperty); }
            set { this.SetRefId(SupplierRefProperty, value); }
        }
        public ClientInfo Supplier
        {
            get { return this.GetRefEntity(SupplierRefProperty); }
            set { this.SetRefEntity(SupplierRefProperty, value); }
        }

        public static readonly RefProperty<User> OperatorRefProperty =
            P<Product>.RegisterRef(e => e.Operator, ReferenceType.Normal);
        public int OperatorId
        {
            get { return this.GetRefId(OperatorRefProperty); }
            set { this.SetRefId(OperatorRefProperty, value); }
        }
        public User Operator
        {
            get { return this.GetRefEntity(OperatorRefProperty); }
            set { this.SetRefEntity(OperatorRefProperty, value); }
        }

        public static readonly Property<string> BarcodeProperty = P<Product>.Register(e => e.Barcode);
        public string Barcode
        {
            get { return this.GetProperty(BarcodeProperty); }
            set { this.SetProperty(BarcodeProperty, value); }
        }

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

        public static readonly Property<DateTime> OperateTimeProperty = P<Product>.Register(e => e.OperateTime);
        public DateTime OperateTime
        {
            get { return this.GetProperty(OperateTimeProperty); }
            set { this.SetProperty(OperateTimeProperty, value); }
        }

        public static readonly Property<int> StorageAmountProperty = P<Product>.Register(e => e.StorageAmount);
        public int StorageAmount
        {
            get { return this.GetProperty(StorageAmountProperty); }
            set { this.SetProperty(StorageAmountProperty, value); }
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

                if (criteria.IncludeSub)
                {
                    foreach (var child in criteria.ProductCategory.GetTreeChildrenRecur())
                    {
                        q.Or().Constrain(Product.ProductCategoryRefProperty).Equal(child.Id);
                    }
                }
            });
        }

        protected void QueryBy(string barcode)
        {
            this.QueryDb(q => q.Constrain(Product.BarcodeProperty).Equal(barcode));
        }
    }

    public class ProductRepository : EntityRepository
    {
        protected ProductRepository() { }

        public Product GetByBarcode(string barcode)
        {
            return this.FetchFirstAs<Product>(barcode);
        }
    }

    internal class ProductConfig : EntityConfig<Product>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();

            Meta.Property(Product.SupplierRefProperty).MapColumn().HasColumnName("ClientInfoId");
            Meta.Property(Product.OperatorRefProperty).MapColumn().HasColumnName("UserId");
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("商品").HasDelegate(Product.MingChengProperty);

            View.UseWPFCommands("JXC.Commands.ResetProductAmountCommand");

            using (View.OrderProperties())
            {
                View.Property(Product.BianMaProperty).HasLabel("编码").ShowIn(ShowInWhere.All)
                    .ShowInDetail(columnSpan: 2, width: 0.7);
                View.Property(Product.MingChengProperty).HasLabel("名称").ShowIn(ShowInWhere.All)
                    .ShowInDetail(columnSpan: 2, width: 600);
                View.Property(Product.BarcodeProperty).HasLabel("条码").ShowIn(ShowInWhere.All)
                    .ShowInDetail(columnSpan: 2, width: 0.7);
                View.Property(Product.ProductCategoryRefProperty).HasLabel("商品类别").ShowIn(ShowInWhere.All);
                View.Property(Product.GuiGeProperty).HasLabel("规格").ShowIn(ShowInWhere.All);
                View.Property(Product.PingPaiProperty).HasLabel("品牌").ShowIn(ShowInWhere.All)
                    .ShowInDetail(columnSpan: 2);
                View.Property(Product.StorageAmountProperty).HasLabel("库存量").ShowIn(ShowInWhere.List).Readonly();
                View.Property(Product.CaiGouDanjiaProperty).HasLabel("采购单价").ShowIn(ShowInWhere.All);
                View.Property(Product.XiaoShouDanJiaProperty).HasLabel("销售单价").ShowIn(ShowInWhere.All);
                View.Property(Product.SupplierRefProperty).HasLabel("销售商名称").ShowIn(ShowInWhere.All);
                View.Property(Product.XiaoShouJia_1Property).HasLabel("一级销售价").ShowIn(ShowInWhere.All);
                View.Property(Product.XiaoShouJia_2Property).HasLabel("二级销售价").ShowIn(ShowInWhere.All);
                View.Property(Product.XiaoShouJia_3Property).HasLabel("三级销售价").ShowIn(ShowInWhere.All);
                View.Property(Product.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.All)
                    .ShowMemoInDetail();
                View.Property(Product.OperateTimeProperty).HasLabel("操作时间").ShowIn(ShowInWhere.Detail).Readonly();
                View.Property(Product.OperatorRefProperty).HasLabel("操作员").ShowIn(ShowInWhere.Detail).Readonly();
            }
        }
    }
}