using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.MetaModel.XmlConfig;

namespace Demo
{
    [QueryEntity, Serializable]
    public class BookQueryCriteria : Criteria
    {
        public static readonly IRefIdProperty BookCategoryIdProperty =
            P<BookQueryCriteria>.RegisterRefId(e => e.BookCategoryId, ReferenceType.Normal);
        public int? BookCategoryId
        {
            get { return (int?)this.GetRefNullableId(BookCategoryIdProperty); }
            set { this.SetRefNullableId(BookCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<BookCategory> BookCategoryProperty =
            P<BookQueryCriteria>.RegisterRef(e => e.BookCategory, BookCategoryIdProperty);
        public BookCategory BookCategory
        {
            get { return this.GetRefEntity(BookCategoryProperty); }
            set { this.SetRefEntity(BookCategoryProperty, value); }
        }

        public static readonly Property<string> BookNameProperty = P<BookQueryCriteria>.Register(e => e.BookName);
        public string BookName
        {
            get { return this.GetProperty(BookNameProperty); }
            set { this.SetProperty(BookNameProperty, value); }
        }
    }

    internal class BookQueryCriteriaConfig : DemoEntityWPFViewConfig<BookQueryCriteria>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("书籍").HasDelegate(Book.NameProperty);

            View.Property(BookQueryCriteria.BookCategoryProperty).HasLabel("书籍类别").ShowIn(ShowInWhere.All);
            View.Property(BookQueryCriteria.BookNameProperty).HasLabel("书籍名称").ShowIn(ShowInWhere.All);
        }
    }
}