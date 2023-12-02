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
        public static readonly IRefIdProperty BIdProperty =
            P<C>.RegisterRefId(e => e.BId, ReferenceType.Normal);
        public int? BId
        {
            get { return this.GetRefNullableId(BIdProperty); }
            set { this.SetRefNullableId(BIdProperty, value); }
        }
        public static readonly RefEntityProperty<B> BProperty =
            P<C>.RegisterRef(e => e.B, BIdProperty);
        public B B
        {
            get { return this.GetRefEntity(BProperty); }
            set { this.SetRefEntity(BProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<C>.RegisterRedundancy(
            e => e.AName, new RedundantPath(BProperty, B.AProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }

        public static readonly Property<string> ANameRefOfBProperty = P<C>.RegisterRedundancy(e => e.ANameRefOfB,
            new RedundantPath(BProperty, B.ANameRefProperty));
        public string ANameRefOfB
        {
            get { return this.GetProperty(ANameRefOfBProperty); }
        }

        public static readonly Property<int> AIdProperty = P<C>.RegisterRedundancy(e => e.AId,
            new RedundantPath(BProperty, B.AProperty, A.IdProperty));
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
        }
    }
}