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
    [RootEntity]
    public class Book : DemoEntity
    {
        protected Book() { }

        public static readonly Property<string> NameProperty = P<Book>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> AuthorProperty = P<Book>.Register(e => e.Author);
        public string Author
        {
            get { return this.GetProperty(AuthorProperty); }
            set { this.SetProperty(AuthorProperty, value); }
        }

        public static readonly Property<int> AmountProperty = P<Book>.Register(e => e.Amount);
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<string> PublisherProperty = P<Book>.Register(e => e.Publisher);
        public string Publisher
        {
            get { return this.GetProperty(PublisherProperty); }
            set { this.SetProperty(PublisherProperty, value); }
        }

        public static readonly RefProperty<BookCategory> BookCategoryRefProperty =
            P<Book>.RegisterRef(e => e.BookCategory, ReferenceType.Normal);
        public int BookCategoryId
        {
            get { return this.GetRefId(BookCategoryRefProperty); }
            set { this.SetRefId(BookCategoryRefProperty, value); }
        }
        public BookCategory BookCategory
        {
            get { return this.GetRefEntity(BookCategoryRefProperty); }
            set { this.SetRefEntity(BookCategoryRefProperty, value); }
        }

        public static readonly Property<ChapterList> ChapterListProperty = P<Book>.Register(e => e.ChapterList);
        [Association]
        public ChapterList ChapterList
        {
            get { return this.GetLazyChildren(ChapterListProperty); }
        }
    }

    [Serializable]
    public class BookList : DemoEntityList
    {
        protected BookList() { }

        private void DataPortal_Fetch(BookQueryCriteria criteria)
        {
            this.QueryDb(q =>
            {
                q.Constrain(Book.BookCategoryRefProperty).Equal(criteria.BookCategoryId)
                    .And().Constrain(Book.NameProperty).Like(criteria.BookName);
            });
        }
    }

    public class BookRepository : EntityRepository
    {
        protected BookRepository() { }

        protected override EntityList GetListImplicitly(object parameter)
        {
            if (parameter is BookQueryCriteria)
            {
                return this.FetchList(parameter);
            }

            return base.GetListImplicitly(parameter);
        }
    }

    internal class BookConfig : EntityConfig<Book>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                Book.NameProperty,
                Book.AuthorProperty,
                Book.AmountProperty,
                Book.PublisherProperty,
                Book.BookCategoryRefProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(Book.NameProperty).HasLabel("书籍");

            View.UseWebCommands("CountLocalBookCommand", "CountServerBookCommand");

            //View.Property(Book.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}