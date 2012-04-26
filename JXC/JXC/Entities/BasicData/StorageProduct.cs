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

namespace JXC
{
    [ChildEntity, Serializable]
    public class StorageProduct : ProductRefItem
    {
        public static readonly RefProperty<Storage> StorageRefProperty =
            P<StorageProduct>.RegisterRef(e => e.Storage, ReferenceType.Parent);
        public int StorageId
        {
            get { return this.GetRefId(StorageRefProperty); }
            set { this.SetRefId(StorageRefProperty, value); }
        }
        public Storage Storage
        {
            get { return this.GetRefEntity(StorageRefProperty); }
            set { this.SetRefEntity(StorageRefProperty, value); }
        }
    }

    [Serializable]
    public class StorageProductList : ProductRefItemList { }

    public class StorageProductRepository : EntityRepository
    {
        protected StorageProductRepository() { }
    }

    internal class StorageProductConfig : EntityConfig<StorageProduct>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("库存货品").HasDelegate(StorageProduct.View_ProductNameProperty);

            using (View.OrderProperties())
            {
                View.Property(StorageProduct.View_ProductNameProperty).HasLabel("商品名称").ShowIn(ShowInWhere.All);
                View.Property(StorageProduct.View_ProductCategoryNameProperty).HasLabel("商品类别").ShowIn(ShowInWhere.List);
                View.Property(StorageProduct.View_SpecificationProperty).HasLabel("规格").ShowIn(ShowInWhere.List);
                View.Property(StorageProduct.AmountProperty).HasLabel("当前数量").ShowIn(ShowInWhere.List);
            }
        }
    }
}