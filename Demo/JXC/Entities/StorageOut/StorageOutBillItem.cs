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
    [ChildEntity]
    public partial class StorageOutBillItem : ProductRefItem
    {
        public static readonly IRefIdProperty StorageOutBillIdProperty =
            P<StorageOutBillItem>.RegisterRefId(e => e.StorageOutBillId, ReferenceType.Parent);
        public int StorageOutBillId
        {
            get { return (int)this.GetRefId(StorageOutBillIdProperty); }
            set { this.SetRefId(StorageOutBillIdProperty, value); }
        }
        public static readonly RefEntityProperty<StorageOutBill> StorageOutBillProperty =
            P<StorageOutBillItem>.RegisterRef(e => e.StorageOutBill, StorageOutBillIdProperty);
        public StorageOutBill StorageOutBill
        {
            get { return this.GetRefEntity(StorageOutBillProperty); }
            set { this.SetRefEntity(StorageOutBillProperty, value); }
        }

        protected override void OnAmountChanging(ManagedPropertyChangingEventArgs<int> e)
        {
            base.OnAmountChanging(e);

            //如果出库的数据大于当前库存，应该修改错误的值为当前库存。
            if (RafyPropertyDescriptor.IsOperating && e.Value > this.Product.StorageAmount)
            {
                e.CoercedValue = this.Product.StorageAmount;
            }
        }

        protected override void OnAmountChanged(ManagedPropertyChangedEventArgs e)
        {
            base.OnAmountChanged(e);

            this.RaiseRoutedEvent(PriceChangedEvent, e);
        }

        public static readonly Property<int> View_ProductStorageAmountProperty = P<StorageOutBillItem>.RegisterReadOnly(e => e.View_ProductStorageAmount, e => (e as StorageOutBillItem).GetView_ProductStorageAmount());
        public int View_ProductStorageAmount
        {
            get { return this.GetProperty(View_ProductStorageAmountProperty); }
        }
        private int GetView_ProductStorageAmount()
        {
            return this.Product.StorageAmount;
        }

        #region 路由事件

        public static readonly EntityRoutedEvent PriceChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);

        #endregion
    }

    public partial class StorageOutBillItemList : ProductRefItemList { }

    public partial class StorageOutBillItemRepository : JXCEntityRepository
    {
        protected StorageOutBillItemRepository() { }
    }

    internal class StorageOutBillItemConfig : EntityConfig<StorageOutBillItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}