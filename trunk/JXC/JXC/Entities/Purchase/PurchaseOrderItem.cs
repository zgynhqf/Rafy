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
    [ChildEntity, Serializable]
    public class PurchaseOrderItem : JXCEntity
    {
        public static readonly RefProperty<PurchaseOrder> PurchaseOrderRefProperty =
            P<PurchaseOrderItem>.RegisterRef(e => e.PurchaseOrder, ReferenceType.Parent);
        public int PurchaseOrderId
        {
            get { return this.GetRefId(PurchaseOrderRefProperty); }
            set { this.SetRefId(PurchaseOrderRefProperty, value); }
        }
        public PurchaseOrder PurchaseOrder
        {
            get { return this.GetRefEntity(PurchaseOrderRefProperty); }
            set { this.SetRefEntity(PurchaseOrderRefProperty, value); }
        }

        public static readonly RefProperty<Product> ProductRefProperty =
            P<PurchaseOrderItem>.RegisterRef(e => e.Product, ReferenceType.Normal);
        public int ProductId
        {
            get { return this.GetRefId(ProductRefProperty); }
            set { this.SetRefId(ProductRefProperty, value); }
        }
        public Product Product
        {
            get { return this.GetRefEntity(ProductRefProperty); }
            set { this.SetRefEntity(ProductRefProperty, value); }
        }

        public static readonly Property<double> AmountProperty = P<PurchaseOrderItem>.Register(e => e.Amount);
        public double Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<double> RawPriceProperty = P<PurchaseOrderItem>.Register(e => e.RawPrice);
        public double RawPrice
        {
            get { return this.GetProperty(RawPriceProperty); }
            set { this.SetProperty(RawPriceProperty, value); }
        }

        public static readonly Property<string> View_ProductNameProperty = P<PurchaseOrderItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as PurchaseOrderItem).GetView_ProductName(), null);
        public string View_ProductName
        {
            get { return this.GetProperty(View_ProductNameProperty); }
        }
        private string GetView_ProductName()
        {
            return this.Product.MingCheng;
        }
    }

    [Serializable]
    public class PurchaseOrderItemList : JXCEntityList { }

    public class PurchaseOrderItemRepository : EntityRepository
    {
        protected PurchaseOrderItemRepository() { }
    }

    internal class PurchaseOrderItemConfig : EntityConfig<PurchaseOrderItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("商品订单项").HasDelegate(PurchaseOrderItem.View_ProductNameProperty);

            using (View.OrderProperties())
            {
                View.Property(PurchaseOrderItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.List);
            }
        }
    }
}