using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.Attributes;
using OEA.Library;
using OEA.MetaModel;

namespace Demo
{
    [ConditionQueryEntity, Serializable]
    public class BookQueryCriteria : Criteria
    {
        public static readonly RefProperty<BookCategory> BookCategoryRefProperty =
            P<BookQueryCriteria>.RegisterRef(e => e.BookCategory, ReferenceType.Normal);
        public int? BookCategoryId
        {
            get { return this.GetRefNullableId(BookCategoryRefProperty); }
            set { this.SetRefNullableId(BookCategoryRefProperty, value); }
        }
        public BookCategory BookCategory
        {
            get { return this.GetRefEntity(BookCategoryRefProperty); }
            set { this.SetRefEntity(BookCategoryRefProperty, value); }
        }

        public static readonly Property<string> BookNameProperty = P<BookQueryCriteria>.Register(e => e.BookName);
        public string BookName
        {
            get { return this.GetProperty(BookNameProperty); }
            set { this.SetProperty(BookNameProperty, value); }
        }
    }
}