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
    public partial class B : UnitTestEntity
    {
        #region 引用属性

        public static readonly Property<string> ANameRefProperty = P<B>.Register(e => e.ANameRef);
        /// <summary>
        /// B 与 A 的关系，使用 Name 这个一般值属性来关联。
        /// </summary>
        public string ANameRef
        {
            get { return this.GetProperty<string>(ANameRefProperty); }
            set { this.SetProperty(ANameRefProperty, value); }
        }
        public static readonly RefEntityProperty<A> AProperty =
            P<B>.RegisterRef(e => e.A, ANameRefProperty, A.NameProperty);
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
            get { return this.GetProperty<string>(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> ANameProperty = P<B>.RegisterRedundancy(
            e => e.AName, new RedundantPath(AProperty, A.NameProperty));
        public string AName
        {
            get { return this.GetProperty<string>(ANameProperty); }
        }

        public static readonly Property<AType> ATypeProperty = P<B>.RegisterRedundancy(e => e.AType,
            new RedundantPath(AProperty, A.TypeProperty));
        public AType AType
        {
            get { return this.GetProperty<AType>(ATypeProperty); }
        }

        #endregion
    }

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
            //Meta.Property(B.ANameRefProperty).MapColumn().IsForeignKey();
        }
    }
}