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
    public class StorageOutBillItem : JXCEntity
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

        public static readonly RefProperty<Product> ProductRefProperty =
            P<StorageOutBillItem>.RegisterRef(e => e.Product, ReferenceType.Normal);
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

        public static readonly Property<int> AmountProperty = P<StorageOutBillItem>.Register(e => e.Amount, new PropertyMetadata<int>
        {
            PropertyChangedCallBack = (o, e) => (o as StorageOutBillItem).OnAmountChanged(e)
        });
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }
        protected virtual void OnAmountChanged(ManagedPropertyChangedEventArgs<int> e)
        {
            this.RaiseRoutedEvent(PriceChangedEvent, e);
        }

        public static readonly EntityRoutedEvent PriceChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        #region 视图属性

        public static readonly Property<string> View_ProductNameProperty = P<StorageOutBillItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as StorageOutBillItem).GetView_ProductName(), null);
        public string View_ProductName
        {
            get { return this.GetProperty(View_ProductNameProperty); }
        }
        private string GetView_ProductName()
        {
            return this.Product.MingCheng;
        }

        public static readonly Property<string> View_ProductCategoryNameProperty = P<StorageOutBillItem>.RegisterReadOnly(e => e.View_ProductCategoryName, e => (e as StorageOutBillItem).GetView_ProductCategoryName(), null);
        public string View_ProductCategoryName
        {
            get { return this.GetProperty(View_ProductCategoryNameProperty); }
        }
        private string GetView_ProductCategoryName()
        {
            return this.Product.ProductCategory.Name;
        }

        public static readonly Property<string> View_SpecificationProperty = P<StorageOutBillItem>.RegisterReadOnly(e => e.View_Specification, e => (e as StorageOutBillItem).GetView_Specification(), null);
        public string View_Specification
        {
            get { return this.GetProperty(View_SpecificationProperty); }
        }
        private string GetView_Specification()
        {
            return this.Product.GuiGe;
        }

        #endregion
    }

    [Serializable]
    public class StorageOutBillItemList : JXCEntityList
    {
        public static readonly EntityRoutedEvent ListChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            this.RaiseRoutedEvent(ListChangedEvent, e);
        }
    }

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