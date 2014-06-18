using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    /// <summary>
    /// 客户
    /// </summary>
    [RootEntity, Serializable]
    public partial class Customer : UnitTest2Entity
    {
        #region 构造函数

        public Customer() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Customer(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Customer>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public partial class CustomerList : UnitTest2EntityList { }

    public partial class CustomerRepository : UnitTest2EntityRepository
    {
        protected CustomerRepository() { }

        #region 数据访问

        #endregion
    }

    internal class CustomerConfig : UnitTest2EntityConfig<Customer>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}