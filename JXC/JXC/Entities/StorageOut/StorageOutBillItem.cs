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
    public class StorageOutBillItem : JXCEntity
    {
        public static readonly RefProperty<StorageOutBill> StorageOutBillRefProperty =
            P<StorageOutBillItem>.RegisterRef(e => e.StorageOutBill, ReferenceType.Parent);
        public int StorageOutBillId
        {
            get { return this.GetRefId(StorageOutBillRefProperty); }
            set { this.SetRefId(StorageOutBillRefProperty, value); }
        }
        public StorageOutBill StorageOutBill
        {
            get { return this.GetRefEntity(StorageOutBillRefProperty); }
            set { this.SetRefEntity(StorageOutBillRefProperty, value); }
        }

        public static readonly RefProperty<Product> ProductRefProperty =
            P<StorageOutBillItem>.RegisterRef(e => e.Product, ReferenceType.Normal);
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

        public static readonly Property<string> View_ProductNameProperty = P<StorageOutBillItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as StorageOutBillItem).GetView_ProductName(), null);
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
    public class StorageOutBillItemList : JXCEntityList { }

    public class StorageOutBillItemRepository : EntityRepository
    {
        protected StorageOutBillItemRepository() { }
    }

    internal class StorageOutBillItemConfig : EntityConfig<StorageOutBillItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("出库单项").HasDelegate(StorageOutBillItem.View_ProductNameProperty);

            using (View.OrderProperties())
            {
                View.Property(StorageOutBillItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}