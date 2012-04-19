/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(ClientTimeSpanCriteria))]
    public class PurchaseOrder : JXCEntity
    {
        public static readonly RefProperty<ClientInfo> SupplierRefProperty =
            P<PurchaseOrder>.RegisterRef(e => e.Supplier, ReferenceType.Normal);
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

        public static readonly ListProperty<PurchaseOrderItemList> PurchaseOrderItemListProperty = P<PurchaseOrder>.RegisterList(e => e.PurchaseOrderItemList);
        public PurchaseOrderItemList PurchaseOrderItemList
        {
            get { return this.GetLazyList(PurchaseOrderItemListProperty); }
        }

        public static readonly Property<string> CodeProperty = P<PurchaseOrder>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<PurchaseOrder>.Register(e => e.Date);
        public DateTime Date
        {
            get { return this.GetProperty(DateProperty); }
            set { this.SetProperty(DateProperty, value); }
        }

        public static readonly Property<DateTime> PlanStorageInDateProperty = P<PurchaseOrder>.Register(e => e.PlanStorageInDate);
        public DateTime PlanStorageInDate
        {
            get { return this.GetProperty(PlanStorageInDateProperty); }
            set { this.SetProperty(PlanStorageInDateProperty, value); }
        }

        public static readonly Property<double> TotalMoneyProperty = P<PurchaseOrder>.Register(e => e.TotalMoney);
        public double TotalMoney
        {
            get { return this.GetProperty(TotalMoneyProperty); }
            set { this.SetProperty(TotalMoneyProperty, value); }
        }

        public static readonly Property<bool> StorageInDirectlyProperty = P<PurchaseOrder>.Register(e => e.StorageInDirectly, new PropertyMetadata<bool>
        {
            PropertyChangedCallBack = (o, e) => (o as PurchaseOrder).OnStorageInDirectlyChanged(e)
        });
        public bool StorageInDirectly
        {
            get { return this.GetProperty(StorageInDirectlyProperty); }
            set { this.SetProperty(StorageInDirectlyProperty, value); }
        }
        protected virtual void OnStorageInDirectlyChanged(ManagedPropertyChangedEventArgs<bool> e)
        {
            if (e.NewValue) { this.StorageInStatus = OrderStorageInStatus.Completed; }
        }

        public static readonly Property<string> CommentProperty = P<PurchaseOrder>.Register(e => e.Comment);
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        public static readonly Property<int> TotalAmountLeftProperty = P<PurchaseOrder>.RegisterReadOnly(e => e.TotalAmountLeft, e => (e as PurchaseOrder).GetTotalAmountLeft(), null);
        public int TotalAmountLeft
        {
            get { return this.GetProperty(TotalAmountLeftProperty); }
        }
        private int GetTotalAmountLeft()
        {
            return this.PurchaseOrderItemList.Cast<PurchaseOrderItem>().Sum(e => e.AmountLeft);
        }

        public static readonly Property<OrderStorageInStatus> StorageInStatusProperty = P<PurchaseOrder>.Register(e => e.StorageInStatus, OrderStorageInStatus.Waiting);
        public OrderStorageInStatus StorageInStatus
        {
            get { return this.GetProperty(StorageInStatusProperty); }
            set { this.SetProperty(StorageInStatusProperty, value); }
        }

        protected override void AddValidations()
        {
            base.AddValidations();

            var rules = this.ValidationRules;
            rules.AddRule(CodeProperty, CommonRules.StringRequired);
            rules.AddRule((e, args) =>
            {
                var po = e as PurchaseOrder;
                if (po.PurchaseOrderItemList.Count == 0)
                {
                    args.BrokenDescription = "订单至少需要一个订单项。";
                }
                else
                {
                    foreach (PurchaseOrderItem item in po.PurchaseOrderItemList)
                    {
                        if (item.View_TotalPrice <= 0)
                        {
                            args.BrokenDescription = "商品项金额应该是正数。";
                            return;
                        }
                    }
                }
            });
        }

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == PurchaseOrderItem.PriceChangedEvent || e.Event == PurchaseOrderItemList.ListChangedEvent)
            {
                this.TotalMoney = this.PurchaseOrderItemList.Sum(poi => (poi as PurchaseOrderItem).View_TotalPrice);
            }
        }
    }

    public enum OrderStorageInStatus
    {
        [Label("等待入库")]
        Waiting,
        [Label("完全入库")]
        Completed,
    }

    [Serializable]
    public class PurchaseOrderList : JXCEntityList
    {
        protected void QueryBy(ClientTimeSpanCriteria criteria)
        {
            this.QueryDb(q =>
            {
                q.Constrain(PurchaseOrder.DateProperty).GreaterEqual(criteria.From)
                    .And().Constrain(PurchaseOrder.DateProperty).LessEqual(criteria.To);

                if (criteria.ClientInfoId.HasValue)
                {
                    q.And().Constrain(PurchaseOrder.SupplierRefProperty).Equal(criteria.ClientInfoId.Value);
                }
            });
        }
    }

    public class PurchaseOrderRepository : EntityRepository
    {
        protected PurchaseOrderRepository() { }
    }

    internal class PurchaseOrderConfig : EntityConfig<PurchaseOrder>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("采购订单").HasDelegate(PurchaseOrder.CodeProperty);

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                "JXC.Commands.AddPurchaseOrder",
                "JXC.Commands.DeletePurchaseOrder",
                "JXC.Commands.ShowBill",
                WPFCommandNames.Refresh,
                "JXC.Commands.CompletePurchaseOrder"
                );

            using (View.OrderProperties())
            {
                View.Property(PurchaseOrder.CodeProperty).HasLabel("订单编号").ShowIn(ShowInWhere.All);
                View.Property(PurchaseOrder.DateProperty).HasLabel("订单日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(PurchaseOrder.SupplierRefProperty).HasLabel("供应商").ShowIn(ShowInWhere.ListDetail);
                View.Property(PurchaseOrder.PlanStorageInDateProperty).HasLabel("计划到货日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(PurchaseOrder.TotalMoneyProperty).HasLabel("总金额").ShowIn(ShowInWhere.ListDetail).Readonly();
                View.Property(PurchaseOrder.StorageInDirectlyProperty).HasLabel("直接入库").ShowIn(ShowInWhere.Detail);
                View.Property(PurchaseOrder.StorageInStatusProperty).HasLabel("入库状态").ShowIn(ShowInWhere.List).Readonly();
                View.Property(PurchaseOrder.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}