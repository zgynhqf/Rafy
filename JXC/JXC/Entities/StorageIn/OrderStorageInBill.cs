using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA;
using JXC.Commands;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public class OrderStorageInBill : StorageInBill
    {
        public static readonly RefProperty<PurchaseOrder> PurchaseOrderRefProperty =
            P<OrderStorageInBill>.RegisterRef(e => e.Order, new RefPropertyMeta
            {
                RefEntityChangedCallBack = (o, e) => (o as OrderStorageInBill).OnOrderChanged(e),
            });
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
        protected virtual void OnOrderChanged(RefEntityChangedEventArgs e)
        {
            var value = e.NewEntity as PurchaseOrder;

            var children = this.StorageInItemList;
            children.Clear();
            if (value != null)
            {
                foreach (PurchaseOrderItem item in value.PurchaseOrderItemList)
                {
                    if (item.AmountLeft > 0)
                    {
                        var siItem = new StorageInBillItem
                        {
                            Id = OEAEnvironment.NewLocalId(),
                            Product = item.Product,
                            Amount = item.AmountLeft,
                            UnitPrice = item.RawPrice
                        };
                        children.Add(siItem);
                    }
                }
            }

            this.OnPropertyChanged(StorageInItemListProperty);
            this.OnPropertyChanged(View_SupplierNameProperty);
        }

        public static readonly Property<PurchaseOrderList> PurchaseOrderDataSourceProperty = P<OrderStorageInBill>.RegisterReadOnly(e => e.PurchaseOrderDataSource, e => (e as OrderStorageInBill).GetPurchaseOrderDataSource(), null);
        public PurchaseOrderList PurchaseOrderDataSource
        {
            get { return this.GetProperty(PurchaseOrderDataSourceProperty); }
        }
        private PurchaseOrderList GetPurchaseOrderDataSource()
        {
            //自定义数据源：选择商品订单时，只显示状态为未完成的订单。
            return RF.Concreate<PurchaseOrderRepository>().GetByStatus(OrderStorageInStatus.Waiting);
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
    public class OrderStorageInBillList : StorageInBillList
    {
        protected void QueryBy(int orderId)
        {
            this.QueryDb(q => q.Constrain(OrderStorageInBill.PurchaseOrderRefProperty).Equal(orderId));
        }
    }

    public class OrderStorageInBillRepository : EntityRepository
    {
        protected OrderStorageInBillRepository() { }

        public OrderStorageInBillList GetByOrderId(int orderId)
        {
            return this.FetchListCast<OrderStorageInBillList>(orderId);
        }
    }

    internal class OrderStorageInBillConfig : EntityConfig<OrderStorageInBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("采购入库单").HasDelegate(StorageInBill.CodeProperty);

            View.ColumnsCountShowInDetail = 2;

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(AddOrderStorageInBill),
                //typeof(DeleteStorageInBill),
                typeof(ShowBill),
                WPFCommandNames.Refresh
                );

            using (View.OrderProperties())
            {
                View.Property(StorageInBill.CodeProperty).HasLabel("商品入库编号").ShowIn(ShowInWhere.All);
                View.Property(StorageInBill.TotalMoneyProperty).HasLabel("总金额").ShowIn(ShowInWhere.ListDetail).Readonly();

                View.Property(OrderStorageInBill.PurchaseOrderRefProperty).HasLabel("商品订单").ShowIn(ShowInWhere.ListDetail)
                    .UseLookupDataSource(OrderStorageInBill.PurchaseOrderDataSourceProperty);
                View.Property(OrderStorageInBill.StorageRefProperty).HasLabel("收入仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(OrderStorageInBill.View_SupplierNameProperty).HasLabel("供应商").ShowIn(ShowInWhere.ListDetail);

                View.Property(StorageInBill.DateProperty).HasLabel("入库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageInBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}