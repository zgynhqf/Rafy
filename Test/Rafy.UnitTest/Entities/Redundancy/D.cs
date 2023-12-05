using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using System.Security.Permissions;
using System.Runtime.Serialization;

namespace UT
{
    [RootEntity]
    public partial class D : UnitTestEntity
    {
        public static readonly Property<int> CIdProperty = P<D>.Register(e => e.CId);
        public int CId
        {
            get { return this.GetProperty(CIdProperty); }
            set { this.SetProperty(CIdProperty, value); }
        }
        public static readonly RefEntityProperty<C> CProperty =
            P<D>.RegisterRef(e => e.C, CIdProperty);
        public C C
        {
            get { return this.GetRefEntity(CProperty); }
            set { this.SetRefEntity(CProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<D>.Register(e => e.AName);
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }
    }

    public partial class DList : UnitTestEntityList { }

    public partial class DRepository : UnitTestEntityRepository
    {
        protected DRepository() { }
    }

    internal class DConfig : UnitTestEntityConfig<D>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
            MapRefValue(e => e.AName, e => e.C.B.A.Name, ReferenceValueDataMode.Redundancy);
        }
    }
}