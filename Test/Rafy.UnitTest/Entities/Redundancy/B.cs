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
    public partial class B : UnitTestEntity
    {
        #region 构造函数

        public B() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected B(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty AIdProperty =
            P<B>.RegisterRefId(e => e.AId, ReferenceType.Normal);
        public int AId
        {
            get { return this.GetRefId(AIdProperty); }
            set { this.SetRefId(AIdProperty, value); }
        }
        public static readonly RefEntityProperty<A> AProperty =
            P<B>.RegisterRef(e => e.A, AIdProperty);
        public A A
        {
            get { return this.GetRefEntity(AProperty); }
            set { this.SetRefEntity(AProperty, value); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<B>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> ANameProperty = P<B>.RegisterRedundancy(
            e => e.AName, new RedundantPath(AProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty(ANameProperty); }
        }

        public static readonly Property<AType> ATypeProperty = P<B>.RegisterRedundancy(e => e.AType,
            new RedundantPath(AProperty, A.TypeProperty));
        public AType AType
        {
            get { return this.GetProperty(ATypeProperty); }
        }

        #endregion
    }

    [Serializable]
    public partial class BList : UnitTestEntityList { }

    public partial class BRepository : UnitTestEntityRepository
    {
        protected BRepository() { }
    }

    internal class BConfig : UnitTestEntityConfig<B>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}