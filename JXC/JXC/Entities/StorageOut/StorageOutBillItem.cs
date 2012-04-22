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

namespace JXC
{
    [ChildEntity, Serializable]
    public class StorageOutBillItem : ProductRefItem
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

        protected override void OnAmountChanged(ManagedPropertyChangedEventArgs<int> e)
        {
            base.OnAmountChanged(e);

            this.RaiseRoutedEvent(PriceChangedEvent, e);
        }

        public static readonly EntityRoutedEvent PriceChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);
    }

    [Serializable]
    public class StorageOutBillItemList : ProductRefItemList { }

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

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                "JXC.Commands.AddStorageOutItem",
                "JXC.Commands.BarcodeSelectProduct",
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(StorageOutBillItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageOutBillItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageOutBillItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageOutBillItem.AmountProperty).HasLabel("出库数量*").ShowIn(ShowInWhere.List);
            }
        }
    }
}