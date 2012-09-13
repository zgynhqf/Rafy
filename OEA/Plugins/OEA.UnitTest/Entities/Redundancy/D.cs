using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.ORM;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;

namespace OEA.Library._Test
{
    [RootEntity, Serializable]
    public class D : UnitTestEntity
    {
        public static readonly RefProperty<C> CRefProperty =
            P<D>.RegisterRef(e => e.C, ReferenceType.Normal);
        public int CId
        {
            get { return this.GetRefId(CRefProperty); }
            set { this.SetRefId(CRefProperty, value); }
        }
        public C C
        {
            get { return this.GetRefEntity(CRefProperty); }
            set { this.SetRefEntity(CRefProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<D>.RegisterRedundancy(
            e => e.AName, new RedundantPath(CRefProperty, C.BRefProperty, B.ARefProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }
    }

    [Serializable]
    public class DList : EntityList { }

    public class DRepository : EntityRepository
    {
        protected DRepository() { }
    }

    internal class DConfig : EntityConfig<D>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("D").HasDelegate(D.ANameProperty);

            using (View.OrderProperties())
            {
                View.Property(D.ANameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}