using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public class OtherStorageOutBill : StorageOutBill
    {
        public static readonly RefProperty<ClientInfo> CustomerRefProperty =
            P<OtherStorageOutBill>.RegisterRef(e => e.Customer, ReferenceType.Normal);
        public int CustomerId
        {
            get { return this.GetRefId(CustomerRefProperty); }
            set { this.SetRefId(CustomerRefProperty, value); }
        }
        public ClientInfo Customer
        {
            get { return this.GetRefEntity(CustomerRefProperty); }
            set { this.SetRefEntity(CustomerRefProperty, value); }
        }
    }

    [Serializable]
    public class OtherStorageOutBillList : StorageOutBillList { }

    public class OtherStorageOutBillRepository : StorageOutBillRepository
    {
        protected OtherStorageOutBillRepository() { }
    }

    internal class OtherStorageOutBillConfig : EntityConfig<OtherStorageOutBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("其它出库单").HasDelegate(OtherStorageOutBill.CodeProperty);

            View.ColumnsCountShowInDetail = 2;

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                "JXC.Commands.AddOtherStorageOutBill",
                "JXC.Commands.ShowBill",
                WPFCommandNames.Refresh
                );

            using (View.OrderProperties())
            {
                View.Property(StorageOutBill.CodeProperty).HasLabel("出库单编号").ShowIn(ShowInWhere.All);
                View.Property(StorageOutBill.DateProperty).HasLabel("出库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(OtherStorageOutBill.CustomerRefProperty).HasLabel("收货单位").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageOutBill.TotalAmountProperty).HasLabel("总数量").ShowIn(ShowInWhere.ListDetail).Readonly();
                View.Property(StorageOutBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail).ShowMemoInDetail();
            }
        }
    }
}