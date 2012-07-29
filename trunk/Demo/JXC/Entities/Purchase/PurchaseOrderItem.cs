using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using System.ComponentModel;
using JXC.Commands;

namespace JXC
{
    [ChildEntity, Serializable]
    public class PurchaseOrderItem : ProductRefItem
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

        protected override void OnAmountChanged(ManagedPropertyChangedEventArgs<int> e)
        {
            this.AmountLeft = e.NewValue;
        }

        /// <summary>
        /// 剩余数量
        /// </summary>
        public static readonly Property<int> AmountLeftProperty = P<PurchaseOrderItem>.Register(e => e.AmountLeft);
        public int AmountLeft
        {
            get { return this.GetProperty(AmountLeftProperty); }
            set { this.SetProperty(AmountLeftProperty, value); }
        }

        public static readonly Property<double> RawPriceProperty = P<PurchaseOrderItem>.Register(e => e.RawPrice);
        public double RawPrice
        {
            get { return this.GetProperty(RawPriceProperty); }
            set { this.SetProperty(RawPriceProperty, value); }
        }

        #region 视图属性

        public static readonly Property<double> View_TotalPriceProperty =
            P<PurchaseOrderItem>.RegisterReadOnly(e => e.View_TotalPrice, e => (e as PurchaseOrderItem).GetView_TotalPrice(), AmountProperty, RawPriceProperty);
        public double View_TotalPrice
        {
            get { return this.GetProperty(View_TotalPriceProperty); }
        }
        private double GetView_TotalPrice()
        {
            return this.Amount * this.RawPrice;
        }

        #endregion

        public static readonly EntityRoutedEvent PriceChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        protected override void OnPropertyChanged(IManagedPropertyChangedEventArgs e)
        {
            if (e.Property == View_TotalPriceProperty)
            {
                this.RaiseRoutedEvent(PriceChangedEvent, e as EventArgs);
            }

            base.OnPropertyChanged(e);
        }
    }

    [Serializable]
    public class PurchaseOrderItemList : ProductRefItemList { }

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

            if (IsWeb)
            {
                View.ClearWebCommands(false)
                    .UseWebCommands(
                    "Jxc.AddPurchaseOrderItem",
                    WebCommandNames.Delete
                    );
            }
            else
            {
                View.ClearWPFCommands(false)
                    .UseWPFCommands(
                    typeof(AddPurchaseOrderItem),
                    WPFCommandNames.Delete
                    );
            }

            using (View.OrderProperties())
            {
                View.Property(PurchaseOrderItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.AmountProperty).HasLabel("数量").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.RawPriceProperty).HasLabel("单价").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.View_TotalPriceProperty).HasLabel("总价").ShowIn(ShowInWhere.List);
                View.Property(PurchaseOrderItem.AmountLeftProperty).HasLabel("未入库数量").ShowIn(ShowInWhere.List).Readonly();
            }
        }
    }
}