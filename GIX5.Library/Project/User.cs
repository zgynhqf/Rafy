using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace GIX5.Library
{
    [Serializable]
    [RootEntity]
    [Label("用户")]
    public class User : GEntity
    {
        protected User() { }

        public static readonly Property<string> NameProperty = P<User>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class UserList : GEntityList
    {
        protected UserList() { }
    }

    public class UserRepository : EntityRepository
    {
        protected UserRepository() { }
    }

    internal class UserConfig : EntityConfig<User>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                User.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(User.NameProperty);

            View.Property(User.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}