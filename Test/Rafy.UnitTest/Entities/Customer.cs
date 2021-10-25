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
using System.Data;
using Rafy.Data;

namespace UT
{
    /// <summary>
    /// 客户
    /// </summary>
    [RootEntity, Serializable]
    public partial class Customer : UnitTest2Entity
    {
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

        public static readonly Property<decimal> DecimalProperty1Property = P<Customer>.Register(e => e.DecimalProperty1);
        public decimal DecimalProperty1
        {
            get { return this.GetProperty(DecimalProperty1Property); }
            set { this.SetProperty(DecimalProperty1Property, value); }
        }

        public static readonly Property<decimal> DecimalProperty2Property = P<Customer>.Register(e => e.DecimalProperty2);
        public decimal DecimalProperty2
        {
            get { return this.GetProperty(DecimalProperty2Property); }
            set { this.SetProperty(DecimalProperty2Property, value); }
        }

        public static readonly Property<decimal> DecimalProperty3Property = P<Customer>.Register(e => e.DecimalProperty3);
        public decimal DecimalProperty3
        {
            get { return this.GetProperty(DecimalProperty3Property); }
            set { this.SetProperty(DecimalProperty3Property, value); }
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

        [RepositoryQuery]
        public virtual LiteDataTable GetAllInTable()
        {
            var sql = @"select * from customer ";
            return (this.DataQueryer as RdbDataQueryer).QueryTable(sql);
        }

        #endregion
    }

    internal class CustomerConfig : UnitTest2EntityConfig<Customer>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.Property(Customer.DecimalProperty2Property).MapColumn().HasLength("18,4");
            Meta.Property(Customer.DecimalProperty3Property).MapColumn().HasDbType(DbType.Double);
        }
    }
}