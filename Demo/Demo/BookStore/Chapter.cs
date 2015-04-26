using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Demo
{
    [ChildEntity, Serializable]
    public partial class Chapter : DemoEntity
    {
        #region 构造函数

        public Chapter() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Chapter(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty BookIdProperty =
            P<Chapter>.RegisterRefId(e => e.BookId, ReferenceType.Parent);
        public int BookId
        {
            get { return (int)this.GetRefId(BookIdProperty); }
            set { this.SetRefId(BookIdProperty, value); }
        }
        public static readonly RefEntityProperty<Book> BookProperty =
            P<Chapter>.RegisterRef(e => e.Book, BookIdProperty);
        public Book Book
        {
            get { return this.GetRefEntity(BookProperty); }
            set { this.SetRefEntity(BookProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Chapter>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public partial class ChapterList : DemoEntityList { }

    public partial class ChapterRepository : DemoEntityRepository
    {
        protected ChapterRepository() { }
    }

    internal class ChapterConfig : DemoEntityConfig<Chapter>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                Chapter.BookIdProperty,
                Chapter.NameProperty
                );
        }
    }

    internal class ChapterWPFViewConfig : DemoEntityWPFViewConfig<Chapter>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.UseDefaultCommands();

            View.HasDelegate(Chapter.NameProperty).DomainName("章节");

            View.Property(Chapter.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}