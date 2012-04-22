using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using JXC.Commands;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public class OtherStorageInBill : StorageInBill
    {
        public static readonly RefProperty<ClientInfo> SupplierRefProperty =
            P<OtherStorageInBill>.RegisterRef(e => e.Supplier, ReferenceType.Normal);
        public int SupplierId
        {
            get { return this.GetRefId(SupplierRefProperty); }
            set { this.SetRefId(SupplierRefProperty, value); }
        }
        public ClientInfo Supplier
        {
            get { return this.GetRefEntity(SupplierRefProperty); }
            set { this.SetRefEntity(SupplierRefProperty, value); }
        }
    }

    [Serializable]
    public class OtherStorageInBillList : StorageInBillList { }

    public class OtherStorageInBillRepository : OrderStorageInBillRepository
    {
        protected OtherStorageInBillRepository() { }
    }

    internal class OtherStorageInBillConfig : EntityConfig<OtherStorageInBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("其它入库单").HasDelegate(StorageInBill.CodeProperty);

            View.ColumnsCountShowInDetail = 2;

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(AddOtherStorageInBill),
                typeof(ShowBill),
                WPFCommandNames.Refresh
                );

            using (View.OrderProperties())
            {
                View.Property(StorageInBill.CodeProperty).HasLabel("商品入库编号").ShowIn(ShowInWhere.All);
                View.Property(StorageInBill.TotalMoneyProperty).HasLabel("金额").ShowIn(ShowInWhere.ListDetail);

                View.Property(OtherStorageInBill.SupplierRefProperty).HasLabel("发货单位").ShowIn(ShowInWhere.ListDetail);

                View.Property(StorageInBill.DateProperty).HasLabel("入库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageInBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}