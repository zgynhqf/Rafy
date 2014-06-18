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
    [ChildEntity, Serializable]
    public partial class Chapter : UnitTestEntity
    {
        #region 构造函数

        public Chapter() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Chapter(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty BookIdProperty =
            P<Chapter>.RegisterRefId(e => e.BookId, ReferenceType.Parent);
        public int BookId
        {
            get { return this.GetRefId(BookIdProperty); }
            set { this.SetRefId(BookIdProperty, value); }
        }
        public static readonly RefEntityProperty<Book> BookProperty =
            P<Chapter>.RegisterRef(e => e.Book, BookIdProperty);
        public Book Book
        {
            get { return this.GetRefEntity(BookProperty); }
            set { this.SetRefEntity(BookProperty, value); }
        }

        #endregion

        #region 子属性

        public static readonly ListProperty<SectionList> SectionListProperty = P<Chapter>.RegisterList(e => e.SectionList);
        public SectionList SectionList
        {
            get { return this.GetLazyList(SectionListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Chapter>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public partial class ChapterList : UnitTestEntityList { }

    public partial class ChapterRepository : UnitTestEntityRepository
    {
        protected ChapterRepository() { }

        public int CountByBookName(string name)
        {
            return this.FetchCount(new CountByBookNameCriteria { Name = name });
        }
        protected EntityList FetchBy(CountByBookNameCriteria c)
        {
            return this.QueryList(q =>
            {
                q.JoinRef(Chapter.BookIdProperty);
                q.Constrain(Book.NameProperty).Equal(c.Name);
            });
        }

        public int CountByBookName2(string name)
        {
            return this.FetchCount(r => r.DA_CountByBookName2(name));
        }
        private EntityList DA_CountByBookName2(string name)
        {
            var source = f.Table(this);
            var bookSource = f.Table<Book>();
            var q = f.Query(
                from: f.Join(source, bookSource)
            );
            q.AddConstraintIf(Book.NameProperty, PropertyOperator.Equal, name);
            return this.QueryList(q);
        }

        public ChapterList LinqGetByBookName(string name)
        {
            return this.FetchList(r => r.DA_LinqCountByBookName(name));
        }
        public int LinqCountByBookName(string name)
        {
            return this.FetchCount(r => r.DA_LinqCountByBookName(name));
        }
        private EntityList DA_LinqCountByBookName(string name)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(c => c.Book.Name == name);
            return this.QueryList(q);
        }

        public int LinqCountByBookName_RealLinqCount(string name)
        {
            return this.FetchCount(new LinqCountByBookNameCriteria { Name = name });
        }
        protected EntityList FetchBy(LinqCountByBookNameCriteria criteria)
        {
            var q = this.CreateLinqQuery();

            q = q.Where(c => c.Book.Name == criteria.Name);

            var count = q.Count();
            var list = new ChapterList();
            list.SetTotalCount(count);

            return list;
        }

        public ChapterList LinqGetByBookNamePaging(string name, PagingInfo pagingInfo)
        {
            return this.FetchList(name, pagingInfo);
        }
        protected EntityList FetchBy(string name, PagingInfo pagingInfo)
        {
            var q = this.CreateLinqQuery();

            q = q.Where(c => c.Book.Name == name).OrderBy(c => c.Name);

            return this.QueryList(q, pagingInfo);
        }

        public ChapterList LinqGetByNameStringAction(StringAction stringAction, string name = null)
        {
            return this.FetchList(new LinqGetByNameContainsCriteria { Name = name, StringAction = stringAction });
        }
        protected EntityList FetchBy(LinqGetByNameContainsCriteria criteria)
        {
            var q = this.CreateLinqQuery();

            switch (criteria.StringAction)
            {
                case StringAction.Contains:
                    q = q.Where(c => c.Name.Contains(criteria.Name) && c.BookId > 0);
                    break;
                case StringAction.StartsWith:
                    q = q.Where(c => c.Name.StartsWith(criteria.Name) && c.BookId > 0);
                    break;
                case StringAction.EndsWith:
                    q = q.Where(c => c.Name.EndsWith(criteria.Name) && c.BookId > 0);
                    break;
                case StringAction.NotEmpty:
                    q = q.Where(c => !string.IsNullOrEmpty(c.Name) && c.BookId > 0);
                    break;
                case StringAction.LengthNotSupport:
                    q = q.Where(c => c.Name.Length > 0 && c.BookId > 0);
                    break;
                case StringAction.RefContains:
                    q = q.Where(c => c.Book.Name.Contains(criteria.Name) && c.BookId > 0);
                    break;
                default:
                    break;
            }

            return this.QueryList(q);
        }

        public ChapterList LinqGetByBookNameIn(string[] names)
        {
            return this.FetchList(new object[] { names });
        }
        protected EntityList FetchBy(string[] names)
        {
            var q = this.CreateLinqQuery();

            q = q.Where(c => names.Contains(c.Book.Name));

            return this.QueryList(q);
        }

        public LiteDataTable QueryChapterTable(int queryType, PagingInfo pi)
        {
            return this.FetchTable(r => r.DA_QueryChapterTable(queryType, pi));
        }
        private LiteDataTable DA_QueryChapterTable(int queryType, PagingInfo pi)
        {
            ConditionalSql sql = null;
            if (queryType == 0)
            {
                sql = @"
Select * from Chapter
    inner join Book on Chapter.BookId = Book.Id
where Book.Id > {0}
order by chapter.name desc
";
                sql.Parameters.Add(0);
            }
            else if (queryType == 1)
            {
                sql = @"
Select Chapter.*, Book.Name BookName
from Chapter
    inner join Book on Chapter.BookId = Book.Id
where Book.Id > {0}
order by chapter.name desc
";
                sql.Parameters.Add(0);
            }
            else
            {
                throw new NotSupportedException();
            }

            return this.QueryTable(sql, pi);
        }

        public LiteDataTable QueryChapterBySqlTree(string name, PagingInfo pi)
        {
            return this.FetchTable(r => r.DA_QueryChapterBySqlTree(name, pi));
        }
        private LiteDataTable DA_QueryChapterBySqlTree(string name, PagingInfo pi)
        {
            var f = QueryFactory.Instance;
            var t = f.Table<Chapter>();
            var q = f.Query(
                from: t,
                where: t.Column(Chapter.NameProperty).Contains(name)
            );

            return this.QueryTable(q, pi);
        }
    }

    internal class ChapterConfig : UnitTestEntityConfig<Chapter>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }

    [Serializable]
    public partial class CountByBookNameCriteria
    {
        public string Name;
    }
    [Serializable]
    public partial class LinqCountByBookNameCriteria
    {
        public string Name;
    }
    [Serializable]
    public partial class LinqGetByNameContainsCriteria
    {
        public string Name;
        public StringAction StringAction;
    }
    public enum StringAction
    {
        Contains, StartsWith, EndsWith,
        NotEmpty, LengthNotSupport, RefContains
    }
}