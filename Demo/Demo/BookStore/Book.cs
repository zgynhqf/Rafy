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
using Rafy.Domain.Validation;
using Demo.WPF.Commands;
using Demo.WPF;
using Rafy.ManagedProperty;
using Rafy.Domain.ORM;

namespace Demo
{
    [RootEntity, Serializable]
    public partial class Book : DemoEntity
    {
        #region 构造函数

        public Book() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Book(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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

        public static readonly Property<DateTime> PublishTimeProperty = P<Book>.Register(e => e.PublishTime);
        public DateTime PublishTime
        {
            get { return this.GetProperty(PublishTimeProperty); }
            set { this.SetProperty(PublishTimeProperty, value); }
        }

        public static readonly Property<string> SubContentProperty = P<Book>.Register(e => e.SubContent);
        public string SubContent
        {
            get { return this.GetProperty(SubContentProperty); }
            set { this.SetProperty(SubContentProperty, value); }
        }

        public static readonly IRefIdProperty BookCategoryIdProperty =
            P<Book>.RegisterRefId(e => e.BookCategoryId, ReferenceType.Normal);
        public int BookCategoryId
        {
            get { return (int)this.GetRefId(BookCategoryIdProperty); }
            set { this.SetRefId(BookCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<BookCategory> BookCategoryProperty =
            P<Book>.RegisterRef(e => e.BookCategory, new RegisterRefArgs
            {
                RefIdProperty = BookCategoryIdProperty,
                PropertyChangingCallBack = (o, e) => (o as Book).OnBookCategoryChanging(e),
            });
        public BookCategory BookCategory
        {
            get { return this.GetRefEntity(BookCategoryProperty); }
            set { this.SetRefEntity(BookCategoryProperty, value); }
        }
        protected virtual void OnBookCategoryChanging(ManagedPropertyChangingEventArgs<Entity> e)
        {
            //业务逻辑示例：只能选择最末级的图书类别。
            var value = e.Value as BookCategory;
            if (value != null && value.TreeChildren.Count > 0)
            {
                e.Cancel = true;
            }
        }

        public static readonly ListProperty<ChapterList> ChapterListProperty = P<Book>.RegisterList(e => e.ChapterList);
        public ChapterList ChapterList
        {
            get { return this.GetLazyList(ChapterListProperty); }
        }
    }

    [Serializable]
    public partial class BookList : DemoEntityList
    {
    }

    public partial class BookRepository : DemoEntityRepository
    {
        protected BookRepository() { }

        /// <summary>
        /// 查询面板
        /// </summary>
        /// <param name="criteria"></param>
        protected EntityList FetchBy(BookQueryCriteria criteria)
        {
            //自定义查询示例。
            return this.QueryList(q =>
            {
                q.Constrain(Book.BookCategoryIdProperty).Equal(criteria.BookCategoryId)
                    .And().Constrain(Book.NameProperty).Contains(criteria.BookName);
            });

            //            //使用 SQL 的自定义查询示例。
            //            this.QueryDb(new SqlQueryDbArgs
            //            {
            //                FormatSql = @"
            //                Select * from Book
            //                where BookCategoryId = {0} and Name = '{1}'",
            //                Parameters = new object[] { criteria.BookCategoryId, criteria.BookName }
            //            });
        }
    }

    [DataProviderFor(typeof(BookRepository))]
    public partial class BookDataProvider : DemoEntityDataProvider
    {
        public override EntityList GetAll(PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            var list = new BookList();

            //聚合 SQL 示例
            //为了降低数据库的查询次数，这里使用了聚合加载。
            AggregateSQL.Instance.LoadEntities<Book>(list, p => p.LoadChildren(b => b.ChapterList));

            return list;
        }
    }

    internal class BookConfig : DemoEntityConfig<Book>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            //示例属性验证。
            rules.AddRule(Book.AuthorProperty, new RequiredRule());
            rules.AddRule(Book.AuthorProperty, new StringLengthRangeRule { Max = 3 });
            rules.AddRule(Book.AmountProperty, new NumberRangeRule { Min = 5, Max = 50 });
            rules.AddRule(Book.PublisherProperty, new RegexMatchRule
            {
                Regex = TextFormatter.ReAllChinese,
                RegexLabel = "全中文"
            });
            rules.AddRule(Book.NameProperty, new HandlerRule
            {
                Handler = (e, args) =>
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
                }
            });
            //rules.AddRule(AmountProperty, (e, args) =>
            //{
            //    var value = (e as Book).Amount;
            //    if (value < 0)
            //    {
            //        args.BrokenDescription = "数量不能是负数。";
            //    }
            //});
        }

        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                Book.NameProperty,
                Book.AuthorProperty,
                Book.AmountProperty,
                Book.PublishTimeProperty,
                Book.PublisherProperty,
                Book.SubContentProperty,
                Book.BookCategoryIdProperty
                );
        }
    }

    internal class BookWPFConfig : DemoEntityWPFViewConfig<Book>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("书籍").HasDelegate(Book.NameProperty);

            View.UseDetailPanel<BookForm>().HasDetailLabelSize(120);

            View.UseDefaultCommands().UseCommands(WPFCommandNames.Filter);

            View.Property(Book.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            View.Property(Book.AuthorProperty).HasLabel("作者").ShowIn(ShowInWhere.All);
            View.Property(Book.AmountProperty).HasLabel("剩余数量").ShowIn(ShowInWhere.All);
            View.Property(Book.PublisherProperty).HasLabel("出版社").ShowIn(ShowInWhere.All);
            View.Property(Book.PublishTimeProperty).HasLabel("出版时间").ShowIn(ShowInWhere.All);
            View.Property(Book.SubContentProperty).HasLabel("简要").ShowIn(ShowInWhere.Detail).UseEditor(WPFEditorNames.Memo);
            View.Property(Book.BookCategoryProperty).HasLabel("所属类别").ShowIn(ShowInWhere.All);
        }
    }

    internal class BookWebConfig : DemoEntityWebViewConfig<Book>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("书籍").HasDelegate(Book.NameProperty);

            //View.UseDetailPanel<BookForm>().HasDetailLabelSize(120);

            View.UseDefaultCommands().UseCommands("CountLocalBookCommand", "CountServerBookCommand");

            View.Property(Book.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            View.Property(Book.AuthorProperty).HasLabel("作者").ShowIn(ShowInWhere.All);
            View.Property(Book.AmountProperty).HasLabel("剩余数量").ShowIn(ShowInWhere.All);
            View.Property(Book.PublisherProperty).HasLabel("出版社").ShowIn(ShowInWhere.All);
            View.Property(Book.PublishTimeProperty).HasLabel("出版时间").ShowIn(ShowInWhere.All);
            View.Property(Book.SubContentProperty).HasLabel("简要").ShowIn(ShowInWhere.Detail);
            View.Property(Book.BookCategoryProperty).HasLabel("所属类别").ShowIn(ShowInWhere.All);
        }
    }
}