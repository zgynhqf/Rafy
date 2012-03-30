using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace Demo
{
    [Serializable]
    [ChildEntity]
    public class Chapter : DemoEntity
    {
        public static readonly RefProperty<Book> BookRefProperty =
            P<Chapter>.RegisterRef(e => e.Book, ReferenceType.Parent);
        public int BookId
        {
            get { return this.GetRefId(BookRefProperty); }
            set { this.SetRefId(BookRefProperty, value); }
        }
        public Book Book
        {
            get { return this.GetRefEntity(BookRefProperty); }
            set { this.SetRefEntity(BookRefProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Chapter>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class ChapterList : DemoEntityList { }

    public class ChapterRepository : EntityRepository
    {
        protected ChapterRepository() { }
    }

    internal class ChapterConfig : EntityConfig<Chapter>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                Chapter.BookRefProperty,
                Chapter.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(Chapter.NameProperty).HasLabel("章节");
        }
    }
}