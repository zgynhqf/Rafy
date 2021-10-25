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
    [ChildEntity, Serializable]
    public partial class StorageMoveItem : ProductRefItem
    {
        public static readonly IRefIdProperty StorageMoveIdProperty =
            P<StorageMoveItem>.RegisterRefId(e => e.StorageMoveId, ReferenceType.Parent);
        public int StorageMoveId
        {
            get { return (int)this.GetRefId(StorageMoveIdProperty); }
            set { this.SetRefId(StorageMoveIdProperty, value); }
        }
        public static readonly RefEntityProperty<StorageMove> StorageMoveProperty =
            P<StorageMoveItem>.RegisterRef(e => e.StorageMove, StorageMoveIdProperty);
        public StorageMove StorageMove
        {
            get { return this.GetRefEntity(StorageMoveProperty); }
            set { this.SetRefEntity(StorageMoveProperty, value); }
        }
    }

    [Serializable]
    public partial class StorageMoveItemList : ProductRefItemList { }

    public partial class StorageMoveItemRepository : JXCEntityRepository
    {
        protected StorageMoveItemRepository() { }
    }

    internal class StorageMoveItemConfig : EntityConfig<StorageMoveItem>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}