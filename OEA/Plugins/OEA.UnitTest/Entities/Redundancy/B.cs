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
    public class B : UnitTestEntity
    {
        public static readonly RefProperty<A> ARefProperty =
            P<B>.RegisterRef(e => e.A, ReferenceType.Normal);
        public int AId
        {
            get { return this.GetRefId(ARefProperty); }
            set { this.SetRefId(ARefProperty, value); }
        }
        public A A
        {
            get { return this.GetRefEntity(ARefProperty); }
            set { this.SetRefEntity(ARefProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<B>.RegisterRedundancy(
            e => e.AName, new RedundantPath(ARefProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }

        public static readonly Property<string> NameProperty = P<B>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class BList : EntityList { }

    public class BRepository : EntityRepository
    {
        protected BRepository() { }
    }

    internal class BConfig : EntityConfig<B>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("B").HasDelegate(B.IdProperty);

            using (View.OrderProperties())
            {
                View.Property(B.IdProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}