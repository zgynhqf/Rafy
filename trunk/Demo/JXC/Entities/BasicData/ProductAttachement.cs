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
    public class ProductAttachement : FileAttachement
    {
        public static readonly RefProperty<Product> ProductRefProperty =
            P<ProductAttachement>.RegisterRef(e => e.Product, ReferenceType.Parent);
        public int ProductId
        {
            get { return this.GetRefId(ProductRefProperty); }
            set { this.SetRefId(ProductRefProperty, value); }
        }
        public Product Product
        {
            get { return this.GetRefEntity(ProductRefProperty); }
            set { this.SetRefEntity(ProductRefProperty, value); }
        }
    }

    [Serializable]
    public class ProductAttachementList : FileAttachementList { }

    public class ProductAttachementRepository : FileAttachementRepository
    {
        protected ProductAttachementRepository() { }
    }

    internal class ProductAttachementConfig : EntityConfig<ProductAttachement>
    {
        protected override void ConfigMeta()
        {
            Meta.EnableCache();
        }

        //    protected override void ConfigView()
        //    {
        //        View.DomainName("实体标签").HasDelegate(ProductAttachement.NameProperty);

        //        using (View.OrderProperties())
        //        {
        //            View.Property(ProductAttachement.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        //        }
        //    }
    }
}