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
    public partial class A : UnitTestEntity
    {
        #region 构造函数

        public A() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected A(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> NameProperty = P<A>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public partial class AList : UnitTestEntityList { }

    public partial class ARepository : UnitTestEntityRepository
    {
        protected ARepository() { }
    }

    internal class AConfig : UnitTestEntityConfig<A>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}