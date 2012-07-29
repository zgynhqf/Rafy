using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.ORM;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;

namespace JXC
{
    /// <summary>
    /// 文章
    /// </summary>
    [RootEntity, Serializable]
    public class Article : JXCEntity
    {
        #region 引用属性

        #endregion

        #region 子属性

        public static readonly ListProperty<ChapterList> ChapterListProperty = P<Article>.RegisterList(e => e.ChapterList);
        public ChapterList ChapterList
        {
            get { return this.GetLazyList(ChapterListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Article>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<DateTime> CreateTimeProperty = P<Article>.Register(e => e.CreateTime);
        public DateTime CreateTime
        {
            get { return this.GetProperty(CreateTimeProperty); }
            set { this.SetProperty(CreateTimeProperty, value); }
        }

        public static readonly Property<string> AuthorProperty = P<Article>.Register(e => e.Author);
        public string Author
        {
            get { return this.GetProperty(AuthorProperty); }
            set { this.SetProperty(AuthorProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public class ArticleList : JXCEntityList { }

    public class ArticleRepository : EntityRepository
    {
        protected ArticleRepository() { }
    }

    internal class ArticleConfig : EntityConfig<Article>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("文章").HasDelegate(Article.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(Article.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(Article.CreateTimeProperty).HasLabel("时间").ShowIn(ShowInWhere.All);
                View.Property(Article.AuthorProperty).HasLabel("作者").ShowIn(ShowInWhere.All);
            }
        }
    }
}