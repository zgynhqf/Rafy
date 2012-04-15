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
    [Serializable]
    [RootEntity]
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

            View.HasLabel("书籍类别").HasTitle(BookCategory.NameProperty);

            View.UseWPFCommands("Demo.WPF.Commands.DemoCommand");

            View.Property(BookCategory.TreeCodeProperty).ShowIn(ShowInWhere.List).HasLabel("编码").HasOrderNo(-1);
            View.Property(BookCategory.NameProperty).ShowIn(ShowInWhere.List | ShowInWhere.DropDown).HasLabel("名称");
        }
    }
}