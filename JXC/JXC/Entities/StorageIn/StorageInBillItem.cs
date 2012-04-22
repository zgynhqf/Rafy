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
    public class StorageInBillItem : ProductRefItem
    {
        public static readonly RefProperty<StorageInBill> StorageInBillRefProperty =
            P<StorageInBillItem>.RegisterRef(e => e.StorageInBill, ReferenceType.Parent);
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

        public static readonly Property<double> UnitPriceProperty = P<StorageInBillItem>.Register(e => e.UnitPrice);
        public double UnitPrice
        {
            get { return this.GetProperty(UnitPriceProperty); }
            set { this.SetProperty(UnitPriceProperty, value); }
        }

        #region 视图属性

        public static readonly Property<double> View_TotalPriceProperty =
            P<StorageInBillItem>.RegisterReadOnly(e => e.View_TotalPrice, e => (e as StorageInBillItem).GetView_TotalPrice(), null,
            AmountProperty, UnitPriceProperty
            );
        public double View_TotalPrice
        {
            get { return this.GetProperty(View_TotalPriceProperty); }
        }
        private double GetView_TotalPrice()
        {
            return this.Amount * this.UnitPrice;
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
    public class StorageInBillItemList : ProductRefItemList { }

    public class StorageInBillItemRepository : EntityRepository
    {
        protected StorageInBillItemRepository() { }
    }

    internal class StorageInBillItemConfig : EntityConfig<StorageInBillItem>
    {
        protected override void ConfigMeta()
        {
            //示例映射自定义表名
            Meta.MapTable("StorageInItem").MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("入库单项").HasDelegate(StorageInBillItem.View_ProductNameProperty);

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                "JXC.Commands.AddStorageInItem",
                "JXC.Commands.BarcodeSelectProduct",
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(StorageInBillItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageInBillItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.UnitPriceProperty).HasLabel("单价*").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.AmountProperty).HasLabel("入库数量*").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.View_TotalPriceProperty).HasLabel("总价").ShowIn(ShowInWhere.List);
            }
        }
    }
}