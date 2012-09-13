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
    public class E : UnitTestEntity
    {
        public static readonly RefProperty<D> DRefProperty =
            P<E>.RegisterRef(e => e.D, ReferenceType.Normal);
        public int? DId
        {
            get { return this.GetRefNullableId(DRefProperty); }
            set { this.SetRefNullableId(DRefProperty, value); }
        }
        public D D
        {
            get { return this.GetRefEntity(DRefProperty); }
            set { this.SetRefEntity(DRefProperty, value); }
        }

        public static readonly Property<string> ANameFromDCBAProperty = P<E>.RegisterRedundancy(e => e.ANameFromDCBA,
            new RedundantPath(DRefProperty, D.CRefProperty, C.BRefProperty, B.ARefProperty, A.NameProperty));
        public string ANameFromDCBA
        {
            get { return this.GetProperty(ANameFromDCBAProperty); }
        }

        public static readonly RefProperty<C> CRefProperty =
            P<E>.RegisterRef(e => e.C, ReferenceType.Normal);
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

        public static readonly Property<string> ANameFromCBAProperty = P<E>.RegisterRedundancy(e => e.ANameFromCBA,
            new RedundantPath(CRefProperty, C.BRefProperty, B.ARefProperty, A.NameProperty));
        public string ANameFromCBA
        {
            get { return this.GetProperty(ANameFromCBAProperty); }
        }
    }

    [Serializable]
    public class EList : EntityList { }

    public class ERepository : EntityRepository
    {
        protected ERepository() { }
    }

    internal class EConfig : EntityConfig<E>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("E").HasDelegate(E.ANameFromDCBAProperty);

            using (View.OrderProperties())
            {
                View.Property(E.ANameFromDCBAProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}