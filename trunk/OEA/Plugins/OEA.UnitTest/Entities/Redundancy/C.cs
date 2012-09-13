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
    public class C : UnitTestEntity
    {
        public static readonly RefProperty<B> BRefProperty =
            P<C>.RegisterRef(e => e.B, ReferenceType.Normal);
        public int? BId
        {
            get { return this.GetRefNullableId(BRefProperty); }
            set { this.SetRefNullableId(BRefProperty, value); }
        }
        public B B
        {
            get { return this.GetRefEntity(BRefProperty); }
            set { this.SetRefEntity(BRefProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<C>.RegisterRedundancy(
            e => e.AName, new RedundantPath(BRefProperty, B.ARefProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }
    }

    [Serializable]
    public class CList : EntityList { }

    public class CRepository : EntityRepository
    {
        protected CRepository() { }
    }

    internal class CConfig : EntityConfig<C>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("C").HasDelegate(C.IdProperty);

            using (View.OrderProperties())
            {
                View.Property(C.IdProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}