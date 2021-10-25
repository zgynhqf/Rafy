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
using Rafy.ManagedProperty;
using System.ComponentModel;

using Rafy;

namespace JXC
{
    [ChildEntity, Serializable]
    public partial class PurchaseOrderItem : ProductRefItem
    {
        public static readonly IRefIdProperty PurchaseOrderIdProperty =
            P<PurchaseOrderItem>.RegisterRefId(e => e.PurchaseOrderId, ReferenceType.Parent);
        public int PurchaseOrderId
        {
            get { return (int)this.GetRefId(PurchaseOrderIdProperty); }
            set { this.SetRefId(PurchaseOrderIdProperty, value); }
        }
        public static readonly RefEntityProperty<PurchaseOrder> PurchaseOrderProperty =
            P<PurchaseOrderItem>.RegisterRef(e => e.PurchaseOrder, PurchaseOrderIdProperty);
        public PurchaseOrder PurchaseOrder
        {
            get { return this.GetRefEntity(PurchaseOrderProperty); }
            set { this.SetRefEntity(PurchaseOrderProperty, value); }
        }

        protected override void OnAmountChanged(ManagedPropertyChangedEventArgs e)
        {
            this.AmountLeft = (int)e.NewValue;
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

        protected override void OnPropertyChanged(ManagedPropertyChangedEventArgs e)
        {
            if (e.Property == View_TotalPriceProperty)
            {
                this.RaiseRoutedEvent(PriceChangedEvent, e);
            }

            base.OnPropertyChanged(e);
        }
    }

    [Serializable]
    public partial class PurchaseOrderItemList : ProductRefItemList { }

    public partial class PurchaseOrderItemRepository : JXCEntityRepository
    {
        protected PurchaseOrderItemRepository() { }
    }

    internal class PurchaseOrderItemConfig : EntityConfig<PurchaseOrderItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}