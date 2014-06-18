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
    public partial class StorageInBillItem : ProductRefItem
    {
        #region 构造函数

        public StorageInBillItem() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected StorageInBillItem(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty StorageInBillIdProperty =
            P<StorageInBillItem>.RegisterRefId(e => e.StorageInBillId, ReferenceType.Parent);
        public int StorageInBillId
        {
            get { return (int)this.GetRefId(StorageInBillIdProperty); }
            set { this.SetRefId(StorageInBillIdProperty, value); }
        }
        public static readonly RefEntityProperty<StorageInBill> StorageInBillProperty =
            P<StorageInBillItem>.RegisterRef(e => e.StorageInBill, StorageInBillIdProperty);
        public StorageInBill StorageInBill
        {
            get { return this.GetRefEntity(StorageInBillProperty); }
            set { this.SetRefEntity(StorageInBillProperty, value); }
        }

        public static readonly Property<double> UnitPriceProperty = P<StorageInBillItem>.Register(e => e.UnitPrice);
        public double UnitPrice
        {
            get { return this.GetProperty(UnitPriceProperty); }
            set { this.SetProperty(UnitPriceProperty, value); }
        }

        #region 视图属性

        public static readonly Property<double> View_TotalPriceProperty =
            P<StorageInBillItem>.RegisterReadOnly(e => e.View_TotalPrice, e => (e as StorageInBillItem).GetView_TotalPrice(), AmountProperty, UnitPriceProperty);
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
    public partial class StorageInBillItemList : ProductRefItemList { }

    public partial class StorageInBillItemRepository : JXCEntityRepository
    {
        protected StorageInBillItemRepository() { }
    }

    internal class StorageInBillItemConfig : EntityConfig<StorageInBillItem>
    {
        protected override void ConfigMeta()
        {
            //示例映射自定义表名
            Meta.MapTable("StorageInItem").MapAllProperties();
        }
    }
}