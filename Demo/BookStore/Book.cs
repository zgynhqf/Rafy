using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Library.Validation;
using hxy;

namespace Demo
{
    [Serializable]
    [RootEntity]
    public class Book : DemoEntity
    {
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

        public static readonly Property<string> SubContentProperty = P<Book>.Register(e => e.SubContent);
        public string SubContent
        {
            get { return this.GetProperty(SubContentProperty); }
            set { this.SetProperty(SubContentProperty, value); }
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

        protected override void AddValidations()
        {
            base.AddValidations();

            //示例属性验证。
            this.ValidationRules.AddRule(Book.AuthorProperty, CommonRules.StringRequired);
            this.ValidationRules.AddRule(Book.AuthorProperty, CommonRules.StringMaxLength, new { MaxLength = 3 });
            this.ValidationRules.AddRule(Book.BookCategoryRefProperty, CommonRules.Required);
            this.ValidationRules.AddRule(Book.AmountProperty, CommonRules.IntegerMinValue, new
            {
                MinValue = 5
            });
            this.ValidationRules.AddRule(Book.AmountProperty, CommonRules.IntegerMaxValue, new
            {
                MaxValue = 50
            });
            this.ValidationRules.AddRule(Book.PublisherProperty, CommonRules.RegexMatch, new
            {
                Regex = TextFormatter.ReAllChinese,
                RegexLabel = "全中文"
            });
            this.ValidationRules.AddRule(Book.NameProperty, (e, args) =>
            {
                var value = e.GetProperty(args.Property) as string;
                if (string.IsNullOrEmpty(value))
                {
                    args.BrokenDescription = "书籍的名称不能为空。";
                }
                else if (!value.Contains("《") || !value.Contains("》"))
                {
                    args.BrokenDescription = "书籍的名称需要带上书名号：《》";
                }
            });
            //this.ValidationRules.AddRule(Book.AmountProperty, (e, args) =>
            //{
            //    var value = (e as Book).Amount;
            //    if (value < 0)
            //    {
            //        args.BrokenDescription = "数量不能是负数。";
            //    }
            //});
        }
    }

    [Serializable]
    public class BookList : DemoEntityList
    {
        protected override void OnGetAll()
        {
            //聚合 SQL 示例
            //为了降低数据库的查询次数，这里使用了聚合加载。
            AggregateSQL.Instance.LoadEntities<Book>(this, p => p.LoadChildren(b => b.ChapterList));
        }

        private void QueryBy(BookQueryCriteria criteria)
        {
            //自定义查询示例。
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
                Book.SubContentProperty,
                Book.BookCategoryRefProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasTitle(Book.NameProperty).HasLabel("书籍");

            View.UseWebCommands("CountLocalBookCommand", "CountServerBookCommand");

            View.Property(Book.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            View.Property(Book.AuthorProperty).HasLabel("作者").ShowIn(ShowInWhere.All);
            View.Property(Book.AmountProperty).HasLabel("剩余数量").ShowIn(ShowInWhere.All);
            View.Property(Book.PublisherProperty).HasLabel("出版社").ShowIn(ShowInWhere.All);
            View.Property(Book.SubContentProperty).HasLabel("简要").ShowIn(ShowInWhere.Detail).UseEditor(WPFEditorNames.Memo);
            View.Property(Book.BookCategoryRefProperty).HasLabel("所属类别").ShowIn(ShowInWhere.All);
        }
    }
}