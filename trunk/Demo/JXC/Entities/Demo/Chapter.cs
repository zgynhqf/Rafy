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
    /// 章节
    /// </summary>
    [ChildEntity, Serializable]
    public class Chapter : JXCEntity
    {
        #region 引用属性

        public static readonly RefProperty<Article> ArticleRefProperty =
            P<Chapter>.RegisterRef(e => e.Article, ReferenceType.Parent);
        public int ArticleId
        {
            get { return this.GetRefId(ArticleRefProperty); }
            set { this.SetRefId(ArticleRefProperty, value); }
        }
        public Article Article
        {
            get { return this.GetRefEntity(ArticleRefProperty); }
            set { this.SetRefEntity(ArticleRefProperty, value); }
        }

        #endregion

        #region 子属性

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
    public class ChapterList : JXCEntityList { }

    public class ChapterRepository : EntityRepository
    {
        protected ChapterRepository() { }
    }

    internal class ChapterConfig : EntityConfig<Chapter>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("章节").HasDelegate(Chapter.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(Chapter.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
            }
        }
    }
}