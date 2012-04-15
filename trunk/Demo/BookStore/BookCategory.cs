using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace Demo
{
    [RootEntity, Serializable]
    public class BookCategory : DemoEntity
    {
        public static readonly Property<string> NameProperty = P<BookCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class BookCategoryList : DemoEntityList { }

    public class BookCategoryRepository : EntityRepository
    {
        protected BookCategoryRepository() { }
    }

    internal class BookCategoryConfig : EntityConfig<BookCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().HasColumns(
                BookCategory.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("书籍类别").HasDelegate(BookCategory.NameProperty);

            View.UseWPFCommands("Demo.WPF.Commands.DemoCommand");

            View.Property(BookCategory.TreeCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.List).HasOrderNo(-1);
            View.Property(BookCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.ListDropDown);
        }
    }
}