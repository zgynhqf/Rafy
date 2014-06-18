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
    public partial class E : UnitTestEntity
    {
        #region 构造函数

        public E() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected E(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty DIdProperty =
            P<E>.RegisterRefId(e => e.DId, ReferenceType.Normal);
        public int? DId
        {
            get { return this.GetRefNullableId(DIdProperty); }
            set { this.SetRefNullableId(DIdProperty, value); }
        }
        public static readonly RefEntityProperty<D> DProperty =
            P<E>.RegisterRef(e => e.D, DIdProperty);
        public D D
        {
            get { return this.GetRefEntity(DProperty); }
            set { this.SetRefEntity(DProperty, value); }
        }

        public static readonly Property<string> ANameFromDCBAProperty = P<E>.RegisterRedundancy(e => e.ANameFromDCBA,
            new RedundantPath(DProperty, D.CProperty, C.BProperty, B.AProperty, A.NameProperty));
        public string ANameFromDCBA
        {
            get { return this.GetProperty(ANameFromDCBAProperty); }
        }

        public static readonly IRefIdProperty CIdProperty =
            P<E>.RegisterRefId(e => e.CId, ReferenceType.Normal);
        public int CId
        {
            get { return this.GetRefId(CIdProperty); }
            set { this.SetRefId(CIdProperty, value); }
        }
        public static readonly RefEntityProperty<C> CProperty =
            P<E>.RegisterRef(e => e.C, CIdProperty);
        public C C
        {
            get { return this.GetRefEntity(CProperty); }
            set { this.SetRefEntity(CProperty, value); }
        }

        public static readonly Property<string> ANameFromCBAProperty = P<E>.RegisterRedundancy(e => e.ANameFromCBA,
            new RedundantPath(CProperty, C.BProperty, B.AProperty, A.NameProperty));
        public string ANameFromCBA
        {
            get { return this.GetProperty(ANameFromCBAProperty); }
        }
    }

    [Serializable]
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
        }
    }
}