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
    public class OrderStorageInBill : StorageInBill
    {
        public static readonly RefProperty<PurchaseOrder> PurchaseOrderRefProperty =
            P<OrderStorageInBill>.RegisterRef(e => e.Order, ReferenceType.Normal);
        public int OrderId
        {
            get { return this.GetRefId(PurchaseOrderRefProperty); }
            set { this.SetRefId(PurchaseOrderRefProperty, value); }
        }
        public PurchaseOrder Order
        {
            get { return this.GetRefEntity(PurchaseOrderRefProperty); }
            set { this.SetRefEntity(PurchaseOrderRefProperty, value); }
        }

        public static readonly Property<string> View_SupplierNameProperty = P<OrderStorageInBill>.RegisterReadOnly(e => e.View_SupplierName, e => (e as OrderStorageInBill).GetView_SupplierName(), null);
        public string View_SupplierName
        {
            get { return this.GetProperty(View_SupplierNameProperty); }
        }
        private string GetView_SupplierName()
        {
            return this.Order.Supplier.Name;
        }
    }

    [Serializable]
    public class OrderStorageInBillList : StorageInBillList { }

    public class OrderStorageInBillRepository : EntityRepository
    {
        protected OrderStorageInBillRepository() { }
    }

    internal class OrderStorageInBillConfig : EntityConfig<OrderStorageInBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("采购入库单").HasDelegate(StorageInBill.CodeProperty);

            using (View.OrderProperties())
            {
                View.Property(StorageInBill.CodeProperty).HasLabel("商品入库编号").ShowIn(ShowInWhere.All);
                View.Property(StorageInBill.TotalMoneyProperty).HasLabel("金额").ShowIn(ShowInWhere.ListDetail);

                View.Property(OrderStorageInBill.PurchaseOrderRefProperty).HasLabel("商品订单").ShowIn(ShowInWhere.ListDetail);
                View.Property(OrderStorageInBill.View_SupplierNameProperty).HasLabel("供应商").ShowIn(ShowInWhere.ListDetail);

                View.Property(StorageInBill.DateProperty).HasLabel("入库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageInBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}