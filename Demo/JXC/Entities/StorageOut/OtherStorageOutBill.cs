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
    public partial class OtherStorageOutBill : StorageOutBill
    {
        public static readonly Property<int> CustomerIdProperty =
            P<OtherStorageOutBill>.Register(e => e.CustomerId);
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

    public partial class OtherStorageOutBillList : StorageOutBillList { }

    public partial class OtherStorageOutBillRepository : StorageOutBillRepository
    {
        protected OtherStorageOutBillRepository() { }
    }
}