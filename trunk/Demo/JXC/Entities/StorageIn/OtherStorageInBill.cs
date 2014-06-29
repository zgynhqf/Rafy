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
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public partial class OtherStorageInBill : StorageInBill
    {
        #region 构造函数

        public OtherStorageInBill() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OtherStorageInBill(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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

    [Serializable]
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