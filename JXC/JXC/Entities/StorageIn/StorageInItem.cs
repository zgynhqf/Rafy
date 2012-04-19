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

        public static readonly Property<double> UnitPriceProperty = P<StorageInItem>.Register(e => e.UnitPrice);
        public double UnitPrice
        {
            get { return this.GetProperty(UnitPriceProperty); }
            set { this.SetProperty(UnitPriceProperty, value); }
        }

        #region 视图属性

        public static readonly Property<string> View_ProductNameProperty = P<StorageInItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as StorageInItem).GetView_ProductName(), null);
        public string View_ProductName
        {
            get { return this.GetProperty(View_ProductNameProperty); }
        }
        private string GetView_ProductName()
        {
            return this.Product.MingCheng;
        }

        public static readonly Property<string> View_ProductCategoryNameProperty = P<StorageInItem>.RegisterReadOnly(e => e.View_ProductCategoryName, e => (e as StorageInItem).GetView_ProductCategoryName(), null);
        public string View_ProductCategoryName
        {
            get { return this.GetProperty(View_ProductCategoryNameProperty); }
        }
        private string GetView_ProductCategoryName()
        {
            return this.Product.ProductCategory.Name;
        }

        public static readonly Property<string> View_SpecificationProperty = P<StorageInItem>.RegisterReadOnly(e => e.View_Specification, e => (e as StorageInItem).GetView_Specification(), null);
        public string View_Specification
        {
            get { return this.GetProperty(View_SpecificationProperty); }
        }
        private string GetView_Specification()
        {
            return this.Product.GuiGe;
        }

        public static readonly Property<double> View_TotalPriceProperty =
            P<StorageInItem>.RegisterReadOnly(e => e.View_TotalPrice, e => (e as StorageInItem).GetView_TotalPrice(), null,
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
    public class StorageInItemList : JXCEntityList
    {
        public static readonly EntityRoutedEvent ListChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            this.RaiseRoutedEvent(ListChangedEvent, e);
        }
    }

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

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                "JXC.Commands.AddStorageInItem",
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(StorageInItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageInItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageInItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageInItem.UnitPriceProperty).HasLabel("单价").ShowIn(ShowInWhere.List).Readonly();
                View.Property(StorageInItem.AmountProperty).HasLabel("数量").ShowIn(ShowInWhere.List);
                View.Property(StorageInItem.View_TotalPriceProperty).HasLabel("总价").ShowIn(ShowInWhere.List);
            }
        }
    }
}