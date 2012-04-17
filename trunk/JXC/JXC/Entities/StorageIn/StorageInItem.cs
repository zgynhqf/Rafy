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
    public class StorageInItem : JXCEntity
    {
        public static readonly RefProperty<StorageInBill> StorageInBillRefProperty =
            P<StorageInItem>.RegisterRef(e => e.StorageInBill, ReferenceType.Parent);
        public int StorageInBillId
        {
            get { return this.GetRefId(StorageInBillRefProperty); }
            set { this.SetRefId(StorageInBillRefProperty, value); }
        }
        public StorageInBill StorageInBill
        {
            get { return this.GetRefEntity(StorageInBillRefProperty); }
            set { this.SetRefEntity(StorageInBillRefProperty, value); }
        }

        public static readonly RefProperty<Product> ProductRefProperty =
            P<StorageInItem>.RegisterRef(e => e.Product, ReferenceType.Normal);
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

        public static readonly Property<int> AmountProperty = P<StorageInItem>.Register(e => e.Amount);
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<string> View_ProductNameProperty = P<StorageInItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as StorageInItem).GetView_ProductName(), null);
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
    public class StorageInItemList : JXCEntityList { }

    public class StorageInItemRepository : EntityRepository
    {
        protected StorageInItemRepository() { }
    }

    internal class StorageInItemConfig : EntityConfig<StorageInItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("入库单项").HasDelegate(StorageInItem.View_ProductNameProperty);

            using (View.OrderProperties())
            {
                View.Property(StorageInItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageInItem.AmountProperty).HasLabel("数量").ShowIn(ShowInWhere.List);
            }
        }
    }
}