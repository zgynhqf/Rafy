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
    public class A : UnitTestEntity
    {
        public static readonly Property<string> NameProperty = P<A>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class AList : EntityList { }

    public class ARepository : EntityRepository
    {
        protected ARepository() { }
    }

    internal class AConfig : EntityConfig<A>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("A").HasDelegate(A.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(A.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}