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
    public partial class C : UnitTestEntity
    {
        public static readonly Property<int?> BIdProperty =
            P<C>.Register(e => e.BId);
        public int? BId
        {
            get { return this.GetProperty(BIdProperty); }
            set { this.SetProperty(BIdProperty, value); }
        }
        public static readonly RefEntityProperty<B> BProperty =
            P<C>.RegisterRef(e => e.B, BIdProperty);
        public B B
        {
            get { return this.GetRefEntity(BProperty); }
            set { this.SetRefEntity(BProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<C>.Register(e => e.AName);
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }

        public static readonly Property<string> ANameRefOfBProperty = P<C>.Register(e => e.ANameRefOfB);
        public string ANameRefOfB
        {
            get { return this.GetProperty(ANameRefOfBProperty); }
        }

        public static readonly Property<int> AIdProperty = P<C>.Register(e => e.AId);
        public int AId
        {
            get { return this.GetProperty(AIdProperty); }
        }
    }

    public partial class CList : UnitTestEntityList { }

    public partial class CRepository : UnitTestEntityRepository
    {
        protected CRepository() { }
    }

    internal class CConfig : UnitTestEntityConfig<C>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            MapRefValue(C.ANameProperty, e => e.B.A.Name, ReferenceValueDataMode.Redundancy);
            MapRefValue(C.ANameRefOfBProperty, e => e.B.ANameRef, ReferenceValueDataMode.Redundancy);
            MapRefValue(C.AIdProperty, e => e.B.A.Id, ReferenceValueDataMode.Redundancy);
        }
    }
}