using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;


namespace JXC
{
    [RootEntity]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public partial class OtherStorageInBill : StorageInBill
    {
        public static readonly IRefIdProperty SupplierIdProperty =
            P<OtherStorageInBill>.RegisterRefId(e => e.SupplierId, ReferenceType.Normal);
        public int SupplierId
        {
            get { return (int)this.GetRefId(SupplierIdProperty); }
            set { this.SetRefId(SupplierIdProperty, value); }
        }
        public static readonly RefEntityProperty<ClientInfo> SupplierProperty =
            P<OtherStorageInBill>.RegisterRef(e => e.Supplier, SupplierIdProperty);
        public ClientInfo Supplier
        {
            get { return this.GetRefEntity(SupplierProperty); }
            set { this.SetRefEntity(SupplierProperty, value); }
        }
    }

    public partial class OtherStorageInBillList : StorageInBillList { }

    public partial class OtherStorageInBillRepository : StorageInBillRepository
    {
        protected OtherStorageInBillRepository() { }

        public void Test()
        {
            //var list = new OtherStorageInBillList();
            //list.Insert
        }
    }
}