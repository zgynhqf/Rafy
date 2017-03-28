/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130318 15:12
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130318 15:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using Rafy.Data;
using Rafy.Domain.ORM.Query;

namespace UT
{
    [RootEntity, Serializable]
    public partial class Book : UnitTestEntity
    {
        #region 构造函数

        public Book() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Book(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty BookCategoryIdProperty =
            P<Book>.RegisterRefId(e => e.BookCategoryId, ReferenceType.Normal);
        public int? BookCategoryId
        {
            get { return this.GetRefNullableId(BookCategoryIdProperty); }
            set { this.SetRefNullableId(BookCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<BookCategory> BookCategoryProperty =
            P<Book>.RegisterRef(e => e.BookCategory, BookCategoryIdProperty);
        /// <summary>
        /// 所属类别，可以为空。
        /// </summary>
        public BookCategory BookCategory
        {
            get { return this.GetRefEntity(BookCategoryProperty); }
            set { this.SetRefEntity(BookCategoryProperty, value); }
        }

        public static readonly IRefIdProperty BookLocIdProperty =
            P<Book>.RegisterRefId(e => e.BookLocId, ReferenceType.Normal);
        public int? BookLocId
        {
            get { return this.GetRefNullableId(BookLocIdProperty); }
            set { this.SetRefNullableId(BookLocIdProperty, value); }
        }
        public static readonly RefEntityProperty<BookLoc> BookLocProperty =
            P<Book>.RegisterRef(e => e.BookLoc, BookLocIdProperty);
        /// <summary>
        /// 书籍所在的货架
        /// </summary>
        public BookLoc BookLoc
        {
            get { return this.GetRefEntity(BookLocProperty); }
            set { this.SetRefEntity(BookLocProperty, value); }
        }

        #endregion

        #region 子属性

        public static readonly ListProperty<ChapterList> ChapterListProperty = P<Book>.RegisterList(e => e.ChapterList);
        public ChapterList ChapterList
        {
            get { return this.GetLazyList(ChapterListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<Book>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Book>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly LOBProperty<string> ContentProperty = P<Book>.RegisterLOB(e => e.Content);
        /// <summary>
        /// 书的内容。
        /// 使用 LOB 属性实现。
        /// </summary>
        public string Content
        {
            get { return this.GetLOBProperty(ContentProperty); }
            set { this.SetLOBProperty(ContentProperty, value); }
        }

        public static readonly Property<string> AuthorProperty = P<Book>.Register(e => e.Author);
        public string Author
        {
            get { return this.GetProperty(AuthorProperty); }
            set { this.SetProperty(AuthorProperty, value); }
        }

        public static readonly Property<double?> PriceProperty = P<Book>.Register(e => e.Price);
        public double? Price
        {
            get { return this.GetProperty(PriceProperty); }
            set { this.SetProperty(PriceProperty, value); }
        }

        public static readonly Property<string> PublisherProperty = P<Book>.Register(e => e.Publisher);
        public string Publisher
        {
            get { return this.GetProperty(PublisherProperty); }
            set { this.SetProperty(PublisherProperty, value); }
        }

        public static readonly Property<byte[]> BytesProperty = P<Book>.Register(e => e.Bytes);
        public byte[] Bytes
        {
            get { return this.GetProperty(BytesProperty); }
            set { this.SetProperty(BytesProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public partial class BookList : UnitTestEntityList { }

    public partial class BookRepository : UnitTestEntityRepository
    {
        protected BookRepository() { }

        [RepositoryQuery]
        public virtual BookList GetWithEager2()
        {
            var args = new EntityQueryArgs
            {
                Query = QueryFactory.Instance.Query(this)
            };

            args.EagerLoad(Book.ChapterListProperty);
            args.EagerLoad(Chapter.SectionListProperty);
            args.EagerLoad(Section.SectionOwnerProperty);

            return (BookList)this.QueryData(args);
        }

        [RepositoryQuery]
        public virtual BookList LinqGetByBookNameInArray(string[] names)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(c => names.Contains(c.Name));
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList GetByComplicateIn(object[] idList)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Id > 0 && idList.Contains(e.Id));
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGetByBookNameInList(List<string> names)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(c => names.Contains(c.Name));
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual LiteDataTable GetLOB(bool withLOB, bool hasTablePrefix)
        {
            ConditionalSql sql = null;
            if (!withLOB)
            {
                if (hasTablePrefix)
                {
                    sql = @"select b.{*} from book b";
                }
                else
                {
                    sql = @"select {*} from book";
                }
            }
            else
            {
                sql = @"select * from book";
            }
            return (this.DataQueryer as RdbDataQueryer).QueryTable(sql);
        }

        [RepositoryQuery]
        public virtual BookList GetBy(BookContainesNameCriteria criteria)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(b => b.Name.Contains(criteria.Name));
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGet_BracketOrAndOr(string code1, string code2, string code3, string name1, string name2, string name3)
        {
            var query = this.CreateLinqQuery();

            query = query.Where(e =>
                (e.Code == code1 || e.Code == code2 || e.Code == code3) &&
                (e.Name == name1 || e.Name == name2 || e.Name == name3)
                );

            return (BookList)this.QueryData(query);
        }

        //[RepositoryQuery]
        //public virtual BookList LinqGet_WithPropertyQuery(string code1, string code2, string code3, string name1, string name2, string name3)
        //{
        //    var query = this.CreatePropertyQuery();

        //    query.AddConstrainEqualIf(Book.CodeProperty, "c3");

        //    var linqQuery = this.CreateLinqQuery();

        //    linqQuery = linqQuery.Where(e =>
        //        (e.Code == code1 || e.Code == code2 || e.Code == code3) &&
        //        (e.Name == name1 || e.Name == name2 || e.Name == name3)
        //        );

        //    query.CombineLinq(linqQuery);

        //    return (BookList)this.QueryList(query);
        //}

        //[RepositoryQuery]
        //public virtual BookList Get_NameEqualsCode()
        //{
        //    var q = this.CreatePropertyQuery();
        //    q.AddConstrain(Book.NameProperty).Equal(Book.CodeProperty);
        //    return (BookList)this.QueryList(q);
        //}

        [RepositoryQuery]
        public virtual BookList Get_NameEqualsCode2()
        {
            var table = f.Table(this);
            var q = f.Query(
                from :table,
                where: table.Column(Book.NameProperty).Equal(table.Column(Book.CodeProperty))
            );
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGet_NameEqualsCode()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Name == e.Code);
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGet_BCIdEqualsRefBCId()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.BookCategoryId != null ||
                e.BookCategoryId == e.BookCategory.Id && e.Code == e.BookCategory.Code);
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGet_RefBCEqualsRefBC()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.BookLoc.Code == e.BookCategory.Code && e.BookCategory.Code == e.BookCategory.Name);
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGetByNullable(double? price)
        {
            var q = this.CreateLinqQuery();
            if (price.HasValue) q = q.Where(e => e.Price == price.Value);
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查找包含任意章节的书籍
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList GetIfChildrenExists()
        {
            var bookTable = f.Table(this);
            var chapterTable = f.Table<Chapter>();
            var q = f.Query(
                from: bookTable,
                where: f.Exists(f.Query(
                    from: chapterTable,
                    where: chapterTable.Column(Chapter.BookIdProperty).Equal(bookTable.IdColumn)
                ))
            );
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查询包含 chapterName 章节的所有书籍。
        /// </summary>
        /// <param name="chapterName"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList GetIfChildrenExists(string chapterName)
        {
            var book = f.Table(this);
            var chapter = f.Table<Chapter>();
            var q = f.Query(
                from: book,
                where: f.Exists(f.Query(
                    from: chapter,
                    where: f.And(
                        chapter.Column(Chapter.BookIdProperty).Equal(book.IdColumn),
                        chapter.Column(Chapter.NameProperty).Equal(chapterName)
                    )
                ))
            );
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查询所有章节名都是 chapterName 的所有书籍。
        /// </summary>
        /// <param name="chapterName"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList GetIfChildrenAll(string chapterName)
        {
            var book = f.Table(this);
            var chapter = f.Table<Chapter>();
            var q = f.Query(
                from: book,
                where: f.Not(f.Exists(f.Query(
                    from: chapter,
                    where: f.And(
                        f.Constraint(chapter.Column(Chapter.BookIdProperty), book.IdColumn),
                        f.Constraint(chapter.Column(Chapter.NameProperty), PropertyOperator.NotEqual, chapterName)
                    )
                )))
            );
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查找包含任意章节的书籍
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList LinqGetIfChildrenExists()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.ChapterList.Any());
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查询包含 chapterName 章节的所有书籍。
        /// </summary>
        /// <param name="chapterName"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList LinqGetIfChildrenExists(string chapterName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(book => book.ChapterList.Concrete().Any(c => c.Name == chapterName));
            q = q.OrderBy(b => b.Name);
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查找包含任意节名称为 sectionName 的书籍
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList LinqGetIfChildrenExistsSectionName(string sectionName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(book => book.ChapterList.Concrete().Any(c => c.SectionList.Cast<Section>().Any(s => s.Name.Contains(sectionName))));
            q = q.OrderBy(b => b.Name);
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查找包含任意节名称为 sectionName 且同时拥有者为 sectionOwner 的书籍。
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList LinqGetIfChildrenExistsSectionAndOwner(string category, string sectionName, string sectionOwner)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(book => book.BookCategory.Name == category && book.ChapterList.Concrete().Any(
                c => c.SectionList.Cast<Section>().Any(s => s.Name.Contains(sectionName) && s.SectionOwner.Name == sectionOwner)
                ));
            q = q.OrderBy(b => b.Name);
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查询所有章节名都是 chapterName 的所有书籍。
        /// </summary>
        /// <param name="chapterName"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList LinqGetIfChildrenAll(string chapterName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.ChapterList.Cast<Chapter>().All(c => c.Name == chapterName));
            q = q.OrderBy(e => e.Name);
            return (BookList)this.QueryData(q);
        }

        /// <summary>
        /// 查找包含任意章节的书籍
        /// </summary>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual BookList LinqGetIfChildren_Complicated(PagingInfo pi = null)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(book => book.Name != "1" && book.BookCategory.Name == "category" && book.ChapterList.Concrete().Any(c => c.Name == "1.2"));
            q = q.Where(b => b.ChapterList.Concrete().Any(c => c.Name == "chapterNeed"
                && c.SectionList.Cast<Section>().All(s =>
                    s.Name.Contains("need")
                    && s.SectionOwner.Name == "huqf")
                ));
            q = q.OrderBy(b => b.Name);
            return (BookList)this.QueryData(q, pi);
        }

        [RepositoryQuery]
        public virtual BookList LinqGetIfChildren_All_Any()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(b => b.ChapterList.Concrete().All(c =>
                c.SectionList.Cast<Section>().Any(s => s.SectionOwnerId != null)
                ));
            q = q.OrderBy(b => b.Name);
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList LinqGetIfChildren_All_All()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(b => b.ChapterList.Concrete().All(c =>
                c.SectionList.Cast<Section>().All(s => s.SectionOwnerId != null)
                ));
            q = q.OrderBy(b => b.Name);
            return (BookList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual BookList GetBookListByIds(IEnumerable<int> ids )
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var query = f.Query(
                from: table,
                where: table.Column(Entity.IdProperty).In(ids)
            );
            return (BookList)this.QueryData(query);
        }

        //不再支持没有参数类型的模糊查找。
        //public BookList LinqGetByNullableFetchBy(double? price)
        //{
        //    return this.FetchList(price);
        //}
        //private EntityList FetchBy(double? price)
        //{
        //    var q = this.CreateLinqQuery();
        //    q = q.Where(e => e.Price == price);
        //    return this.QueryList(q);
        //}

        //[RepositoryQuery]
        //public virtual BookList GetWithEager()
        //{
        //    return (BookList)this.QueryList(q =>
        //    {
        //        q.EagerLoad(Book.ChapterListProperty);
        //        q.EagerLoad(Chapter.SectionListProperty);
        //        q.EagerLoad(Section.SectionOwnerProperty);
        //    });
        //}
    }

    [DataProviderFor(typeof(BookRepository))]
    public partial class BookRepositoryDataProvider : UnitTestEntityRepositoryDataProvider
    {
        public bool UpdateCurrent;

        protected override void Submit(SubmitArgs e)
        {
            if (UpdateCurrent && e.Action == SubmitAction.ChildrenOnly)
            {
                e.UpdateCurrent();
            }

            base.Submit(e);
        }
    }

    internal class BookConfig : UnitTestEntityConfig<Book>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(Book.CodeProperty, new NotDuplicateRule());

            //书名和作者名不能同时一致。
            rules.AddRule(new NotDuplicateRule
            {
                Properties = { Book.AuthorProperty, Book.NameProperty }
            });
        }

        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.Property(Book.ContentProperty).MapColumn().HasLength("MAX");
            Meta.Property(Book.BytesProperty).MapColumn().HasDataType(System.Data.DbType.Binary);
        }
    }

    [Serializable]
    public partial class BookContainesNameCriteria
    {
        public string Name { get; set; }
    }
}