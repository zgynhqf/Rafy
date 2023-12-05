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
    public partial class E : UnitTestEntity
    {
        public static readonly Property<int?> DIdProperty =
            P<E>.Register(e => e.DId);
        public int? DId
        {
            get { return this.GetProperty(DIdProperty); }
            set { this.SetProperty(DIdProperty, value); }
        }
        public static readonly RefEntityProperty<D> DProperty =
            P<E>.RegisterRef(e => e.D, DIdProperty);
        public D D
        {
            get { return this.GetRefEntity(DProperty); }
            set { this.SetRefEntity(DProperty, value); }
        }

        public static readonly Property<string> ANameFromDCBAProperty = P<E>.Register(e => e.ANameFromDCBA);
        public string ANameFromDCBA
        {
            get { return this.GetProperty(ANameFromDCBAProperty); }
        }

        public static readonly Property<int> CIdProperty =
            P<E>.Register(e => e.CId);
        public int CId
        {
            get { return this.GetProperty(CIdProperty); }
            set { this.SetProperty(CIdProperty, value); }
        }
        public static readonly RefEntityProperty<C> CProperty =
            P<E>.RegisterRef(e => e.C, CIdProperty);
        public C C
        {
            get { return this.GetRefEntity(CProperty); }
            set { this.SetRefEntity(CProperty, value); }
        }

        public static readonly Property<string> ANameFromCBAProperty = P<E>.Register(e => e.ANameFromCBA);
        public string ANameFromCBA
        {
            get { return this.GetProperty(ANameFromCBAProperty); }
        }
    }

    public partial class EList : UnitTestEntityList { }

    public partial class ERepository : UnitTestEntityRepository
    {
        protected ERepository() { }
    }

    internal class EConfig : UnitTestEntityConfig<E>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
            MapRefValue(E.ANameFromDCBAProperty, e => e.D.C.B.A.Name, ReferenceValueDataMode.Redundancy);
            MapRefValue(E.ANameFromCBAProperty, e => e.C.B.A.Name, ReferenceValueDataMode.Redundancy);
        }
    }
}