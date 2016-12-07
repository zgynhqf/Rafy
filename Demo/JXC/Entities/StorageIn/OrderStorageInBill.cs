using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy;

using Rafy.ManagedProperty;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public partial class OrderStorageInBill : StorageInBill
    {
        #region 构造函数

        public OrderStorageInBill() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OrderStorageInBill(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty OrderIdProperty =
            P<OrderStorageInBill>.RegisterRefId(e => e.OrderId, ReferenceType.Normal);
        public int OrderId
        {
            get { return (int)this.GetRefId(OrderIdProperty); }
            set { this.SetRefId(OrderIdProperty, value); }
        }
        public static readonly RefEntityProperty<PurchaseOrder> OrderProperty =
            P<OrderStorageInBill>.RegisterRef(e => e.Order, new RegisterRefArgs
            {
                RefIdProperty = OrderIdProperty,
                PropertyChangedCallBack = (o, e) => (o as OrderStorageInBill).OnOrderChanged(e),
            });
        public PurchaseOrder Order
        {
            get { return this.GetRefEntity(OrderProperty); }
            set { this.SetRefEntity(OrderProperty, value); }
        }

        protected virtual void OnOrderChanged(ManagedPropertyChangedEventArgs e)
        {
            var value = e.NewValue as PurchaseOrder;

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
                            Id = RafyEnvironment.NewLocalId(),
                            Product = item.Product,
                            Amount = item.AmountLeft,
                            UnitPrice = item.RawPrice
                        };
                        children.Add(siItem);
                    }
                }
            }

            this.NotifyPropertyChanged(StorageInItemListProperty);
            this.NotifyPropertyChanged(View_SupplierNameProperty);
        }

        public static readonly Property<PurchaseOrderList> PurchaseOrderDataSourceProperty = P<OrderStorageInBill>.RegisterReadOnly(e => e.PurchaseOrderDataSource, e => (e as OrderStorageInBill).GetPurchaseOrderDataSource());
        public PurchaseOrderList PurchaseOrderDataSource
        {
            get { return this.GetProperty(PurchaseOrderDataSourceProperty); }
        }
        private PurchaseOrderList GetPurchaseOrderDataSource()
        {
            //自定义数据源：选择商品订单时，只显示状态为未完成的订单。
            return RF.ResolveInstance<PurchaseOrderRepository>().GetByStatus(OrderStorageInStatus.Waiting);
        }

        public static readonly Property<string> View_SupplierNameProperty = P<OrderStorageInBill>.RegisterReadOnly(e => e.View_SupplierName, e => (e as OrderStorageInBill).GetView_SupplierName());
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
    public partial class OrderStorageInBillList : StorageInBillList
    {
    }

    public partial class OrderStorageInBillRepository : StorageInBillRepository
    {
        protected OrderStorageInBillRepository() { }

        public OrderStorageInBillList GetByOrderId(int orderId)
        {
            return this.GetBy(new CommonQueryCriteria
            {
                new PropertyMatch(OrderStorageInBill.OrderIdProperty, orderId),
            });
        }
    }
}