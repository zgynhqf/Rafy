using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Demo
{
    [RootEntity, Serializable]
    public partial class BookCategory : DemoEntity
    {
        #region 构造函数

        public BookCategory() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected BookCategory(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> NameProperty = P<BookCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public partial class BookCategoryList : DemoEntityList { }

    public partial class BookCategoryRepository : DemoEntityRepository
    {
        protected BookCategoryRepository() { }
    }

    internal class BookCategoryConfig : DemoEntityConfig<BookCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().MapProperties(
                BookCategory.NameProperty
                );
        }
    }

    internal class BookCategoryWPFViewConfig : DemoEntityWPFViewConfig<BookCategory>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("书籍类别").HasDelegate(BookCategory.NameProperty);

            View.UseDefaultCommands();

            View.Property(BookCategory.TreeIndexProperty).HasLabel("编码").ShowIn(ShowInWhere.ListDetail).HasOrderNo(-1);
            View.Property(BookCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }

    internal class BookCategoryWebViewConfig : DemoEntityWebViewConfig<BookCategory>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("书籍类别").HasDelegate(BookCategory.NameProperty);

            View.UseDefaultCommands().UseCommands("Demo.WPF.Commands.DemoCommand");

            View.Property(BookCategory.TreeIndexProperty).HasLabel("编码").ShowIn(ShowInWhere.ListDetail).HasOrderNo(-1);
            View.Property(BookCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}