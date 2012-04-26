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
using JXC.Commands;

namespace JXC
{
    [ChildEntity, Serializable]
    public class StorageMoveItem : ProductRefItem
    {
        public static readonly RefProperty<StorageMove> StorageMoveRefProperty =
            P<StorageMoveItem>.RegisterRef(e => e.StorageMove, ReferenceType.Parent);
        public int StorageMoveId
        {
            get { return this.GetRefId(StorageMoveRefProperty); }
            set { this.SetRefId(StorageMoveRefProperty, value); }
        }
        public StorageMove StorageMove
        {
            get { return this.GetRefEntity(StorageMoveRefProperty); }
            set { this.SetRefEntity(StorageMoveRefProperty, value); }
        }
    }

    [Serializable]
    public class StorageMoveItemList : ProductRefItemList { }

    public class StorageMoveItemRepository : EntityRepository
    {
        protected StorageMoveItemRepository() { }
    }

    internal class StorageMoveItemConfig : EntityConfig<StorageMoveItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("库存调拔项").HasDelegate(StorageMoveItem.View_ProductNameProperty);

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(SelectStorageProduct),
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(StorageMoveItem.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageMoveItem.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageMoveItem.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageMoveItem.AmountProperty).HasLabel("数量*").ShowIn(ShowInWhere.List);
            }
        }
    }
}