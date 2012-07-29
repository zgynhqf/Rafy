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
    public class PurchaseOrderAttachement : FileAttachement
    {
        public static readonly RefProperty<PurchaseOrder> PurchaseOrderRefProperty =
            P<PurchaseOrderAttachement>.RegisterRef(e => e.PurchaseOrder, ReferenceType.Parent);
        public int PurchaseOrderId
        {
            get { return this.GetRefId(PurchaseOrderRefProperty); }
            set { this.SetRefId(PurchaseOrderRefProperty, value); }
        }
        public PurchaseOrder PurchaseOrder
        {
            get { return this.GetRefEntity(PurchaseOrderRefProperty); }
            set { this.SetRefEntity(PurchaseOrderRefProperty, value); }
        }
    }

    [Serializable]
    public class PurchaseOrderAttachementList : FileAttachementList { }

    public class PurchaseOrderAttachementRepository : FileAttachementRepository
    {
        protected PurchaseOrderAttachementRepository() { }
    }

    //internal class PurchaseOrderAttachementConfig : EntityConfig<PurchaseOrderAttachement>
    //{
    //    protected override void ConfigMeta()
    //    {
    //        Meta.MapTable().MapAllPropertiesToTable();
    //    }

    //    protected override void ConfigView()
    //    {
    //        View.DomainName("实体标签").HasDelegate(PurchaseOrderAttachement.NameProperty);

    //        using (View.OrderProperties())
    //        {
    //            View.Property(PurchaseOrderAttachement.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
    //        }
    //    }
    //}
}