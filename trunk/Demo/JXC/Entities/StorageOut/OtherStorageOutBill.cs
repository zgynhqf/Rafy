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
    public partial class OtherStorageOutBill : StorageOutBill
    {
        #region 构造函数

        public OtherStorageOutBill() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OtherStorageOutBill(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty CustomerIdProperty =
            P<OtherStorageOutBill>.RegisterRefId(e => e.CustomerId, ReferenceType.Normal);
        public int CustomerId
        {
            get { return (int)this.GetRefId(CustomerIdProperty); }
            set { this.SetRefId(CustomerIdProperty, value); }
        }
        public static readonly RefEntityProperty<ClientInfo> CustomerProperty =
            P<OtherStorageOutBill>.RegisterRef(e => e.Customer, CustomerIdProperty);
        public ClientInfo Customer
        {
            get { return this.GetRefEntity(CustomerProperty); }
            set { this.SetRefEntity(CustomerProperty, value); }
        }
    }

    [Serializable]
    public partial class OtherStorageOutBillList : StorageOutBillList { }

    public partial class OtherStorageOutBillRepository : StorageOutBillRepository
    {
        protected OtherStorageOutBillRepository() { }
    }
}