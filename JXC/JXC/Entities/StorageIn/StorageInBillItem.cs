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
    public class StorageInBillItem : JXCEntity
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

        public static readonly RefProperty<Product> ProductRefProperty =
            P<StorageInBillItem>.RegisterRef(e => e.Product, ReferenceType.Normal);
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

        public static readonly Property<int> AmountProperty = P<StorageInBillItem>.Register(e => e.Amount);
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<double> UnitPriceProperty = P<StorageInBillItem>.Register(e => e.UnitPrice);
        public double UnitPrice
        {
            get { return this.GetProperty(UnitPriceProperty); }
            set { this.SetProperty(UnitPriceProperty, value); }
        }

        #region 视图属性

        public static readonly Property<string> View_ProductNameProperty = P<StorageInBillItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as StorageInBillItem).GetView_ProductName(), null);
        public string View_ProductName
        {
            get { return this.GetProperty(View_ProductNameProperty); }
        }
        private string GetView_ProductName()
        {
            return this.Product.MingCheng;
        }

        public static readonly Property<string> View_ProductCategoryNameProperty = P<StorageInBillItem>.RegisterReadOnly(e => e.View_ProductCategoryName, e => (e as StorageInBillItem).GetView_ProductCategoryName(), null);
        public string View_ProductCategoryName
        {
            get { return this.GetProperty(View_ProductCategoryNameProperty); }
        }
        private string GetView_ProductCategoryName()
        {
            return this.Product.ProductCategory.Name;
        }

        public static readonly Property<string> View_SpecificationProperty = P<StorageInBillItem>.RegisterReadOnly(e => e.View_Specification, e => (e as StorageInBillItem).GetView_Specification(), null);
        public string View_Specification
        {
            get { return this.GetProperty(View_SpecificationProperty); }
        }
        private string GetView_Specification()
        {
            return this.Product.GuiGe;
        }

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
    public class StorageInBillItemList : JXCEntityList
    {
        public static readonly EntityRoutedEvent ListChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            this.RaiseRoutedEvent(ListChangedEvent, e);
        }
    }

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
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(StorageInBillItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageInBillItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.UnitPriceProperty).HasLabel("单价").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.AmountProperty).HasLabel("入库数量*").ShowIn(ShowInWhere.List);
                View.Property(StorageInBillItem.View_TotalPriceProperty).HasLabel("总价").ShowIn(ShowInWhere.List);
            }
        }
    }
}