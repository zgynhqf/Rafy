using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace UT
{
    /// <summary>
    /// 收藏项
    /// </summary>
    [RootEntity, Serializable]
    public partial class Favorate : UnitTestEntity
    {
        #region 构造函数

        public Favorate() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Favorate(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty BookIdProperty =
            P<Favorate>.RegisterRefId(e => e.BookId, ReferenceType.Normal);
        public int? BookId
        {
            get { return (int?)this.GetRefNullableId(BookIdProperty); }
            set { this.SetRefNullableId(BookIdProperty, value); }
        }
        public static readonly RefEntityProperty<Book> BookProperty =
            P<Favorate>.RegisterRef(e => e.Book, BookIdProperty);
        /// <summary>
        /// 收藏的书籍
        /// </summary>
        public Book Book
        {
            get { return this.GetRefEntity(BookProperty); }
            set { this.SetRefEntity(BookProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Favorate>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 收藏项 列表类。
    /// </summary>
    [Serializable]
    public partial class FavorateList : UnitTestEntityList { }

    /// <summary>
    /// 收藏项 仓库类。
    /// 负责 收藏项 类的查询、保存。
    /// </summary>
    public partial class FavorateRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected FavorateRepository() { }

        public FavorateList GetByBookName(string bookName)
        {
            return this.FetchList(r => r.DA_GetByBookName(bookName));
        }
        private EntityList DA_GetByBookName(string bookName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Book.Name == bookName);
            return this.QueryList(q);
        }

        public FavorateList GetByBookNameNotOrNull(string bookName)
        {
            return this.FetchList(r => r.DA_GetByBookNameNotOrNull(bookName));
        }
        private EntityList DA_GetByBookNameNotOrNull(string bookName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Book.Name != bookName || e.BookId == null);
            return this.QueryList(q);
        }

        public FavorateList GetByChapterName(string chapterName)
        {
            return this.FetchList(r => r.DA_GetByChapterName(chapterName));
        }
        private EntityList DA_GetByChapterName(string chapterName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Book.ChapterList.Cast<Chapter>().Any(c => c.Name == chapterName));
            return this.QueryList(q);
        }
    }

    /// <summary>
    /// 收藏项 配置类。
    /// 负责 收藏项 类的实体元数据的配置。
    /// </summary>
    internal class FavorateConfig : UnitTestEntityConfig<Favorate>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}