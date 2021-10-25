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
    [RootEntity, Serializable]
    public partial class D : UnitTestEntity
    {
        public static readonly IRefIdProperty CIdProperty =
            P<D>.RegisterRefId(e => e.CId, ReferenceType.Normal);
        public int CId
        {
            get { return this.GetRefId(CIdProperty); }
            set { this.SetRefId(CIdProperty, value); }
        }
        public static readonly RefEntityProperty<C> CProperty =
            P<D>.RegisterRef(e => e.C, CIdProperty);
        public C C
        {
            get { return this.GetRefEntity(CProperty); }
            set { this.SetRefEntity(CProperty, value); }
        }

        public static readonly Property<string> ANameProperty = P<D>.RegisterRedundancy(
            e => e.AName, new RedundantPath(CProperty, C.BProperty, B.AProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }
    }

    [Serializable]
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
        }
    }
}