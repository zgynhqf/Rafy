using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace JXC
{
    /// <summary>
    /// 库存货品
    /// </summary>
    [ChildEntity, Serializable]
    public partial class StorageProduct : ProductRefItem
    {
        public static readonly IRefIdProperty StorageIdProperty =
            P<StorageProduct>.RegisterRefId(e => e.StorageId, ReferenceType.Parent);
        public int StorageId
        {
            get { return (int)this.GetRefId(StorageIdProperty); }
            set { this.SetRefId(StorageIdProperty, value); }
        }
        public static readonly RefEntityProperty<Storage> StorageProperty =
            P<StorageProduct>.RegisterRef(e => e.Storage, StorageIdProperty);
        public Storage Storage
        {
            get { return this.GetRefEntity(StorageProperty); }
            set { this.SetRefEntity(StorageProperty, value); }
        }
    }

    [Serializable]
    public partial class StorageProductList : ProductRefItemList { }

    public partial class StorageProductRepository : JXCEntityRepository
    {
        protected StorageProductRepository() { }
    }

    internal class StorageProductConfig : EntityConfig<StorageProduct>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}