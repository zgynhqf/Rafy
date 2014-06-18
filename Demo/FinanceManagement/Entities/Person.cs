using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace FM
{
    [RootEntity, Serializable]
    public partial class Person : FMEntity
    {
        #region 构造函数

        public Person() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Person(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> NameProperty = P<Person>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<Person>.Register(e => e.IsDefault);
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }
    }

    [Serializable]
    public partial class PersonList : FMEntityList { }

    public partial class PersonRepository : FMEntityRepository
    {
        protected PersonRepository() { }
    }

    internal class PersonConfig : FMEntityConfig<Person>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache();
        }
    }

    internal class PersonWPFViewConfig : WPFViewConfig<Person>
    {
        protected override void ConfigView()
        {
            View.DomainName("相关人").HasDelegate(Person.NameProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(Person.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(Person.IsDefaultProperty).HasLabel("默认").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}