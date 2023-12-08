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
        public static readonly Property<int?> DIdProperty = P<E>.Register(e => e.DId);
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

        public static readonly Property<int?> CIdProperty = P<E>.Register(e => e.CId);
        public int? CId
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

        public static readonly Property<int?> AIdProperty = P<E>.Register(e => e.AId);
        public int? AId
        {
            get { return this.GetProperty(AIdProperty); }
            set { this.SetProperty(AIdProperty, value); }
        }
        public static readonly RefEntityProperty<A> AProperty =
            P<E>.RegisterRef(e => e.A, AIdProperty);
        public A A
        {
            get { return this.GetRefEntity(AProperty); }
            set { this.SetRefEntity(AProperty, value); }
        }

        public static readonly Property<string> ANameFromDCBAProperty = P<E>.Register(e => e.ANameFromDCBA);
        public string ANameFromDCBA
        {
            get { return this.GetProperty(ANameFromDCBAProperty); }
        }

        public static readonly Property<string> ANameFromCBAProperty = P<E>.Register(e => e.ANameFromCBA);
        public string ANameFromCBA
        {
            get { return this.GetProperty(ANameFromCBAProperty); }
        }

        public static readonly Property<AType> Join_DCBA_TypeProperty = P<E>.Register(e => e.Join_DCBA_Type);
        public AType Join_DCBA_Type
        {
            get { return this.GetProperty(Join_DCBA_TypeProperty); }
            set { this.SetProperty(Join_DCBA_TypeProperty, value); }
        }

        public static readonly Property<string> Join_DCBA_NameProperty = P<E>.Register(e => e.Join_DCBA_Name);
        public string Join_DCBA_Name
        {
            get { return this.GetProperty(Join_DCBA_NameProperty); }
            set { this.SetProperty(Join_DCBA_NameProperty, value); }
        }

        public static readonly Property<string> Join_CBA_NameProperty = P<E>.Register(e => e.Join_CBA_Name);
        public string Join_CBA_Name
        {
            get { return this.GetProperty(Join_CBA_NameProperty); }
            set { this.SetProperty(Join_CBA_NameProperty, value); }
        }

        public static readonly Property<string> Join_A_NameProperty = P<E>.Register(e => e.Join_A_Name);
        public string Join_A_Name
        {
            get { return this.GetProperty(Join_A_NameProperty); }
            set { this.SetProperty(Join_A_NameProperty, value); }
        }

        public static readonly Property<string> Join_D_ANameProperty = P<E>.Register(e => e.Join_D_AName);
        public string Join_D_AName
        {
            get { return this.GetProperty(Join_D_ANameProperty); }
            set { this.SetProperty(Join_D_ANameProperty, value); }
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

            MapRefValue(E.Join_DCBA_TypeProperty, e => e.D.C.B.A.Type);
            MapRefValue(E.Join_DCBA_NameProperty, e => e.D.C.B.A.Name);
            MapRefValue(E.Join_CBA_NameProperty, e => e.C.B.A.Name);
            MapRefValue(E.Join_A_NameProperty, e => e.A.Name);
            MapRefValue(E.Join_D_ANameProperty, e => e.D.AName);
        }
    }
}