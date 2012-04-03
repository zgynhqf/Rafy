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
        #region 支持树型操作

        public static readonly Property<string> TreeCodeProperty = P<BookCategory>.Register(e => e.TreeCode);
        [Column]
        public override string TreeCode
        {
            get { return GetProperty(TreeCodeProperty); }
            set { SetProperty(TreeCodeProperty, value); }
        }

        public static readonly Property<int?> TreePIdProperty = P<BookCategory>.Register(e => e.TreePId);
        [Column]
        public override int? TreePId
        {
            get { return this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }

        public override bool SupportTree { get { return true; } }

        #endregion

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
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                BookCategory.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("书籍类别").HasTitle(BookCategory.NameProperty);

            View.UseWPFCommands("Demo.WPF.Commands.DemoCommand");

            View.Property(BookCategory.TreeCodeProperty).ShowIn(ShowInWhere.List).HasLabel("编码");
            View.Property(BookCategory.NameProperty).ShowIn(ShowInWhere.List).HasLabel("名称");
        }
    }
}