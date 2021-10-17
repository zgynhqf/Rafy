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

        public static readonly Property<int[]> ArrayValueProperty = P<Favorate>.Register(e => e.ArrayValue);
        public int[] ArrayValue
        {
            get { return this.GetProperty(ArrayValueProperty); }
            set { this.SetProperty(ArrayValueProperty, value); }
        }

        public static readonly Property<List<string>> ListValueProperty = P<Favorate>.Register(e => e.ListValue);
        public List<string> ListValue
        {
            get { return this.GetProperty(ListValueProperty); }
            set { this.SetProperty(ListValueProperty, value); }
        }

        public static readonly Property<byte[]> BytesContentProperty = P<Favorate>.Register(e => e.BytesContent);
        public byte[] BytesContent
        {
            get { return this.GetProperty(BytesContentProperty); }
            set { this.SetProperty(BytesContentProperty, value); }
        }

        public static readonly Property<FavorateType> FavorateTypeProperty = P<Favorate>.Register(e => e.FavorateType);
        public FavorateType FavorateType
        {
            get { return this.GetProperty(FavorateTypeProperty); }
            set { this.SetProperty(FavorateTypeProperty, value); }
        }

        public static readonly Property<FavorateType?> NullableFavorateTypeProperty = P<Favorate>.Register(e => e.NullableFavorateType);
        public FavorateType? NullableFavorateType
        {
            get { return this.GetProperty(NullableFavorateTypeProperty); }
            set { this.SetProperty(NullableFavorateTypeProperty, value); }
        }

        public static readonly Property<FavorateTypeWithLabel> FavorateTypeWithLabelProperty = P<Favorate>.Register(e => e.FavorateTypeWithLabel);
        public FavorateTypeWithLabel FavorateTypeWithLabel
        {
            get { return this.GetProperty(FavorateTypeWithLabelProperty); }
            set { this.SetProperty(FavorateTypeWithLabelProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> RO_FavorateTypeStringProperty = P<Favorate>.RegisterReadOnly(
            e => e.RO_FavorateTypeString, e => e.GetRO_FavorateTypeString(), FavorateTypeProperty);
        public string RO_FavorateTypeString
        {
            get { return this.GetProperty(RO_FavorateTypeStringProperty); }
        }
        private string GetRO_FavorateTypeString()
        {
            return this.FavorateType.ToString();
        }

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

        [RepositoryQuery]
        public virtual FavorateList GetByBookName(string bookName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Book.Name == bookName);
            return (FavorateList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual FavorateList GetByBookNameNotOrNull(string bookName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Book.Name != bookName || e.BookId == null)
                .OrderBy(e => e.Id);
            return (FavorateList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual FavorateList GetByChapterName(string chapterName)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Book.ChapterList.Cast<Chapter>().Any(c => c.Name == chapterName));
            return (FavorateList)this.QueryData(q);
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
            Meta.MapTable().MapAllPropertiesExcept(
                Favorate.ArrayValueProperty,
                Favorate.ListValueProperty,
                Favorate.BytesContentProperty
                );
        }
    }

    public enum FavorateType { A, B }
    public enum FavorateTypeWithLabel
    {
        [Label("第一个")]
        A,
        [Label("第二个")]
        B
    }
}